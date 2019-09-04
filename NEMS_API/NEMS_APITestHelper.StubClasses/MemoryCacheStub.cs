using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;

namespace NEMS_APITestHelper.StubClasses
{
    public static class MemoryCacheStub
    {
        public static IMemoryCache GetMemoryCache(IDictionary<string, Tuple<object, bool>> cache)
        {
            var mockMemoryCache = new Mock<IMemoryCache>();

            foreach(var key in cache.Keys)
            {
                var cacheItem = cache[key];
                var data = cacheItem.Item1;
                var success = cacheItem.Item2;

                mockMemoryCache
                    .Setup(x => x.TryGetValue(It.Is<string>(y => y == key), out data))
                    .Returns(success);

                mockMemoryCache
                    .Setup(x => x.CreateEntry(It.IsAny<string>()))
                    .Returns((string value) => new CacheEntryStub(value));
            }



            return mockMemoryCache.Object;
        }
    }

    public class CacheEntryStub : MemoryCacheEntryOptions, ICacheEntry
    {
        public CacheEntryStub(string key)
        {
            Key = key;
        }

        public object Key { get; set; }

        public object Value { get; set; }

        public void Dispose()
        {
            //Do stuff;
        }
    }
}
