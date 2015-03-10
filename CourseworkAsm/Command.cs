using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CourseworkAsm
{
    abstract class Command : Instruction
    {
        public static Regex r16 = new Regex("^((a|b|c|d)x|bp|sp|si|di)$");
        public static Regex r32 = new Regex("^(e(a|b|c|d)x|ebp|esp|esi|edi)$");

        public Command(Label l)
        {
            Label = l;
        }

        public static Command Match(string s, Label l)
        {
            if (s.StartsWith("nop")) 
            {
                return new Nop(s, l);
            }
            
            return null;
        }

        public static string[] SplitArgs(string s)
        {
            return s.Split(new char[3] {' ', ',', '\t'}, StringSplitOptions.RemoveEmptyEntries);
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
        public Sbb(string s, Label l) : base(l) 
        {
            var args = SplitArgs(s);
            if (args.Length > 3)
            {
                Error = "Wrong arguments count";
            }

            var dict = new Dictionary<string,int>
         {
            ["one"]=1,
            ["two"]=2,
            ["three"]=3
         };
        }


    }

    
}
