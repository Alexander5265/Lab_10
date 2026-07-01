using System;

namespace Компилятор
{
    class LexicalAnalyzer
    {
        public const byte
            star = 21, slash = 60, equal = 16, comma = 20, semicolon = 14, colon = 5,
            point = 61, arrow = 62, leftpar = 9, rightpar = 4, lbracket = 11, rbracket = 12,
            flpar = 63, frpar = 64, later = 65, greater = 66, laterequal = 67, greaterequal = 68,
            latergreater = 69, plus = 70, minus = 71, lcomment = 72, rcomment = 73, assign = 51,
            twopoints = 74, ident = 2, floatc = 82, intc = 15, stringc = 83, charc = 84,
            casesy = 31, elsesy = 32, filesy = 57, gotosy = 33, thensy = 52, typesy = 34,
            untilsy = 53, dosy = 54, withsy = 37, ifsy = 56, insy = 100, ofsy = 101,
            orsy = 102, tosy = 103, endsy = 104, varsy = 105, divsy = 106, andsy = 107,
            notsy = 108, forsy = 109, modsy = 110, nilsy = 111, setsy = 112, beginsy = 113,
            whilesy = 114, arraysy = 115, constsy = 116, labelsy = 117, downtosy = 118,
            packedsy = 119, recordsy = 120, repeatsy = 121, programsy = 122, functionsy = 123,
            proceduresy = 124, integersy = 125, realsy = 126, booleansy = 127, charsy = 128, stringsy = 129;

        private const int MAX_IDENT_SIZE = 127;
        private const int MAX_STRING_SIZE = 255;

        private byte _symbol;
        private object _tokenPosition;
        private string _addressName;
        private int _integerValue;
        private double _floatValue;
        private char _charValue;
        private string _stringValue;

        private Keywords _keywords;
        private bool _hasSavedToken;
        private byte _savedSymbol;

        public byte Symbol => _symbol;
        public object TokenPosition => _tokenPosition;
        public string AddressName => _addressName;

        public LexicalAnalyzer()
        {
            _keywords = new Keywords();
            ClearState();
        }

        private void ClearState()
        {
            _symbol = 0;
            _addressName = string.Empty;
            _integerValue = 0;
            _floatValue = 0.0;
            _charValue = '\0';
            _stringValue = string.Empty;
            _hasSavedToken = false;
            _savedSymbol = 0;
        }

        public byte NextSym()
        {
            if (_hasSavedToken)
            {
                _hasSavedToken = false;
                _symbol = _savedSymbol;
                return _symbol;
            }

            while (!InputOutput.EndOfFile && InputOutput.Ch == ' ')
            {
                InputOutput.NextCh();
            }

            if (InputOutput.EndOfFile)
            {
                _symbol = 0;
                return _symbol;
            }

            _tokenPosition = InputOutput.CurrentTokenPosition;
            char ch = InputOutput.Ch;

            if (char.IsLetter(ch) || ch == '_') return ScanIdentOrKey();
            if (char.IsDigit(ch)) return ScanNumericLiteral();
            if (ch == '\'') return ScanCharacter();
            if (ch == '"') return ScanStringLiteral();

            return ScanSymbols(ch);
        }

        private byte ScanIdentOrKey()
        {
            string accumulator = string.Empty;

            while (!InputOutput.EndOfFile && (char.IsLetterOrDigit(InputOutput.Ch) || InputOutput.Ch == '_'))
            {
                accumulator += InputOutput.Ch;
                InputOutput.NextCh();
            }

            if (accumulator.Length > MAX_IDENT_SIZE)
            {
                InputOutput.Error(209, _tokenPosition);
                accumulator = accumulator.Substring(0, MAX_IDENT_SIZE);
            }

            byte keyWordCode = _keywords.FindKeyword(accumulator);
            if (keyWordCode > 0)
            {
                _symbol = keyWordCode;
            }
            else
            {
                _symbol = ident;
                _addressName = accumulator;
            }

            return _symbol;
        }

        private byte ScanNumericLiteral()
        {
            _integerValue = 0;
            bool isOverflow = false;

            while (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch) && !isOverflow)
            {
                byte currentDigit = (byte)(InputOutput.Ch - '0');

                if (_integerValue > Int16.MaxValue / 10 || (_integerValue == Int16.MaxValue / 10 && currentDigit > Int16.MaxValue % 10))
                {
                    InputOutput.Error(203, _tokenPosition);
                    isOverflow = true;
                }
                else
                {
                    _integerValue = 10 * _integerValue + currentDigit;
                    InputOutput.NextCh();
                }
            }

            if (isOverflow)
            {
                while (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch)) InputOutput.NextCh();
                _integerValue = 0;
            }

            if (!InputOutput.EndOfFile && InputOutput.Ch == '.')
            {
                InputOutput.NextCh();
                if (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch))
                {
                    return ScanFloatPart();
                }
                
