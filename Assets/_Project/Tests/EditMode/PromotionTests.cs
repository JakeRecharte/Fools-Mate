using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FoolsMate.Core.Tests
{
    public class PromotionTests
    {
        private const int MaxHealth = 3;

        private BoardState board;
        private int nextId;

        [SetUp]
        public void SetUp()
        {
            board = new BoardState();
            nextId = 1;
        }

        private Piece MakePiece(PieceType type)
        {
            return new Piece(nextId++, "test", type, new PieceStats(1, 1, 1, MaxHealth));
        }

        private Piece Place(PieceType type, PieceColor color, int file, int rank)
        {
            Piece piece = MakePiece(type);
            board.Place(piece, color, new Cell(file, rank));
            return piece;
        }

        /// <summary>Places a piece, then takes it off again so it's in its side's Box at 0 Health.</summary>
        private Piece PutInBox(PieceType type, PieceColor color)
        {
            Piece piece = Place(type, color, 0, 3);
            board.TakeDamage(piece, MaxHealth);
            board.Remove(piece);
            return piece;
        }

        [Test]
        public void LastRank_White_IsRank8()
        {
            Assert.AreEqual(7, Promotion.LastRank(PieceColor.White));
        }

        [Test]
        public void LastRank_Black_IsRank1()
        {
            Assert.AreEqual(0, Promotion.LastRank(PieceColor.Black));
        }

        [Test]
        public void GetWaitingPawns_FindsOnlyPawnsOnTheirLastRank()
        {
            Piece waiting = Place(PieceType.Pawn, PieceColor.White, 4, 7);
            Place(PieceType.Pawn, PieceColor.White, 3, 6);
            Place(PieceType.Rook, PieceColor.White, 0, 7);

            CollectionAssert.AreEqual(new[] { waiting }, Promotion.GetWaitingPawns(board, PieceColor.White));
        }

        [Test]
        public void GetWaitingPawns_Black_LooksAtRank1()
        {
            Piece waiting = Place(PieceType.Pawn, PieceColor.Black, 2, 0);
            Place(PieceType.Pawn, PieceColor.Black, 5, 7);

            CollectionAssert.AreEqual(new[] { waiting }, Promotion.GetWaitingPawns(board, PieceColor.Black));
        }

        [Test]
        public void GetWaitingPawns_IgnoresTheOtherSide()
        {
            Place(PieceType.Pawn, PieceColor.Black, 4, 7);

            Assert.IsEmpty(Promotion.GetWaitingPawns(board, PieceColor.White));
        }

        [Test]
        public void GetChoices_LeavesOutPawnsAndKingsAndTheOtherSide()
        {
            Piece rook = PutInBox(PieceType.Rook, PieceColor.White);
            PutInBox(PieceType.Pawn, PieceColor.White);
            PutInBox(PieceType.King, PieceColor.White);
            PutInBox(PieceType.Queen, PieceColor.Black);
            Piece knight = PutInBox(PieceType.Knight, PieceColor.White);

            CollectionAssert.AreEqual(new[] { rook, knight }, Promotion.GetChoices(board, PieceColor.White));
        }

        [Test]
        public void Promote_FromBox_PutsThePieceOnThePawnsCell()
        {
            Piece queen = PutInBox(PieceType.Queen, PieceColor.White);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 7);

            Promotion.Promote(board, pawn, queen);

            Assert.AreEqual(queen, board.GetPieceAt(new Cell(4, 7)));
            Assert.AreEqual(PieceColor.White, board.GetColor(queen));
            Assert.IsFalse(board.Pieces.Contains(pawn));
        }

        [Test]
        public void Promote_FromBox_RestoresFullHealth()
        {
            Piece queen = PutInBox(PieceType.Queen, PieceColor.White);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 7);

            Promotion.Promote(board, pawn, queen);

            Assert.AreEqual(MaxHealth, board.GetCurrentHealth(queen));
        }

        [Test]
        public void Promote_FromBox_PawnTakesThePiecesSlotInTheBox()
        {
            Piece rook = PutInBox(PieceType.Rook, PieceColor.White);
            Piece queen = PutInBox(PieceType.Queen, PieceColor.White);
            Piece knight = PutInBox(PieceType.Knight, PieceColor.White);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 7);

            Promotion.Promote(board, pawn, queen);

            CollectionAssert.AreEqual(new[] { rook, pawn, knight }, board.GetRemoved(PieceColor.White));
        }

        [Test]
        public void Promote_ReturnsAPromotedEvent()
        {
            Piece queen = PutInBox(PieceType.Queen, PieceColor.White);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 7);

            ResolutionEvent promoted = Promotion.Promote(board, pawn, queen);

            Assert.AreEqual(ResolutionEventKind.Promoted, promoted.Kind);
            Assert.AreEqual(queen, promoted.Piece);
            Assert.AreEqual(pawn, promoted.By);
            Assert.AreEqual(new Cell(4, 7), promoted.To);
            Assert.IsNull(promoted.ByColor);
        }

        [Test]
        public void Promote_NewPiece_LeavesTheBoxAlone()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.Black, 4, 0);
            Piece queen = MakePiece(PieceType.Queen);

            Promotion.Promote(board, pawn, queen);

            Assert.AreEqual(queen, board.GetPieceAt(new Cell(4, 0)));
            Assert.AreEqual(PieceColor.Black, board.GetColor(queen));
            Assert.IsEmpty(board.GetRemoved(PieceColor.Black));
        }

        [Test]
        public void Promote_PawnNotOnLastRank_Throws()
        {
            Piece queen = PutInBox(PieceType.Queen, PieceColor.White);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 6);

            Assert.Throws<InvalidOperationException>(() => Promotion.Promote(board, pawn, queen));
        }

        [Test]
        public void Promote_ToPawnOrKing_Throws()
        {
            Piece boxPawn = PutInBox(PieceType.Pawn, PieceColor.White);
            Piece boxKing = PutInBox(PieceType.King, PieceColor.White);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 7);

            Assert.Throws<ArgumentException>(() => Promotion.Promote(board, pawn, boxPawn));
            Assert.Throws<ArgumentException>(() => Promotion.Promote(board, pawn, boxKing));
        }

        [Test]
        public void Promote_WithTheOtherSidesBoxPiece_Throws()
        {
            Piece enemyQueen = PutInBox(PieceType.Queen, PieceColor.Black);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 7);

            Assert.Throws<InvalidOperationException>(() => Promotion.Promote(board, pawn, enemyQueen));
        }

        [Test]
        public void Promote_WithAPieceOnTheBoard_Throws()
        {
            Piece rook = Place(PieceType.Rook, PieceColor.White, 0, 0);
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 7);

            Assert.Throws<InvalidOperationException>(() => Promotion.Promote(board, pawn, rook));
        }

        [Test]
        public void PromoteAll_ReplacesEveryWaitingPawnWithANewPiece()
        {
            Place(PieceType.Pawn, PieceColor.Black, 1, 0);
            Place(PieceType.Pawn, PieceColor.Black, 6, 0);
            Piece notWaiting = Place(PieceType.Pawn, PieceColor.Black, 3, 4);

            List<ResolutionEvent> events = Promotion.PromoteAll(board, PieceColor.Black, new Random(1), MakePiece);

            Assert.AreEqual(2, events.Count);
            Assert.IsTrue(events.All(e => e.Kind == ResolutionEventKind.Promoted));
            Assert.IsTrue(events.All(e => Promotion.EnemyChoices.Contains(e.Piece.Type)));
            Assert.IsEmpty(Promotion.GetWaitingPawns(board, PieceColor.Black));
            Assert.IsTrue(board.Pieces.Contains(notWaiting));
        }

        [Test]
        public void PromoteAll_SameSeed_PicksTheSameTypes()
        {
            Place(PieceType.Pawn, PieceColor.Black, 1, 0);
            Place(PieceType.Pawn, PieceColor.Black, 6, 0);
            List<PieceType> first = Promotion.PromoteAll(board, PieceColor.Black, new Random(7), MakePiece)
                .Select(e => e.Piece.Type).ToList();

            SetUp();
            Place(PieceType.Pawn, PieceColor.Black, 1, 0);
            Place(PieceType.Pawn, PieceColor.Black, 6, 0);
            List<PieceType> second = Promotion.PromoteAll(board, PieceColor.Black, new Random(7), MakePiece)
                .Select(e => e.Piece.Type).ToList();

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void Forfeit_PawnDiesIntoTheBox()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.White, 4, 7);

            ResolutionEvent died = Promotion.Forfeit(board, pawn);

            Assert.AreEqual(ResolutionEventKind.Died, died.Kind);
            Assert.IsNull(died.ByColor);
            Assert.IsNull(board.GetPieceAt(new Cell(4, 7)));
            CollectionAssert.AreEqual(new[] { pawn }, board.GetRemoved(PieceColor.White));
        }

        [Test]
        public void Forfeit_IsNotAKill()
        {
            Piece pawn = Place(PieceType.Pawn, PieceColor.Black, 4, 0);
            var score = new RoundScore(PieceColor.White, 4, 5);

            score.ApplyTurn(new[] { Promotion.Forfeit(board, pawn) });

            Assert.AreEqual(0, score.Kills);
            Assert.AreEqual(0, score.Points);
        }
    }
}
