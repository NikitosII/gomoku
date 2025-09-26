
using gomoku.Entities;

namespace gomoku.AI.Services
{
    public class PatternBasedEvaluator : IPositionEvaluator
    {
        private readonly Dictionary<string, int> _patternScores = new()
        {
            ["XXXXX"] = 100000, // Win
            ["_XXXX_"] = 10000,
            ["_XXXXO"] = 5000,
            ["OXXXX_"] = 5000,
            ["_XXX_"] = 1000,
            ["_XX_"] = 100,
            ["_X_"] = 10
        };

        public int Evaluate(GameBoard board, Player player)
        {
            var score = 0;
            var opponent = player == Player.Black ? Player.White : Player.Black;

            // Evaluate all lines (horizontal, vertical, diagonal)
            score += EvaluateLines(board, player, opponent);
            score -= EvaluateLines(board, opponent, player) * 2; // Opponent's threats are more dangerous

            return score;
        }

        private int EvaluateLines(GameBoard board, Player player, Player opponent)
        {
            var score = 0;
            var size = board.Size;

            // Horizontal, vertical, and diagonal evaluations
            for (int row = 0; row < size.Rows; row++)
            {
                for (int col = 0; col < size.Columns; col++)
                {
                    if (col <= size.Columns - 5)
                        score += EvaluateSequence(board, player, opponent, row, col, 0, 1); // Horizontal
                    if (row <= size.Rows - 5)
                        score += EvaluateSequence(board, player, opponent, row, col, 1, 0); // Vertical
                    if (row <= size.Rows - 5 && col <= size.Columns - 5)
                        score += EvaluateSequence(board, player, opponent, row, col, 1, 1); // Diagonal \
                    if (row <= size.Rows - 5 && col >= 4)
                        score += EvaluateSequence(board, player, opponent, row, col, 1, -1); // Diagonal /
                }
            }

            return score;
        }

        private int EvaluateSequence(GameBoard board, Player player, Player opponent, int startRow, int startCol, int deltaRow, int deltaCol)
        {
            var pattern = "";
            for (int i = 0; i < 5; i++)
            {
                var pos = new BoardPosition(startRow + i * deltaRow, startCol + i * deltaCol);
                if (!board.Size.IsValidPosition(pos))
                {
                    pattern += "O"; // Out of board treated as opponent stone
                    continue;
                }

                var cell = board[pos];
                pattern += cell == player ? 'X' : cell == opponent ? 'O' : '_';
            }

            return _patternScores
                .Where(p => pattern.Contains(p.Key.Replace("X", "X").Replace("O", "O")))
                .Sum(p => p.Value);
        }
    }
}
