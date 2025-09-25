using gomoku.Entities;

namespace gomoku.ValueObjects;
public record BoardSize(int Rows, int Columns)
{
    public bool IsValidPosition(BoardPosition position) =>
        position.Row >= 0 && position.Row < Rows &&
        position.Column >= 0 && position.Column < Columns;
}
