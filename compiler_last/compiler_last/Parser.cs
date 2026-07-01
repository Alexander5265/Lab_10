using System;
using System.Collections.Generic;

namespace Компилятор
{
    class Parser
    {
        private LexicalAnalyzer _lexer;
        private byte _sym;
        private Dictionary<string, byte> _symbolTable;

        public Parser(LexicalAnalyzer lexer)
        {
            _lexer = lexer;
            _symbolTable = new Dictionary<string, byte>();
        }

        public void Parse()
        {
            FetchToken();
            AnalyzeProgram();
        }

        private void FetchToken() => _sym = _lexer.NextSym();

        private void ErrorRecovery(byte syncToken)
        {
            while (_sym != 0 && _sym != syncToken) FetchToken();
            if (_sym == syncToken) FetchToken();
        }

        private void AnalyzeProgram()
        {
            if (_sym == LexicalAnalyzer.programsy)
            {
                FetchToken();
                if (_sym == LexicalAnalyzer.ident) FetchToken();
                else InputOutput.Error(202, _lexer.TokenPosition);

                if (_sym == LexicalAnalyzer.semicolon) FetchToken();
                else InputOutput.Error(210, _lexer.TokenPosition);
            }

            if (_sym == LexicalAnalyzer.varsy)
            {
                ParseVariables();
            }

            while (_sym == LexicalAnalyzer.functionsy)
            {
                ParseFunction();
            }

            if (_sym == LexicalAnalyzer.beginsy)
            {
                ParseCompound();
                if (_sym == LexicalAnalyzer.point) FetchToken();
                else InputOutput.Error(210, _lexer.TokenPosition);
            }
            else
            {
                InputOutput.Error(212, _lexer.TokenPosition);
            }
        }

        private void ParseVariables()
        {
            FetchToken();
            while (_sym == LexicalAnalyzer.ident)
            {
                List<string> syntaxIdentifiers = new List<string>();

                while (_sym == LexicalAnalyzer.ident)
                {
                    syntaxIdentifiers.Add(_lexer.AddressName);
                    FetchToken();
                    if (_sym == LexicalAnalyzer.comma) FetchToken();
                    else break;
                }

                if (_sym == LexicalAnalyzer.colon) FetchToken();
                else
                {
                    InputOutput.Error(210, _lexer.TokenPosition);
                    ErrorRecovery(LexicalAnalyzer.semicolon);
                    continue;
                }

                byte determinedType = _sym;
                if (_sym == LexicalAnalyzer.integersy || _sym == LexicalAnalyzer.realsy ||
                    _sym == LexicalAnalyzer.booleansy || _sym == LexicalAnalyzer.charsy)
                {
                    FetchToken();
                }
                else
                {
                    InputOutput.Error(211, _lexer.TokenPosition);
                    ErrorRecovery(LexicalAnalyzer.semicolon);
                    continue;
                }

                foreach (string id in syntaxIdentifiers)
                {
                    string normalizedKey = id.ToLower();
                    if (_symbolTable.ContainsKey(normalizedKey))
                    {
                        InputOutput.Error(215, _lexer.TokenPosition);
                    }
                    else
                    {
                        _symbolTable[normalizedKey] = determinedType;
                    }
                }

                if (_sym == LexicalAnalyzer.semicolon) FetchToken();
                else
                {
                    InputOutput.Error(210, _lexer.TokenPosition);
                    ErrorRecovery(LexicalAnalyzer.semicolon);
                }
            }
        }

        private void ParseFunction()
        {
            FetchToken();
            if (_sym == LexicalAnalyzer.ident) FetchToken();
            else InputOutput.Error(202, _lexer.TokenPosition);

            if (_sym == LexicalAnalyzer.leftpar) ErrorRecovery(LexicalAnalyzer.rightpar);

            if (_sym == LexicalAnalyzer.colon)
            {
                FetchToken();
                if (_sym == LexicalAnalyzer.integersy || _sym == LexicalAnalyzer.realsy) FetchToken();
                else InputOutput.Error(211, _lexer.TokenPosition);
            }

            if (_sym == LexicalAnalyzer.semicolon) FetchToken();
            else InputOutput.Error(210, _lexer.TokenPosition);

            if (_sym == LexicalAnalyzer.varsy) ParseVariables();

            ParseCompound();

            if (_sym == LexicalAnalyzer.semicolon) FetchToken();
            else InputOutput.Error(210, _lexer.TokenPosition);
        }

