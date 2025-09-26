using gomoku.Entities;
using gomoku.ValueObjects;

namespace gomoku.UI.Services
{
    public class BoardRenderer : IDisposable
    {
        private readonly BoardSize _boardSize;
        private readonly int _cellSize;
        private readonly int _stoneRadius;
        private readonly Pen _gridPen;
        private readonly Brush[] _stoneBrushes;
        private bool _disposed = false;

        public BoardRenderer(BoardSize boardSize, int cellSize)
        {
            _boardSize = boardSize;
            _cellSize = cellSize;
            _stoneRadius = (int)(cellSize * 0.4);
            _gridPen = new Pen(Color.Black, 1.5f);

            _stoneBrushes = new Brush[]
            {
            Brushes.Transparent, // None
            Brushes.Black,       // Black
            Brushes.White        // White
            };
        }

        public void DrawBoard(Graphics g, GameBoard board, IReadOnlyList<BoardPosition> winningLine)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawGrid(g);
            DrawStones(g, board);

            if (winningLine != null && winningLine.Any())
            {
                DrawWinningLine(g, winningLine);
            }
        }

        private void DrawGrid(Graphics g)
        {
            var boardWidth = (_boardSize.Columns - 1) * _cellSize;
            var boardHeight = (_boardSize.Rows - 1) * _cellSize;

            // Draw horizontal lines
            for (int row = 0; row < _boardSize.Rows; row++)
            {
                var y = row * _cellSize;
                g.DrawLine(_gridPen, 0, y, boardWidth, y);
            }

            // Draw vertical lines
            for (int col = 0; col < _boardSize.Columns; col++)
            {
                var x = col * _cellSize;
                g.DrawLine(_gridPen, x, 0, x, boardHeight);
            }
        }

        private void DrawStones(Graphics g, GameBoard board)
        {
            for (int row = 0; row < _boardSize.Rows; row++)
            {
                for (int col = 0; col < _boardSize.Columns; col++)
                {
                    var player = board[new BoardPosition(row, col)];
                    if (player != Player.None)
                    {
                        DrawStone(g, new BoardPosition(row, col), player);
                    }
                }
            }
        }

        private void DrawStone(Graphics g, BoardPosition position, Player player)
        {
            var center = GetStoneCenter(position);
            var brush = _stoneBrushes[(int)player];

            g.FillEllipse(brush,
                center.X - _stoneRadius,
                center.Y - _stoneRadius,
                _stoneRadius * 2,
                _stoneRadius * 2);

            g.DrawEllipse(Pens.Gray,
                center.X - _stoneRadius,
                center.Y - _stoneRadius,
                _stoneRadius * 2,
                _stoneRadius * 2);
        }

        private void DrawWinningLine(Graphics g, IReadOnlyList<BoardPosition> winningLine)
        {
            if (winningLine.Count < 2) return;

            var points = winningLine.Select(GetStoneCenter).ToArray();
            using var pen = new Pen(Color.Red, 3f);
            g.DrawLines(pen, points);
        }

        private Point GetStoneCenter(BoardPosition position) =>
            new Point(position.Column * _cellSize, position.Row * _cellSize);

        public BoardPosition? GetBoardPositionFromPixel(Point pixel)
        {
            var col = (int)Math.Round((double)pixel.X / _cellSize);
            var row = (int)Math.Round((double)pixel.Y / _cellSize);

            var position = new BoardPosition(row, col);
            return _boardSize.IsValidPosition(position) ? position : null;
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _gridPen?.Dispose();
                _disposed = true;
            }
        }
    }
}
