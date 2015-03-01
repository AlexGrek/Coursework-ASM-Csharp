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

            return "Error";
        }
    }
}
