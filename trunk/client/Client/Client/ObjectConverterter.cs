using System;
using System.ComponentModel;

namespace Client
{
    public static class ObjectConverterter
    {
        public static bool TryConvert<TConvertFrom, UConvertTo>(TConvertFrom convertFrom, out UConvertTo convertTo)
        {
            convertTo = default(UConvertTo);
            bool converted = false;

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TConvertFrom));

            if (converter.CanConvertTo(typeof(UConvertTo)))
            {
                convertTo = (UConvertTo)converter.ConvertTo(convertFrom, typeof(UConvertTo));
                converted = true;
            }
            else
            {
                converter = TypeDescriptor.GetConverter(typeof(UConvertTo));

                if (converter.CanConvertFrom(typeof(TConvertFrom)))
                {
                    convertTo = (UConvertTo)converter.ConvertFrom(convertFrom);
                    converted = true;
                }
            }

            return converted;
        }

        public static bool TryConvert(Type convertFrom, object from, Type convertTo, out object to)
        {
            to = null;
            bool converted = false;

            TypeConverter converter = TypeDescriptor.GetConverter(convertFrom);

            if (converter.CanConvertTo(convertTo))
            {
                to = converter.ConvertTo(from, convertTo);
                converted = true;
            }
            else
            {
                converter = TypeDescriptor.GetConverter(convertTo);

                if (converter.CanConvertFrom(convertFrom))
                {
                    to = converter.ConvertFrom(from);
                    converted = true;
                }
            }

            return converted;
        }
    }
}
