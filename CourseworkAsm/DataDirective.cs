using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    class Variable : Instruction
    {
        Regex name = new Regex(@"^[a-zA-Z][a-zA-Z\d]*(?=\s*d[bwd])");
        Regex data = new Regex(@"(?<=d[dwb]\s*)[-\d][\dabcdef]*[hbd]?");
        Regex stringData = new Regex(@"(?<=db\s*)'.*'");
        Regex type = new Regex(@"\sd[bwd]\s");

        string _name;
        DataType _type;
        int _len = 0;

        byte[] _bytes;

        public override int Length
        {
            get { return _len; }
        }

        public Variable(string s)
        {
            var varName = name.Match(s);
            if (varName.Success)
            {
                _name = varName.ToString();
            }
            else
            {
                Error = "Cannot detect variable name: " + s;
                return;
            }

            var strMatch = stringData.Match(s);
            if (strMatch.Success)
            {
                var str = strMatch.ToString().Substring(1, strMatch.Length - 2);
                _bytes = System.Text.Encoding.ASCII.GetBytes(str);
                _len = _bytes.Length;
                _type = DataType.Byte;
                return;
            }

            var typeMatch = type.Match(s);
            if (typeMatch.Success)
            {
                switch (typeMatch.ToString()[2])
                {
                    case 'b':
                        _len = 1;
                        _type = DataType.Byte;
                        break;
                    case 'w':
                        _len = 2;
                        _type = DataType.Word;
                        break;
                    case 'd':
                        _len = 4;
                        _type = DataType.Dword;
                        break;
                }
            }
            else
            {
                Error = "Cannot detect variable type: " + s;
                return;
            }

            var numericMatch = data.Match(s);
            if (numericMatch.Success)
            {
                var raw = numericMatch.ToString();
                try
                {
                    Parser.ParseConstant(raw, _type, out _bytes);
                }
                catch (Exception ex)
                {
                    Error = "Cannot parse numeric: " + ex.ToString();
                    return;
                }
            }
            else
            {
                Error = "Cannot detect variable data: " + s;
                return;
            }
            
        }

        public override string ToString()
        {
            if (IsError)
                return Error;
            StringBuilder ans = new StringBuilder();
            foreach (var b in _bytes)
            {
                ans.AppendFormat("{0:X} ", b);
            }
            ans.AppendFormat(" type: {0}, length: {1}", _type, _len);
            return ans.ToString();
        }
    }

    public enum DataType {
        Byte,
        Word,
        Dword
    }
}
