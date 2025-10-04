
using gomoku.AI.Interfaces;
using gomoku.Entities;
using gomoku.ValueObjects;

namespace gomoku.AI.Evaluation
{
    public class BoardEvaluator : IPositionEvaluator
    {
        private readonly IThreatDetector _threatDetector;
        public BoardEvaluator(IThreatDetector threatDetector)
        {
            _threatDetector = threatDetector;
        }

        public int EvaluateMove(GameBoard board, BoardPosition move, Player player, Player opponent)
        {
            int score = 0;

            score += EvaluateCentrality(move, board.Size);
            score += EvaluateMobility(board, move, player, opponent);

            // Оценка угроз
            score += _threatDetector.EvaluateThreatLevel(board, move, player, opponent);

            // Штраф за создание возможностей противнику
            score -= EvaluateThreats(board, move, player, opponent);

            return score;
        }

        public int EvaluatePosition(GameBoard board, Player player, Player opponent)
        {
            int score = 0;
            var size = board.Size;

            // Оцениваем всю доску, но фокусируемся на активных зонах
            for (int row = 0; row < size.Rows; row++)
            {
                for (int col = 0; col < size.Columns; col++)
                {
                    var pos = new BoardPosition(row, col);
                    if (board[pos] == player)
                    {
                        score += EvaluateValue(board, pos, player, opponent);
                    }
                    else if (board[pos] == opponent)
                    {
                        // Блокировка важнее
                        score -= (int)(EvaluateValue(board, pos, opponent, player) * 1.2); 
                    }
                }
            }

            return score;
        }

        public IEnumerable<BoardPosition> GetStratMoves(GameBoard board, Player player, Player opponent, int maxMoves)
        {
            var allMoves = GetPossibleMoves(board).ToList();
            var scoredMoves = new List<(BoardPosition move, int score)>();

            foreach (var move in allMoves)
            {
                var score = EvaluateMove(board, move, player, opponent);
                scoredMoves.Add((move, score));
            }

            return scoredMoves
                .OrderByDescending(x => x.score)
                .Take(maxMoves)
                .Select(x => x.move);
        }

        private int EvaluateValue(GameBoard board, BoardPosition stone, Player player, Player opponent)
        {
            int value = 0;
            var directions = new (int, int)[] { (0, 1), (1, 0), (1, 1), (1, -1) };

            foreach (var (dr, dc) in directions)
            {
                value += EvaluateDirection(board, stone, player, opponent, dr, dc);
            }

            return value;
        }

        private int EvaluateDirection(GameBoard board, BoardPosition stone, Player player, Player opponent, int dr, int dc)
        {
            int playerCount = 1; // Текущий камень
            int openEnds = 0;

            // Проверяем в одном направлении
            for (int i = 1; i <= 4; i++)
            {
                var pos = new BoardPosition(stone.Row + dr * i, stone.Column + dc * i);
                if (!board.Size.IsValidPosition(pos)) break;

                if (board[pos] == player) playerCount++;
                else if (board[pos] == Player.None) { openEnds++; break; }
                else break;
            }

            // Проверяем в противоположном направлении
            for (int i = 1; i <= 4; i++)
            {
                var pos = new BoardPosition(stone.Row - dr * i, stone.Column - dc * i);
                if (!board.Size.IsValidPosition(pos)) break;

                if (board[pos] == player) playerCount++;
                else if (board[pos] == Player.None) { openEnds++; break; }
                else break;
            }

            // Оценка на основе количества камней и открытых концов
            return (playerCount, openEnds) switch
            {
                (5, _) => 100000,  // Победа
                (4, 2) => 10000,   // Открытая четверка
                (4, 1) => 5000,    // Полуоткрытая четверка
                (3, 2) => 1000,    // Открытая тройка
                (3, 1) => 500,     // Полуоткрытая тройка
                (2, 2) => 100,     // Открытая двойка
                (2, 1) => 50,      // Полуоткрытая двойка
                (1, 2) => 10,      // Одиночка с двумя открытыми концами
                _ => 1
            };
        }

        private int EvaluateCentrality(BoardPosition move, BoardSize size)
        {
            var center = new BoardPosition(size.Rows / 2, size.Columns / 2);
            var distance = Math.Abs(move.Row - center.Row) + Math.Abs(move.Column - center.Column);
            return Math.Max(0, 20 - distance * 2);
        }

        private int EvaluateMobility(GameBoard board, BoardPosition move, Player player, Player opponent)
        {
            int mobility = 0;

            // Ходы, которые открывают новые возможности
            for (int dr = -2; dr <= 2; dr++)
            {
                for (int dc = -2; dc <= 2; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    var pos = new BoardPosition(move.Row + dr, move.Column + dc);
                    if (board.Size.IsValidPosition(pos) && board[pos] == Player.None)
                        mobility += 2;
                }
            }

            return mobility;
        }

        private int EvaluateThreats(GameBoard board, BoardPosition move, Player player, Player opponent)
        {
            int threatScore = 0;
            board[move] = player;

            // Проверяем, не создает ли ход возможности для противника
            foreach (var opponentMove in GetPossibleMoves(board).Take(5))
            {
                threatScore += _threatDetector.EvaluateThreatLevel(board, opponentMove, opponent, player) / 2;
            }

            board[move] = Player.None;
            return threatScore;
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
            return moves;
        }

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
