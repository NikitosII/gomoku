using System.Runtime.CompilerServices;

// ThreatPatterns - внутренний класс, но именно его таблицу форм нужно
// проверять напрямую: через AI отдельный шаблон уже не разглядеть.
[assembly: InternalsVisibleTo("gomoku.Tests")]
