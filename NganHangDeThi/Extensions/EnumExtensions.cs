using System.ComponentModel;
using System.Reflection;

namespace NganHangDeThi.Extensions;

public static class EnumExtensions
{
    public static string? GetDescription(this Enum value)
    {
        return value.GetType()
                    .GetField(value.ToString())
                    ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .FirstOrDefault()
                    ?.Description;
    }
}


