
using gomoku.Entities;
using gomoku.ValueObjects;

namespace gomoku.Interfaces
{

    // Контракт для правил игры
    /// Определяет основные операции проверки валидности ходов и условий победы.
    public interface IRules
    {
        bool IsMoveValid(GameBoard board, BoardPosition position); // Проверяет допустимость хода 
        
        bool CheckCondition(GameBoard board, BoardPosition position);// Проверяет условие победы

        GameResult? GetResult(GameBoard board, BoardPosition position); // Получает результат игры после последнего хода
        BoardSize GetBoardSize(); // Возвращает размер доски
    }
}
