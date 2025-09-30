
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
        private readonly Dictionary<string, int> _Table;

        public MinimaxAI(IRules rules, IPositionEvaluator evaluator, int maxDepth = 2)
        {
            _rules = rules;
            _evaluator = evaluator;
            _maxDepth = maxDepth;
            _random = new Random();
            _Table = new Dictionary<string, int>();
        }

        public async Task<BoardPosition> FindBestMoveAsync(GameBoard board, Player aiPlayer, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => FindBestMove(board, aiPlayer), cancellationToken);
        }

        private BoardPosition FindBestMove(GameBoard board, Player aiPlayer)
        {
            _Table.Clear(); // Очищаем таблицу транспозиций для новой игры

            // Быстрая проверка выигрышных и блокирующих ходов
            var immediate = FindWinOrBlock(board, aiPlayer);
            if (immediate != null)
                return immediate;

            var bestScore = int.MinValue;
            var bestMoves = new List<BoardPosition>();
            var opponent = aiPlayer == Player.Black ? Player.White : Player.Black;

            var moves = GetPrioritizedMoves(board, aiPlayer, opponent);
            var movesToAnalyze = moves.Take(12).ToList();

            foreach (var move in movesToAnalyze)
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

        private BoardPosition? FindWinOrBlock(GameBoard board, Player aiPlayer)
        {
            var opponent = aiPlayer == Player.Black ? Player.White : Player.Black;

            // Проверяем выигрышный ход для AI
            foreach (var move in GetPossibleMoves(board))
            {
                board[move] = aiPlayer;
                if (_rules.CheckCondition(board, move))
                {
                    board[move] = Player.None;
                    return move;
                }
                board[move] = Player.None;
            }

            // Проверяем выигрышный ход для противника (блокируем)
            foreach (var move in GetPossibleMoves(board))
            {
                board[move] = opponent;
                if (_rules.CheckCondition(board, move))
                {
                    board[move] = Player.None;
                    return move;
                }
                board[move] = Player.None;
            }

            return null;
        }

        private int Minimax(GameBoard board, int depth, bool isMaximizing, Player aiPlayer, Player opponent, int alpha, int beta)
        {
            // Генерируем ключ для таблицы транспозиций
            var boardKey = GenerateBoardKey(board);
            if (_Table.TryGetValue(boardKey, out int cachedValue) && depth <= 2)
                return cachedValue;

            // Проверяем терминальное состояние
            if (board.GetLastMove(out BoardPosition lastMove))
            {
                var result = _rules.GetResult(board, lastMove);
                if (result != null)
                {
                    int terminalScore = result.Winner switch
                    {
                        var w when w == aiPlayer => 1000000 - depth,
                        var w when w == opponent => -1000000 + depth,
                        _ => 0
                    };
                    _Table[boardKey] = terminalScore;
                    return terminalScore;
                }
            }

            // Если достигнута максимальная глубина или доска заполнена
            if (depth <= 0 || board.IsFull)
            {
                var evaluation = _evaluator.Evaluate(board, aiPlayer);
                _Table[boardKey] = evaluation;
                return evaluation;
            }

            var possibleMoves = GetPrioritizedMoves(board, aiPlayer, opponent);

            // На больших глубинах анализируем меньше ходов
            var movesToAnalyze = depth > 1 ? possibleMoves.Take(8) : possibleMoves.Take(5);

            if (isMaximizing)
            {
                var maxScore = int.MinValue;
                foreach (var move in movesToAnalyze)
                {
                    board[move] = aiPlayer;
                    var score = Minimax(board, depth - 1, false, aiPlayer, opponent, alpha, beta);
                    board[move] = Player.None;

                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha) break;
                }
                _Table[boardKey] = maxScore;
                return maxScore;
            }
            else
            {
                var minScore = int.MaxValue;
                foreach (var move in movesToAnalyze)
                {
                    board[move] = opponent;
                    var score = Minimax(board, depth - 1, true, aiPlayer, opponent, alpha, beta);
                    board[move] = Player.None;

                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);
                    if (beta <= alpha) break;
                }
                _Table[boardKey] = minScore;
                return minScore;
            }
        }

        private List<BoardPosition> GetPrioritizedMoves(GameBoard board, Player aiPlayer, Player opponent)
        {
            var moves = new List<(BoardPosition pos, int score)>();
            var size = board.Size;

            // проверяем клетки рядом с существующими камнями
            for (int row = 0; row < size.Rows; row++)
            {
                for (int col = 0; col < size.Columns; col++)
                {
                    var pos = new BoardPosition(row, col);
                    if (board[pos] == Player.None && HasAdjacentStone(board, pos))
                    {
                        var score = EvaluateMove(board, pos, aiPlayer, opponent);
                        moves.Add((pos, score));
                    }
                }
            }
            return moves.OrderByDescending(x => x.score)
                       .Select(x => x.pos)
                       .ToList();
        }
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


        private int EvaluateMove(GameBoard board, BoardPosition move, Player aiPlayer, Player opponent)
        {
            int score = 0;

            // Бонус за центр на ранней стадии
            if (board.MoveCount < 6)
            {
                var center = new BoardPosition(board.Size.Rows / 2, board.Size.Columns / 2);
                var distance = Math.Abs(move.Row - center.Row) + Math.Abs(move.Column - center.Column);
                score += Math.Max(0, 10 - distance * 2);
            }

            // Проверяем потенциал в 4 направлениях
            var directions = new (int, int)[] { (0, 1), (1, 0), (1, 1), (1, -1) };

            foreach (var (dr, dc) in directions)
            {
                score += EvaluateDirection(board, move, aiPlayer, opponent, dr, dc);
            }

            return score;
        }
        private int EvaluateDirection(GameBoard board, BoardPosition move, Player aiPlayer, Player opponent, int dr, int dc)
        {
            int score = 0;
            int aiCount = 0, opponentCount = 0, emptyCount = 0;

            // Проверяем в обе стороны от потенциального хода
            for (int i = -4; i <= 4; i++)
            {
                if (i == 0) continue; // Пропускаем саму позицию хода

                var pos = new BoardPosition(move.Row + dr * i, move.Column + dc * i);
                if (!board.Size.IsValidPosition(pos)) continue;

                var cell = board[pos];
                if (cell == aiPlayer) aiCount++;
                else if (cell == opponent) opponentCount++;
                else if (cell == Player.None) emptyCount++;
            }

            // Оцениваем потенциал на основе соседей
            if (aiCount >= 2) score += aiCount * 10;
            if (opponentCount >= 2) score += opponentCount * 8; // Блокировка важна, но меньше чем атака

            return score;
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

        private string GenerateBoardKey(GameBoard board)
        {
            // Простой ключ на основе хэша позиций камней
            var key = new System.Text.StringBuilder();
            for (int row = 0; row < board.Size.Rows; row++)
            {
                for (int col = 0; col < board.Size.Columns; col++)
                {
                    var pos = new BoardPosition(row, col);
                    key.Append((int)board[pos]);
                }
            }
            return key.ToString();
        }
    }
}
