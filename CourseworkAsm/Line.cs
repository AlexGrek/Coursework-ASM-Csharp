using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    class Line
    {
        public object Content;
        public string Input;

        public Line(object cont, string inp)
        {
            Content = cont;
            Input = inp;
        }

        public override string ToString()
        {
            return Content.ToString();
        }
    }
}
