using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CourseworkAsm
{
    class Parser
    {
        private string[] _strings; //входной массив строк
        private Dictionary<string, Segment> _segments;    //список сегментов
        private Segment _currentSeg;    //текущий сегмент
        private Dictionary<string, Variable> _vars; //таблица переменных
        private Assume _assume;
        private bool _endfile = false;
        private List<Line> _lines;
        private StreamWriter _writer;


        public Parser(string[] strings)
        {
            _strings = strings;
            _lines = new List<Line>(_strings.Length);
        }

        public void Parse()
        {
            _segments = new Dictionary<string,Segment>();
            _vars = new Dictionary<string, Variable>();
            foreach (var s in _strings)
            {
                var line = AnalyseString(s);
                _lines.Add(line);
                Console.WriteLine(line.ToString() + " \t\t " + s);
            }
        }

        public void ShowLst(StreamWriter to)
        {
            _writer = to;

            Out(String.Format("Alex Grek's Assembler, {0}", DateTime.Now.ToString()));
            int n = 1;
            foreach (Line line in _lines)
            {
                if (line.IsInstruction())
                {
                    var instr = line.Content;
                    try
                        {
                            instr.Render();
                            if ((instr as Command) != null || (instr as Variable) != null)
                                Out(String.Format("{0}  {1}\t {2}", LineNumber(instr), instr.GetBytes(), line.Input));
                            else
                                Out(String.Format("\t\t\t{0}", line.Input));
                        }
                    catch (AssemblerException) 
                    {
                        ShowError(n, line);
                        Out(String.Format("{0}\t\t{2}\n Error: {1}", LineNumber(instr), instr.Error, line.Input));
                    }
                }
                n++;
            }

            Out("\n");
            Out("\tVariables\n");
            foreach (var pair in _vars)
            {
                var v = pair.Value;
                Out(String.Format("{0}\t\t{1}({3:x}) : {4:x} in {2}", v.Name, v.Type, v.Seg != null ? v.Seg.Name : "UNKNOWN", v.Length, v.Offset));
            }

            Out("\n");
            Out("\tSegments\n");
            foreach (var pair in _segments)
            {
                var seg = pair.Value;
                Out(String.Format("{0}\t\t : 0{1:x}", seg.Name, seg.CurrentOffset));
            }

            Out("\n");
            Out("\tLabels\n");
            foreach (var pair in _segments)
            {
                var labels = pair.Value.Labels;
                foreach (var p in labels)
                {
                    var label = p.Value;
                    Out(String.Format("{0}\t\t : 0{1:x} in {2}", label.Name, label.Offset, pair.Value.Name));
                }
            }
        }

        public void Out(String s)
        {
            if (_writer != null)
            {
                _writer.WriteLine(s);
                _writer.Flush();
            } else
                Console.WriteLine(s);
        }

        public static string LineNumber(Instruction instr)
        {
            if (instr.Offset < 0x0010)
                return String.Format("000{0:X}", instr.Offset);
            if (instr.Offset < 0x0100)
                return String.Format("00{0:X}", instr.Offset);
            if (instr.Offset < 0x1000)
                return String.Format("0{0:X}", instr.Offset);
            return String.Format("{0:X}", instr.Offset);
        }

        private void ShowError(int lineNumber, Line line)
        {
            if (line.IsInstruction())
            {
                Console.WriteLine("Error at line {0}: {1}\n{2}", lineNumber, line.Input, line.Content.Error);
            }
            else
            {
                Console.WriteLine("Error at line {0}: {1}\n{2}", lineNumber, line.Input, line.Error);
            }
        }



        public Line AnalyseString(string sl)
        {
            //если директива 'end' уже была и строка не пуста
            if (_endfile && !string.IsNullOrWhiteSpace(sl))
            {
                //не будем обрабатывать строку
                return new Line("Cannot read after 'end' directive", sl);
            }

            var instr = sl; //сохраним исходную строку

            //сначала считаем метку и удалим ее из строки при нахождении
            var label = Label.ReadLabels(ref sl, _currentSeg);
            if (label != null)
                try
                {
                    _currentSeg.AddLabel(label);
                }
                catch
                {
                    return new Line("Error: cannot add label " + label.Name, instr);
                }

            if (string.IsNullOrEmpty(sl)) //если оставшаяся строка пустая
            {
                if (_currentSeg != null) //если пустая строка внутри сегмента
                    return new Line(new Empty("", label), instr);
                else
                    return new Line("", instr);
            }

            //в нижний регистр для удобства
            var s = sl.ToLower();

            //если это директива assume
            if (s.StartsWith("assume "))
            {
                if (label != null)
                    return new Line("Error: cannot add label to 'assume' directive", instr);
                var a = Assume.Matches(sl, _segments);
                if (a != null)
                {
                    _assume = a;
                    return new Line("Assume directive", instr);
                }
                else
                    return new Line("Error: incorrect assume directive", instr);
            }

            //если это директива конца
            if (s == "end")
            {
                //отметим конец файла и завершим обработку строки
                _endfile = true;
                return new Line("End of file", instr);
            }

            //если это директива сегмента
            if (Segment.segStart.Match(sl).Success) //если начало сегмента
            {
                //начать сегмент
                var segName = Segment.segStart.Match(sl).Value;
                if (_currentSeg != null)
                {
                    return new Line("Error: declaring new segment inside segment " + _currentSeg.Name, instr);
                }
                _currentSeg = new Segment(segName);
                _segments.Add(segName, _currentSeg);
                return new Line(new Empty("Segment " + segName + " started", label), instr);
            } else
            if (Segment.segEnds.Match(sl).Success) //если конец сегмента
            {
                //закрыть сегмент
                var segName =  Segment.segEnds.Match(sl).Value;
                if (_currentSeg == null || segName != _currentSeg.Name)
                {
                    return new Line("Error: cannot close segment " + segName, instr);
                }
                _currentSeg = null;
                return new Line(new Empty("Segment " + segName + " ended", label), instr);
            }

            Instruction ins; //если это не директива - значит, наверное, инструкция

            //может это переменная?
            ins = Variable.Matches(sl, label, _currentSeg);
            if (ins != null)
            {
                try
                {
                    var v = (Variable)ins;
                    _vars.Add(v.Name, v); //добавим переменную в таблицу переменных
                }
                catch (ArgumentException ex)
                {
                    //если элемент уже был добавлен
                    return new Line(new Empty(ex.Message, label), instr);
                }
            }
            else
            {
                //может это команда процессора?
                ins = Command.Matches(sl, label, _assume, _currentSeg);
                if (ins != null)
                {

                }            
            }
            

            if (ins != null) //если строка была распознана
            {
                if (_currentSeg == null) //если вне сегмента
                {
                    //вернуть ошибку
                    return new Line("Error: no segment to write in", instr);
                }
                _currentSeg.Add(ins); //добавить в сегмент
                ins.Offset = _currentSeg.CurrentOffset; //установить смещение в команде
                _currentSeg.CurrentOffset += ins.Length; //увеличить текущеее смещение в сегменте на длину комманды
                return new Line(ins, s);
            }

            //если строка не была распознана
            return new Line("Error: no match", instr);
        }

        /// <summary>
        /// Считывает строку, распознает константу и выводит ее байты
        /// </summary>
        /// <param name="r">Строка</param>
        /// <param name="_type">Тип данных</param>
        /// <param name="_bytes">Массив байтов для вывода</param>
        /// <exception cref="System.Exception">Если невозможно конвертировать в массив байтов</exception>
        public static void ParseConstant(string r, DataType _type, out byte[] _bytes)
        {
            int b = 10;
            bool neg = false;

            //если впереди минус - запомнить это и убрать минус из строки
            if (r.StartsWith("-"))
            {
                neg = true;
                r = r.Substring(1);
            }

            //определить тип константы
            if (r.EndsWith("h")) //шестнадцетиричная
            {
                b = 16;
                r = r.Substring(0, r.Length - 1);
            }
            else if (r.EndsWith("b")) //двоичная
            {
                b = 2;
                r = r.Substring(0, r.Length - 1);
            }
            else if (r.EndsWith("d")) //десятичная
            {
                r = r.Substring(0, r.Length - 1);
            }

            if (_type == DataType.Byte) //распознать байт
            {
                byte bt = Convert.ToByte(r, b);
                _bytes = new byte[1];
                if (neg)
                    bt = (byte)-bt;
                _bytes[0] = bt;
            } else
            if (_type == DataType.Word) //распознать слово
            {
                short bt = Convert.ToInt16(r, b);
                if (neg)
                    bt = (short)-bt;
                _bytes = BitConverter.GetBytes(bt);
                Array.Reverse(_bytes);
            } else
            {   //распознать двойное слово
                int bt = Convert.ToInt32(r, b);
                _bytes = BitConverter.GetBytes((Int32)(neg ? -bt : bt));
            }

            
        }
    }
}
