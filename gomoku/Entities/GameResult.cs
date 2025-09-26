
namespace gomoku.Entities
{
    public record GameResult(Player Winner, IReadOnlyList<BoardPosition> WinningLine);
}
