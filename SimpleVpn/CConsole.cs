using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVpn
{
    public static class CConsole
    {
        public static void WriteLine(string msg, ConsoleColor color, ConsoleColor? bgcol = null)
        {
            setcolor(color, bgcol);
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void Write(string msg, ConsoleColor color, ConsoleColor? bgcol = null)
        {
            setcolor(color, bgcol);
            Console.Write(msg);
            Console.ResetColor();
        }


        private static void setcolor(ConsoleColor col, ConsoleColor? bgcol = null)
        {
            if (bgcol != null)
            {
                Console.BackgroundColor = bgcol.Value;
            }
            Console.ForegroundColor = col;
        }

        public static string ByteArrToStr(this IEnumerable<byte> input)
        {
            var arr = input.ToArray();
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(arr);
            }
            return "0x" + BitConverter.ToString(arr).Replace("-","");
        }
    }
}
