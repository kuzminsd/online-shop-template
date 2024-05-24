namespace OnlineShop.Application.Extensions;

public static class DateTimeExtensions
{
    private static readonly DateTime StartDateTime = new DateTime(1970, 1, 1);
    
    /// <summary>
    /// Extension for getting time in the same format as Kotlin and Java have.
    /// (The same format as System.currentTimeMillis() provides)
    /// </summary>
    /// <param name="dateTime">Variable for conversion</param>
    /// <returns>The difference, measured in milliseconds, between the dateTime and midnight, January 1, 1970 UTC.</returns>
    public static long ToTimeMilliseconds(this DateTime dateTime)
    {
        return (long)(dateTime - StartDateTime).TotalMilliseconds;
    }
}