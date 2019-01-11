namespace Crispy
{
    internal static class StringExtensions
    {
        internal static string LowerFirstLetter(this string input)
            => input.Substring(0, 1).ToLowerInvariant() 
             + input.Substring(1);
        
    }
}