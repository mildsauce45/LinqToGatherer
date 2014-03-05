using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToGatherer
{
    public static class EnumExtensions
    {
        public static TAttr GetAttribute<TAttr>(this Enum e, int index = 0) where TAttr : Attribute
        {
            var attributes = e.GetAttributes<TAttr>();

            return index < attributes.Length ? attributes[index] : null;
        }

        public static TAttr[] GetAttributes<TAttr>(this Enum e) where TAttr : Attribute
        {
            if (e == null) return new TAttr[0];

            var fi = e.GetType().GetField(e.ToString());

            return fi != null ? fi.GetCustomAttributes(typeof(TAttr), false) as TAttr[] : new TAttr[0];
        }

        public static IEnumerable<TEnum> GetEnumValues<TEnum>()
        {
            var enumType = typeof(TEnum);

            if (!enumType.IsEnum)
                throw new Exception("Supplied type is not an enum");

            var fields = from f in enumType.GetFields() where f.IsLiteral select f;

            var values = new List<TEnum>();

            fields.ForEach(fi => values.Add((TEnum)fi.GetValue(enumType)));

            return values;
        }
    }
}
