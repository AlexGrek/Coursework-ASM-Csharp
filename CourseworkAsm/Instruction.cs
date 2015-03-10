using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    abstract class Instruction
    {
        public abstract int Length { get;}

        internal byte[] _bytes;

        public bool IsError 
        {
            get { return !string.IsNullOrEmpty(_error); }
        }

        private string _error = null;

        public string Error
        {
            get { return _error; }
            set { _error = value; }
        }

        public Label Label { get; set;}

    }

    class Empty : Instruction
    {
        private string _contents;

        public Empty(string s, Label l)
        {
            _contents = s;
            Label = l;
        }

        public override string ToString()
        {
            if (Label != null && _contents == "")
                return "Label: " + Label.Name;
            return _contents;
        }

        public override int Length
        {
            get { return 0; }
        }
    }
}
