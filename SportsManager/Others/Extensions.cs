using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SportsManager.Others
{
    /// <summary>
    /// Class for extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method for sorting ObservableCollection.
        /// </summary>
        /// <typeparam name="T">The generic parameter of the collection.</typeparam>
        /// <param name="collection">Collection to sort.</param>
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
