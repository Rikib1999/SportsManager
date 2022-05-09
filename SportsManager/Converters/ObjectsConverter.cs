using System;
using System.Globalization;
using System.Windows.Data;

namespace SportsManager.Converters
{
    /// <summary>
    /// For converting multiple input command parameters in view to array of objects.
    /// </summary>
    public class ObjectsConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts command parameters into array of objects in multibinding.
        /// </summary>
        /// <param name="values">Command parameters.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}