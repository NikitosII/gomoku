using gomoku.AI.Interfaces;
using gomoku.Entities;
using gomoku.Interfaces;

namespace gomoku.AI.ThreatDetection
{
    public class ThreatDetector : IThreatDetector
    {
        private readonly IRules _rules;
        private readonly (int, int)[] _directions = new (int, int)[]
        {
            (0, 1), (1, 0), (1, 1), (1, -1)
        };
        public ThreatDetector (IRules rules)
        {
            _rules = rules;
        }
        private Player GetOpponent(Player player)
        {
            return player == Player.Black ? Player.White : Player.Black;
        }

        public int EvaluateThreatLevel(GameBoard board, BoardPosition move, Player player, Player opponent)
        {
            int threatLevel = 0;

            foreach (var (dr, dc) in _directions)
            {
                threatLevel += EvaluateDirectionThreat(board, move, player, opponent, dr, dc);
            }

            return threatLevel;
        }

        public IEnumerable<BoardPosition> FindCritMoves(GameBoard board, Player player, Player opponent)
        {
            var criticalMoves = new List<BoardPosition>();
            var possibleMoves = GetPossibleMoves(board);

            // 1. Выигрышные ходы
            foreach (var move in possibleMoves)
            {
                if (IsImmediateWin(board, move, player))
                    criticalMoves.Add(move);
            }
            if (criticalMoves.Count > 0) return criticalMoves;

            // 2. Блокировка выигрыша противника
            foreach (var move in possibleMoves)
            {
                if (IsImmediateWin(board, move, opponent))
                    criticalMoves.Add(move);
            }
            if (criticalMoves.Count > 0) return criticalMoves;

            // 3. Создание двойных угроз
            foreach (var move in possibleMoves)
            {
                if (CountSimultaneousThreats(board, move, player, opponent) >= 2)
                    criticalMoves.Add(move);
            }

            return criticalMoves;
        }

        public bool IsImmediateThreat(GameBoard board, BoardPosition move, Player player, out int threatLevel)
        {
            threatLevel = 0;
            board[move] = player;

            foreach (var (dr, dc) in _directions)
            {
                var threat = EvaluateDirectionThreat(board, move, player, GetOpponent(player),dr, dc);
                threatLevel = Math.Max(threatLevel, threat);
            }

            board[move] = Player.None;
            return threatLevel >= 500;
        }

        public bool IsImmediateWin(GameBoard board, BoardPosition move, Player player)
        {
            board[move] = player;
            bool isWin = _rules.CheckCondition(board, move);
            board[move] = Player.None;
            return isWin;
        }



        private int EvaluateDirectionThreat(GameBoard board, BoardPosition move, Player player, Player opponent, int dr, int dc)
        {
            int maxScore = 0;

            // Проверяем в 4 направлениях
            for (int i = -4; i <= 0; i++)
            {
                var score = EvaluateSequence(board, move, player, opponent, dr, dc, i);
                maxScore = Math.Max(maxScore, score);
            }

            return maxScore;
        }

        private int EvaluateSequence(GameBoard board, BoardPosition start, Player player, Player opponent, int dr, int dc, int offset)
        {
            int playerCount = 0;
            int emptyCount = 0;
            bool blocked = false;

            // Анализируем последовательность из 5 клеток
            for (int j = 0; j < 5; j++)
            {
                var pos = new BoardPosition(
                    start.Row + dr * (offset + j),
                    start.Column + dc * (offset + j)
                );

                if (!board.Size.IsValidPosition(pos))
                {
                    blocked = true;
                    break;
                }

                var cell = board[pos];
                if (cell == player)
                    playerCount++;
                else if (cell == opponent)
                {
                    blocked = true;
                    break;
                }
                else
                    emptyCount++;
            }

            if (blocked) return 0;

            // Оценка на основе количества камней игрока и пустых клеток
            return playerCount switch
            {
                4 when emptyCount == 1 => 10000, // Открытая четверка
                3 when emptyCount == 2 => 1000,  // Открытая тройка
                2 when emptyCount == 3 => 100,   // Открытая двойка
                1 when emptyCount == 4 => 10,    // Одиночный камень
                _ => 0
            };
        }

        private int CountSimultaneousThreats(GameBoard board, BoardPosition move, Player player, Player opponent)
        {
            int threatCount = 0;
            board[move] = player;

            foreach (var (dr, dc) in _directions)
            {
                if (EvaluateDirectionThreat(board, move, player, opponent, dr, dc) >= 1000)
                    threatCount++;
            }

            board[move] = Player.None;
            return threatCount;
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

