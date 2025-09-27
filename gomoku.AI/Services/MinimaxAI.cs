
using gomoku.AI.Interfaces;
using gomoku.Entities;
using gomoku.Interfaces;
using gomoku.ValueObjects;

namespace gomoku.AI.Services
{
    public class MinimaxAI : IGomokuAI
    {
        private readonly IRules _rules;
        private readonly IPositionEvaluator _evaluator;
        private readonly int _maxDepth;
        private readonly Random _random;

        public MinimaxAI(IRules rules, IPositionEvaluator evaluator, int maxDepth = 2)
        {
            _rules = rules;
            _evaluator = evaluator;
            _maxDepth = maxDepth;
            _random = new Random();
        }

        public async Task<BoardPosition> FindBestMoveAsync(GameBoard board, Player aiPlayer, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => FindBestMove(board, aiPlayer), cancellationToken);
        }

        private BoardPosition FindBestMove(GameBoard board, Player aiPlayer)
        {
            var bestScore = int.MinValue;
            var bestMoves = new List<BoardPosition>();
            var opponent = aiPlayer == Player.Black ? Player.White : Player.Black;

            var moves = GetPossibleMoves(board);

            if (board.MoveCount <= 1)
            {
                var center = new BoardPosition(board.Size.Rows / 2, board.Size.Columns / 2);
                if (_rules.IsMoveValid(board, center))
                    return center;

                // Или рядом с центром, если центр занят
                var neighbors = GetNeighborPos(center, board.Size);
                return neighbors.FirstOrDefault(pos => _rules.IsMoveValid(board, pos)) ?? moves.First();
            }

            foreach (var move in moves)
            {
                // Пробуем ход
                board[move] = aiPlayer;

                var score = Minimax(board, _maxDepth - 1, false, aiPlayer, opponent, int.MinValue, int.MaxValue);

                // Откатываем ход
                board[move] = Player.None;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add(move);
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(move);
                }
            }

            // Выбираем случайный ход из лучших
            return bestMoves.Count > 0 ? bestMoves[_random.Next(bestMoves.Count)] : moves.First();
        }

        private int Minimax(GameBoard board, int depth, bool isMaximizing, Player aiPlayer, Player opponent, int alpha, int beta)
        {
            // Проверяем терминальное состояние
            if(board.GetLastMove(out BoardPosition lastMove))
            {
                var result = _rules.GetResult(board, lastMove);
                if (result != null)
                {
                    if (result.Winner == aiPlayer) return 1000000 - depth; // Победа AI
                    if (result.Winner == opponent) return -1000000 + depth; // Победа противника
                    if (result.Winner == Player.None) return 0; // Ничья
                }
            }

            // Если достигнута максимальная глубина
            if (depth <= 0) return _evaluator.Evaluate(board, aiPlayer);

            var possibleMoves = GetPossibleMoves(board);

            if (isMaximizing)
            {
                var maxScore = int.MinValue;
                foreach (var move in possibleMoves)
                {
                    board[move] = aiPlayer;
                    var score = Minimax(board, depth - 1, false, aiPlayer, opponent, alpha, beta);
                    board[move] = Player.None;

                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha) break; // Альфа-бета отсечение
                }
                return maxScore;
            }
            else
            {
                var minScore = int.MaxValue;
                foreach (var move in possibleMoves)
                {
                    board[move] = opponent;
                    var score = Minimax(board, depth - 1, true, aiPlayer, opponent, alpha, beta);
                    board[move] = Player.None;

                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);
                    if (beta <= alpha) break; // Альфа-бета отсечение
                }
                return minScore;
            }
        }

        private IEnumerable<BoardPosition> GetPossibleMoves(GameBoard board)
        {
            var moves = new List<BoardPosition>();
            var size = board.Size;

            // проверяем клетки рядом с существующими камнями
            for (int row = 0; row < size.Rows; row++)
            {
                for (int col = 0; col < size.Columns; col++)
                {
                    var pos = new BoardPosition(row, col);
                    if (board[pos] == Player.None && HasAdjacentStone(board, pos))
                    {
                        moves.Add(pos);
                    }
                }
            }
            if (moves.Count == 0)
            {
                var center = new BoardPosition(size.Rows / 2, size.Columns / 2);
                if (_rules.IsMoveValid(board, center))
                    moves.Add(center);
            }

            return moves;
        }

        private bool HasAdjacentStone(GameBoard board, BoardPosition position)
        {
            // Проверяем соседние клетки в радиусе 2
            for (int dr = -2; dr <= 2; dr++)
            {
                for (int dc = -2; dc <= 2; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    var checkPos = new BoardPosition(position.Row + dr, position.Column + dc);
                    if (board.Size.IsValidPosition(checkPos) && board[checkPos] != Player.None)
                        return true;
                }
            }
            return false;
        }

        private BoardPosition? GetLastMove(GameBoard board)
        {
            // В идеале отслеживать последний ход
            for (int row = board.Size.Rows - 1; row >= 0; row--)
            {
                for (int col = board.Size.Columns - 1; col >= 0; col--)
                {
                    var pos = new BoardPosition(row, col);
                    if (board[pos] != Player.None)
                        return pos;
                }
            }
            return null;
        }
        private List<BoardPosition> GetNeighborPos(BoardPosition center, BoardSize size)
        {
            var neighbors = new List<BoardPosition>();
            var directions = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1) };

            foreach (var (dr, dc) in directions)
            {
                var neighbor = new BoardPosition(center.Row + dr, center.Column + dc);
                if (size.IsValidPosition(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }
    }
}
