
using gomoku.AI.Interfaces;
using gomoku.Entities;

namespace gomoku.AI.Services
{
    public class PatternBasedEvaluator : IPositionEvaluator
    {
        private readonly Dictionary<string, int> _patternScores = new()
        {
            ["XXXXX"] = 1000000,  // win
            ["_XXXX_"] = 100000,
            ["_XXXXO"] = 10000,
            ["OXXXX_"] = 10000,
            ["_XXX_"] = 1000,
            ["_XXXO"] = 500,
            ["OXXX_"] = 500,
            ["_XX_"] = 100,
            ["_XXO"] = 50,
            ["OXX_"] = 50,
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
            var lines = new (int, int)[] { (0, 1), (1, 0), (1, 1), (1, -1) };

            // Horiz, vert, and diag
            for (int x = 0; x < size.Rows; x++)
            {
                for(int y = 0; y < size.Columns; y++)
                {
                    foreach(var (z, h) in lines)
                    {
                        if(CanEvaluate(board, x, y, z, h, 5))
                        {
                            score += EvaluateDirection(board, player, opponent, x, y, z, h);
                        }
                    }
                }
            }
            

            return score;
        }
        private bool CanEvaluate(GameBoard board, int row, int col, int dr, int dc, int length)
        {
            var endRow = row + dr * (length - 1);
            var endCol = col + dc * (length - 1);

            return endRow >= 0 && endRow < board.Size.Rows &&
                   endCol >= 0 && endCol < board.Size.Columns;
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
