using System.Collections.Generic;

namespace Компилятор
{
    class Keywords
    {
        private Dictionary<string, byte> _keywordsDictionary;

        public Keywords()
        {
            _keywordsDictionary = new Dictionary<string, byte>
            {
                { "do", LexicalAnalyzer.dosy },
                { "if", LexicalAnalyzer.ifsy },
                { "in", LexicalAnalyzer.insy },
                { "of", LexicalAnalyzer.ofsy },
                { "or", LexicalAnalyzer.orsy },
                { "to", LexicalAnalyzer.tosy },
                { "end", LexicalAnalyzer.endsy },
                { "var", LexicalAnalyzer.varsy },
                { "div", LexicalAnalyzer.divsy },
                { "and", LexicalAnalyzer.andsy },
                { "not", LexicalAnalyzer.notsy },
                { "for", LexicalAnalyzer.forsy },
                { "mod", LexicalAnalyzer.modsy },
                { "nil", LexicalAnalyzer.nilsy },
                { "set", LexicalAnalyzer.setsy },
                { "then", LexicalAnalyzer.thensy },
                { "else", LexicalAnalyzer.elsesy },
                { "case", LexicalAnalyzer.casesy },
                { "file", LexicalAnalyzer.filesy },
                { "goto", LexicalAnalyzer.gotosy },
                { "type", LexicalAnalyzer.typesy },
                { "with", LexicalAnalyzer.withsy },
                { "char", LexicalAnalyzer.charsy },
                { "real", LexicalAnalyzer.realsy },
                { "begin", LexicalAnalyzer.beginsy },
                { "while", LexicalAnalyzer.whilesy },
                { "array", LexicalAnalyzer.arraysy },
                { "const", LexicalAnalyzer.constsy },
                { "label", LexicalAnalyzer.labelsy },
                { "until", LexicalAnalyzer.untilsy },
                { "downto", LexicalAnalyzer.downtosy },
                { "packed", LexicalAnalyzer.packedsy },
                { "record", LexicalAnalyzer.recordsy },
                { "repeat", LexicalAnalyzer.repeatsy },
                { "string", LexicalAnalyzer.stringsy },
                { "program", LexicalAnalyzer.programsy },
                { "integer", LexicalAnalyzer.integersy },
                { "boolean", LexicalAnalyzer.booleansy },
                { "function", LexicalAnalyzer.functionsy },
                { "procedure", LexicalAnalyzer.proceduresy }
            };
        }

        public byte FindKeyword(string word)
        {
            string lowerWord = word.ToLower();
            return _keywordsDictionary.ContainsKey(lowerWord) ? _keywordsDictionary[lowerWord] : (byte)0;
        }
    }
}