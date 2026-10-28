using System.Collections.Generic;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class MoveCheckTests
    {
        private BoardState board;
        private TurnPlan plan;
        private int nextId;

        [SetUp]
        public void SetUp()
        {
            board = new BoardState();
            plan = new TurnPlan(4);
            nextId = 1;
        }

        private Piece Place(PieceType type, PieceColor color, int file, int rank)
        {
            var piece = new Piece(nextId++, "test", type, new PieceStats(1, 1, 1, 1));
            board.Place(piece, color, new Cell(file, rank));
            return piece;
        }

        private void PlanMove(Piece piece, int file, int rank)
        {
            Assert.IsTrue(plan.TryPlan(new PlannedMove(piece, board.GetCell(piece), new Cell(file, rank))));
        }

        private List<MoveProblem> Validate()
        {
            return MoveCheck.Validate(board, plan);
        }

        [Test]
        public void EmptyPlan_HasNoProblems()
        {
            Place(PieceType.King, PieceColor.White, 4, 0);

            Assert.IsEmpty(Validate());
        }

        [Test]
        public void MoveToEmptyCell_HasNoProblems()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 1);
            PlanMove(pawn, 4, 3);

            Assert.IsEmpty(Validate());
        }

        [Test]
        public void MoveOntoEnemy_HasNoProblems()
        {
            Piece rook = Place(PieceType.Rook, PieceColor.White, 0, 0);
            Place(PieceType.Pawn, PieceColor.Black, 0, 5);
            PlanMove(rook, 0, 5);

            Assert.IsEmpty(Validate());
        }

        [Test]
        public void MoveOntoOwnStayingPiece_ReportsDestinationOccupied()
        {
            Piece knight = Place(PieceType.Knight, PieceColor.White, 1, 0);
            Place(PieceType.Pawn, PieceColor.White, 3, 1);
            PlanMove(knight, 3, 1);

            List<MoveProblem> problems = Validate();

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(MoveProblemKind.DestinationOccupied, problems[0].Kind);
            Assert.AreEqual(knight, problems[0].Piece);
        }

        [Test]
        public void MoveOntoOwnMovingPiece_HasNoProblems()
        {
            Piece knight = Place(PieceType.Knight, PieceColor.White, 1, 0);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 3, 1);
            PlanMove(knight, 3, 1);
            PlanMove(pawn, 3, 3);

            Assert.IsEmpty(Validate());
        }

        [Test]
        public void PathThroughMovingPiece_BecomesUnreachable_WhenThatMoveIsCancelled()
        {
            Piece bishop = Place(PieceType.Bishop, PieceColor.White, 2, 0);
            Piece knight = Place(PieceType.Knight, PieceColor.White, 3, 1);
            PlanMove(knight, 5, 2);
            PlanMove(bishop, 5, 3);
            Assert.IsEmpty(Validate(), "Bishop should pass through the moving knight.");

            plan.Cancel(knight);
            List<MoveProblem> problems = Validate();

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(MoveProblemKind.Unreachable, problems[0].Kind);
            Assert.AreEqual(bishop, problems[0].Piece);
        }

        [Test]
        public void TwoPiecesToSameCell_HasNoProblems()
        {
            Piece left = Place(PieceType.Rook, PieceColor.White, 0, 0);
            Piece right = Place(PieceType.Rook, PieceColor.White, 2, 0);
            PlanMove(left, 1, 0);
            PlanMove(right, 1, 0);

            Assert.IsEmpty(Validate());
        }

        [Test]
        public void TwoPiecesSwapping_HasNoProblems()
        {
            Piece low = Place(PieceType.Rook, PieceColor.White, 0, 0);
            Piece high = Place(PieceType.Rook, PieceColor.White, 0, 3);
            PlanMove(low, 0, 3);
            PlanMove(high, 0, 0);

            Assert.IsEmpty(Validate());
        }

        [Test]
        public void SeveralProblems_AreAllReported()
        {
            Piece knight = Place(PieceType.Knight, PieceColor.White, 1, 0);
            Piece otherKnight = Place(PieceType.Knight, PieceColor.White, 6, 0);
            Place(PieceType.Pawn, PieceColor.White, 3, 1);
            Place(PieceType.Pawn, PieceColor.White, 4, 1);
            PlanMove(knight, 3, 1);
            PlanMove(otherKnight, 4, 1);

            Assert.AreEqual(2, Validate().Count);
        }

        [Test]
        public void Message_NamesBothPieces()
        {
            Piece knight = Place(PieceType.Knight, PieceColor.White, 1, 0);
            Place(PieceType.Pawn, PieceColor.White, 3, 1);
            PlanMove(knight, 3, 1);

            Assert.AreEqual("Knight #1 can't move to d2: Pawn #2 is staying there.", Validate()[0].Message);
        }
    }
}
