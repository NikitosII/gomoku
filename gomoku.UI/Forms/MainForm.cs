
using gomoku.AI.Services;
using gomoku.Services;
using gomoku.UI.Services;
using gomoku.ValueObjects;
using System.Windows;
using System.Windows.Input;

namespace gomoku.UI.Forms
{
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
            var evaluator = new PatternBasedEvaluator();
            var ai = new MinimaxAI(rules, evaluator);

            _gameController = new GameController(rules, ai);
            _renderer = new BoardRenderer(boardSize, CellSize);

            // _gameController.GameStateChanged += OnGameStateChanged;
            StartNewGame();
        }

        private void StartNewGame()
        {
            var difficulty = difficultyComboBox.SelectedItem?.ToString() switch
            {
                "Easy" => 2,
                "Medium" => 3,
                "Hard" => 4,
                _ => 3
            };

            _gameController.StartNewGame(difficulty);
            statusLabel.Text = "Your turn (Black)";
            boardPictureBox.Invalidate();
        }

/*        private void OnGameStateChanged(object sender, GameStateChangedEventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                statusLabel.Text = e.StatusMessage;
                boardPictureBox.Invalidate();

                if (e.IsGameOver)
                {
                    MessageBox.Show(e.StatusMessage, "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } }));
        }*/

        private void BoardPictureBox_Paint(object sender, PaintEventArgs e)
        {
            _renderer.DrawBoard(e.Graphics, _gameController.Board, _gameController.WinningLine);
        }

        /*private async void BoardPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (_gameController.IsGameOver) return;

            var position = _renderer.GetBoardPositionFromPixel(e.Location);
            if (position != null)
            {
                await _gameController.HumanMoveAsync(position.Value);
            }
        }*/

        private void NewGameButton_Click(object sender, EventArgs e) => StartNewGame();
    }
}
