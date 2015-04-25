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

        internal Dictionary<string, Label> Labels
        {
            get { return _labels; }
        }
        private Dictionary<string, Variable> _vars;
        public int CurrentOffset = 0;

        public string Name
        {
            get { return _segName; }
        }

        public void AddLabel(Label l)
        {
            _labels.Add(l.Name, l);
        }

        public static Regex segStart = new Regex(@"^[_a-zA-Z][a-zA-Z\d]*(?=\s+segment\s*$)", RegexOptions.IgnoreCase);
        public static Regex segEnds = new Regex(@"^[a-zA-Z][a-zA-Z\d]*(?=\s+ends\s*$)", RegexOptions.IgnoreCase);

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
            _vars = new Dictionary<string, Variable>();
        }

        public void Add(Instruction inst)
        {
            _instructions.Add(inst);
            var v = inst as Variable;
            if (v != null)
            {
                _vars.Add(v.Name, v);
            }
        }

        public void Add(Label l)
        {
            _labels.Add(l.Name, l);
        }

        public bool ContainsVar(string name)
        {
            return _vars.ContainsKey(name);
        }

        public Variable GetVar(string name)
        {
            if (ContainsVar(name))
                return _vars[name];
            return null;
        }
    }
}
