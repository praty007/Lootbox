using System;
using System.Text;

namespace utils
{
    public static class Rand
    {

        /// <summary>
        /// Generates a random float between a and b
        /// </summary>
        /// <param name="a">min value</param>
        /// <param name="b">max value</param>
        /// <returns></returns>
        public static float Float(float a = 0, float b = 1)
        {
            return UnityEngine.Random.Range(0, 1);
        }

        /// <summary>
        /// Generates a random int between a and b
        /// </summary>
        /// <param name="a">min value</param>
        /// <param name="b">max value</param>
        /// <returns></returns>
        public static uint Int(uint a, uint b)
        {
            return (uint)UnityEngine.Random.Range(a, b);
        }

        /// <summary>
        /// Generates random string 
        /// </summary>
        /// <param name="length">length of required string</param>
        /// <param name="chars">chars to randomize out of</param>
        /// <returns></returns>
        public static string String(uint length = 10, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ12345678900")
        {
            StringBuilder stringBuilder = new StringBuilder();
            int i = 0;
            while (i < length)
            {
                stringBuilder.Append(chars.Substring(UnityEngine.Random.Range(0, chars.Length - 2), 1));
                i++;
            }
            return stringBuilder.ToString();
        }

    }

    public static class Const
    {
        public const string DateTimeFormat = "F";

        /// <summary>
        /// Parses a given dateTime string in <see cref="DateTimeFormat"/> to DateTime type.
        /// </summary>
        /// <param name="dateTimeString"> Make sure to use UTC times. If nulls value is provided, it returns minimum DateTime value available.</param>
        public static DateTime GetDateTime(string dateTimeString)
        {
            if (dateTimeString == null)
                return DateTime.MinValue.ToUniversalTime();

            if (DateTime.TryParseExact(dateTimeString, DateTimeFormat, null, System.Globalization.DateTimeStyles.None, out DateTime temp))
                return temp;
            else return DateTime.MinValue.ToUniversalTime();
        }

        /// <summary>
        /// Converts a given DateTime object to string of <see cref="DateTimeFormat"/>
        /// </summary>
        /// <param name="dateTime">Make sure to use UTC times</param>
        public static string GetDateTimeString(DateTime dateTime)
        {
            return dateTime.ToString(DateTimeFormat);

        }
    }

}
