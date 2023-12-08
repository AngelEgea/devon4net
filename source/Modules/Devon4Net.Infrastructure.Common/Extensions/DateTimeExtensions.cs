namespace Devon4Net.Infrastructure.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsValidDateTime(this DateTime dateTime)
        {
            return dateTime != default && dateTime > DateTime.MinValue && dateTime < DateTime.MaxValue;
        }
    }
}
