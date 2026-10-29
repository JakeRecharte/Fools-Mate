using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class EnemyPlannerTests
    {
        // The enemy is Black throughout; White is the player.
        private const PieceColor Enemy = PieceColor.Black;
        private const PieceColor Player = PieceColor.White;

        private BoardState board;
        private int nextId;

        [SetUp]
        public void SetUp()
        {
            board = new BoardState();
            nextId = 1;
        }

        private Piece Place(PieceType type, PieceColor color, int file, int rank, int points = 1)
        {
            var piece = new Piece(nextId++, "test", type, new PieceStats(points, 1, 1, 1));
            board.Place(piece, color, new Cell(file, rank));
            return piece;
        }

        private TurnPlan Plan(int moves = 2, int seed = 1)
        {
            return EnemyPlanner.Plan(board, Enemy, moves, new Random(seed));
        }

        private static PlannedMove OnlyMove(TurnPlan plan)
        {
            Assert.AreEqual(1, plan.EnergyUsed);
            return plan.Moves.Single();
        }

        [Test]
        public void CapturesMostValuablePiece()
        {
            Piece rook = Place(PieceType.Rook, Enemy, 0, 7, points: 5);
            Place(PieceType.Pawn, Player, 0, 2, points: 1);
            Piece queen = Place(PieceType.Queen, Player, 5, 7, points: 9);

            PlannedMove move = OnlyMove(Plan(moves: 1));

            Assert.AreEqual(rook, move.Piece);
            Assert.AreEqual(board.GetCell(queen), move.To);
        }

        [Test]
        public void GoesForThePlayersKing()
        {
            Place(PieceType.Rook, Enemy, 0, 7, points: 5);
            Piece king = Place(PieceType.King, Player, 0, 0, points: 0);
            Place(PieceType.Queen, Player, 7, 7, points: 9);

            PlannedMove move = OnlyMove(Plan(moves: 1));

            Assert.AreEqual(board.GetCell(king), move.To);
        }

        [Test]
        public void AvoidsUnsafeCell()
        {
            // The knight at (1, 7) can go to (0, 5), (2, 5), or (3, 6). The white rook on file 2 covers (2, 5),
            // and the white rook on rank 6 covers (3, 6), so only (0, 5) is safe.
            Place(PieceType.Knight, Enemy, 1, 7, points: 3);
            Place(PieceType.Rook, Player, 2, 0, points: 5);
            Place(PieceType.Rook, Player, 7, 6, points: 5);

            for (int seed = 1; seed <= 20; seed++)
            {
                PlannedMove move = OnlyMove(Plan(moves: 1, seed: seed));
                Assert.AreEqual(new Cell(0, 5), move.To, $"Seed {seed}");
            }
        }

        [Test]
        public void TradesUpWhenWorthIt()
        {
            // The white queen is defended by the rook behind it, but 9 is worth more than the knight's 3.
            Place(PieceType.Knight, Enemy, 2, 5, points: 3);
            Piece queen = Place(PieceType.Queen, Player, 3, 3, points: 9);
            Place(PieceType.Rook, Player, 3, 0, points: 5);

            PlannedMove move = OnlyMove(Plan(moves: 1));

            Assert.AreEqual(board.GetCell(queen), move.To);
        }

        [Test]
        public void DoesNotTradeDown()
        {
            // Taking the defended pawn would risk the queen; any safe quiet move scores higher.
            Place(PieceType.Queen, Enemy, 0, 7, points: 9);
            Place(PieceType.Pawn, Player, 0, 3, points: 1);
            Place(PieceType.Rook, Player, 7, 3, points: 5);

            for (int seed = 1; seed <= 20; seed++)
            {
                PlannedMove move = OnlyMove(Plan(moves: 1, seed: seed));
                Assert.AreNotEqual(new Cell(0, 3), move.To, $"Seed {seed}");
            }
        }

        [Test]
        public void NeverTargetsOwnPiece()
        {
            // Boxed in by its own pieces apart from one cell.
            Place(PieceType.King, Enemy, 0, 7);
            Place(PieceType.Pawn, Enemy, 0, 6);
            Place(PieceType.Pawn, Enemy, 1, 6);

            foreach (PlannedMove move in Plan(moves: 3).Moves)
            {
                Piece occupant = board.GetPieceAt(move.To);
                Assert.IsTrue(occupant == null || board.GetColor(occupant) != Enemy, $"{move} lands on its own piece.");
            }
        }

        [Test]
        public void NoTwoMovesToSameCell()
        {
            // Both knights can reach (2, 5); it's the only cell for either that captures something.
            Place(PieceType.Knight, Enemy, 1, 7, points: 3);
            Place(PieceType.Knight, Enemy, 3, 7, points: 3);
            Place(PieceType.Queen, Player, 2, 5, points: 9);

            TurnPlan plan = Plan(moves: 2);

            Assert.AreEqual(2, plan.EnergyUsed);
            List<Cell> destinations = plan.Moves.Select(move => move.To).ToList();
            CollectionAssert.AllItemsAreUnique(destinations);
            CollectionAssert.Contains(destinations, new Cell(2, 5));
        }

        [Test]
        public void UsesAtMostMoves()
        {
            for (int file = 0; file < 4; file++)
            {
                Place(PieceType.Knight, Enemy, file * 2, 7, points: 3);
            }

            Assert.AreEqual(2, Plan(moves: 2).EnergyUsed);
        }

        [Test]
        public void FewerPiecesThanMoves_PlansWhatItCan()
        {
            Place(PieceType.Knight, Enemy, 1, 7, points: 3);

            Assert.AreEqual(1, Plan(moves: 2).EnergyUsed);
        }

        [Test]
        public void ZeroMoves_PlansNothing()
        {
            Place(PieceType.Knight, Enemy, 1, 7, points: 3);

            Assert.AreEqual(0, Plan(moves: 0).EnergyUsed);
        }

        [Test]
        public void EveryMove_IsAReachableTarget()
        {
            Place(PieceType.Queen, Enemy, 3, 7, points: 9);
            Place(PieceType.Knight, Enemy, 1, 7, points: 3);
            Place(PieceType.Pawn, Enemy, 4, 6);
            Place(PieceType.Rook, Player, 0, 0, points: 5);

            foreach (PlannedMove move in Plan(moves: 3).Moves)
            {
                CollectionAssert.Contains(MoveTargets.GetTargets(board, move.Piece), move.To);
                Assert.AreEqual(board.GetCell(move.Piece), move.From);
            }
        }

        [Test]
        public void SameSeed_SamePlan()
        {
            Place(PieceType.Queen, Enemy, 3, 7, points: 9);
            Place(PieceType.Knight, Enemy, 1, 7, points: 3);
            Place(PieceType.Rook, Player, 0, 0, points: 5);

            List<PlannedMove> first = Plan(seed: 42).Moves.OrderBy(move => move.Piece.Id).ToList();
            List<PlannedMove> second = Plan(seed: 42).Moves.OrderBy(move => move.Piece.Id).ToList();

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => EnemyPlanner.Plan(null, Enemy, 2, new Random(1)));
            Assert.Throws<ArgumentNullException>(() => EnemyPlanner.Plan(board, Enemy, 2, null));
        }
    }
}
