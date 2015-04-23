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
        static Regex name = new Regex(@"^[a-zA-Z_]\w*(?=\s+d[bwd])", RegexOptions.IgnoreCase);
        Regex data = new Regex(@"(?<=\sd[dwb]\s+)-?\d[\dabcdef]*[hbd]?", RegexOptions.IgnoreCase);
        Regex stringData = new Regex(@"(?<=db\s*)'.*'", RegexOptions.IgnoreCase);
        Regex type = new Regex(@"\sd[bwd]\s", RegexOptions.IgnoreCase);

        string _name;

        public string Name
        {
            get { return _name; }
        }
        DataType _type;

        public DataType Type
        {
            get { return _type; }
        }

        int _len = 0;

        public Segment Seg { get; private set; }

        public override int Length
        {
            get { return _len; }
        }

        public Variable(string s, Label l, Segment seg)
        {
            Label = l;

            Seg = seg;

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
                switch (typeMatch.ToString().ToLower()[2])
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
                    default:
                        throw new Exception("Aaaaaaaaa!");
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
                var raw = numericMatch.ToString().ToLower();
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

            if (Seg == null)
            {
                Error = "No segment: " + s;
                return;
            }
            
        }

        public override string ToString()
        {
            if (IsError)
                return Error;
            StringBuilder ans = new StringBuilder();
            /*foreach (var b in _bytes)
            {
                ans.AppendFormat("{0:X} ", b);
            }*/
            ans.AppendFormat("Var name: '{2}', type: {0}, length: {1}, segment: {3}", _type, _len, Name, Seg.Name);
            return ans.ToString();
        }

        public static Variable Matches(string s, Label l, Segment seg)
        {
            if (name.Match(s).Success)
            {
                return new Variable(s, l, seg);
            }
            else return null;
        }
    }

    public enum DataType {
        Byte,
        Word,
        Dword
    }
}
