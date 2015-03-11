using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    class Segment
    {
        private List<Instruction> _instructions;
        private string _segName;
        private Dictionary<string, Label> _labels;

        public string Name
        {
            get { return _segName; }
        }

        public static Regex segStart = new Regex(@"^[_a-zA-Z][a-zA-Z\d]*(?=\s+segment$)");
        public static Regex segEnds = new Regex(@"^[a-zA-Z][a-zA-Z\d]*(?=\s+ends$)");

        internal List<Instruction> Instructions
        {
            get { return _instructions; }
            set { }
        }

        public Segment(string name)
        {
            _segName = name;
            _instructions = new List<Instruction>();
            _labels = new Dictionary<string, Label>();
        }

        public void Add(Instruction inst)
        {
            _instructions.Add(inst);
        }

        public void Add(Label l)
        {
            _labels.Add(l.Name, l);
        }
    }
}
