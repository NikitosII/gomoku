
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

        // Во сколько раз чужая угроза важнее своей при равном уровне.
        private const double DefenseWeight = 1.0;

        public int EvaluateMove(GameBoard board, BoardPosition move, Player player, Player opponent)
        {
            // Ход оценивается с двух сторон:
            //   атака  - какую угрозу он создаёт нам;
            //   защита - какую угрозу противника он у него отбирает.
            int attack = _threatDetector.EvaluateThreatLevel(board, move, player, opponent);
            int defense = _threatDetector.EvaluateThreatLevel(board, move, opponent, player);

            int score = attack + (int)(defense * DefenseWeight);
            score += EvaluateCentrality(move, board.Size);
            score += EvaluateMobility(board, move, player, opponent);
            return score;
        }

        // Вес чужих структур в оценке позиции.
        private const double OpponentWeight = 1.05;

        public int EvaluatePosition(GameBoard board, Player player, Player opponent)
        {
            long score = 0;
            var size = board.Size;

            // Оцениваем всю доску, но фокусируемся на активных зонах
            for (int row = 0; row < size.Rows; row++)
            {
                for (int col = 0; col < size.Columns; col++)
                {
                    var pos = new BoardPosition(row, col);
                    if (board[pos] == player)
                    {
                        score += EvaluateValue(board, pos, player);
                    }
                    else if (board[pos] == opponent)
                    {
                        // Блокировка важнее
                        score -= (long)(EvaluateValue(board, pos, opponent) * OpponentWeight);
                    }
                }
            }

            // Позиционная оценка не имеет права дорасти до Scores.Win
            return (int)Math.Clamp(score, -Scores.MaxHeuristic, Scores.MaxHeuristic);
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

        private int EvaluateValue(GameBoard board, BoardPosition stone, Player player)
        {
            int value = 0;
            var directions = new (int, int)[] { (0, 1), (1, 0), (1, 1), (1, -1) };

            foreach (var (dr, dc) in directions)
            {
                value += EvaluateDirection(board, stone, player, dr, dc);
            }

            return value;
        }

        private int EvaluateDirection(GameBoard board, BoardPosition stone, Player player, int dr, int dc)
        {
            var before = new BoardPosition(stone.Row - dr, stone.Column - dc);
            bool beforeValid = board.Size.IsValidPosition(before);
            if (beforeValid && board[before] == player) return 0;

            int count = 1;
            var after = new BoardPosition(stone.Row + dr, stone.Column + dc);
            while (board.Size.IsValidPosition(after) && board[after] == player)
            {
                count++;
                after = new BoardPosition(after.Row + dr, after.Column + dc);
            }

            int openEnds = 0;
            if (beforeValid && board[before] == Player.None) openEnds++;
            if (board.Size.IsValidPosition(after) && board[after] == Player.None) openEnds++;

            return Scores.ForLine(count, openEnds);
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
