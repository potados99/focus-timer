using System.Collections.Generic;
using System.Linq;

namespace FocusTimer.Lib.Utility
{
    public static class EnumExtensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
           => self.Select((item, index) => (item, index));
    }
}
