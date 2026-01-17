using System;
using System.Globalization;
using System.Windows.Data;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Classe base para conversores de valores
    /// </summary>
    public abstract class ValueConverterBase : IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Classe base para conversores de valores com tipo genérico
    /// </summary>
    public abstract class ValueConverterBase<TSource, TTarget> : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not TSource sourceValue)
                return GetDefaultTargetValue();

            return Convert(sourceValue, parameter, culture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not TTarget targetValue)
                return GetDefaultSourceValue();

            return ConvertBack(targetValue, parameter, culture);
        }

        protected abstract TTarget Convert(TSource value, object parameter, CultureInfo culture);

        protected virtual TSource ConvertBack(TTarget value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected virtual TTarget GetDefaultTargetValue() => default;
        protected virtual TSource GetDefaultSourceValue() => default;
    }
}
