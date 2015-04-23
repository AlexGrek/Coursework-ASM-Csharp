using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    class Line
    {
        public Instruction Content;
        public string Error;
        public string Input;

        public bool IsInstruction() {
            return Content != null;
        }

        public Line(string cont, string inp)
        {
            Error = cont;
            Input = inp;
        }

        public Line(Instruction cont, string inp)
        {
            Content = cont;
            Input = inp;
        }

        public override string ToString()
        {
            if (Content != null)
                return Content.ToString();
            else return Error;
        }
    }
}
