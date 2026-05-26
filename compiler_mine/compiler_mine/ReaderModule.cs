namespace PascalCompiler.Core;

public class SourceReader
{
    private readonly string _text;
    private int _position;

    private readonly ErrorTable _errors;

    public char CurrentChar { get; private set; }

    public int Line { get; private set; } = 1;
    public int Column { get; private set; } = 0;

    public SourceReader(string text, ErrorTable errors)
    {
        _text = text;
        _errors = errors;

        NextCh();
    }

    public void NextCh()
    {
        // EOF
        if (_position >= _text.Length)
        {
            CurrentChar = '\0';
            return;
        }

        CurrentChar = _text[_position];
        _position++;

        // Переход на новую строку
        if (CurrentChar == '\n')
        {
            Line++;
            Column = 0;
        }
        else
        {
            Column++;
        }

        // Проверка недопустимых символов
        if (CurrentChar == '@')
        {
            _errors.Add(Line, Column,
                "Недопустимый символ '@'");
        }
    }
}