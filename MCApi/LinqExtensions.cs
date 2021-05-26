using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MCApi
{
    public static class LinqExtensions
    {
        private class KeyedEqualityComparer<TIn, TKey> : EqualityComparer<TIn>
        {
            private Func<TIn, TKey> key;
            private IEqualityComparer<TKey> passt;

            public KeyedEqualityComparer(Func<TIn, TKey> key)
            {
                this.key = key;
                this.passt = EqualityComparer<TKey>.Default;
            }
            public KeyedEqualityComparer(Func<TIn, TKey> key, IEqualityComparer<TKey> passthrough)
            {
                this.key = key;
                this.passt = passthrough;
            }

            public override bool Equals(TIn x, TIn y)
            {
                return passt.Equals(key(x), key(y));
            }

            public override int GetHashCode(TIn obj) => passt.GetHashCode(key(obj));
        }
        public static IEnumerable<TIn> DistinctBy<TIn, TKey>(this IEnumerable<TIn> enumer, Func<TIn, TKey> key)
        {
            return enumer.Distinct(new KeyedEqualityComparer<TIn, TKey>(key));
        }
        public static IEnumerable<TIn> DistinctBy<TIn, TKey>(this IEnumerable<TIn> enumer, Func<TIn, TKey> key, IEqualityComparer<TKey> comparer)
        {
            return enumer.Distinct(new KeyedEqualityComparer<TIn, TKey>(key, comparer));
        }
    }
}