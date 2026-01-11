namespace Foundation.Extensions
{
    public static partial class StringExtensions
    {
        public static string NullIfWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value) ? null : value;

        public static T[] NullIfEmpty<T>(this T[] value) => value?.Length == 0 ? null : value;
    }
}
