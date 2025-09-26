using gomoku.Entities;
using gomoku.Interfaces;
using gomoku.ValueObjects;

namespace gomoku.Services
{
    public class GomokuRules : IRules
    {
        private readonly BoardSize _boardSize;
        private static readonly BoardPosition[] Directions =
        {
            new(1,0),    //   vert
            new(0,1),    //   horiz
            new(1,1),    //   diag \
            new(1,-1),   //   diag /
        };

        public GomokuRules(BoardSize boardSize)
        {
            _boardSize = boardSize;
        }

        public bool IsMoveValid(GameBoard board, BoardPosition position) =>
           _boardSize.IsValidPosition(position) && board[position] == Player.None;

        public bool CheckCondition (GameBoard board, BoardPosition position)
        {
            var player = board[position];
            if (player == Player.None) 
                return false;

            foreach (var direction in Directions)
            {
                var count = 1;

                for (var i = 1; i <= 4; i++)
                {
                    var pos = position + direction * i;
                    if (!_boardSize.IsValidPosition(pos) || board[pos] != player) break;
                    count++;
                }

                for (var i = 1; i <= 4; i++)
                {
                    var pos = position + direction * -i;
                    if (!_boardSize.IsValidPosition(pos) || board[pos] != player) break;
                    count++;
                }

                if (count >= 5) return true;
            }
            return false;

        }
        private List<BoardPosition> FindWinningLine(GameBoard board, BoardPosition lastMove)
        {
            var player = board[lastMove];
            var winningLine = new List<BoardPosition> { lastMove };

            foreach (var direction in Directions)
            {
                var line = new List<BoardPosition> { lastMove };

                // Positive direction
                for (var i = 1; i <= 4; i++)
                {
                    var pos = lastMove + direction * i;
                    if (!_boardSize.IsValidPosition(pos) || board[pos] != player) break;
                    line.Add(pos);
                }

                // Negative direction
                for (var i = 1; i <= 4; i++)
                {
                    var pos = lastMove + direction * -i;
                    if (!_boardSize.IsValidPosition(pos) || board[pos] != player) break;
                    line.Insert(0, pos);
                }

                if (line.Count >= 5)
                {
                    winningLine = line.Take(5).ToList();
                    break;
                }
            }

            return winningLine;
        }

        public GameResult? GetResult(GameBoard board, BoardPosition position)
        {
            if(CheckCondition(board, position))
            {
                return new GameResult(board[position], FindWinningLine(board, position)); 
            }
            return board.IsFull ? new GameResult(Player.None, Array.Empty<BoardPosition>()) : null;
        }

        public BoardSize GetBoardSize() => _boardSize;
        
    }
}
