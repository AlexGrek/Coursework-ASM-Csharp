using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    abstract class Instruction
    {
        public abstract int Length { get;}

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
    }
}
