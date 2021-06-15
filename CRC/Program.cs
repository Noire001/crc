using System;
using System.Collections.Generic;

namespace CRC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter k:");
            uint k = Convert.ToUInt32(Console.ReadLine());
            Console.WriteLine("Enter P:");
            uint pNum = Convert.ToUInt32(Console.ReadLine());
            uint[] P = DigitsToArray(pNum);
            Console.WriteLine("Enter BER:");
            double BER = Convert.ToDouble(Console.ReadLine());

            uint[] block = RandomMessage(k, P.Length - 1);
            uint[] test = {1, 0, 1, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0};
            GetFCS(test, P);
        }

        static uint[] DigitsToArray(uint digits)
        {
            if (digits == 0) return new uint[] {0};
            var dList = new List<uint>();
            for (; digits != 0; digits /= 10)
                dList.Add(digits % 10);
            var arr = dList.ToArray();
            Array.Reverse(arr);
            return arr;
        }

        static uint[] RandomMessage(uint k, int padding)
        {
            var block = new uint[k + padding];
            var random = new Random();
            for (int i = 0; i < block.Length - padding; i++)
            {
                block[i] = Convert.ToUInt32(random.Next(0, 2));
            }

            return block;
        }

        static uint[] MessageWithNoise(uint[] message, double BER, out bool isNoisy)
        {
            isNoisy = false;
            var random = new Random();
            for (int i = 0; i < message.Length; i++)
            {
                if (random.NextDouble() < BER)
                {
                    isNoisy = true;
                    if (message[i] == 1)
                        message[i] = 0;
                    else
                        message[i] = 1;
                }
            }

            return message;
        }

        static uint[] GetFCS(uint[] message, uint[] P)
        {
            uint[] result = new uint[P.Length - 1];
            int index = Array.IndexOf(message, Convert.ToUInt32(1)); //start from non-zero 'bit'
            for (int i = index; i < message.Length - P.Length - 1; i++) //avoid padded area
            {
                index = Array.IndexOf(message, Convert.ToUInt32(1));
                if (index + P.Length < message.Length) //ensure not out of bounds
                    for (int j = 0; j < P.Length; j++)
                    {
                        message[index + j] ^= P[j];
                    }
            }

            for (int i = result.Length - 1; i >= 0; i--)
            {
                result[i] = message[message.Length - result.Length + i];
            }
            return result;
        }
    }
}