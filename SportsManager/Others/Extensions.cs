using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SportsManager.Others
{
    public static class Extensions
    {
        /// <summary>
        /// Extension method for sorting ObservableCollection<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable
        {
            List<T> sorted = collection.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                collection.Move(collection.IndexOf(sorted[i]), i);
            }
        }
    }
}
