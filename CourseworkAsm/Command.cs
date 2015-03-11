using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CourseworkAsm
{
    abstract class Command : Instruction
    {
        public static Regex r16 = new Regex("^((a|b|c|d)x|bp|sp|si|di)$");
        public static Regex r32 = new Regex("^(e(a|b|c|d)x|ebp|esp|esi|edi)$");
        public static Regex segment = new Regex(@"^(e|c|s|d|f|g|)s:");
        public static Regex addr16 = new Regex(@"^[a-zA-Z_]\w*\[bx\+di]");

        private string _seg;

        public void DetectSeg(ref string arg) 
        {
            var m = segment.Match(arg);
            if (m.Success)
            {
                _seg = m.ToString();
                arg = arg.Replace(_seg, "");
            }
        }

        public Command(Label l)
        {
            Label = l;
        }

        public static Command Matches(string s, Label l)
        {
            if (s.StartsWith("nop")) 
            {
                return new Nop(s, l);
            }
            if (s.StartsWith("jl"))
            {
                return new Jl(s, l);
            }
            if (s.StartsWith("jmp"))
            {
                return new Jmp(s, l);
            }
            if (s.StartsWith("sbb"))
            {
                return new Sbb(s, l);
            }
            if (s.StartsWith("shl"))
            {
                return new Shl(s, l);
            }
            if (s.StartsWith("mul"))
            {
                return new Mul(s, l);
            }
            if (s.StartsWith("not"))
            {
                return new Not(s, l);
            }
            if (s.StartsWith("add"))
            {
                return new Add(s, l);
            }
            return null;
        }

        public override string ToString()
        {
            return IsError ? Error : this.GetType().ToString();
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
            if (s != "nop")
            {
                Error = "Error parsing NOP command";
            }
        }
    }

    class Sbb : Command
    {
        DataType _type;


        public Sbb(string s, Label l) : base(l) 
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
            else
            {
                Error = "Wrong first argument";
                return;
            }
           
        }

        public override string ToString()
        {
            return base.ToString() + (IsError ? "" : " Type: " + _type);
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
            else
            {
                Error = "Wrong first argument";
                return;
            }

            //проверить второй аргумент - он может быть только cl
            if (args[1] != "cl")
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

        public Add(string s, Label l) : base(l)
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
            else
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

    class Not : Command
    {
        DataType _type;

        public Not(string s, Label l)
            : base(l)
        {
            var args = SplitArgs(s, 3);
            if (args.Length != 1)
            {
                Error = "Wrong arguments count";
                return;
            }

            
        }
    }
    
}
