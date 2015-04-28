using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    abstract class AdressCommand : Command
    {

        internal Variable _arg;
        internal String reg1, reg2;
        internal bool a32; //32-битная адресация
        internal string _reg;

        public bool Address32
        {
            get
            {
                return a32;
            }
        }

        public byte SegPrefix
        {
            get
            {
                if (this._defSeg != this._targetSeg)
                {
                    return Regs.SegRegPrefix[_targetSeg[0]];
                }
                else return 0;
            }
        }

        public override string GetBytes()
        {
            StringBuilder s = new StringBuilder();
            if (_bytes == null)
                _bytes = new byte[this.Length];
            if (SegPrefix != 0)
                s.AppendFormat("{0:X}: ", SegPrefix);
            if (a32)
                s.Append("67| ");
            if (Data32)
                s.Append("66| ");
            foreach (byte b in _bytes)
            {
                if (b > 16)
                    s.AppendFormat("{0:X} ", b);
                else
                    s.AppendFormat("0{0:X} ", b);
            }
            s.Append("  (" + this.Length + ") ");
            return s.ToString();
        }

        public override void Render()
        {
            if (IsError)
                throw new AssemblerException(Error, this);
            _bytes = new byte[a32 ? 7 : 4];
        }

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
                if (this._defSeg != this._targetSeg)
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

            _reg = args[1].ToLower();

            DetectSeg(ref args[0]);

            //обработать первый аргумент
            if (ParseVarArg(args[0], a, out a32, out _arg, out reg1, out reg2))
            {
                if (_arg.Type != _type)
                    Error = "Type mismatch: " + _arg.Type + " and " + _type;
            }
            else Error = "Wrong first arg: " + args[0];
        }

        public override void Render()
        {
            base.Render();

            //код команды
            if (_type == DataType.Byte)
                _bytes[0] = 0x00;
            else
                _bytes[0] = 0x01;

            if (!a32) //16-разрядная адресация
            {
                byte modrm = 128; //mod = 10
                //reg
                switch (_type)
                {
                    case DataType.Dword:
                        modrm ^= (byte)(Regs.Reg32[_reg] << 3);
                        break;
                    case DataType.Byte:
                        modrm ^= (byte)(Regs.Reg8[_reg] << 3);
                        break;
                    case DataType.Word:
                        modrm ^= (byte)(Regs.Reg16[_reg] << 3);
                        break;
                }
                modrm ^= Regs.Addr16[reg1 + reg2];

                _bytes[1] = modrm;

                var t = BitConverter.GetBytes((Int16)_arg.Offset);

                InsertBytes(t, _bytes, 2, true);
            }
            else //32-разрядная адресация
            {
                byte modrm = 0x84; // = 10(mod) 000(reg) 100(SIB)
                //reg
                switch (_type)
                {
                    case DataType.Dword:
                        modrm ^= (byte)(Regs.Reg32[_reg] << 3);
                        break;
                    case DataType.Byte:
                        modrm ^= (byte)(Regs.Reg8[_reg] << 3);
                        break;
                    case DataType.Word:
                        modrm ^= (byte)(Regs.Reg16[_reg] << 3);
                        break;
                }
                _bytes[1] = modrm;

                //байт sib
                byte sib = 0;
                sib ^= Regs.Reg32[reg1]; //00 000 reg
                sib ^= (byte)(Regs.Reg32[reg2] << 3); //00 reg 000
                _bytes[2] = sib;

                var t = BitConverter.GetBytes((Int32)_arg.Offset);
                InsertBytes(t, _bytes, 3, true);
            }
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

            _reg = args[0].ToLower();

            DetectSeg(ref args[1]);

            //обработать второй аргумент
            if (ParseVarArg(args[1], a, out a32, out _arg, out reg1, out reg2))
            {
                if (_arg.Type != _type)
                    Error = "Type mismatch: " + _arg.Type + " and " + _type;
            }
            else Error = "Wrong second arg: " + args[1];

        }

        public override void Render()
        {
            base.Render();

            //код команды
            if (_type == DataType.Byte)
                _bytes[0] = 0x1A;
            else
                _bytes[0] = 0x1B;

            if (!a32) //16-разрядная адресация
            {
                byte modrm = 128; //mod = 10
                //reg
                switch (_type)
                {
                    case DataType.Dword:
                        modrm ^= (byte)(Regs.Reg32[_reg] << 3);
                        break;
                    case DataType.Byte:
                        modrm ^= (byte)(Regs.Reg8[_reg] << 3);
                        break;
                    case DataType.Word:
                        modrm ^= (byte) (Regs.Reg16[_reg] << 3);
                        break;
                }
                modrm ^= Regs.Addr16[reg1 + reg2];

                _bytes[1] = modrm;

                var t = BitConverter.GetBytes((Int16)_arg.Offset);

                InsertBytes(t, _bytes, 2, true);
            }
            else //32-разрядная адресация
            {
                byte modrm = 0x84; // = 10(mod) 000(reg) 100(SIB)
                //reg
                switch (_type)
                {
                    case DataType.Dword:
                        modrm ^= (byte)(Regs.Reg32[_reg] << 3);
                        break;
                    case DataType.Byte:
                        modrm ^= (byte)(Regs.Reg8[_reg] << 3);
                        break;
                    case DataType.Word:
                        modrm ^= (byte)(Regs.Reg16[_reg] << 3);
                        break;
                }
                _bytes[1] = modrm;

                //байт sib
                byte sib = 0;
                sib ^= Regs.Reg32[reg1]; //00 000 reg
                sib ^= (byte)(Regs.Reg32[reg2] << 3); //00 reg 000
                _bytes[2] = sib;

                var t = BitConverter.GetBytes((Int32)_arg.Offset);
                InsertBytes(t, _bytes, 3, true);
            }
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

        public override void Render()
        {
            base.Render();

            //код команды
            if (_type == DataType.Byte)
                _bytes[0] = 0xF6;
            else
                _bytes[0] = 0xF7;

            if (!a32) //16-разрядная адресация
            {
                byte modrm = 0x90; // = 10 010 000
                modrm ^= Regs.Addr16[reg1 + reg2];
                _bytes[1] = modrm;

                var t = BitConverter.GetBytes((Int16)_arg.Offset);

                InsertBytes(t, _bytes, 2, true);
            }
            else //32-разрядная адресация
            {
                byte modrm = 0x94; // = 10(mod) 010(reg) 100(SIB)
                _bytes[1] = modrm;

                //байт sib
                byte sib = 0;
                sib ^= Regs.Reg32[reg1]; //00 000 reg
                sib ^= (byte)(Regs.Reg32[reg2] << 3); //00 reg 000
                _bytes[2] = sib;

                var t = BitConverter.GetBytes((Int32)_arg.Offset);
                InsertBytes(t, _bytes, 3, true);
            }
        }
    }

    #endregion

}
