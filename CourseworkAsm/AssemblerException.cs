using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    class AssemblerException : Exception
    {
        private string _exText;
        Instruction _bad;

        internal Instruction Bad
        {
            get { return _bad; }
        }

        public AssemblerException(string text, Instruction bad)
        {
            _bad = bad;
            _exText = text;
        }

        public override string ToString()
        {
            return _exText;
        }
    }
}
