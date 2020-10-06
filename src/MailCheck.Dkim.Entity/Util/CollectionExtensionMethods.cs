using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dkim.Entity.Util
{
    internal static class CollectionExtensionMethods
    {
        public static bool CollectionEqual<T>(this ICollection<T> x, ICollection<T> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.SequenceEqual(y);
        }
    }
}
