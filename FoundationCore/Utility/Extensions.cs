namespace Foundation.Extensions
{
    public static partial class StringExtensions
    {
        public static T[] EmptyIfNull<T>(this T[] value) => value ?? [];
    }
}
