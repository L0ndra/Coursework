using System;
using System.ComponentModel;
using System.Linq;

namespace Coursework.Data.Util
{
    public static class EnumHelper
    {
        public static string GetDescriptionOfEnum(this Enum enumVal)
        {
            var type = enumVal.GetType();

            var memInfo = type.GetMember(enumVal.ToString());

            var attributes = (DescriptionAttribute[])memInfo[0]
                .GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.FirstOrDefault()?.Description;
        }

        public static T[] GetEnumValueByDescription<T>(string description) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(value => (Enum)Enum.Parse(typeof(T), value.ToString()))
                .Where(enumVal => GetDescriptionOfEnum(enumVal) == description)
                .Cast<T>()
                .ToArray();
        }
    }

}
