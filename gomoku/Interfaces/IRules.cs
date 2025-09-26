
using gomoku.Entities;
using gomoku.ValueObjects;

namespace gomoku.Interfaces
{
    public interface IRules
    {
        bool IsMoveValid(GameBoard board, BoardPosition position);
        bool CheckCondition(GameBoard board, BoardPosition position);
        GameResult? GetResult(GameBoard board, BoardPosition position);
        BoardSize GetBoardSize();
    }
}
