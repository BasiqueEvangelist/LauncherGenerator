using System.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace MCApi
{
    public class VariableResourceManager
    {
        public static VariableResourceManager NetworkConnections { get; } = new VariableResourceManager(10);


        private SemaphoreSlim semaphore;

        public VariableResourceManager(int limit)
        {
            semaphore = new SemaphoreSlim(limit, limit);
        }

        public async Task<ResourceHolder> Wait()
        {
            await semaphore.WaitAsync();
            return new ResourceHolder(this);
        }

        internal void Release() { semaphore.Release(); }

        public async Task<WrappingResourceHolder<T>> WrapWait<T>()
        {
            await semaphore.WaitAsync();
            return new WrappingResourceHolder<T>(this);
        }
    }

    public class ResourceHolder : IDisposable
    {
        private VariableResourceManager manager;

        public ResourceHolder(VariableResourceManager mgr)
        {
            manager = mgr;
        }

        public void Dispose()
        {
            manager.Release();
        }
    }

    public class WrappingResourceHolder<T> : ResourceHolder
    {
        protected T value;

        public WrappingResourceHolder(VariableResourceManager mgr) : base(mgr)
        {
        }

        public T Value { get; set; }
    }

}