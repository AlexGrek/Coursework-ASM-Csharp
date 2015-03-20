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
        private Dictionary<string, Segment> _assumedSegs;

        public Assume()
        {
            _assumedSegs = new Dictionary<string, Segment>(6);
        }

        public bool TryAdd(string reg, string seg, Dictionary<string, Segment> segs)
        {
            try
            {
                _assumedSegs.Add(reg.ToLower(), segs[seg]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string WhatSegment(string varName)
        {
            //искать во всех определенных сегментах
            foreach (var pair in _assumedSegs)
            {
                if (pair.Value.ContainsVar(varName))
                    return pair.Key;
            }

            //если переменная не была найдена
            return null;
        }

        public Variable GetVarFrom(string seg, string name)
        {
            try
            {
                return _assumedSegs[seg].GetVar(name);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Variable GetVariable(string name)
        {
            //искать во всех определенных сегментах
            foreach (var pair in _assumedSegs)
            {
                if (pair.Value.ContainsVar(name))
                    return pair.Value.GetVar(name);
            }

            //если переменная не была найдена
            return null;
        }

        static Regex segName = new Regex(@"(?<=^[csedgf]s:\s*)[_a-zA-Z][a-zA-Z\d]*", RegexOptions.IgnoreCase);

        public static Assume Matches(string s, Dictionary<string, Segment> segs)
        {
            var args = Command.SplitArgs(s, 6);
            var assm = new Assume();

            foreach (string str in args)
            {
                if (segName.Match(str).Success)
                {
                    var name = segName.Match(str).ToString();

                    if (!assm.TryAdd(str.Substring(0, 2), segName.Match(str).ToString(), segs))
                        return null;
                }
                else return null;
            }

            return assm;
        }
    }
}
