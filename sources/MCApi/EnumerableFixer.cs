using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MCApi
{
    public class EnumerableFixer<T> : IEnumerable<T>
    {
        private readonly IEnumerable wrap;

        public EnumerableFixer(IEnumerable wrap)
        {
            this.wrap = wrap;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (object o in wrap)
            {
                yield return (T)o;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
