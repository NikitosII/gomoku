using gomoku.Entities;

namespace gomoku.AI.Interfaces
{
    public interface IThreatDetector
    {
        bool IsImmediateWin(GameBoard board, BoardPosition move, Player player);
        bool IsImmediateThreat(GameBoard board, BoardPosition move, Player player, out int threatLevel);
        IEnumerable<BoardPosition> FindCritMoves(GameBoard board, Player player, Player opponent);
        int EvaluateThreatLevel(GameBoard board, BoardPosition move, Player player, Player opponent);
    }
}
