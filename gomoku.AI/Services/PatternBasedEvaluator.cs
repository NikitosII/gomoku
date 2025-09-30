
using gomoku.AI.Interfaces;
using gomoku.Entities;

namespace gomoku.AI.Services
{
    public class PatternBasedEvaluator : IPositionEvaluator
    {
        private readonly Dictionary<string, int> _patternScores = new()
        {
            ["XXXXX"] = 1000000, // win
            ["_XXXX_"] = 50000,
            ["_XXXXO"] = 10000,
            ["OXXXX_"] = 10000,
            ["_XXX_"] = 5000,
            ["_XXXO"] = 1000,
            ["OXXX_"] = 1000,
            ["_XX_"] = 500,
            ["_XXO"] = 100,
            ["OXX_"] = 100,
            ["_X_"] = 10
        };

        public int Evaluate(GameBoard board, Player player)
        {
            var score = 0;
            var opponent = player == Player.Black ? Player.White : Player.Black;

            // Оценка направлений
            score += EvaluateLines(board, player, opponent);

            // Вычесть оценку противника
            score -= (int)(EvaluateLines(board, opponent, player) * 0.8);

            return score;
        }

        private int EvaluateLines(GameBoard board, Player player, Player opponent)
        {
            var score = 0;
            var size = board.Size;

            // Horiz, vert, and diag
            for (int row = 0; row < size.Rows; row++)
            {
                for (int col = 0; col < size.Columns; col++)
                {
                    var pos = new BoardPosition(row, col);
                    if (board[pos] != Player.None || HasAdjacentStones(board, pos))
                    {
                        score += EvaluatePosition(board, player, opponent, row, col);
                    }
                }
            }
            return score;
        }
        private int EvaluatePosition(GameBoard board, Player player, Player opponent, int row, int col)
        {
            var score = 0;
            var directions = new (int, int)[] { (0, 1), (1, 0), (1, 1), (1, -1) };

            foreach (var (dr, dc) in directions)
            {
                if (CanEvaluate(board, row, col, dr, dc, 5))
                {
                    score += EvaluateDirection(board, player, opponent, row, col, dr, dc);
                }
            }

            return score;
        }

        private bool HasAdjacentStones(GameBoard board, BoardPosition position)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    var checkPos = new BoardPosition(position.Row + dr, position.Column + dc);
                    if (board.Size.IsValidPosition(checkPos) && board[checkPos] != Player.None)
                        return true;
                }
            }
            return false;
        }

        private bool CanEvaluate(GameBoard board, int row, int col, int dr, int dc, int length)
        {
            var endRow = row + dr * (length - 1);
            var endCol = col + dc * (length - 1);

            return endRow >= 0 && endRow < board.Size.Rows && endCol >= 0 && endCol < board.Size.Columns;
        }

        private int EvaluateDirection(GameBoard board, Player player, Player opponent, int startRow, int startCol, int deltaRow, int deltaCol)
        {
            var pattern = "";

            // Собираем паттерн 
            for (int i = 0; i < 5; i++)
            {
                var row = startRow + i * deltaRow;
                var col = startCol + i * deltaCol;
                var pos = new BoardPosition(row, col);

                var cell = board[pos];
                if (cell == player)
                    pattern += 'X';
                else if (cell == opponent)
                    pattern += 'O';
                else
                    pattern += '_';
            }

            // Ищем совпадения 
            foreach (var (key, value) in _patternScores)
            {
                if (pattern.Contains(key.Replace("X", "X").Replace("O", "O")))
                {
                    return value;
                }
            }

            return 0;
        }
    }
}
