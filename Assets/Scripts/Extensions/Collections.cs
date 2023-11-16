using System;
using System.Collections.Generic;

namespace Extensions
{
    public static class Collections
    {
        private static Random random = new Random(123);
        public static T Random<T>(this IList<T> list)
        {
            return list[random.Next(0, list.Count)];
        }
    }
}