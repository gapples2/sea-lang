﻿using System;
using System.Text;
using System.Text.RegularExpressions;
namespace Sea
{
    public class Program
    {
        public static void Main()
        {
            new ErrorConfig().ErrorSetup();
            new Lexer().Lex("(1+1) 2+2", true);
            new Parser().Parse(Lexer._tokens, true);
        }
    }
    internal class Message
    {
        internal static bool _errored = false;

        internal static void _writeWithColor(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        internal static void _throw(byte severity, string message)
        {
            if (severity == 1) _writeWithColor(ConsoleColor.Gray, $"INFO: {message}");
            if (severity == 2) _writeWithColor(ConsoleColor.Yellow, $"WARNING: {message}");
            if (severity == 3) { _writeWithColor(ConsoleColor.Red, $"ERROR: {message}"); _errored = true; };
        }
    }
    internal class Position
    {
        internal int index;
        internal int lineIdx;
        internal int line;
        string text;
        public Position(string txt, int idx, int lnIdx, int ln)
        {
            index = idx;
            lineIdx = lnIdx;
            line = ln;
            text = txt;
        }
        internal void advance(char cChar)
        {
            ++this.index;
            ++this.lineIdx;

            /*if(cChar == "\n"){
                ++this.line;
                this.lineIdx = 0;
            }
            */
        }
        internal Position copy()
        {
            return new Position(text, index, lineIdx, line);
        }
    }
    internal class Lexer
    {
        internal static string digits = "0123456789";
        internal static string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal static List<string> _tokens = new List<string> { };
        internal static List<string> _ids = new List<string> { };
        internal static List<string> _numbers = new List<string> { };
        internal static List<string> _strings = new List<string> { };
        internal static List<string> _variables = new List<string> { };
        private readonly List<string> numberTypes = new List<string>() { "int8", "int16", "int", "int32", "int64", "float32", "float64" };
        private bool shouldLex = true;
        private char cChar;
        private char pChar;
        private Position pos;
        private void advance(string text, bool debug)
        {
            pChar = cChar;
            pos.advance(cChar);
            if (pos.index < text.Length) { cChar = text[pos.index]; if(debug)Console.WriteLine(cChar); }
            else shouldLex = false;
        }

