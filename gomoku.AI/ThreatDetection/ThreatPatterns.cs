using gomoku.AI.Evaluation;

namespace gomoku.AI.ThreatDetection
{
    // Распознавание форм в одной линии.
    //
    // Линия - 9 клеток с оцениваемым ходом ровно в центре. 
    //
    // Уровни задаются рекурсивно, через вопрос "сколькими разными ходами отсюда достраивается пятёрка":
    //   пятёрка            - пять своих подряд;
    //   открытая четвёрка  - пятёрку дают ДВА разных хода (оба не закрыть);
    //   закрытая четвёрка  - ровно один такой ход;
    //   открытая тройка    - есть ход, дающий открытую четвёрку;
    //   закрытая тройка    - есть ход, дающий закрытую четвёрку;
    //   двойки             - то же самое, но через тройки.
    //
    internal static class ThreatPatterns
    {
        public const int Radius = 4;
        public const int Length = Radius * 2 + 1;
        private const int Center = Radius;

        public const byte Empty = 0;
        public const byte Own = 1;
        public const byte Blocked = 2; // чужой камень ИЛИ край доски - для линии это одно и то же

        // Кэш по коду линии (9 клеток в троичной системе - всего 19683 варианта).
        // -1 значит "ещё не считали".
        private static readonly int[] Cache = CreateCache();

        private static int[] CreateCache()
        {
            int size = 1;
            for (int i = 0; i < Length; i++) size *= 3;

            var cache = new int[size];
            for (int i = 0; i < size; i++) cache[i] = -1;
            return cache;
        }

        public static int Score(ReadOnlySpan<byte> line)
        {
            int code = 0;
            for (int i = Length - 1; i >= 0; i--) code = code * 3 + line[i];

            int cached = Cache[code];
            if (cached >= 0) return cached;

            int score = Classify(line);
            Cache[code] = score;
            return score;
        }

        private static int Classify(ReadOnlySpan<byte> line)
        {
            // Мёртвая линия: пятёрку через центр уже не построить ни при каком
            // раскладе, значит камень здесь не стоит вообще ничего.
            if (!FiveIsPossible(line)) return 0;
            if (HasFive(line)) return Scores.Five;

            int makers = CountFiveMakers(line);
            if (makers >= 2) return Scores.OpenFour;
            if (makers == 1) return Scores.ClosedFour;

            int three = ThreeLevel(line);
            if (three > 0) return three;

            // Двойка - ход, после которого получается тройка.
            int best = 0;
            Span<byte> probe = stackalloc byte[Length];
            for (int i = 0; i < Length; i++)
            {
                if (line[i] != Empty) continue;

                line.CopyTo(probe);
                probe[i] = Own;

                int level = ThreeLevel(probe);
                if (level == Scores.OpenThree) return Scores.OpenTwo;
                if (level == Scores.ClosedThree) best = Scores.ClosedTwo;
            }

            return best > 0 ? best : Scores.Single;
        }

        // Тройка - ход, после которого появляется четвёрка.
        private static int ThreeLevel(ReadOnlySpan<byte> line)
        {
            int best = 0;
            Span<byte> probe = stackalloc byte[Length];

            for (int i = 0; i < Length; i++)
            {
                if (line[i] != Empty) continue;

                line.CopyTo(probe);
                probe[i] = Own;

                int makers = CountFiveMakers(probe);
                if (makers >= 2) return Scores.OpenThree; // выше тройка не бывает
                if (makers == 1) best = Scores.ClosedThree;
            }

            return best;
        }

        // Сколько РАЗНЫХ пустых клеток достраивают пятёрку через центр.
        // Одна такая клетка - четвёрку можно закрыть; две - уже нет.
        private static int CountFiveMakers(ReadOnlySpan<byte> line)
        {
            int count = 0;

            for (int i = 0; i < Length; i++)
            {
                if (line[i] != Empty) continue;

                for (int start = Center - 4; start <= Center; start++)
                {
                    if (start < 0 || start + 5 > Length) continue;
                    if (i < start || i >= start + 5) continue;

                    bool complete = true;
                    for (int k = 0; k < 5; k++)
                    {
                        int index = start + k;
                        if (index != i && line[index] != Own) { complete = false; break; }
                    }

                    if (complete) { count++; break; }
                }
            }

            return count;
        }

        private static bool HasFive(ReadOnlySpan<byte> line)
        {
            for (int start = Center - 4; start <= Center; start++)
            {
                if (start < 0 || start + 5 > Length) continue;

                bool all = true;
                for (int k = 0; k < 5; k++)
                    if (line[start + k] != Own) { all = false; break; }

                if (all) return true;
            }

            return false;
        }

        private static bool FiveIsPossible(ReadOnlySpan<byte> line)
        {
            for (int start = Center - 4; start <= Center; start++)
            {
                if (start < 0 || start + 5 > Length) continue;

                bool free = true;
                for (int k = 0; k < 5; k++)
                    if (line[start + k] == Blocked) { free = false; break; }

                if (free) return true;
            }

            return false;
        }
    }
}
