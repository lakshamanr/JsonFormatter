using System;
using System.Text;
using System.Web;

namespace JsonFormatterApp.Services
{
    public class EncodingService
    {
        /// <summary>
        /// Encode string to Base64
        /// </summary>
        public string EncodeBase64(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decode Base64 string
        /// </summary>
        public string DecodeBase64(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            try
            {
                var bytes = Convert.FromBase64String(input);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Invalid Base64 string: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// URL Encode string
        /// </summary>
        public string UrlEncode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return Uri.EscapeDataString(input);
        }

        /// <summary>
        /// URL Decode string
        /// </summary>
        public string UrlDecode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return Uri.UnescapeDataString(input);
        }

        /// <summary>
        /// Escape Unicode characters
        /// </summary>
        public string EscapeUnicode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (c > 127)
                {
                    sb.Append($"\\u{(int)c:x4}");
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Unescape Unicode characters
        /// </summary>
        public string UnescapeUnicode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Unescape(input);
        }
    }
}
