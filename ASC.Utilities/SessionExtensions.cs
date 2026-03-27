using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace ASC.Utilities
{
    public static class SessionExtensions
    {
        // Ghi đối tượng vào Session
        public static void SetSession(this ISession session, string key, object value)
        {
            session.Set(key, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(value)));
        }

        // Đọc đối tượng từ Session
        public static T GetSession<T>(this ISession session, string key)
        {
            byte[] value;
            if (session.TryGetValue(key, out value))
            {
                var jsonData = Encoding.ASCII.GetString(value);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }

            return default(T);
        }
    }
}