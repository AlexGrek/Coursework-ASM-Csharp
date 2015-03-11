using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    class Assume
    {
        private Dictionary<string, string> _assumedSegs;

        public Assume()
        {
            _assumedSegs = new Dictionary<string, string>(6);
        }

        public bool TryAdd(string reg, string seg)
        {
            try
            {
                _assumedSegs.Add(reg, seg);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static Regex segName = new Regex(@"(?<=^[csedgf]s:\s*)[_a-zA-Z][a-zA-Z\d]*");

        public static Assume Matches(string s)
        {
            var args = Command.SplitArgs(s, 6);
            var assm = new Assume();

            foreach (string str in args)
            {
                if (segName.Match(str).Success)
                {
                    if (!assm.TryAdd(str.Substring(0, 2), segName.Match(str).ToString()))
                        return null;
                }
                else return null;
            }

            return assm;
        }
    }
}
