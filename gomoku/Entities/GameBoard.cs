
using gomoku.ValueObjects;

namespace gomoku.Entities
{

    // Представляет игровую доску
    public class GameBoard
    {
        private readonly Player[,] _cells; // Двумерный массив состояния клеток
        public BoardSize Size { get; } // Размер доски
        public int MoveCount { get; private set; } // Общее количество сделанных ходов
        public bool IsFull => MoveCount >= Size.Rows * Size.Columns;
        private BoardPosition? _lastMove; // Последний сделанный ход
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

        // Индексатор для доступа к состоянию клетки по позиции.
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

        // Создает глубокую копию доски.
        /// Используется AI для анализа без изменения оригинальной доски.
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
