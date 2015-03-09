using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    class Parser
    {
        private string[] _strings;

        Regex segStart = new Regex(@"(!?;)[a-zA-Z][a-zA-Z\d]*(?=\ssegment)");
        Regex segEnds = new Regex(@"[a-zA-Z][a-zA-Z\d]*(?=\sends)");
        Regex dataDirective = new Regex(@"^[a-zA-Z][a-zA-Z\d]*\s*d[bdw]"); 

        public Parser(string[] strings)
        {
            _strings = strings;
        }

        public void Parse()
        {
            foreach (var s in _strings)
            {
                Console.WriteLine(AnalyseString(s));
            }
        }

        public string AnalyseString(string s)
        {
            var sl = s.ToLower();
            if (segStart.Match(sl).Success)
            {
                return "Segment " + segStart.Match(sl).Value + " started.";
            }
            if (segEnds.Match(sl).Success)
            {
                return "Segment " + segEnds.Match(sl).Value + " ends.";
            }
            if (dataDirective.Match(sl).Success)
            {
                var Var = new Variable(s);
                return Var.ToString() ;
            }

            return "Error";
        }

        public static void ParseConstant(string r, DataType _type, out byte[] _bytes)
        {
            int b = 10;
            bool neg = false;

            if (r.StartsWith("-"))
            {
                neg = true;
                r = r.Substring(1);
            }

            if (r.EndsWith("h"))
            {
                b = 16;
                r = r.Substring(0, r.Length - 1);
            }
            else if (r.EndsWith("b"))
            {
                b = 2;
                r = r.Substring(0, r.Length - 1);
            }
            else if (r.EndsWith("d"))
            {
                r = r.Substring(0, r.Length - 1);
            }

            if (_type == DataType.Byte)
            {
                byte bt = Convert.ToByte(r, b);
                _bytes = new byte[1];
                if (neg)
                    bt = (byte)-bt;
                _bytes[0] = bt;
            } else
            if (_type == DataType.Word)
            {
                short bt = Convert.ToInt16(r, b);
                if (neg)
                    bt = (short)-bt;
                _bytes = BitConverter.GetBytes(bt);
                Array.Reverse(_bytes);
            } else
            {
                int bt = Convert.ToInt32(r, b);
                _bytes = BitConverter.GetBytes((Int32)(neg ? -bt : bt));
            }

            
        }
    }
}
