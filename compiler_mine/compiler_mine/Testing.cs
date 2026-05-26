using PascalCompiler.Core;

namespace PascalCompiler.Tests;

public class TestRunner
{
    public static void Run()
    {
        Console.WriteLine("===== ТЕСТ 1 =====");

        string code = """
                      begin
                        x := 10;
                      end.
                      """;

        ErrorTable errors = new();

        SourceReader reader = new(code, errors);

        while (reader.CurrentChar != '\0')
        {
            Console.WriteLine(
                $"'{reader.CurrentChar}' " +
                $"Line={reader.Line} " +
                $"Column={reader.Column}"
            );

            reader.NextCh();
        }

        Console.WriteLine();

        if (errors.HasErrors())
        {
            errors.Print();
        }
        else
        {
            Console.WriteLine("Ошибок нет.");
        }

        Console.WriteLine();
        Console.WriteLine("===== ТЕСТ 2 =====");

        string badCode = "x := 5 !@";

        ErrorTable errors2 = new();

        SourceReader reader2 = new(badCode, errors2);

        while (reader2.CurrentChar != '\0')
        {
            Console.WriteLine(
                $"'{reader2.CurrentChar}' " +
                $"Line={reader2.Line} " +
                $"Column={reader2.Column}"
            );

            reader2.NextCh();
        }

        Console.WriteLine();

        if (errors2.HasErrors())
        {
            errors2.Print();
        }
        else
        {
            Console.WriteLine("Ошибок нет.");
        }
    }
}