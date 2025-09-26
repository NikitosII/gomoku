
using gomoku.Entities;
using gomoku.Interfaces;

namespace gomoku.AI.Services
{
    public class MinimaxAI : IGomokuAI
    {
        private readonly IRules _rules;
        private readonly IPositionEvaluator _evaluator;
        private readonly int _maxDepth;

        public MinimaxAI(IRules rules, IPositionEvaluator evaluator, int maxDepth = 3)
        {
            _rules = rules;
            _evaluator = evaluator;
            _maxDepth = maxDepth;
        }

        public async Task<BoardPosition> FindBestMoveAsync(GameBoard board, Player aiPlayer, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => FindBestMove(board, aiPlayer), cancellationToken);
        }

        private BoardPosition FindBestMove(GameBoard board, Player aiPlayer)
        {
            var bestScore = int.MinValue;
            var bestMove = BoardPosition.Zero;
            var opponent = aiPlayer == Player.Black ? Player.White : Player.Black;

            var moves = GetPossibleMoves(board, aiPlayer);

            foreach (var move in moves)
            {
                board[move] = aiPlayer;
                var score = Minimax(board, _maxDepth - 1, false, aiPlayer, opponent, int.MinValue, int.MaxValue);
                board[move] = Player.None;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private int Minimax(GameBoard board, int depth, bool isMaximizing, Player aiPlayer, Player opponent, int alpha, int beta)
        {
            // Check terminal state
            var result = _rules.GetResult(board, GetLastMove(board));
            if (result?.Winner == aiPlayer) return int.MaxValue - 1;
            if (result?.Winner == opponent) return int.MinValue + 1;
            if (result?.Winner == Player.None || depth == 0) return _evaluator.Evaluate(board, aiPlayer);

            var moves = GetPossibleMoves(board, isMaximizing ? aiPlayer : opponent);

            if (isMaximizing)
            {
                var maxScore = int.MinValue;
                foreach (var move in moves)
                {
                    board[move] = aiPlayer;
                    var score = Minimax(board, depth - 1, false, aiPlayer, opponent, alpha, beta);
                    board[move] = Player.None;

                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha) break;
                }
                return maxScore;
            }
            else
            {
                var minScore = int.MaxValue;
                foreach (var move in moves)
                {
                    board[move] = opponent;
                    var score = Minimax(board, depth - 1, true, aiPlayer, opponent, alpha, beta);
                    board[move] = Player.None;

                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);
                    if (beta <= alpha) break;
                }
                return minScore;
            }
        }

        private IEnumerable<BoardPosition> GetPossibleMoves(GameBoard board, Player player)
        {
            // Get moves around existing stones for efficiency
            var moves = new List<BoardPosition>();
            var size = board.Size;

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

            return moves.OrderBy(_ => Random.Shared.Next()); // Shuffle for variety
        }

        private bool HasAdjacentStone(GameBoard board, BoardPosition position)
        {
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

        private BoardPosition GetLastMove(GameBoard board)
        {
            // Simplified - in real implementation you'd track this
            for (int row = 0; row < board.Size.Rows; row++)
                for (int col = 0; col < board.Size.Columns; col++)
                    if (board[new BoardPosition(row, col)] != Player.None)
                        return new BoardPosition(row, col);
            return BoardPosition.Zero;
        }
    }
}
