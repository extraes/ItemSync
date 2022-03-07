using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemSync
{
    internal static class Extensions
    {
        public static byte[] Flatten(this byte[][] arrarr)
        {
            int totalToNow = 0;
            int sum = arrarr.Sum(arr => arr.Length);
            byte[] result = new byte[sum];

            for (int i = 0; i < arrarr.Length; i++)
            {
                Buffer.BlockCopy(arrarr[i], 0, result, totalToNow, arrarr[i].Length);
                totalToNow += arrarr[i].Length;
            }

            return result;
        }

        public static byte[] ToBytes(this Vector3 vec)
        {
            byte[][] bytess = new byte[][]
            {
                BitConverter.GetBytes(vec.x),
                BitConverter.GetBytes(vec.y),
                BitConverter.GetBytes(vec.z)
            };
            return bytess.Flatten();
        }
    }
}
