﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CrossModGui.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]

    public class IsNotNullBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
