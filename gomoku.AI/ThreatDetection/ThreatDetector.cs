using gomoku.AI.Interfaces;
using gomoku.AI.Evaluation;
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

        public int EvaluateThreatLevel(GameBoard board, BoardPosition move, Player player, Player opponent)
        {
            if (board[move] != Player.None) return 0;

            board[move] = player;

            int threatLevel = 0;
            foreach (var (dr, dc) in _directions)
            {
                threatLevel += EvaluateDirectionThreat(board, move, player, dr, dc);
            }

            board[move] = Player.None;
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
                if (CountSimultaneousThreats(board, move, player) >= 2)
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
                var threat = EvaluateDirectionThreat(board, move, player, dr, dc);
                threatLevel = Math.Max(threatLevel, threat);
            }

            board[move] = Player.None;
            return threatLevel >= Scores.ClosedThree;
        }

        public bool IsImmediateWin(GameBoard board, BoardPosition move, Player player)
        {
            board[move] = player;
            bool isWin = _rules.CheckCondition(board, move);
            board[move] = Player.None;
            return isWin;
        }

        // Снимает линию из 9 клеток с ходом в центре и отдаёт её на разбор в ThreatPatterns. 
        private int EvaluateDirectionThreat(GameBoard board, BoardPosition move, Player player, int dr, int dc)
        {
            Span<byte> line = stackalloc byte[ThreatPatterns.Length];

            for (int i = 0; i < ThreatPatterns.Length; i++)
            {
                int offset = i - ThreatPatterns.Radius;
                var pos = new BoardPosition(move.Row + dr * offset, move.Column + dc * offset);

                if (!board.Size.IsValidPosition(pos)) line[i] = ThreatPatterns.Blocked;
                else if (board[pos] == player) line[i] = ThreatPatterns.Own;
                else if (board[pos] == Player.None) line[i] = ThreatPatterns.Empty;
                else line[i] = ThreatPatterns.Blocked;
            }

            return ThreatPatterns.Score(line);
        }

        private int CountSimultaneousThreats(GameBoard board, BoardPosition move, Player player)
        {
            int threatCount = 0;
            board[move] = player;

            // Планка - ОТКРЫТАЯ тройка: две таких в разных направлениях есть классическая вилка, её уже не закрыть одним ходом.
            // Планка держится на том, что ThreatPatterns отличает открытую тройку от закрытой.
            foreach (var (dr, dc) in _directions)
            {
                if (EvaluateDirectionThreat(board, move, player, dr, dc) >= Scores.OpenThree)
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

