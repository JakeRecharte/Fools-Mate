using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class MoveResolverTests
    {
        private BoardState board;
        private TurnPlan plan;
        private TurnPlan enemyPlan;
        private int nextId;

        [SetUp]
        public void SetUp()
        {
            board = new BoardState();
            plan = new TurnPlan(8);
            enemyPlan = new TurnPlan(8);
            nextId = 1;
        }

        private Piece Place(PieceColor color, int file, int rank, int priority = 1, int damage = 1, int health = 1)
        {
            var piece = new Piece(nextId++, "test", PieceType.Rook, new PieceStats(1, priority, damage, health));
            board.Place(piece, color, new Cell(file, rank));
            return piece;
        }

        private void PlanMove(Piece piece, int file, int rank)
        {
            Assert.IsTrue(plan.TryPlan(new PlannedMove(piece, board.GetCell(piece), new Cell(file, rank))));
        }

        private void PlanEnemyMove(Piece piece, int file, int rank)
        {
            Assert.IsTrue(enemyPlan.TryPlan(new PlannedMove(piece, board.GetCell(piece), new Cell(file, rank))));
        }

        private List<ResolutionEvent> Resolve(int seed = 1)
        {
            return MoveResolver.Resolve(board, plan, enemyPlan, new Random(seed));
        }

        private static List<ResolutionEvent> OfKind(List<ResolutionEvent> events, ResolutionEventKind kind)
        {
            return events.Where(e => e.Kind == kind).ToList();
        }

        private static bool IsOnBoard(BoardState state, Piece piece)
        {
            return state.Pieces.Contains(piece);
        }

        private static int Distance(Cell a, Cell b)
        {
            return Math.Max(Math.Abs(a.File - b.File), Math.Abs(a.Rank - b.Rank));
        }

        [Test]
        public void EmptyPlan_DoesNothing()
        {
            Piece rook = Place(PieceColor.White, 0, 0);

            Assert.IsEmpty(Resolve());
            Assert.AreEqual(new Cell(0, 0), board.GetCell(rook));
        }

        [Test]
        public void MoveToEmptyCell_MovesThePiece()
        {
            Piece rook = Place(PieceColor.White, 0, 0);
            PlanMove(rook, 0, 4);

            List<ResolutionEvent> events = Resolve();

            Assert.AreEqual(new Cell(0, 4), board.GetCell(rook));
            Assert.IsNull(board.GetPieceAt(new Cell(0, 0)));
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(ResolutionEventKind.Moved, events[0].Kind);
            Assert.AreEqual(new Cell(0, 0), events[0].From);
            Assert.AreEqual(new Cell(0, 4), events[0].To);
        }

        [Test]
        public void Swap_BothPiecesMove()
        {
            Piece low = Place(PieceColor.White, 0, 0);
            Piece high = Place(PieceColor.White, 0, 3);
            PlanMove(low, 0, 3);
            PlanMove(high, 0, 0);

            List<ResolutionEvent> events = Resolve();

            Assert.AreEqual(new Cell(0, 3), board.GetCell(low));
            Assert.AreEqual(new Cell(0, 0), board.GetCell(high));
            Assert.IsTrue(events.All(e => e.Kind == ResolutionEventKind.Moved));
        }

        [Test]
        public void Chain_PieceMovesIntoCellBeingLeft()
        {
            Piece back = Place(PieceColor.White, 0, 0);
            Piece front = Place(PieceColor.White, 0, 1);
            PlanMove(back, 0, 1);
            PlanMove(front, 0, 5);

            Resolve();

            Assert.AreEqual(new Cell(0, 1), board.GetCell(back));
            Assert.AreEqual(new Cell(0, 5), board.GetCell(front));
        }

        [Test]
        public void AttackThatKills_CapturesAndTakesTheCell()
        {
            Piece attacker = Place(PieceColor.White, 0, 0, damage: 2);
            Piece defender = Place(PieceColor.Black, 0, 5, health: 2);
            PlanMove(attacker, 0, 5);

            List<ResolutionEvent> events = Resolve();

            Assert.IsFalse(IsOnBoard(board, defender));
            Assert.AreEqual(new Cell(0, 5), board.GetCell(attacker));

            ResolutionEvent captured = events.Single(e => e.Kind == ResolutionEventKind.Captured);
            Assert.AreEqual(defender, captured.Piece);
            Assert.AreEqual(PieceColor.Black, captured.Color);
            Assert.AreEqual(attacker, captured.By);
            Assert.AreEqual(PieceColor.White, captured.ByColor);
            Assert.AreEqual(2, captured.Amount);
        }

        [Test]
        public void AttackThatKills_LogsCaptureBeforeMove()
        {
            Piece attacker = Place(PieceColor.White, 0, 0);
            Place(PieceColor.Black, 0, 5);
            PlanMove(attacker, 0, 5);

            List<ResolutionEvent> events = Resolve();

            CollectionAssert.AreEqual(
                new[] { ResolutionEventKind.Captured, ResolutionEventKind.Moved },
                events.Select(e => e.Kind));
        }

        [Test]
        public void AttackThatDoesntKill_DamagesDefenderAndBouncesAttacker()
        {
            Piece attacker = Place(PieceColor.White, 0, 0, damage: 1);
            Piece defender = Place(PieceColor.Black, 3, 5, health: 3);
            PlanMove(attacker, 3, 5);

            List<ResolutionEvent> events = Resolve();

            Assert.AreEqual(2, board.GetCurrentHealth(defender));
            Assert.AreEqual(new Cell(3, 5), board.GetCell(defender));

            Cell landed = board.GetCell(attacker);
            Assert.AreNotEqual(new Cell(3, 5), landed);
            Assert.LessOrEqual(Distance(landed, new Cell(3, 5)), Bounce.Range);

            CollectionAssert.AreEqual(
                new[] { ResolutionEventKind.Damaged, ResolutionEventKind.Bounced },
                events.Select(e => e.Kind));
            Assert.AreEqual(attacker, events[0].By);
            Assert.AreEqual(new Cell(0, 0), events[1].From);
            Assert.AreEqual(landed, events[1].To);
        }

        [Test]
        public void FriendlyCollision_HigherPriorityTakesTheCell()
        {
            Piece strong = Place(PieceColor.White, 0, 0, priority: 3);
            Piece weak = Place(PieceColor.White, 6, 0, priority: 1);
            PlanMove(strong, 3, 0);
            PlanMove(weak, 3, 0);

            List<ResolutionEvent> events = Resolve();

            Assert.AreEqual(new Cell(3, 0), board.GetCell(strong));
            Assert.AreNotEqual(new Cell(3, 0), board.GetCell(weak));
            Assert.LessOrEqual(Distance(board.GetCell(weak), new Cell(3, 0)), Bounce.Range);
            Assert.AreEqual(ResolutionEventKind.Bounced, events.Single(e => e.Piece == weak).Kind);
        }

        [Test]
        public void FriendlyCollision_TiedPriority_BothBounceAndCellStaysEmpty()
        {
            Piece left = Place(PieceColor.White, 0, 0, priority: 2);
            Piece right = Place(PieceColor.White, 6, 0, priority: 2);
            PlanMove(left, 3, 0);
            PlanMove(right, 3, 0);

            List<ResolutionEvent> events = Resolve();

            Assert.IsNull(board.GetPieceAt(new Cell(3, 0)));
            Assert.AreEqual(2, events.Count);
            Assert.IsTrue(events.All(e => e.Kind == ResolutionEventKind.Bounced));
            Assert.AreNotEqual(board.GetCell(left), board.GetCell(right));
        }

        [Test]
        public void BounceWithNoRoom_PieceDies()
        {
            // Attack d1 from a1 with every other cell within 2 tiles of d1 filled by White pieces that stay.
            var target = new Cell(3, 0);
            Piece attacker = Place(PieceColor.White, 0, 0, damage: 1);
            Place(PieceColor.Black, target.File, target.Rank, health: 5);

            for (int file = target.File - Bounce.Range; file <= target.File + Bounce.Range; file++)
            {
                for (int rank = 0; rank <= target.Rank + Bounce.Range; rank++)
                {
                    var cell = new Cell(file, rank);
                    if (cell.IsOnBoard && cell != target && board.GetPieceAt(cell) == null)
                    {
                        Place(PieceColor.White, file, rank);
                    }
                }
            }

            // The attacker's own cell is out of range, so its lift doesn't open a gap.
            PlanMove(attacker, target.File, target.Rank);

            List<ResolutionEvent> events = Resolve();

            Assert.IsFalse(IsOnBoard(board, attacker));
            ResolutionEvent died = events.Single(e => e.Piece == attacker);
            Assert.AreEqual(ResolutionEventKind.Died, died.Kind);
            Assert.IsNull(died.By);
            Assert.IsNull(died.ByColor);
        }

        [Test]
        public void MoveOntoOwnStayingPiece_Throws()
        {
            Piece rook = Place(PieceColor.White, 0, 0);
            Place(PieceColor.White, 0, 3);
            PlanMove(rook, 0, 3);

            Assert.Throws<InvalidOperationException>(() => Resolve());
        }

        [Test]
        public void SameSeed_GivesSameResult()
        {
            List<Cell> RunOnce()
            {
                SetUp();
                Piece left = Place(PieceColor.White, 0, 0, priority: 2);
                Piece right = Place(PieceColor.White, 6, 0, priority: 2);
                PlanMove(left, 3, 0);
                PlanMove(right, 3, 0);
                Resolve(seed: 7);
                return new List<Cell> { board.GetCell(left), board.GetCell(right) };
            }

            CollectionAssert.AreEqual(RunOnce(), RunOnce());
        }

        // Both sides

        [Test]
        public void EnemyCollision_TiedPriority_BothDie_NobodyEarnsIt()
        {
            Piece white = Place(PieceColor.White, 3, 0);
            Piece black = Place(PieceColor.Black, 3, 7);
            PlanMove(white, 3, 4);
            PlanEnemyMove(black, 3, 4);

            List<ResolutionEvent> events = Resolve();

            Assert.IsFalse(IsOnBoard(board, white));
            Assert.IsFalse(IsOnBoard(board, black));
            Assert.IsEmpty(OfKind(events, ResolutionEventKind.Captured));
            Assert.AreEqual(2, OfKind(events, ResolutionEventKind.Died).Count);
            Assert.IsNull(board.GetPieceAt(new Cell(3, 4)));
        }

        [Test]
        public void EnemyCollision_BothDie_HigherPriorityEarnsIt()
        {
            Piece white = Place(PieceColor.White, 3, 0, priority: 2);
            Piece black = Place(PieceColor.Black, 3, 7, priority: 1);
            PlanMove(white, 3, 4);
            PlanEnemyMove(black, 3, 4);

            List<ResolutionEvent> events = Resolve();

            List<ResolutionEvent> captures = OfKind(events, ResolutionEventKind.Captured);
            Assert.AreEqual(1, captures.Count);
            Assert.AreEqual(black, captures[0].Piece);
            Assert.AreEqual(white, captures[0].By);
            Assert.AreEqual(PieceColor.White, captures[0].ByColor);

            List<ResolutionEvent> deaths = OfKind(events, ResolutionEventKind.Died);
            Assert.AreEqual(1, deaths.Count);
            Assert.AreEqual(white, deaths[0].Piece);
            Assert.IsFalse(IsOnBoard(board, white));
        }

        [Test]
        public void EnemyCollision_OneSurvives_TakesTheCell()
        {
            Piece white = Place(PieceColor.White, 3, 0, health: 2);
            Piece black = Place(PieceColor.Black, 3, 7);
            PlanMove(white, 3, 4);
            PlanEnemyMove(black, 3, 4);

            List<ResolutionEvent> events = Resolve();

            Assert.AreEqual(new Cell(3, 4), board.GetCell(white));
            Assert.AreEqual(1, board.GetCurrentHealth(white));
            Assert.IsFalse(IsOnBoard(board, black));

            List<ResolutionEvent> captures = OfKind(events, ResolutionEventKind.Captured);
            Assert.AreEqual(1, captures.Count);
            Assert.AreEqual(black, captures[0].Piece);
            Assert.AreEqual(PieceColor.White, captures[0].ByColor);
            Assert.AreEqual(1, OfKind(events, ResolutionEventKind.Damaged).Count);
            Assert.AreEqual(1, OfKind(events, ResolutionEventKind.Moved).Count);
        }

        [Test]
        public void EnemyCollision_BothSurvive_Track1_BothDie()
        {
            Piece white = Place(PieceColor.White, 3, 0, priority: 1, health: 3);
            Piece black = Place(PieceColor.Black, 3, 7, priority: 2, health: 3);
            PlanMove(white, 3, 4);
            PlanEnemyMove(black, 3, 4);

            List<ResolutionEvent> events = Resolve();

            Assert.IsFalse(IsOnBoard(board, white));
            Assert.IsFalse(IsOnBoard(board, black));

            List<ResolutionEvent> captures = OfKind(events, ResolutionEventKind.Captured);
            Assert.AreEqual(1, captures.Count);
            Assert.AreEqual(white, captures[0].Piece);
            Assert.AreEqual(PieceColor.Black, captures[0].ByColor);
        }

        [Test]
        public void Swap_AcrossSides_PassesThrough()
        {
            Piece white = Place(PieceColor.White, 3, 3);
            Piece black = Place(PieceColor.Black, 3, 4);
            PlanMove(white, 3, 4);
            PlanEnemyMove(black, 3, 3);

            List<ResolutionEvent> events = Resolve();

            Assert.AreEqual(new Cell(3, 4), board.GetCell(white));
            Assert.AreEqual(new Cell(3, 3), board.GetCell(black));
            Assert.IsTrue(events.All(e => e.Kind == ResolutionEventKind.Moved));
        }

        [Test]
        public void EnemyMovingAway_DodgesTheAttack()
        {
            Piece white = Place(PieceColor.White, 3, 0);
            Piece black = Place(PieceColor.Black, 3, 5);
            PlanMove(white, 3, 5);
            PlanEnemyMove(black, 6, 5);

            List<ResolutionEvent> events = Resolve();

            Assert.AreEqual(new Cell(3, 5), board.GetCell(white));
            Assert.AreEqual(new Cell(6, 5), board.GetCell(black));
            Assert.IsEmpty(OfKind(events, ResolutionEventKind.Captured));
        }

        [Test]
        public void EnemyAttack_CapturesStayingPlayerPiece()
        {
            Piece white = Place(PieceColor.White, 3, 2);
            Piece black = Place(PieceColor.Black, 3, 7);
            PlanEnemyMove(black, 3, 2);

            List<ResolutionEvent> events = Resolve();

            Assert.IsFalse(IsOnBoard(board, white));
            Assert.AreEqual(new Cell(3, 2), board.GetCell(black));
            List<ResolutionEvent> captures = OfKind(events, ResolutionEventKind.Captured);
            Assert.AreEqual(1, captures.Count);
            Assert.AreEqual(PieceColor.Black, captures[0].ByColor);
        }

        [Test]
        public void FriendlyTie_ThenEnemyTakesTheCellUncontested()
        {
            Piece left = Place(PieceColor.White, 0, 4);
            Piece right = Place(PieceColor.White, 6, 4);
            Piece black = Place(PieceColor.Black, 3, 7);
            PlanMove(left, 3, 4);
            PlanMove(right, 3, 4);
            PlanEnemyMove(black, 3, 4);

            List<ResolutionEvent> events = Resolve();

            Assert.AreEqual(new Cell(3, 4), board.GetCell(black));
            Assert.AreEqual(2, OfKind(events, ResolutionEventKind.Bounced).Count
                + OfKind(events, ResolutionEventKind.Died).Count);
            Assert.AreNotEqual(new Cell(3, 4), board.GetCell(left));
            Assert.AreNotEqual(new Cell(3, 4), board.GetCell(right));
        }

        [Test]
        public void CollisionWinner_GoesOnToAttackStayingPiece()
        {
            // White and a black mover collide on a cell where another black piece is staying put. White survives
            // the collision, then attacks the staying piece.
            Piece white = Place(PieceColor.White, 3, 0, health: 2);
            Piece mover = Place(PieceColor.Black, 0, 4);
            Piece stayer = Place(PieceColor.Black, 3, 4);
            PlanMove(white, 3, 4);

            // Not a move the planner would make, but the resolver still copes with it.
            PlanEnemyMove(mover, 3, 4);

            List<ResolutionEvent> events = Resolve();

            Assert.AreEqual(new Cell(3, 4), board.GetCell(white));
            Assert.IsFalse(IsOnBoard(board, mover));
            Assert.IsFalse(IsOnBoard(board, stayer));
            Assert.AreEqual(2, OfKind(events, ResolutionEventKind.Captured).Count);
        }

        [Test]
        public void NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => MoveResolver.Resolve(null, plan, new Random(1)));
            Assert.Throws<ArgumentNullException>(() => MoveResolver.Resolve(board, null, new Random(1)));
            Assert.Throws<ArgumentNullException>(() => MoveResolver.Resolve(board, plan, null));
            Assert.Throws<ArgumentNullException>(() => MoveResolver.Resolve(board, plan, enemyPlan, null));
        }
    }
}
