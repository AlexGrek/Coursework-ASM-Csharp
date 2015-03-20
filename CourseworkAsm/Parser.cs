﻿using System;
using System.Collections.Generic;
using System.Data.Common;
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


        public Parser(string[] strings)
        {
            _strings = strings;
        }

        public void Parse()
        {
            _segments = new Dictionary<string,Segment>();
            _vars = new Dictionary<string, Variable>();
            foreach (var s in _strings)
            {
                Console.WriteLine(AnalyseString(s) + " \t\t " + s);
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

            //сначала считаем метку и удалимм ее из строки при нахождении
            var label = Label.ReadLabels(ref sl);

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
                ins = Command.Matches(sl, label, _assume);
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
