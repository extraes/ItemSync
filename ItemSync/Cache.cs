using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync
{
    internal static class Cache
    {
        public static readonly Dictionary<string, string> finishedItemsByMd5 = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> finishedModelsByMd5 = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> hashCache = new Dictionary<string, string>(); // path, hash

        public static string GetOrCreateHash(string path)
        {
            if (hashCache.ContainsKey(path))
            {
                return hashCache[path];
            }
            else return Utilities.Md5File(path);
        }
    }
}
