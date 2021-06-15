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
            
        }

        static uint[] DigitsToArray(uint digits)
        {
            if (digits == 0) return new uint[] { 0 };
            var dList = new List<uint>();
            for (; digits != 0; digits /= 10)
                dList.Add(digits % 10);
            var arr = dList.ToArray();
            Array.Reverse(arr);
            return arr;
        }

        static uint[] RandomMessage(uint k, int slide)
        {
            var block = new uint[k + slide];
            var random = new Random();
            for (int i = 0; i < block.Length - slide; i++)
            {
                block[i] = Convert.ToUInt32(random.Next(0, 2));
            }
            return block;
        }

        static uint[] MessageWithNoise(uint[] message, double BER)
        {
            var random = new Random();
            for (int i = 0; i < message.Length; i++)
            {
                if (random.NextDouble() < BER)
                {
                    if (message[i] == 1)
                        message[i] = 0;
                    else
                        message[i] = 1;
                }
            }

            return message;
        }
    }
}