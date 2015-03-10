using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CourseworkAsm
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введiть шлях до файлу .asm: ");
            string s = Console.ReadLine();
            if (string.IsNullOrEmpty(s)) //если введена пустая строка
            {
                Console.WriteLine("Некоректне iм'я файлу");
                Console.ReadKey();
                return; //выйти
            }
            if (!s.ToLower().EndsWith(".asm")) //дописать .asm если не дописано
            {
                s = s + ".asm";
            }
            string[] strings = null;
            try
            {
                strings = ReadFile(s); //считаем файл как массив строк
                int n = 0;
                foreach (var s1 in strings)
                {
                    Console.WriteLine(++n + " " + s1);
                }
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("При читаннi файлу сталася помилка [{0}]:\n {1} \nPress any key to leave", e.GetType(), e.Message);
                Console.ReadKey();
                return;
            }
            
            var p = new Parser(strings); //создадим парсер
            p.Parse();                   //и запустим его
            Console.ReadKey();
        }

        static string[] ReadFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open)) //создадим поток чтения
            {
                var reader = new StreamReader(stream);
                var strings = new List<string>(); //список считанных строк
                while (!reader.EndOfStream)
                {
                    var str = reader.ReadLine(); //читаем строку
                    str = str.Trim(); //обрезаем начальные и конечные пробелы
                    int comment;      //обрезаем комментарии
                    comment = str.IndexOf(';');
                    str = comment == -1 ? str : str.Substring(0, comment);
                    strings.Add(str); //добавляем в список
                }
                return strings.ToArray(); //возвращаем как массив строк
            }
        }
    }
}
