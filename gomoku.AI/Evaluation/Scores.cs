namespace gomoku.AI.Evaluation
{
    // Единая шкала оценок для всего AI.
    //
    // Главный инвариант: Win > MaxHeuristic >= любая позиционная оценка.
    public static class Scores
    {
        public const int Win = 10_000_000;

        // Потолок эвристики. Строго меньше Win с большим запасом.
        public const int MaxHeuristic = 1_000_000;

        public const int Five = 100_000;
        public const int OpenFour = 20_000;
        public const int ClosedFour = 4_000;
        public const int OpenThree = 3_000;
        public const int ClosedThree = 400;
        public const int OpenTwo = 200;
        public const int ClosedTwo = 30;
        public const int Single = 10;

        // Оценка непрерывного ряда из count камней с openEnds свободными концами.
        public static int ForLine(int count, int openEnds) => (count, openEnds) switch
        {
            ( >= 5, _) => Five,
            (4, 2) => OpenFour,
            (4, 1) => ClosedFour,
            (3, 2) => OpenThree,
            (3, 1) => ClosedThree,
            (2, 2) => OpenTwo,
            (2, 1) => ClosedTwo,
            (1, 2) => Single,
            _ => 0
        };
    }
}
