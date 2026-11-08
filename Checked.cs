using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSka
{
    internal static class Checkd
    {
        public static int ReadInteger(string message)
        {
            int number;
            bool isCorrect;

            number = 0;
            isCorrect = false;

            while (!isCorrect)
            {
                Console.Write(message);

                string? input = Console.ReadLine();

                if (int.TryParse(input, out number))
                {
                    isCorrect = true;
                }
                else
                {
                    Console.WriteLine(
                        "Введите целое число.");
                }
            }

            return number;
        }

        public static int ReadNaturalNumber(string message)
        {
            int number;
            bool isCorrect;

            number = 0;
            isCorrect = false;

            while (!isCorrect)
            {
                Console.Write(message);

                string? input = Console.ReadLine();

                if (int.TryParse(input, out number)
                    && number > 0)
                {
                    isCorrect = true;
                }
                else
                {
                    Console.WriteLine(
                        "Введите натуральное число.");
                }
            }

            return number;
        }

        public static int ReadNumberInRange(
            string message,
            int minimum,
            int maximum)
        {
            int number;
            bool isCorrect;

            number = 0;
            isCorrect = false;

            while (!isCorrect)
            {
                Console.Write(message);

                string? input = Console.ReadLine();

                if (int.TryParse(input, out number)
                    && number >= minimum
                    && number <= maximum)
                {
                    isCorrect = true;
                }
                else
                {
                    Console.WriteLine(
                        "Введите число от "
                        + minimum + " до " + maximum + ".");
                }
            }

            return number;
        }
    }
}
