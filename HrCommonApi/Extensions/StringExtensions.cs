using HrCommonApi.Enums;
using System.Globalization;

namespace HrCommonApi.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Tries to parse the string as a DateOnly. If it fails, it returns null.
        /// </summary>
        public static DateOnly? TryGetAsDateOnly(this string value, string format = "dd-MM-yyyy")
        {
            try
            {
                return DateOnly.FromDateTime(DateTime.ParseExact(value, format, CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                return null; // Could log the exception, but out of scope for now.
            }
        }

        /// <summary>
        /// Tries to parse the string as a TimeOnly. If it fails, it returns null.
        /// </summary>
        public static TimeOnly? TryGetAsTimeOnly(this string value, List<string>? formats = null)
        {
            if (formats == null)
                formats = new List<string> { "HH:mm", "HH:mm:ss" };

            foreach (var format in formats)
            {
                if (format.Length != value.Length)
                    continue; // Skip if the format and value length do not match.

                try
                {
                    return TimeOnly.FromDateTime(DateTime.ParseExact(value, format, CultureInfo.InvariantCulture));
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to parse the string as a Role. If it fails, it returns Role.None.
        /// </summary>
        public static Role TryGetAsRole(this string value)
        {
            try
            {
                var roleId = int.Parse(value);

                return (Role)roleId;
            }
            catch (Exception)
            {
                return Role.None; // Could log the exception, but out of scope for now.
            }
        }

        /// <summary>
        /// Tries to parse the string as a Guid. If it fails, it returns Guid.Empty.
        /// </summary>
        public static Guid TryGetAsGuid(this string value)
        {
            try
            {
                return new Guid(value);
            }
            catch (Exception)
            {
                return Guid.Empty; // Could log the exception, but out of scope for now.
            }
        }
    }
}
