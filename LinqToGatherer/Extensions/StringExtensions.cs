using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LinqToGatherer
{
    public static class StringExtensions
    {
        public static int ToInt(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return 0;

            int result;

            if (!Int32.TryParse(s, out result))
                return 0;

            return result;
        }

        public static bool IsOnlyNumeric(this string s)
        {
            return !string.IsNullOrWhiteSpace(s) && s.All(c => char.IsNumber(c));
        }

        public static T? ParseDisplayValue<T>(this string s) where T : struct, IConvertible
        {
            var enumType = typeof(T);

            if (enumType.IsGenericType)
                enumType = enumType.GetGenericArguments().FirstOrDefault();

            if (enumType == null)
                return null;

            if (!enumType.IsEnum)
                throw new ArgumentException(string.Format("The type {0} is not an enum.", enumType.Name));

            var fields = from field in enumType.GetFields() where field.IsLiteral select field;

            foreach (var field in fields)
            {
                var fieldValue = field.GetValue(enumType).ToString();

                if (fieldValue == s)
                    return (T)Enum.Parse(enumType, fieldValue, true);
            }

            return null;
        }

        public static string CollapseWhitespace(this string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : Regex.Replace(s, @"\s+", " ");
        }
    }
}
