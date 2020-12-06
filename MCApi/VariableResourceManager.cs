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

        public async Task<WrappingResourceHolder<T>> WrapWait<T>(Func<Task<T>> action)
        {
            await semaphore.WaitAsync();
            return new WrappingResourceHolder<T>(this, await action());
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
        public WrappingResourceHolder(VariableResourceManager mgr, T value) : base(mgr)
        {
            this.Value = value;
        }

        public T Value { get; set; }
    }

}