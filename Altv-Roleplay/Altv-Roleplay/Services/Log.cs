using System;
using System.Text.RegularExpressions;

namespace Altv_Roleplay.Utils
{
    class Log
    {
        public static void OutputLog(string message, ConsoleColor color = ConsoleColor.DarkCyan)
        {

            var pieces = Regex.Split(message, @"(\[[^\]]*\])");

            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i];

                if (piece.StartsWith("[") && piece.EndsWith("]"))
                {
                    Console.ForegroundColor = color;
                    piece = piece.Substring(1, piece.Length - 2);
                }

                Console.Write(piece);
                Console.ResetColor();

            }
            Console.WriteLine();
        }
    }
}