        private void ParseCompound()
        {
            if (_sym == LexicalAnalyzer.beginsy)
            {
                FetchToken();
                while (_sym != LexicalAnalyzer.endsy && _sym != 0)
                {
                    ParseStatement();
                    if (_sym == LexicalAnalyzer.semicolon) FetchToken();
                    else if (_sym != LexicalAnalyzer.endsy)
                    {
                        InputOutput.Error(210, _lexer.TokenPosition);
                        ErrorRecovery(LexicalAnalyzer.semicolon);
                    }
                }

                if (_sym == LexicalAnalyzer.endsy) FetchToken();
                else InputOutput.Error(210, _lexer.TokenPosition);
            }
        }

        private void ParseStatement()
        {
            if (_sym == LexicalAnalyzer.beginsy) ParseCompound();
            else if (_sym == LexicalAnalyzer.ident) ParseAssignment();
            else
            {
                InputOutput.Error(212, _lexer.TokenPosition);
                ErrorRecovery(LexicalAnalyzer.semicolon);
            }
        }

        private void ParseAssignment()
        {
            string idName = _lexer.AddressName.ToLower();
            byte targetDataType = 0;

            if (!_symbolTable.ContainsKey(idName)) InputOutput.Error(218, _lexer.TokenPosition);
            else targetDataType = _symbolTable[idName];

            FetchToken();

            if (_sym == LexicalAnalyzer.assign) FetchToken();
            else
            {
                InputOutput.Error(210, _lexer.TokenPosition);
                ErrorRecovery(LexicalAnalyzer.semicolon);
                return;
            }

            byte rightSideType = EvaluateExpression();

            if (targetDataType != 0 && rightSideType != 0 && targetDataType != rightSideType)
            {
                if (!(targetDataType == LexicalAnalyzer.realsy && rightSideType == LexicalAnalyzer.integersy))
                {
                    InputOutput.Error(216, _lexer.TokenPosition);
                }
            }
        }

        private byte EvaluateExpression()
        {
            byte leftType = EvaluateTerm();
            while (_sym == LexicalAnalyzer.plus || _sym == LexicalAnalyzer.minus)
            {
                FetchToken();
                byte rightType = EvaluateTerm();
                if (leftType == LexicalAnalyzer.realsy || rightType == LexicalAnalyzer.realsy) leftType = LexicalAnalyzer.realsy;
            }
            return leftType;
        }

        private byte EvaluateTerm()
        {
            byte leftType = EvaluateFactor();
            while (_sym == LexicalAnalyzer.star || _sym == LexicalAnalyzer.slash)
            {
                byte currentOp = _sym;
                FetchToken();
                byte rightType = EvaluateFactor();
                if (currentOp == LexicalAnalyzer.slash) leftType = LexicalAnalyzer.realsy;
                else if (leftType == LexicalAnalyzer.realsy || rightType == LexicalAnalyzer.realsy) leftType = LexicalAnalyzer.realsy;
            }
            return leftType;
        }

        private byte EvaluateFactor()
        {
            if (_sym == LexicalAnalyzer.ident)
            {
                string id = _lexer.AddressName.ToLower();
                FetchToken();
                if (!_symbolTable.ContainsKey(id))
                {
                    InputOutput.Error(218, _lexer.TokenPosition);
                    return 0;
                }
                return _symbolTable[id];
            }
            if (_sym == LexicalAnalyzer.intc)
            {
                FetchToken();
                return LexicalAnalyzer.integersy;
            }
            if (_sym == LexicalAnalyzer.floatc)
            {
                FetchToken();
                return LexicalAnalyzer.realsy;
            }
            if (_sym == LexicalAnalyzer.leftpar)
            {
                FetchToken();
                byte typeInside = EvaluateExpression();
                if (_sym == LexicalAnalyzer.rightpar) FetchToken();
                else InputOutput.Error(210, _lexer.TokenPosition);
                return typeInside;
            }

            InputOutput.Error(213, _lexer.TokenPosition);
            return 0;
        }
    }
}