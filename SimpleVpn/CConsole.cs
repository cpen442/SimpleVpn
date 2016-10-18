using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVpn
{
    public static class CConsole
    {
        public static bool verbose = false;
        public static bool step = false;

        public static void WriteLine(string msg, ConsoleColor color, ConsoleColor? bgcol = null, bool nostep = false, bool forceVerbose = false)
        {
            if(!forceVerbose && !verbose) return;
            setcolor(color, bgcol);
            Console.WriteLine(msg);
            Console.ResetColor();
            if(!nostep) waitInput();
        }

        public static void Write(string msg, ConsoleColor color, ConsoleColor? bgcol = null, bool forceVerbose = false)
        {
            if (!forceVerbose && !verbose) return;
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

        private static void waitInput()
        {
            if (!step) return;
            Console.WriteLine("Press ENTER to continue...");

            Console.ReadLine();
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
