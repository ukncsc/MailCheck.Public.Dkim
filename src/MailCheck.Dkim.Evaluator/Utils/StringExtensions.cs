using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailCheck.Dkim.Evaluator.Utils
{
    public static class StringExtensions
    {
        public static bool TryParseExactEnum<T>(this string value, out T t, bool caseInsensitive = true)
            where T : struct =>
            Enum.TryParse(value, caseInsensitive, out t) &&
            Enum.GetNames(typeof(T)).Any(_ => _.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
