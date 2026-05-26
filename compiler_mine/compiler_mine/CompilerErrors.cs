namespace PascalCompiler.Core;

public class CompilerError
{
    public int Line { get; }
    public int Column { get; }
    public string Message { get; }

    public CompilerError(int line, int column, string message)
    {
        Line = line;
        Column = column;
        Message = message;
    }

    public override string ToString()
    {
        return $"Ошибка: {Message} (строка {Line}, столбец {Column})";
    }
}