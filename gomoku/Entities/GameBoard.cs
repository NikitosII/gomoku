
using gomoku.ValueObjects;

namespace gomoku.Entities
{
    public class GameBoard
    {
        private readonly Player[,] _cells;
        public BoardSize Size { get; }
        public int MoveCount { get; private set; }
        public bool IsFull => MoveCount >= Size.Rows * Size.Columns;
        private BoardPosition? _lastMove;
        private bool _hasLastMove = false;

        public GameBoard(BoardSize size)
        {
            Size = size;
            _cells = new Player[Size.Rows, Size.Columns];
            _hasLastMove = false;
        }
        public bool GetLastMove(out BoardPosition lastMove)
        {
            lastMove = _lastMove;
            return _hasLastMove;
        }

        public Player this[BoardPosition boardPosition]
        {
            get => _cells[boardPosition.Row, boardPosition.Column];
            set
            {
                if (value != Player.None && _cells[boardPosition.Row, boardPosition.Column] == Player.None)
                {
                    MoveCount++;
                    _lastMove = boardPosition;
                    _hasLastMove = true;
                }
                else if(value == Player.None && _cells[boardPosition.Row, boardPosition.Column] != Player.None)
                {
                    MoveCount--;
                    _lastMove = null;
                    _hasLastMove = true;
                }
                _cells[boardPosition.Row, boardPosition.Column] = value;
            }
        }


        public GameBoard Clone()
        {
            var clone = new GameBoard(Size);
            Array.Copy(_cells, clone._cells, _cells.Length);
            clone.MoveCount = MoveCount;
            clone._lastMove = _lastMove;
            clone._hasLastMove = _hasLastMove;
            return clone;
        }
    }
}
