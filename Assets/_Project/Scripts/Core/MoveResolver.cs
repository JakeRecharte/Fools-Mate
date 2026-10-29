using System;
using System.Collections.Generic;
using System.Linq;

namespace FoolsMate.Core
{
    /// <summary>
    /// Plays out a turn's planned moves, both sides' at the same time, and changes the board to match. Assumes
    /// the player's plan already passed <see cref="MoveCheck"/> and the enemy's came from
    /// <see cref="EnemyPlanner"/>.
    /// </summary>
    public static class MoveResolver
    {
        /// <summary>Resolves one side's plan on its own.</summary>
        public static List<ResolutionEvent> Resolve(BoardState board, TurnPlan plan, Random random)
        {
            return Resolve(board, plan, null, random);
        }

        /// <summary>
        /// Resolves both plans in four phases: lift every mover, group the moves by destination, settle each
        /// destination (friendly collisions, then enemy collisions, then plain moves and attacks), and Bounce
        /// the losers last. Returns what happened, in order.
        /// </summary>
        /// <param name="enemyPlan">The enemy's moves this turn. Null means the enemy stays put.</param>
        public static List<ResolutionEvent> Resolve(BoardState board, TurnPlan plan, TurnPlan enemyPlan,
            Random random)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var events = new List<ResolutionEvent>();

            // Sorted by ID so the same plans and seed always play out the same way.
            IEnumerable<PlannedMove> allMoves = enemyPlan != null ? plan.Moves.Concat(enemyPlan.Moves) : plan.Moves;
            List<PlannedMove> moves = allMoves.OrderBy(move => move.Piece.Id).ToList();

            // Phase 0: colors, read now while every piece is still on the board.
            var colors = new Dictionary<Piece, PieceColor>();
            foreach (PlannedMove move in moves)
            {
                colors[move.Piece] = board.GetColor(move.Piece);
            }

            // Phase 1: every mover leaves its cell, so swaps and chains need no special cases.
            foreach (PlannedMove move in moves)
            {
                board.Lift(move.Piece);
            }

            // Phase 2: group by destination, keeping the order each destination first appears.
            var destinations = new List<Cell>();
            var movesByDestination = new Dictionary<Cell, List<PlannedMove>>();
            foreach (PlannedMove move in moves)
            {
                if (!movesByDestination.TryGetValue(move.To, out List<PlannedMove> group))
                {
                    group = new List<PlannedMove>();
                    movesByDestination.Add(move.To, group);
                    destinations.Add(move.To);
                }

                group.Add(move);
            }

            // Phase 3: settle each destination. Each side settles its own friendly collision first, then the
            // two sides' survivors collide.
            var bounces = new List<PlannedMove>();
            foreach (Cell destination in destinations)
            {
                var winners = new List<PlannedMove>();
                foreach (IGrouping<PieceColor, PlannedMove> side in movesByDestination[destination]
                             .GroupBy(move => colors[move.Piece]))
                {
                    if (TryPickWinner(side.ToList(), bounces, out PlannedMove sideWinner))
                    {
                        winners.Add(sideWinner);
                    }
                }

                if (winners.Count == 2)
                {
                    ResolveCollision(board, winners[0], winners[1], colors, events, bounces);
                }
                else if (winners.Count == 1)
                {
                    ResolveArrival(board, winners[0], colors[winners[0].Piece], events, bounces);
                }
            }

            // Phase 4: bounces last, so "empty" means empty on the final board.
            foreach (PlannedMove move in bounces)
            {
                BouncePiece(board, move, colors[move.Piece], random, events);
            }

            return events;
        }

        /// <summary>
        /// Friendly collision: the highest Priority goes on and the rest are Bounced. On a tie they're all
        /// Bounced and nobody goes on.
        /// </summary>
        private static bool TryPickWinner(List<PlannedMove> group, List<PlannedMove> bounces, out PlannedMove winner)
        {
            if (group.Count == 1)
            {
                winner = group[0];
                return true;
            }

            int highest = group.Max(move => move.Piece.Stats.Priority);
            List<PlannedMove> best = group.Where(move => move.Piece.Stats.Priority == highest).ToList();

            if (best.Count > 1)
            {
                bounces.AddRange(group);
                winner = default;
                return false;
            }

            winner = best[0];
            foreach (PlannedMove move in group)
            {
                if (move.Piece != winner.Piece)
                {
                    bounces.Add(move);
                }
            }

            return true;
        }

