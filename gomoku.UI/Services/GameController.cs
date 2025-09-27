
using gomoku.AI.Services;
using gomoku.Entities;
using gomoku.Interfaces;

namespace gomoku.UI.Services
{
    public class GameController
    {
        public event EventHandler<GameStateChangedEventArgs>? GameStateChanged;

        public GameBoard Board { get; private set; }
        public bool IsGameOver { get; private set; }
        public IReadOnlyList<BoardPosition> WinningLine { get; private set; } = Array.Empty<BoardPosition>();

        private readonly IRules _rules;
        private readonly IGomokuAI _ai;
        private Player _currentPlayer;
        private int _aiDepth;

        public GameController(IRules rules, IGomokuAI ai)
        {
            _rules = rules;
            _ai = ai;
        }

        public void StartNewGame(int aiDepth = 3)
        {
            _aiDepth = aiDepth;
            Board = new GameBoard(_rules.GetBoardSize());
            _currentPlayer = Player.Black; // Human starts
            IsGameOver = false;
            WinningLine = Array.Empty<BoardPosition>();

            OnGameStateChanged("Your turn (Black)");
        }

        public async Task HumanMoveAsync(BoardPosition position)
        {
            if (IsGameOver || _currentPlayer != Player.Black) return;

            if (!_rules.IsMoveValid(Board, position))
            {
                OnGameStateChanged("Invalid move!");
                return;
            }

            MakeMove(position, Player.Black);

            if (CheckGameResult(position)) return;

            _currentPlayer = Player.White;
            OnGameStateChanged("AI is thinking...");

            await AIMoveAsync();
        }

        private async Task AIMoveAsync()
        {
            try
            {
                var aiMove = await _ai.FindBestMoveAsync(Board.Clone(), Player.White);
                MakeMove(aiMove, Player.White);
                CheckGameResult(aiMove);
            }
            catch (Exception ex)
            {
                OnGameStateChanged($"AI error: {ex.Message}");
            }
        }

        private void MakeMove(BoardPosition position, Player player)
        {
            Board[position] = player;
        }

        private bool CheckGameResult(BoardPosition lastMove)
        {
            var result = _rules.GetResult(Board, lastMove);
            if (result != null)
            {
                IsGameOver = true;
                WinningLine = result.WinningLine;

                var message = result.Winner switch
                {
                    Player.Black => "You win!",
                    Player.White => "AI wins!",
                    _ => "It's a draw!"
                };

                OnGameStateChanged(message, true);
                return true;
            }

            return false;
        }

        private void OnGameStateChanged(string message, bool isGameOver = false)
        {
            GameStateChanged?.Invoke(this, new GameStateChangedEventArgs(message, isGameOver));
        }
    }

    public class GameStateChangedEventArgs : EventArgs
    {
        public string StatusMessage { get; }
        public bool IsGameOver { get; }

        public GameStateChangedEventArgs(string statusMessage, bool isGameOver = false)
        {
            StatusMessage = statusMessage;
            IsGameOver = isGameOver;
        }
    }
}
