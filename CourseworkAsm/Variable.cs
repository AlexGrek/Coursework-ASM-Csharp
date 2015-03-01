using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    class Variable
    {
        private readonly string _name;
        private int size;
        private int offset;
        private Segment seg;

        public Variable(string name, int size, int offset, Segment seg)
        {
            _name = name;
            this.size = size;
            this.offset = offset;
            this.seg = seg;
        }

        public int Offset
        {
            get { return offset; }
        }

        public int Size
        {
            get { return size; }
        }

        public string Name
        {
            get { return _name; }
        }

        public Segment Segment
        {
            get { return seg; }
        }
    }
}
