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
            if (string.IsNullOrEmpty(s))
            {
                Console.WriteLine("Некоректне iм'я файлу");
                Console.ReadKey();
                return;
            }
            if (!s.ToLower().EndsWith(".asm"))
            {
                s = s + ".asm";
            }
            string[] strings = null;
            try
            {
                strings = ReadFile(s);
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
            
            var p = new Parser(strings);
            p.Parse();
            Console.ReadKey();
        }

        static string[] ReadFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var reader = new StreamReader(stream);
                var strings = new List<string>();
                while (!reader.EndOfStream)
                {
                    var str = reader.ReadLine();
                    str = str.Trim();
                    int comment;
                    comment = str.IndexOf(';');
                    str = comment == -1 ? str : str.Substring(0, comment);
                    strings.Add(str);
                }
                return strings.ToArray();
            }
        }
    }
}
