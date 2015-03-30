using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CourseworkAsm
{
    abstract class Command : Instruction
    {
        public static Regex r16 = new Regex("^((a|b|c|d)x|bp|sp|si|di)$", RegexOptions.IgnoreCase);
        public static Regex r8 = new Regex("^((a|b|c|d)(h|l))$", RegexOptions.IgnoreCase);
        public static Regex r32 = new Regex("^(e(a|b|c|d)x|ebp|esp|esi|edi)$", RegexOptions.IgnoreCase);
        public static Regex segment = new Regex(@"^(e|c|s|d|f|g|)s:", RegexOptions.IgnoreCase);
        public static Regex addr16ds = new Regex(@"^[a-zA-Z_]\w*(?=\[bx\+(d|s)i\]$)", RegexOptions.IgnoreCase);
        public static Regex addr16ss = new Regex(@"^[a-zA-Z_]\w*(?=\[bp\+(d|s)i\]$)", RegexOptions.IgnoreCase);
        public static Regex addr32ds = new Regex(@"^[a-zA-Z_]\w*(?=\[e([abcd]x|bp|[sd]i)\+e([abcd]x|[sb]p|[sd]i)\]$)", RegexOptions.IgnoreCase);
        public static Regex addr2r = new Regex(@"(?<=\[\s*(b[xp]|e([abcd]x|bp|[sd]i))\s*\+\s*)\w{2,3}(?=\s*\])", RegexOptions.IgnoreCase);
        public static Regex addr32base = new Regex(@"(?<=\[\s*)\w+(?=\s*\+)", RegexOptions.IgnoreCase);

        internal string _seg;
        private bool _explicitSegChange;

        public bool DetectSeg(ref string arg) 
        {
            var m = segment.Match(arg);
            if (m.Success)
            {
                _seg = m.ToString().Substring(0, 2).ToLower();
                arg = arg.Replace(m.ToString(), "");
                _explicitSegChange = true;
            }
            return m.Success;
        }

        public Command(Label l)
        {
            Label = l;
        }

        public bool ParseVarArg(string s, Assume a, out bool addr32, out Variable arg, out string reg1, out string reg2)
        {
            addr32 = false;
            Match m;
            var sOverride = _seg;
            arg = null;
            reg1 = reg2 = null;

            //var[bx+...]
            m = addr16ds.Match(s);
            if (m.Success)
            {
                reg1 = "bx";
                reg2 = addr2r.Match(s).ToString();
                var name = m.ToString();
                var varSegment = a.WhatSegment(name);
                if (_seg == null)
                    _seg = varSegment;
                if (varSegment == null)
                    return false;
                arg = a.GetVariable(name);
                return true;
            }

            //var[bp+...]
            m = addr16ss.Match(s);
            if (m.Success)
            {
                reg1 = "bp";
                reg2 = addr2r.Match(s).ToString();
                var name = m.ToString();
                var varSegment = a.WhatSegment(name);
                if (!_explicitSegChange)
                {
                    _seg = varSegment;
                    if (_seg == "ds")
                        _seg = "ss";
                }
                if (varSegment == null)
                    return false;
                arg = a.GetVariable(name);
                return true;
            }

            //var[32bit]
            m = addr32ds.Match(s);
            if (m.Success)
            {
                reg1 = addr32base.Match(s).ToString();
                reg2 = addr2r.Match(s).ToString();
                var name = m.ToString();
                var varSegment = a.WhatSegment(name);
                if (varSegment == null)
                    return false;
                if (!_explicitSegChange) {
                    if (reg1.ToLower() == "ebp")
                    {
                        if (varSegment == "ds")
                            _seg = "ss";
                        else
                            _seg = varSegment;
                    }
                    else
                    {
                       _seg = varSegment;
                    }

                }
                arg = a.GetVariable(name);
                return true;
            }

            return false;
        }

        public static Command Matches(string s, Label l, Assume a)
        {
            var sl = s.ToLower();
            if (sl.StartsWith("nop")) 
            {
                return new Nop(s, l);
            }
            if (sl.StartsWith("jl"))
            {
                return new Jl(s, l);
            }
            if (sl.StartsWith("jmp"))
            {
                return new Jmp(s, l);
            }
            if (sl.StartsWith("sbb"))
            {
                return new Sbb(s, l, a);
            }
            if (sl.StartsWith("shl"))
            {
                return new Shl(s, l);
            }
            if (sl.StartsWith("mul"))
            {
                return new Mul(s, l);
            }
            if (sl.StartsWith("not"))
            {
                return new Not(s, l, a);
            }
            if (sl.StartsWith("add"))
            {
                return new Add(s, l, a);
            }
            return null;
        }

        public override string ToString()
        {
            return IsError ? Error : (this.GetType().ToString() + ((_seg == null) ? " noseg " : (" seg: " + _seg)));
        }

        public static string[] SplitArgs(string s, int commandLen)
        {
            s = s.Remove(0, commandLen + 1).Trim();
            return s.Split(new string[1] {", "}, StringSplitOptions.RemoveEmptyEntries);
        }

        public override int Length
        {
            get { throw new NotImplementedException(); }
        }
    }

    class Nop : Command
    {
        public Nop(string s, Label l) : base(l) 
        {
            if (s.ToLower() != "nop")
            {
                Error = "Error parsing NOP command";
            }
        }

        public override int Length
        {
            get
            {
                return 1;
            }
        }
    }

    class Sbb : Command
    {
        DataType _type;
        Variable _arg;
        String reg1, reg2;
        bool a32; //32-битная адресация

        public Sbb(string s, Label l, Assume a) : base(l) 
        {
            var args = SplitArgs(s, 3);
            if (args.Length != 2)
            {
                Error = "Wrong arguments count";
                return;
            }

            // проверить первый аргумент
            if (r16.Match(args[0]).Success)
            {
                _type = DataType.Word;
            }
            else if (r32.Match(args[0]).Success)
            {
                _type = DataType.Dword;
            }
            else if (r8.Match(args[0]).Success)
            {
                _type = DataType.Byte;
            }
            else
            {
                Error = "Wrong first argument";
                return;
            }

            DetectSeg(ref args[1]);

            //обработать второй аргумент
            if (ParseVarArg(args[1], a, out a32, out _arg, out reg1, out reg2))
            {
                if (_arg.Type != _type)
                    Error = "Type mismatch: " + _arg.Type + " and " + _type;
            }
            else Error = "Wrong second arg: " + args[1];
           
        }

        public override string ToString()
        {
            return base.ToString() + (IsError ? "" : " Type: " + _type) + " regs:" + reg1 + "+" + reg2;
        }
    }

    class Shl : Command
    {
        DataType _type;

        public Shl(string s, Label l)
            : base(l)
        {
            var args = SplitArgs(s, 3);
            if (args.Length != 2)
            {
                Error = "Wrong arguments count";
                return;
            }

            // проверить первый аргумент
            if (r16.Match(args[0]).Success)
            {
                _type = DataType.Word;
            }
            else if (r32.Match(args[0]).Success)
            {
                _type = DataType.Dword;
            }
            else if (r8.Match(args[0]).Success)
            {
                _type = DataType.Byte;
            }
            else
            {
                Error = "Wrong first argument";
                return;
            }

            //проверить второй аргумент - он может быть только cl
            if (args[1].ToLower() != "cl")
            {
                Error = "Wrong second argument";
                return;
            }
        }

        public override string ToString()
        {
            return base.ToString() + (IsError ? "" : " Type: " + _type);
        }
    }

    class Mul : Command
    {
        DataType _type;

        public Mul(string s, Label l)
            : base(l)
        {
            var args = SplitArgs(s, 3);
            if (args.Length != 1)
            {
                Error = "Wrong arguments count";
                return;
            }

            // проверить аргумент
            if (r16.Match(args[0]).Success)
            {
                _type = DataType.Word;
            }
            else if (r32.Match(args[0]).Success)
            {
                _type = DataType.Dword;
            }
            else if (r8.Match(args[0]).Success)
            {
                _type = DataType.Byte;
            }
            else
            {
                Error = "Wrong argument";
                return;
            }
        }

        public override string ToString()
        {
            return base.ToString() + (IsError ? "" : " Type: " + _type);
        }
    }

    class Add : Command
    {
        DataType _type;
        Variable _arg;
        String reg1, reg2;
        bool a32; //32-битная адресация

        public Add(string s, Label l, Assume a) : base(l)
        {
            var args = SplitArgs(s, 3);
            if (args.Length != 2)
            {
                Error = "Wrong arguments count";
                return;
            }

            // проверить второй аргумент
            if (r16.Match(args[1]).Success)
            {
                _type = DataType.Word;
            }
            else if (r32.Match(args[1]).Success)
            {
                _type = DataType.Dword;
            }
            else if (r8.Match(args[1]).Success)
            {
                _type = DataType.Byte;
            }
            else
            {
                Error = "Wrong second argument";
                return;
            }

            DetectSeg(ref args[0]);

            //обработать первый аргумент
            if (ParseVarArg(args[0], a, out a32, out _arg, out reg1, out reg2))
            {
                if (_arg.Type != _type)
                    Error = "Type mismatch: " + _arg.Type + " and " + _type;
            }
            else Error = "Wrong first arg: " + args[0];
        }

        public override string ToString()
        {
            return base.ToString() + (IsError ? "" : " Type: " + _type);
        }
    }

    class Not : Command
    {
        DataType _type;
        Variable _arg;
        String reg1, reg2;
        bool a32; //32-битная адресация

        public Not(string s, Label l, Assume a)
            : base(l)
        {
            var args = SplitArgs(s, 3);
            if (args.Length != 1)
            {
                Error = "Wrong arguments count";
                return;
            }

            DetectSeg(ref args[0]);

            //обработать первый аргумент
            if (ParseVarArg(args[0], a, out a32, out _arg, out reg1, out reg2))
            {
                if (_arg.Type != _type)
                    Error = "Type mismatch: " + _arg.Type + " and " + _type;
            }
            else Error = "Wrong first arg: " + args[0];
        }
    }
    
}
