using UnityEngine;
using Random = System.Random;

namespace Extensions
{
    public static class Color
    {
        public static UnityEngine.Color Random(int seed)
        {
            var rand = new Random(seed);
            return UnityEngine.Color.HSVToRGB(rand.Next(0, 100) / 100f, 1, 1);
        }
    }
}