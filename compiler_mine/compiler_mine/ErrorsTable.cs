namespace PascalCompiler.Core;

public class ErrorTable
{
    private readonly List<CompilerError> _errors = new();

    public void Add(int line, int column, string message)
    {
        _errors.Add(new CompilerError(line, column, message));
    }

    public bool HasErrors()
    {
        return _errors.Count > 0;
    }

    public void Print()
    {
        foreach (var error in _errors)
        {
            Console.WriteLine(error);
        }
    }
}