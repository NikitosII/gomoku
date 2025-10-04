
using gomoku.AI.Services;
using gomoku.Services;
using gomoku.UI.Services;
using gomoku.ValueObjects;

namespace gomoku.UI.Forms
{
    // Главная форма приложения
    /// Обеспечивает взаимодействие пользователя с игрой через графический интерфейс.
    public partial class MainForm : Form
    {
        private const int BoardSize = 15;
        private const int CellSize = 40;

        private readonly GameController _gameController;
        private readonly BoardRenderer _renderer;

        public MainForm()
        {
            InitializeComponent();

            var boardSize = new BoardSize(BoardSize, BoardSize);
            var rules = new GomokuRules(boardSize);
            // var evaluator = new PatternBasedEvaluator();
            var ai = new MinimaxAI(rules, 3);

            _gameController = new GameController(rules, ai);
            _renderer = new BoardRenderer(boardSize, CellSize);

            this.Load += MainForm_Load;
        }
        private void MainForm_Load(object? sender, EventArgs e)
        {
            _gameController.GameStateChanged += OnGameStateChanged;
            StartNewGame();
        }

        // Начинает новую игру с выбранным уровнем сложности
        private void StartNewGame()
        {
            var (depth, limit) = difficultyComboBox.SelectedItem?.ToString() switch
            {
                "Easy" => (1, 8),
                "Medium" => (2, 10),
                "Hard" => (3, 12),
                _ => (2, 10)
            };

            var ai = new MinimaxAI(_gameController.Rules, depth);
            _gameController.SetAI(ai);
            _gameController.StartNewGame();
            statusLabel.Text = "Your turn Black";
            boardPictureBox.Invalidate();

        }

        // Обработчик изменения состояния игры
        private void OnGameStateChanged(object sender, GameStateChangedEventArgs e)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => UpdateUI(e)));
                }
                else
                {
                    UpdateUI(e);
                }
            }
        }

        // Обновляет элементы интерфейса
        private void UpdateUI(GameStateChangedEventArgs e)
        {
            // Дополнительная проверка на случай, если форма закрывается
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            UpdateStatusLabel(e.StatusMessage);
            boardPictureBox.Invalidate();

            if (e.IsGameOver)
            {
                MessageBox.Show(e.StatusMessage, "Game Over",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void UpdateStatusLabel(string message)
        {
            if (statusLabel != null && !statusLabel.IsDisposed)
            {
                statusLabel.Text = message;
            }
        }

        // Отрисовка игровой доски и камней
        private void BoardPictureBox_Paint(object sender, PaintEventArgs e)
        {
            _renderer.DrawBoard(e.Graphics, _gameController.Board, _gameController.WinningLine);
        }

        // Обработчик клика мыши по доске
        private async void BoardPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (_gameController.IsGameOver) return;

            var position = _renderer.GetBoardPositionFromPixel(e.Location);
            if (position != null)
            {
                await _gameController.HumanMoveAsync(position);
            }
        }

        // Обработчик кнопки начала новой игры
        private void NewGameButton_Click(object sender, EventArgs e) => StartNewGame();
    }
}
