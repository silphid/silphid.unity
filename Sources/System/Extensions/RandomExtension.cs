using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Silphid.Extensions
{
    public static class RandomExtension
    {
        public static double NextDouble(this Random obj, double max)
        {
            return obj.NextDouble(0, max);
        }

        public static double NextDouble(this Random obj, double min, double max)
        {
            return obj.NextDouble() * (max - min) + min;
        }

        public static TimeSpan NextTimeSpan(this Random obj, TimeSpan min, TimeSpan max)
        {
            return TimeSpan.FromSeconds(obj.NextDouble(min.TotalSeconds, max.TotalSeconds));
        }

        public static float NextFloat(this Random obj)
        {
            return (float) obj.NextDouble();
        }

        public static float NextFloat(this Random obj, float max)
        {
            return (float) obj.NextDouble(0, max);
        }

        public static float NextFloat(this Random obj, float min, float max)
        {
            return (float) obj.NextDouble(min, max);
        }

        public static bool NextBool(this Random obj)
        {
            return obj.NextBool(0.5);
        }

        public static bool NextBool(this Random obj, double trueProbability)
        {
            return obj.NextDouble() <= trueProbability;
        }

        public static byte NextByte(this Random obj)
        {
            return (byte) obj.Next(256);
        }

        public static T Next<T>(this Random obj, IEnumerable<T> elements)
        {
            List<T> list = elements.ToList();
            int index = obj.Next(list.Count);
            return list[index];
        }

        public static int Next(this Random obj, int exclusiveMax, IEnumerable<double> probabilities)
        {
            return obj.Next(0, exclusiveMax - 1, probabilities);
        }

        public static int Next(this Random obj, int inclusiveMin, int inclusiveMax, IEnumerable<double> probabilities)
        {
            int count = (inclusiveMax + 1) - inclusiveMin;
            List<double> probabilityList = probabilities.ToList();
            Debug.Assert(count > 0);
            Debug.Assert(probabilityList.Count == count);

            double totalProbabilities = probabilityList.Sum(x => x);
            double randomValue = obj.NextDouble(0, totalProbabilities);

            double threshold = 0;
            for (int i = 0; i < count; i++)
            {
                threshold += probabilityList[i];

                if (randomValue < threshold)
                    return inclusiveMin + i;
            }

            return 0;
        }

        public static T Next<T>(this Random obj, IEnumerable<T> elements, IEnumerable<double> probabilities)
        {
            List<T> elementList = elements.ToList();
            int randomIndex = obj.Next(elementList.Count);
            return elementList[randomIndex];
        }

        public static TimeSpan Next(this Random obj, TimeSpan min, TimeSpan max)
        {
            return TimeSpan.FromSeconds(obj.NextDouble(min.TotalSeconds, max.TotalSeconds));
        }
    }
}