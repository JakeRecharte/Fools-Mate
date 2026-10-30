using System;
using System.Collections.Generic;
using System.Linq;
using FoolsMate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FoolsMate.Game
{
    /// <summary>
    /// Turns mouse clicks into a turn plan. Left-click one of your pieces to see where it can go, then
    /// left-click a marked cell to plan that move. Click the selected piece again (or right-click any of your
    /// pieces) to cancel its planned move.
    /// </summary>
    public class PlanningInput : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;

        [Tooltip("Optional. Shows energy used this turn, e.g. \"Energy 1/2\".")]
        [SerializeField] private TMP_Text energyText;

        private BoardState board;
        private PieceColor playerColor;
        private TurnPlan enemyPlan;
        private Piece selected;
        private List<Cell> selectedTargets = new List<Cell>();

        /// <summary>This turn's plan. Read by <see cref="TurnController"/> when MOVE is pressed.</summary>
        public TurnPlan Plan { get; private set; }

        /// <summary>Raised whenever a move is planned or cancelled.</summary>
        public event Action PlanChanged;

        /// <summary>
        /// Called by <see cref="TurnController"/> at the start of a turn, after the enemy has telegraphed
        /// <paramref name="enemyPlan"/>.
        /// </summary>
        public void Begin(BoardState board, PieceColor playerColor, int energy, TurnPlan enemyPlan)
        {
            this.board = board;
            this.playerColor = playerColor;
            this.enemyPlan = enemyPlan;
            Plan = new TurnPlan(energy);
            Deselect();
            RefreshPlan();
        }

        /// <summary>Called by <see cref="TurnController"/> when the round ends. Clicks do nothing until the next Begin.</summary>
        public void End()
        {
            Deselect();
            boardView.ClearPlan();
            board = null;
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (board == null || mouse == null)
            {
                return;
            }

            // Clicks on UI (like the MOVE button) shouldn't also land on the board underneath.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (TryGetClickedCell(mouse, out Cell cell))
                {
                    OnLeftClick(cell);
                }
                else
                {
                    Deselect();
                }
            }
            else if (mouse.rightButton.wasPressedThisFrame && TryGetClickedCell(mouse, out Cell cell))
            {
                OnRightClick(cell);
            }
        }

        private bool TryGetClickedCell(Mouse mouse, out Cell cell)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            world.z = 0f;
            return boardView.TryWorldToCell(world, out cell);
        }

        private void OnLeftClick(Cell cell)
        {
            // A marked cell plans the move, even if one of your own pieces is standing there.
            if (selected != null && selectedTargets.Contains(cell))
            {
                PlanMove(cell);
                return;
            }

            Piece piece = board.GetPieceAt(cell);

            // Clicking the selected piece again means it stays put this turn.
            if (piece != null && piece == selected)
            {
                bool cancelled = Plan.Cancel(piece);
                Deselect();

                if (cancelled)
                {
                    SfxPlayer.Play(Sfx.Cancel);
                    RefreshPlan();
                }

                return;
            }

            if (IsOwnPiece(piece))
            {
                Select(piece, cell);
            }
            else
            {
                Deselect();
            }
        }

        private void OnRightClick(Cell cell)
        {
            Piece piece = board.GetPieceAt(cell);
            if (IsOwnPiece(piece) && Plan.Cancel(piece))
            {
                SfxPlayer.Play(Sfx.Cancel);
                Deselect();
                RefreshPlan();
            }
        }

        private void PlanMove(Cell to)
        {
            var move = new PlannedMove(selected, board.GetCell(selected), to);

            if (!Plan.TryPlan(move))
            {
                Debug.Log($"No energy left to plan {move}. Cancel a move with right-click first.", this);
                SfxPlayer.Play(Sfx.Error);
                return;
            }

            SfxPlayer.Play(Sfx.Plan);
            Deselect();
            RefreshPlan();
        }

        private void Select(Piece piece, Cell cell)
        {
            selected = piece;
            SfxPlayer.Play(Sfx.Select);

            // The plan is passed in so sliding pieces can see past planned movers but stop where they land.
            // The enemy plan rules out cells its pieces are leaving.
            selectedTargets = MoveTargets.GetTargets(board, piece, Plan, enemyPlan);
            boardView.ShowTargets(cell, selectedTargets);
        }

        private void Deselect()
        {
            selected = null;
            selectedTargets.Clear();
            boardView.ClearTargets();
        }

        private void RefreshPlan()
        {
            // Checked live so a ghost turns red the moment its plan stops being valid.
            var problemPieces = new HashSet<Piece>(MoveCheck.Validate(board, Plan, enemyPlan).Select(problem => problem.Piece));
            boardView.ShowPlan(board, Plan, problemPieces);

            if (energyText != null)
            {
                energyText.text = $"Energy {Plan.EnergyUsed}/{Plan.Energy}";
            }

            PlanChanged?.Invoke();
        }

        private bool IsOwnPiece(Piece piece)
        {
            return piece != null && board.GetColor(piece) == playerColor;
        }
    }
}