        /// <summary>
        /// Two enemies moving into the same cell. Both deal their Damage at once. A lone survivor goes on into the
        /// cell. If both survive, Track 1 applies: both die anyway. When both die, the owner of the
        /// higher-Priority piece earns the other; on a tie nobody does.
        /// </summary>
        private static void ResolveCollision(BoardState board, PlannedMove a, PlannedMove b,
            Dictionary<Piece, PieceColor> colors, List<ResolutionEvent> events, List<PlannedMove> bounces)
        {
            Piece pieceA = a.Piece;
            Piece pieceB = b.Piece;
            Cell cell = a.To;

            bool aDies = board.TakeDamage(pieceA, pieceB.Stats.Damage);
            bool bDies = board.TakeDamage(pieceB, pieceA.Stats.Damage);

            if (aDies != bDies)
            {
                PlannedMove survivor = aDies ? b : a;
                PlannedMove loser = aDies ? a : b;
                int damage = survivor.Piece.Stats.Damage;

                board.Remove(loser.Piece);
                events.Add(ResolutionEvent.Captured(loser.Piece, colors[loser.Piece], cell, damage, survivor.Piece,
                    colors[survivor.Piece]));
                events.Add(ResolutionEvent.Damaged(survivor.Piece, colors[survivor.Piece], cell,
                    loser.Piece.Stats.Damage, loser.Piece, colors[loser.Piece]));

                ResolveArrival(board, survivor, colors[survivor.Piece], events, bounces);
                return;
            }

            // Both died, or both survived and Track 1 kills them anyway.
            board.Remove(pieceA);
            board.Remove(pieceB);

            int priorityA = pieceA.Stats.Priority;
            int priorityB = pieceB.Stats.Priority;

            if (priorityA == priorityB)
            {
                events.Add(ResolutionEvent.Died(pieceA, colors[pieceA], cell));
                events.Add(ResolutionEvent.Died(pieceB, colors[pieceB], cell));
                return;
            }

            Piece higher = priorityA > priorityB ? pieceA : pieceB;
            Piece lower = priorityA > priorityB ? pieceB : pieceA;

            events.Add(ResolutionEvent.Captured(lower, colors[lower], cell, higher.Stats.Damage, higher,
                colors[higher]));
            events.Add(ResolutionEvent.Died(higher, colors[higher], cell));
        }

        /// <summary>A plain move onto an empty cell, or an attack on a piece that's staying put.</summary>
        private static void ResolveArrival(BoardState board, PlannedMove move, PieceColor color,
            List<ResolutionEvent> events, List<PlannedMove> bounces)
        {
            Piece attacker = move.Piece;

            // Every mover was lifted, so anything still here is staying put.
            Piece defender = board.GetPieceAt(move.To);

            if (defender == null)
            {
                board.Land(attacker, move.To);
                events.Add(ResolutionEvent.Moved(attacker, color, move.From, move.To));
                return;
            }

            PieceColor defenderColor = board.GetColor(defender);
            if (defenderColor == color)
            {
                throw new InvalidOperationException(
                    $"{attacker} can't land on {move.To}: {defender} is staying there. Run the Move Check first.");
            }

            int damage = attacker.Stats.Damage;
            if (board.TakeDamage(defender, damage))
            {
                board.Remove(defender);
                events.Add(ResolutionEvent.Captured(defender, defenderColor, move.To, damage, attacker, color));

                board.Land(attacker, move.To);
                events.Add(ResolutionEvent.Moved(attacker, color, move.From, move.To));
            }
            else
            {
                events.Add(ResolutionEvent.Damaged(defender, defenderColor, move.To, damage, attacker, color));
                bounces.Add(move);
            }
        }

        /// <summary>Bounces from the cell the piece tried to enter, or kills it if there's no room.</summary>
        private static void BouncePiece(BoardState board, PlannedMove move, PieceColor color, Random random,
            List<ResolutionEvent> events)
        {
            if (Bounce.TryFindCell(board, move.To, random, out Cell cell))
            {
                board.Land(move.Piece, cell);
                events.Add(ResolutionEvent.Bounced(move.Piece, color, move.From, cell));
            }
            else
            {
                board.Remove(move.Piece);
                events.Add(ResolutionEvent.Died(move.Piece, color, move.From));
            }
        }
    }
}
