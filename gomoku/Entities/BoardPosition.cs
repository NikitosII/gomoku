namespace gomoku.Entities;
public record BoardPosition(int Row, int Column)
{
    public static BoardPosition operator +(BoardPosition a, BoardPosition b) =>
        new(a.Row + b.Row, a.Column + b.Column);

    public static BoardPosition operator *(BoardPosition position, int multiplier) =>
        new(position.Row * multiplier, position.Column * multiplier);
}