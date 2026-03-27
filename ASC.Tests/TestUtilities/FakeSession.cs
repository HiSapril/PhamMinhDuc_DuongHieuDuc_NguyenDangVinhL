using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASC.Tests.TestUtilities
{
    public class FakeSession : ISession
    {
        // Sử dụng Dictionary để lưu trữ dữ liệu session giả lập trong bộ nhớ
        private Dictionary<string, byte[]> sessionFactory = new Dictionary<string, byte[]>();

        public bool IsAvailable => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public IEnumerable<string> Keys => throw new NotImplementedException();

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            sessionFactory.Remove(key);
        }

        // --- Phần Override Set và TryGetValue (Vùng được khoanh đỏ trong hình) ---

        public void Set(string key, byte[] value)
        {
            if (!sessionFactory.ContainsKey(key))
            {
                sessionFactory.Add(key, value);
            }
            else
            {
                sessionFactory[key] = value;
            }
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            if (sessionFactory.ContainsKey(key) && sessionFactory[key] != null)
            {
                value = sessionFactory[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}