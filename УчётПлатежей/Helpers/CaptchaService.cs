using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace УчётПлатежей.Helpers
{
    static class CaptchaService
    {
        private static readonly Random _random = new Random();
        private static readonly IReadOnlyList<char> _availableChars = (
            Enumerable.Range('0', 10)
            .Concat(Enumerable.Range('A', 26))
            .Concat(Enumerable.Range('a', 26))
            .Select(code => (char)code)
            .ToList()
            .AsReadOnly()
            );

        public static string GenerateCaptcha(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentException("Length must be greater than 0");
            }

            return string.Join("", Enumerable.Range(0, length).Select(i => _availableChars[_random.Next(0, _availableChars.Count)]));
        }
    }
}
