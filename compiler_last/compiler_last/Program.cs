using System;
using System.IO;

namespace Компилятор
{
    class Program
    {
        static void Main()
        {
            Console.Write("Укажите путь к исходному файлу Pascal: ");
            string filePath = Console.ReadLine();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Критическая ошибка: файл не найден по указанному пути.");
                return;
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                InputOutput.Initialize(reader);

                LexicalAnalyzer lexer = new LexicalAnalyzer();
                Parser parser = new Parser(lexer);

                Console.WriteLine("\nЗапуск синтаксического и семантического анализа...");
                parser.Parse();

                InputOutput.Close();
            }

            Console.WriteLine("\nДля завершения работы нажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}