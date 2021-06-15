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
            uint pDigits = Convert.ToUInt32(Console.ReadLine());
            uint[] P = DigitsToArray(pDigits);
            Console.WriteLine("Enter BER:");
            double BER = Convert.ToDouble(Console.ReadLine());
            Console.WriteLine("Enter number of messages to transmit:");
            ulong numOfMessages = Convert.ToUInt64(Console.ReadLine());

            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Run(numOfMessages, P, BER, k, out var noisyMessages, out var detectedNoisyMessages);
            long benchmark = DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds;

            Console.WriteLine("Transmitted " + numOfMessages + " messages.");
            Console.WriteLine(
                noisyMessages + " messages had errors. (" + (noisyMessages * 100.0) / numOfMessages + "%)");
            Console.WriteLine("CRC successfully detected " + detectedNoisyMessages + " of these. (" +
                              (detectedNoisyMessages * 100.0) / numOfMessages + "%)");
            Console.WriteLine("And failed to detect " + (noisyMessages - detectedNoisyMessages) + " (" + (
                (noisyMessages - detectedNoisyMessages) * 100.0) / numOfMessages + "%)");
            Console.WriteLine("Validated in " + benchmark + "ms.");
        }

        static void Run(ulong numOfMessages, uint[] P, double BER, uint k, out ulong noisyMessages,
            out ulong detectedNoisyMessages)
        {
            noisyMessages = 0;
            detectedNoisyMessages = 0;
            for (ulong i = 0; i < numOfMessages; i++)
            {
                var block = RandomPaddedMessage(k, P.Length - 1);
                var fcs = GetFcs(block, P);
                for (var j = fcs.Length - 1; j >= 0; j--)
                {
                    block[block.Length - fcs.Length + j] = fcs[j];
                }

                var noisyBlock = MessageWithNoise(block, BER, out var isNoisy);
                var noisyRemainder = GetFcs(noisyBlock, P);
                var isDetected = false;
                foreach (var t in noisyRemainder)
                    if (t == 1)
                    {
                        isDetected = true;
                    }

                if (isNoisy) noisyMessages++;
                if (isDetected) detectedNoisyMessages++;
            }
        }

        static uint[] DigitsToArray(uint digits)
        {
            var dList = new List<uint>();
            for (; digits != 0; digits /= 10)
                dList.Add(Convert.ToUInt32(digits % 10));
            var arr = dList.ToArray();
            Array.Reverse(arr);
            return arr;
        }

        static uint[] RandomPaddedMessage(uint k, int padding)
        {
            var message = new uint[k + padding];
            var random = new Random();
            for (var i = 0; i < message.Length - padding; i++)
            {
                message[i] = Convert.ToUInt32(random.Next(0, 2));
            }

            return message;
        }

        static uint[] MessageWithNoise(uint[] message, double BER, out bool isNoisy)
        {
            var temp = new uint[message.Length];
            Array.Copy(message, temp, temp.Length);
            isNoisy = false;
            var random = new Random();
            for (var i = 0; i < temp.Length; i++)
            {
                if (random.NextDouble() < BER)
                {
                    isNoisy = true;
                    if (temp[i] == 1)
                        temp[i] = 0;
                    else
                        temp[i] = 1;
                }
            }

            return temp;
        }

        static uint[] GetFcs(uint[] message, uint[] P)
        {
            var temp = new uint[message.Length];
            Array.Copy(message, temp, temp.Length);
            var result = new uint[P.Length - 1];
            var index = Array.IndexOf(temp, Convert.ToUInt32(1)); //start from non-zero 'bit'
            for (var i = index; i < temp.Length - P.Length - 1; i++) //avoid padded area
            {
                index = Array.IndexOf(temp, Convert.ToUInt32(1));
                if (index != -1 && index + P.Length < temp.Length) //ensure not out of bounds
                    for (var j = 0; j < P.Length; j++)
                    {
                        temp[index + j] ^= P[j];
                    }
            }

            for (var i = result.Length - 1; i >= 0; i--)
            {
                result[i] = temp[temp.Length - result.Length + i];
            }

            return result;
        }
    }
}