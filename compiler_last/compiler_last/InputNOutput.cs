using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    class InputOutput
    {
        private struct TextPosition
        {
            private uint _lineNumber;
            private byte _charNumber;

            public TextPosition(uint line, byte character)
            {
                _lineNumber = line;
                _charNumber = character;
            }

            public uint LineNumber 
            { 
                get => _lineNumber; 
                set => _lineNumber = value; 
            }
            public byte CharNumber 
            { 
                get => _charNumber; 
                set => _charNumber = value; 
            }
        }

        private struct Err
        {
            private TextPosition _errorPosition;
            private byte _errorCode;

            public Err(TextPosition position, byte code)
            {
                _errorPosition = position;
                _errorCode = code;
            }

            public TextPosition ErrorPosition 
            { 
                get => _errorPosition; 
                set => _errorPosition = value; 
            }
            public byte ErrorCode 
            { 
                get => _errorCode; 
                set => _errorCode = value; 
            }
        }

        private const byte MAX_ERRORS_PER_LINE = 9;

        private static char _ch;
        private static TextPosition _positionNow;
        private static List<Err> _errors;
        private static string _line;
        private static byte _lastInLine;
        private static uint _errorCount;
        private static StreamReader _file;
        private static bool _endOfFile;
        private static bool _linePrinted;
        private static bool _suppressOutput;

        public static char Ch
        {
            get => _ch;
            set => _ch = value;
        }

        public static bool EndOfFile => _endOfFile;
        public static bool EndOfLine => _positionNow.CharNumber == _lastInLine;

        public static bool SuppressOutput
        {
            get => _suppressOutput;
            set => _suppressOutput = value;
        }

        // Внутреннее свойство для лексера, чтобы не светить структуру наружу как public
        public static object CurrentTokenPosition => _positionNow;

        public static void Initialize(StreamReader streamFile)
        {
            _file = streamFile;
            _errorCount = 0;
            _endOfFile = false;
            _linePrinted = false;
            _suppressOutput = false;
            _line = " ";
            _lastInLine = 0;
            _ch = ' ';
            _positionNow = new TextPosition(0, 0);
            _errors = new List<Err>();
            ReadNextLine();
            _positionNow.LineNumber = 1;
            _positionNow.CharNumber = 0;
            _ch = _line[0];
        }

        public static void NextCh()
        {
            if (_endOfFile) return;

            if (_positionNow.CharNumber == _lastInLine)
            {
                if (!_suppressOutput && !_linePrinted)
                {
                    Console.WriteLine($"{_positionNow.LineNumber,4}: {_line}");
                    _linePrinted = true;
                }

                if (!_suppressOutput && _errors.Count > 0)
                {
                    PrintErrors();
                }

                ReadNextLine();

                if (_endOfFile) return;

                _positionNow.LineNumber++;
                _positionNow.CharNumber = 0;
                _linePrinted = false;
            }
            else
            {
                _positionNow.CharNumber++;
            }

            _ch = _line[_positionNow.CharNumber];
        }

        public static void Close()
        {
            if (!_suppressOutput && !_linePrinted && _positionNow.LineNumber > 0)
            {
                Console.WriteLine($"{_positionNow.LineNumber,4}: {_line}");
                _linePrinted = true;
            }

            if (!_suppressOutput && _errors.Count > 0)
            {
                PrintErrors();
            }

            Console.WriteLine($"\nАнализ завершен. Обнаружено ошибок: {_errorCount}");
            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }

        private static void ReadNextLine()
        {
            if (!_file.EndOfStream)
            {
                _line = _file.ReadLine() + " ";
                _lastInLine = (byte)(_line.Length - 1);
                _errors = new List<Err>();
            }
            else
            {
                _endOfFile = true;
            }
        }

        private static void PrintErrors()
        {
            foreach (Err err in _errors)
            {
                _errorCount++;
                string formattedNum = _errorCount < 10 ? $"0{_errorCount}" : $"{_errorCount}";
                int offset = 6 + err.ErrorPosition.CharNumber - 6;
                string spaces = new string(' ', Math.Max(0, offset));

                Console.WriteLine($"**{formattedNum}**{spaces}^ Ошибка код {err.ErrorCode}: {GetErrorMessage(err.ErrorCode)}");
            }
        }

        public static void Error(byte code, object position)
        {
            if (_errors.Count <= MAX_ERRORS_PER_LINE && position is TextPosition pos)
            {
                _errors.Add(new Err(pos, code));
            }
        }

        private static string GetErrorMessage(byte code)
        {
            switch (code)
            {
                case 201: return "Неожиданный или недопустимый символ";
                case 202: return "Неверный формат идентификатора";
                case 203: return "Целочисленный литерал вышел за допустимые пределы";
                case 204: return "Обнаружен незакрытый блок комментария";
                case 205: return "Нарушена структура строковой или символьной константы";
                case 206: return "Синтаксическая ошибка в вещественном числе";
                case 209: return "Превышена максимальная длина идентификатора";
                case 210: return "Синтаксическая ошибка: ожидался другой токен (символ)";
                case 211: return "Ожидалось имя стандартного типа данных (integer, real, и т.д.)";
                case 212: return "Ожидался оператор языка";
                case 213: return "Ожидался операнд в выражении";
                case 215: return "Семантическая ошибка: переменная переопределена повторно";
                case 216: return "Семантическая ошибка: несовместимость типов в операции присваивания";
                case 218: return "Семантическая ошибка: использование необъявленного идентификатора";
                default: return "Неизвестная синтаксическая или семантическая ошибка";
            }
        }
    }
}