                _hasSavedToken = true;
                _savedSymbol = point;
                _symbol = intc;
                return _symbol;
            }

            _symbol = intc;
            return _symbol;
        }

        private byte ScanFloatPart()
        {
            double fractionValue = 0.0;
            double scale = 10.0;

            while (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch))
            {
                fractionValue += (InputOutput.Ch - '0') / scale;
                scale *= 10.0;
                InputOutput.NextCh();
            }

            _floatValue = _integerValue + fractionValue;
            _symbol = floatc;
            return _symbol;
        }

        private byte ScanCharacter()
        {
            InputOutput.NextCh();
            if (InputOutput.EndOfFile || InputOutput.Ch == '\'')
            {
                InputOutput.Error(205, _tokenPosition);
                if (!InputOutput.EndOfFile) InputOutput.NextCh();
                _charValue = '\0';
                return _symbol = charc;
            }

            _charValue = InputOutput.Ch;
            InputOutput.NextCh();

            if (!InputOutput.EndOfFile && InputOutput.Ch == '\'')
            {
                InputOutput.NextCh();
                return _symbol = charc;
            }

            InputOutput.Error(205, _tokenPosition);
            return _symbol = charc;
        }

        private byte ScanStringLiteral()
        {
            InputOutput.NextCh();
            _stringValue = string.Empty;
            bool closed = false;

            while (!InputOutput.EndOfFile && !closed)
            {
                if (InputOutput.Ch == '"')
                {
                    closed = true;
                    InputOutput.NextCh();
                }
                else if (InputOutput.EndOfLine || _stringValue.Length >= MAX_STRING_SIZE)
                {
                    InputOutput.Error(205, _tokenPosition);
                    break;
                }
                else
                {
                    _stringValue += InputOutput.Ch;
                    InputOutput.NextCh();
                }
            }

            return _symbol = stringc;
        }

        private byte ScanSymbols(char current)
        {
            switch (current)
            {
                case '<':
                    InputOutput.NextCh();
                    if (!InputOutput.EndOfFile && InputOutput.Ch == '=') { InputOutput.NextCh(); return _symbol = laterequal; }
                    if (!InputOutput.EndOfFile && InputOutput.Ch == '>') { InputOutput.NextCh(); return _symbol = latergreater; }
                    return _symbol = later;
                case '>':
                    InputOutput.NextCh();
                    if (!InputOutput.EndOfFile && InputOutput.Ch == '=') { InputOutput.NextCh(); return _symbol = greaterequal; }
                    return _symbol = greater;
                case ':':
                    InputOutput.NextCh();
                    if (!InputOutput.EndOfFile && InputOutput.Ch == '=') { InputOutput.NextCh(); return _symbol = assign; }
                    return _symbol = colon;
                case '.':
                    InputOutput.NextCh();
                    if (!InputOutput.EndOfFile && InputOutput.Ch == '.') { InputOutput.NextCh(); return _symbol = twopoints; }
                    return _symbol = point;
                case ';': InputOutput.NextCh(); return _symbol = semicolon;
                case ',': InputOutput.NextCh(); return _symbol = comma;
                case '(':
                    InputOutput.NextCh();
                    if (!InputOutput.EndOfFile && InputOutput.Ch == '*')
                    {
                        InputOutput.NextCh();
                        SkipOldComment();
                        return NextSym();
                    }
                    return _symbol = leftpar;
                case ')': InputOutput.NextCh(); return _symbol = rightpar;
                case '{':
                    InputOutput.NextCh();
                    SkipCurlyComment();
                    return NextSym();
                case '}': InputOutput.NextCh(); return _symbol = frpar;
                case '*': InputOutput.NextCh(); return _symbol = star;
                case '+': InputOutput.NextCh(); return _symbol = plus;
                case '-': InputOutput.NextCh(); return _symbol = minus;
                case '/': InputOutput.NextCh(); return _symbol = slash;
                case '=': InputOutput.NextCh(); return _symbol = equal;
                case '[': InputOutput.NextCh(); return _symbol = lbracket;
                case ']': InputOutput.NextCh(); return _symbol = rbracket;
                default:
                    InputOutput.Error(201, _tokenPosition);
                    InputOutput.NextCh();
                    return _symbol = 0;
            }
        }

        private void SkipOldComment()
        {
            int nestLevel = 1;
            InputOutput.SuppressOutput = true;

            while (nestLevel > 0 && !InputOutput.EndOfFile)
            {
                if (InputOutput.Ch == '*')
                {
                    InputOutput.NextCh();
                    if (!InputOutput.EndOfFile && InputOutput.Ch == ')') { nestLevel--; InputOutput.NextCh(); }
                }
                else if (InputOutput.Ch == '停') // Заглушка, если встретим структуры
                {
                    InputOutput.NextCh();
                }
                else
                {
                    InputOutput.NextCh();
                }
            }

            InputOutput.SuppressOutput = false;
            if (nestLevel > 0) InputOutput.Error(204, _tokenPosition);
        }

        private void SkipCurlyComment()
        {
            int nestLevel = 1;
            InputOutput.SuppressOutput = true;

            while (nestLevel > 0 && !InputOutput.EndOfFile)
            {
                if (InputOutput.Ch == '{') { nestLevel++; InputOutput.NextCh(); }
                else if (InputOutput.Ch == '}') { nestLevel--; InputOutput.NextCh(); }
                else InputOutput.NextCh();
            }

            InputOutput.SuppressOutput = false;
            if (nestLevel > 0) InputOutput.Error(204, _tokenPosition);
        }
    }
}