using gomoku.Entities;

namespace gomoku.AI.Interfaces
{
    public interface IPositionEvaluator
    {
        int EvaluatePosition(GameBoard board, Player player, Player opponent);
        int EvaluateMove(GameBoard board, BoardPosition move, Player player, Player opponent);
        IEnumerable<BoardPosition> GetStratMoves(GameBoard board, Player player, Player opponent, int maxMoves);
    }
}
