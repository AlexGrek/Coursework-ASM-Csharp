using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    abstract class AdressCommand : Command
    {

        internal DataType _type;
        internal Variable _arg;
        internal String reg1, reg2;
        internal bool a32; //32-битная адресация

        public AdressCommand(Label l) : base(l) { }

        public override int Length
        {
            get
            {
                if (_len != 0)
                    return _len;
                int len = 4;
                if (a32) //учтем режим 32-разрядной адресации
                    len = 8;
                if (this._seg != "ds" || this._explicitSegChange)
                    len++; //учтем возможный префикс замены сегмента
                if (this._type == DataType.Dword)
                    len++; //и префикс замены разрядности данных
                _len = len;
                return len;
            }
        }
    }

    #region commands

    class Add : AdressCommand
    {

        public Add(string s, Label l, Assume a)
            : base(l)
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

    class Sbb : AdressCommand
    {

        public Sbb(string s, Label l, Assume a)
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

    class Not : AdressCommand
    {

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

            //обработать аргумент
            if (!ParseVarArg(args[0], a, out a32, out _arg, out reg1, out reg2))
                Error = "Wrong argument: " + args[0];
            else
                _type = _arg.Type;
        }

    }

    #endregion

}
