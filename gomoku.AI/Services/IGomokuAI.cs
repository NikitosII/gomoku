using gomoku.Entities;

namespace gomoku.AI.Services
{
    public interface IGomokuAI
    {
        Task<BoardPosition> FindBestMoveAsync(GameBoard board, Player aiPlayer, CancellationToken cancellationToken = default);
    }
}
