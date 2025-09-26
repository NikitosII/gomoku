
using gomoku.ValueObjects;

namespace gomoku.Entities
{
    public class GameBoard
    {
        private readonly Player[,] _cells;
        public BoardSize Size { get; }
        public int MoveCount { get; private set; }
        public bool IsFull => MoveCount >= Size.Rows * Size.Columns;

        public Player this[BoardPosition boardPosition]
        {
            get => _cells[boardPosition.Row, boardPosition.Column];
            set
            {
                if (value != Player.None && _cells[boardPosition.Row, boardPosition.Column] == Player.None)
                {
                    MoveCount++;
                }
                else if(value == Player.None && _cells[boardPosition.Row, boardPosition.Column] != Player.None)
                {
                    MoveCount--;
                }
                _cells[boardPosition.Row, boardPosition.Column] = value;
            }
        }
        public GameBoard(BoardSize size)
        {
            Size = size;
            _cells = new Player[Size.Rows, Size.Columns];
        }

        public GameBoard Clone()
        {
            var clone = new GameBoard(Size);
            Array.Copy(_cells, clone._cells, _cells.Length);
            clone.MoveCount = MoveCount;
            return clone;
        }
    }
}
