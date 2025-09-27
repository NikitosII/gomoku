using gomoku.Entities;

namespace gomoku.AI.Interfaces
{
    public interface IGomokuAI
    {
        Task<BoardPosition> FindBestMoveAsync(GameBoard board, Player aiPlayer, CancellationToken cancellationToken = default);
    }
}