        private void makeNumber(string text, bool debug)
        {
            string number = "";
            string digitsF = "0123456789.";
            bool isDecimal = false;
            Position positionStart = pos.copy();
            while (shouldLex && digitsF.Contains(cChar))
            {
                if (cChar == '.')
                {
                    if (isDecimal) { Message._throw(3, "Cannot have multipe decimal points in a float."); return; }
                    isDecimal = true;
                    number += ".";
                }
                else
                {
                    number += cChar;
                    advance(text, debug);
                }
            }
            if (debug) Console.WriteLine($"Added {number}");
            _variables.Add(number);
            _numbers.Add(number);
            _tokens.Add(number);
        }
        private void makeID(string text, bool debug)
        {
            string id = "";
            string lettersF = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            Position positionStart = pos.copy();
            while (shouldLex && lettersF.Contains(cChar.ToString()))
            {
                id += cChar;
                advance(text, debug);
            }
            if (debug) Console.WriteLine($"Added {id}");
            if (id == "int" || id == "float") { makeNumberType(id, text, debug); if (debug) Console.WriteLine("shifted to numberType"); }
            else { _ids.Add(id); _tokens.Add(id); }
        }
        private void makeNumberType(string t, string text, bool debug)
        {
            string digitsT = "123468";
            string type = t;
            while (shouldLex && digitsT.Contains(cChar.ToString()))
            {
                type += cChar;
                advance(text, debug);
            }
            if (!numberTypes.Contains(type)) Message._throw(3, "Invalid Type");
            if (!Message._errored) _tokens.Add(type);
        }
        private void makeString(string text, bool debug)
        {
            string str = "";
            Position positionStart = pos.copy();
            advance(text, debug);
            while (shouldLex && cChar != '"')
            {
                str += cChar;
                advance(text, debug);
            }
            advance(text, debug);
            if (debug) Console.WriteLine($"Added {str}");
            _variables.Add(str);
            _strings.Add(str);
            _tokens.Add(str);
        }
        private void makeOp(char type, string text, bool debug)
        {
            if (!shouldLex) return;
            if (type == '+')
            {
                advance(text, debug);
                if (cChar == '+') { _tokens.Add("++"); advance(text, debug); }
                else if (cChar == '=') { _tokens.Add("+="); advance(text, debug); }
                else { _tokens.Add("+"); }
            }
            if (type == '-')
            {
                advance(text, debug);
                if (cChar == '-') { _tokens.Add("--"); advance(text, debug); }
                else if (cChar == '=') { _tokens.Add("+="); advance(text, debug); }
                else _tokens.Add("-");
            }
            if (type == '*')
            {
                advance(text, debug);
                if (cChar == '=')
                {
                    advance(text, debug);
                    if (cChar == '=') { _tokens.Add("*=="); advance(text, debug); }
                    else _tokens.Add("*=");
                }
                else _tokens.Add("*");
            }
            if (type == '/')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add("/="); advance(text, debug); }
                else _tokens.Add("/");
            }
            if (type == '%')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add("%="); advance(text, debug); }
                else _tokens.Add("%");
            }
            if (type == '=')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add("=="); advance(text, debug); }
                else _tokens.Add("=");
            }
            if (type == '!')
            {
                advance(text, debug);
                if (cChar == '=')
                {
                    advance(text, debug);
                    if (cChar == '=') { _tokens.Add("!=="); advance(text, debug); }
                    else Message._throw(3, "Expected '==' after '!' ");
                }
                else Message._throw(3, "Expected '==' after '!' ");
            }
            if (type == '>')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add(">="); advance(text, debug); }
                else _tokens.Add(">");
            }
            if (type == '<')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add("<="); advance(text, debug); }
                else _tokens.Add(">");
            }
        }

        internal void Lex(string text, bool debug = false)
        {
            pos = new Position(text, -1, -1, 0);
            advance(text, debug);
            cChar = text[pos.index];
            pChar = text[pos.index];

            while (shouldLex && !Message._errored)
            {
                if (cChar == ' ') advance(text, debug);
                else if (digits.Contains(cChar)) makeNumber(text, debug);
                else if (letters.Contains(cChar)) makeID(text, debug);
                else if (cChar == '"') makeString(text,  debug);
                else if (cChar == '+') makeOp('+', text, debug);
                else if (cChar == '-') makeOp('-', text, debug);
                else if (cChar == '*') makeOp('*', text, debug);
                else if (cChar == '/') makeOp('/', text, debug);
                else if (cChar == '%') makeOp('%', text, debug);
                else if (cChar == '=') makeOp('=', text, debug);
                else if (cChar == '!') makeOp('!', text, debug);
                else if (cChar == '>') makeOp('>', text, debug);
                else if (cChar == '<') makeOp('<', text, debug);
                else if (cChar == '(') { _tokens.Add("("); advance(text, debug); }
                else if (cChar == ')') { _tokens.Add(")"); advance(text, debug); }
                else if (cChar == '[') { _tokens.Add("["); advance(text, debug); }
                else if (cChar == ']') { _tokens.Add("]"); advance(text, debug); }
                else if (cChar == '{') { _tokens.Add("{"); advance(text, debug); }
                else if (cChar == '}') { _tokens.Add("}"); advance(text, debug); }
                else { shouldLex = false; }
            }
        }
    }
    internal class Parser
    {
        internal static List<string> _nodes = new List<string> { };

        private List<string> aMods = new List<string>() { "global", "local", "embedded" };
        private List<string> mods = new List<string>() { "const", "simple", "unsigned" };
        private List<string> types = new List<string>() { "bool", "char", "string", "byte", "int8", "int16", "int", "int32", "int64", "float32", "float64" };
        private List<string> lManagers = new List<string>() { "break", "return", "continue" };
        private List<string> operators = new List<string>() { "+", "-", "*", "/", "%" };
        private List<string> specialToks = new List<string>() { "[", "]", "(", ")", "{", "}" };

        internal void MakeValueNode(string aMod, string mod, string type, string id, string all, string value = "null", string special = "null")
        {

        }
        internal void MakeEquationNode(string v1, string op, string v2)
        {

        }
        internal void MakeObjectNode() { } //soon (for things within curly braces)

        private void simpleError(int i, string x)
        {
            if (ErrorConfig.errorFlags[i] > 1) Message._throw(ErrorConfig.errorFlags[i], $"{ErrorConfig.errorNames[i]} at {x}");
        }

        private string all(List<string> strings)
        {
            StringBuilder allBuilder = new StringBuilder();
            foreach (string str in strings)
            {
                allBuilder.Append(str);
            }
            return (allBuilder.ToString());
        }
        int tokIdx = -1;
        private bool shouldParse()
        {
            return tokIdx < Lexer._tokens.Count;
        }
        private void advance()
        {
            if (shouldParse()) ++tokIdx;
        }
        private void makeValue(List<string> toks, bool debug)
        {
            List<string> strings = new List<string>() { };
            bool typeGot = false;
            bool hasValue = false;
            strings.Add(toks[tokIdx]);
            advance();
            if (mods.Contains(toks[tokIdx]) && shouldParse())
            {
                strings.Add(toks[tokIdx]);
                advance();
            }
            else if (types.Contains(toks[tokIdx]) && shouldParse())
            {
                strings.Add("simple");
                strings.Add(toks[tokIdx]);
                advance();
                typeGot = true;
            }
            else
            {
                Message._throw(3, "Expected type or modifier");
                return;
            }

            if (!typeGot && !Message._errored && shouldParse())
            {
                if (types.Contains(toks[tokIdx]))
                {
                    strings.Add(toks[tokIdx]);
                    advance();
                    typeGot = true;
                }
                else
                {
                    Message._throw(3, "Expected type");
                    return;
                }
            }

            if (shouldParse() && Lexer._ids.Contains(toks[tokIdx]))
            {
                strings.Add(toks[tokIdx]);
                advance();
            }
            else
            {
                Message._throw(3, "Expected ID");
                return;
            }

            if (shouldParse() && toks[tokIdx] == "=")
            {
                strings.Add(toks[tokIdx]);
                advance();
                hasValue = true;
            }
            else
            {
                MakeValueNode(strings[0], strings[1], strings[2], strings[3], all(strings));
                Message._throw(2, $"{strings[3]} is null");
                if (debug) Console.WriteLine($"Variable declared: {all(strings)}");
                return;
            }

            if (shouldParse() && hasValue && Lexer._variables.Contains(toks[tokIdx]))
            {
                strings.Add(toks[tokIdx]);
                MakeValueNode(strings[0], strings[1], strings[2], strings[3], all(strings), strings[5]);
                if (debug) Console.WriteLine($"Variable declared: {all(strings)}");
            }
            else
            {
                Message._throw(3, "Expected Value");
                return;
            }
        }
        private void makeEquation(List<string> toks, bool debug)
        {
            if (toks[tokIdx] == "(")
            {
                while (toks[tokIdx] != ")")
                {
                    advance();
                    if (!Lexer._numbers.Contains(toks[tokIdx])) { Message._throw(3, "Number Expected"); return; }
                    string num1 = toks[tokIdx];
                    advance();
                    if (!operators.Contains(toks[tokIdx])) { Message._throw(3, "Operator Expected"); return; }
                    string op = toks[tokIdx];
                    advance();
                    if (!Lexer._numbers.Contains(toks[tokIdx])) { Message._throw(3, "Number Expected"); return; }
                    MakeEquationNode(num1, op, toks[tokIdx]);
                    if(debug)Console.WriteLine($"Equation declared: ({num1} {op} {toks[tokIdx]})");
                    advance();
                }
                return;
            }
            if(Lexer.digits.Contains(toks[tokIdx]))
            {
                    string num1 = toks[tokIdx];
                    advance();
                    if (!operators.Contains(toks[tokIdx])) { Message._throw(3, "Operator Expected"); return; }
                    string op = toks[tokIdx];
                    advance();
                    if (!Lexer._numbers.Contains(toks[tokIdx])) { Message._throw(3, "Number Expected"); return; }
                    MakeEquationNode(num1, op, toks[tokIdx]);
                    if(debug)Console.WriteLine($"Equation declared: {num1} {op} {toks[tokIdx]}");
                
            }
        }

        internal void Parse(List<string> toks, bool debug = false)
        {
            while (shouldParse())
            {
                advance();
                if (shouldParse() && aMods.Contains(toks[tokIdx])) { makeValue(toks, debug); }
                else if (shouldParse() && (toks[tokIdx] == "(")) { makeEquation(toks, debug); }
                else if (shouldParse() && Lexer.digits.Contains(toks[tokIdx])) { makeEquation(toks, debug); }
            }
        }
    };
}

