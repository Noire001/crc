using System;
using System.Collections.Generic;

namespace CRC
{
    class Program
    {
        /// <summary>
        /// Handles console input & output.
        /// </summary>
        private static void Main()
        {
            Console.Write("Enter k: ");
            var k = Convert.ToUInt32(Console.ReadLine());
            Console.Write("Enter P: ");
            var pDigits = Convert.ToUInt32(Console.ReadLine());
            var P = DigitsToArray(pDigits);
            Console.Write("Enter BER: ");
            var BER = Convert.ToDouble(Console.ReadLine());
            Console.Write("Enter number of messages to transmit: ");
            var numOfMessages = Convert.ToUInt64(Console.ReadLine());

            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Run(numOfMessages, P, BER, k, out var noisyMessages, out var detectedNoisyMessages);
            var benchmark = DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds;

            Console.WriteLine("Transmitted " + numOfMessages + " messages.");
            Console.WriteLine(
                noisyMessages + " messages had errors. (" + (noisyMessages * 100.0) / numOfMessages + "%)");
            Console.WriteLine("CRC successfully detected " + detectedNoisyMessages + " of these. (" +
                              (detectedNoisyMessages * 100.0) / numOfMessages + "% of messages) (" +
                              Math.Round((detectedNoisyMessages * 100.0) / noisyMessages) + "% of errors)");
            Console.WriteLine("And failed to detect " + (noisyMessages - detectedNoisyMessages) + " (" + (
                (noisyMessages - detectedNoisyMessages) * 100.0) / numOfMessages + "%)");
            Console.WriteLine("Validated in " + benchmark + "ms.");
        }

        /// <summary>
        /// Runs the exercise.
        /// </summary>
        /// <param name="numOfMessages">The number of messages to be generated.</param>
        /// <param name="P">A binary unsigned integer array message used as a divisor.</param>
        /// <param name="BER">The Bit Error Rate.</param>
        /// <param name="k">The size of each message.</param>
        /// <param name="noisyMessages">The number of messages that have been altered</param>
        /// <param name="detectedNoisyMessages">The number of messages that have been altered and detected by CRC.</param>
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

        /// <summary>
        /// Converts an unsigned integer to an array of its digits.
        /// </summary>
        /// <param name="digits">The unsigned integer input.</param>
        /// <returns>The converted unsigned integer array.</returns>
        static uint[] DigitsToArray(uint digits)
        {
            var dList = new List<uint>();
            for (; digits != 0; digits /= 10)
                dList.Add(Convert.ToUInt32(digits % 10));
            var arr = dList.ToArray();
            Array.Reverse(arr);
            return arr;
        }

        /// <summary>
        /// Generates a random binary message represented as an unsigned integer array whose values range is [0, 1]
        /// and has a right padding.
        /// </summary>
        /// <param name="k">The size of the actual message.</param>
        /// <param name="padding">The size of the padding on the right.</param>
        /// <returns>An unsigned integer array whose first <c>k</c> values are random integers of [0, 1] and the last
        /// <c>padding</c> values are 0</returns>
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

        /// <summary>
        /// Adds noise to a message block according to a predefined bit error rate and returns the result.
        /// </summary>
        /// <param name="message">A binary message represented as an unsigned integer array.</param>
        /// <param name="BER">The Bit Error Rate.</param>
        /// <param name="isNoisy">Whether noise has been added to the block.</param>
        /// <returns>The noisy message if BER is hit, otherwise the original. </returns>
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

        /// <summary>
        /// Calculates and returns the FCS of a block. 
        /// </summary>
        /// <param name="message">A binary message represented as an unsigned integer array.</param>
        /// <param name="P">A binary unsigned integer array message used as a divisor.</param>
        /// <returns>The FCS array</returns>
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