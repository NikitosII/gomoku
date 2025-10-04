
using gomoku.AI.Evaluation;
using gomoku.AI.Interfaces;
using gomoku.AI.ThreatDetection;
using gomoku.Entities;
using gomoku.Interfaces;

namespace gomoku.AI.Services
{
    // Основной класс AI для игры
    public class MinimaxAI : IGomokuAI
    {
        private readonly IRules _rules;
        private readonly IThreatDetector _threatDetector;
        private readonly IPositionEvaluator _boardEvaluator;
        private readonly int _maxDepth;
        private readonly Random _random;

        public MinimaxAI(IRules rules, int maxDepth = 2)
        {
            _rules = rules;
            _maxDepth = maxDepth;
            _random = new Random();

            // Инициализация компонентов
            _threatDetector = new ThreatDetector(rules);
            _boardEvaluator = new BoardEvaluator(_threatDetector);
        }

        // Асинхронно находит лучший ход для AI игрока.
        public async Task<BoardPosition> FindBestMoveAsync(GameBoard board, Player aiPlayer, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => FindBestMove(board, aiPlayer), cancellationToken);
        }

        // // Поиск лучшего хода.
        private BoardPosition FindBestMove(GameBoard board, Player aiPlayer)
        {

            var opponent = aiPlayer == Player.Black ? Player.White : Player.Black;

            // 1. Проверка немедленных выигрышных ходов
            var winningMoves = _threatDetector.FindCritMoves(board, aiPlayer, opponent).ToList();
            if (winningMoves.Count > 0)
                return winningMoves[_random.Next(winningMoves.Count)];

            // 2. Выбор стратегии в зависимости от уровня сложности
            return _maxDepth switch
            {
                1 => FindAggressiveMove(board, aiPlayer, opponent),    // Easy: быстрая агрессивная стратегия
                2 => FindBalancedMove(board, aiPlayer, opponent),      // Medium: сбалансированная стратегия
                _ => FindStrategicMove(board, aiPlayer, opponent)      // Hard: полная стратегия с поиском
            };
        }
        private BoardPosition FindAggressiveMove(GameBoard board, Player aiPlayer, Player opponent)
        {
            // Агрессивная стратегия: фокус на создание угроз
            var strategicMoves = _boardEvaluator.GetStratMoves(board, aiPlayer, opponent, 8).ToList();

            if (strategicMoves.Count == 0)
                return FindRandomMove(board);

            // Выбираем самый агрессивный ход
            return strategicMoves.OrderByDescending(move =>
                _threatDetector.EvaluateThreatLevel(board, move, aiPlayer, opponent)
            ).First();
        }
        private BoardPosition FindBalancedMove(GameBoard board, Player aiPlayer, Player opponent)
        {
            // Сбалансированная стратегия: атака + защита
            var strategicMoves = _boardEvaluator.GetStratMoves(board, aiPlayer, opponent, 6).ToList();

            if (strategicMoves.Count == 0)
                return FindRandomMove(board);

            var bestMove = strategicMoves[0];
            var bestScore = int.MinValue;

            foreach (var move in strategicMoves)
            {
                var score = _boardEvaluator.EvaluateMove(board, move, aiPlayer, opponent);

                // Добавляем небольшой поиск вперед
                board[move] = aiPlayer;
                var opponentThreat = EvaluateOpponentBestResponse(board, aiPlayer, opponent, 1);
                board[move] = Player.None;

                var totalScore = score - opponentThreat;

                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestMove = move;
                }
            }

            return bestMove;
        }
        private BoardPosition FindStrategicMove(GameBoard board, Player aiPlayer, Player opponent)
        {
            // Полная стратегия с минимаксом
            var strategicMoves = _boardEvaluator.GetStratMoves(board, aiPlayer, opponent, 10).ToList();

            if (strategicMoves.Count == 0)
                return FindRandomMove(board);

            var bestScore = int.MinValue;
            var bestMoves = new List<BoardPosition>();

            foreach (var move in strategicMoves)
            {
                board[move] = aiPlayer;
                var score = Minimax(board, _maxDepth - 1, false, aiPlayer, opponent, int.MinValue, int.MaxValue);
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

            return bestMoves.Count > 0 ? bestMoves[_random.Next(bestMoves.Count)] : strategicMoves[0];
        }

        // Алгоритм минимакс с альфа-бета отсечением для поиска оптимального хода
        private int Minimax(GameBoard board, int depth, bool isMaximizing, Player aiPlayer, Player opponent, int alpha, int beta)
        {
            // Проверка терминального состояния
            if (board.GetLastMove(out BoardPosition lastMove))
            {
                var result = _rules.GetResult(board, lastMove);
                if (result != null)
                {
                    return result.Winner == aiPlayer ? 100000 - depth :
                           result.Winner == opponent ? -100000 + depth : 0;
                }
            }

            if (depth <= 0 || board.IsFull)
                return _boardEvaluator.EvaluatePosition(board, aiPlayer, opponent);

            var moves = _boardEvaluator.GetStratMoves(board,
                isMaximizing ? aiPlayer : opponent,
                isMaximizing ? opponent : aiPlayer, 8).ToList();

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

        // Оценивает лучший ответ противника на данный ход
        private int EvaluateOpponentBestResponse(GameBoard board, Player aiPlayer, Player opponent, int depth)
        {
            if (depth <= 0) return 0;

            var opponentMoves = _boardEvaluator.GetStratMoves(board, opponent, aiPlayer, 4).ToList();
            if (opponentMoves.Count == 0) return 0;

            var bestResponse = opponentMoves.Max(move =>
                _threatDetector.EvaluateThreatLevel(board, move, opponent, aiPlayer)
            );

            return bestResponse;
        }

        // Случайный ход из возможных
        private BoardPosition FindRandomMove(GameBoard board)
        {
            var moves = GetPossibleMoves(board).ToList();
            return moves.Count > 0 ? moves[_random.Next(moves.Count)] : new BoardPosition(7, 7);
        }

        // Получение возможных ходов
        private IEnumerable<BoardPosition> GetPossibleMoves(GameBoard board)
        {
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

            if (moves.Count == 0)
            {
                var center = new BoardPosition(size.Rows / 2, size.Columns / 2);
                if (_rules.IsMoveValid(board, center))
                    moves.Add(center);
            }

            return moves;
        }

        // Проверка наличия камней в соседних клетках
        private bool HasAdjacentStone(GameBoard board, BoardPosition position)
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
    }

}

