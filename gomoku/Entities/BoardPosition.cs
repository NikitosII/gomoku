namespace gomoku.Entities;

// Представляет позицию на игровой доске.
public record BoardPosition(int Row, int Column)
{
    public static readonly BoardPosition Zero = new(0, 0);
    public static BoardPosition operator +(BoardPosition a, BoardPosition b) =>
        new(a.Row + b.Row, a.Column + b.Column);

    public static BoardPosition operator *(BoardPosition position, int multiplier) =>
        new(position.Row * multiplier, position.Column * multiplier);

    public override string ToString() => $"({Row}, {Column})";
}