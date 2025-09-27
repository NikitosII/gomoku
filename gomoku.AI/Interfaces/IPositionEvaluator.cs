using gomoku.Entities;

namespace gomoku.AI.Interfaces
{
    public interface IPositionEvaluator
    {
        int Evaluate(GameBoard board, Player player);
    }
}
