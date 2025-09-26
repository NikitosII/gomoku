
using gomoku.Entities;

namespace gomoku.AI.Services
{
    public interface IPositionEvaluator
    {
        int Evaluate(GameBoard board, Player player);
    }
}
