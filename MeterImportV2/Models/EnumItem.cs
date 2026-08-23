using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MeterImportV2.Models
{
    public class EnumItem<T> where T : Enum
    {
        public T Value { get; }
        public string DisplayName { get; }
        public EnumItem(T value)
        {
            Value = value;
            DisplayName = GetDisplayName(value);
        }
        public override string ToString()
        {
            return DisplayName;
        }
        private static string GetDisplayName(Enum value)
        {
            var field = typeof(T).GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? value.ToString();
        }
    }
}
