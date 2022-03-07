using Entanglement.Network;
using MelonLoader.Lemons.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemSync
{
    internal static class Utilities
    {
        static LemonMD5 md5 = new LemonMD5();
        /* Input: [
         *      [1,2,3,4,5,6,7]
         *      [2,3,4,5,6,7,8,9,10]
         * ]
         * Output: [
         *      3, <- header, says how many things there are
         *      ushorts(7,9)  <- header, says where to split
         *      1,2,3,4,5,6,7,
         *      2,3,4,5,6,7,8,9,10
         *      
         * ]
         */
        public static byte[] JoinBytes(byte[][] bytess)
        {
            byte arrayCount = (byte)bytess.Length;
            // bytes.Length - 1 because you dont put a delim at the end (unless youre weird i guess, but im not)
            List<ushort> indices = new List<ushort>(bytess.Length - 1);
            byte[] bytesJoined = new byte[(bytess.Length * 2 + 1) + bytess.Sum(b => b.Length)];
            // need to get the indices beforehand to compose the header
            foreach (byte[] arr in bytess)
            {
                indices.Add((ushort)arr.Length);
            }
            // Compose the header
            bytesJoined[0] = arrayCount;
            for (int i = 0; i < indices.Count; i++)
            {
                ushort idx = indices[i];
                BitConverter.GetBytes(idx).CopyTo(bytesJoined, i * sizeof(ushort) + 1);
            }

            ushort lastLen = 0;
            int index = (ushort)arrayCount * 2 + 1;
            for (int i = 0; i < arrayCount; i++)
            {
                int thisLen = indices[i];
                Buffer.BlockCopy(bytess[i], 0, bytesJoined, index, thisLen);

                // set the next index
                lastLen = (ushort)thisLen;
                index += thisLen;
            }

            return bytesJoined;
        }

        internal static byte[][] SplitBytes(byte[] bytes)
        {
            // Grab data from the header
            // get number of splits
            int splitsCount = (int)bytes[0];
            // actually get the list of element sizes
            int[] splitIndices = new int[splitsCount];
            for (int i = 0; i < splitsCount; i++)
            {
                // add 1 to skip the byte that says how many split indicies there are
                ushort idx = BitConverter.ToUInt16(bytes, 1 + i * sizeof(ushort));
                splitIndices[i] = idx;
            }

            byte[][] res = new byte[splitsCount][];
            // start after the header
            int lastIdx = splitsCount * sizeof(ushort) + 1;
            for (int i = 0; i < res.Length; i++)
            {
                int len = splitIndices[i];
                // dont overallocate lest there be a dead byte
                byte[] arr = new byte[len];
                // copy the bytes
                Buffer.BlockCopy(bytes, lastIdx, arr, 0, len);


                // save our changes to the array 
                res[i] = arr;
                // perpetuate the nevereding cycle
                lastIdx += len;
            }
            return res;
        }

        public static Vector3 DebyteV3(byte[] bytes, int startIdx = 0)
        {
#if DEBUG
            if (startIdx == 0 && bytes.Length != sizeof(float) * 3) ItemSync.Warn($"Trying to debyte a Vector3 of length {bytes.Length}, this is not the expected {sizeof(float) * 3} bytes!");
            if (startIdx + (sizeof(float) * 3) > bytes.Length) ItemSync.Warn($"{bytes.Length} is too short for the given index of {startIdx}");
#endif
            return new Vector3(
                BitConverter.ToSingle(bytes, startIdx),
                BitConverter.ToSingle(bytes, startIdx + sizeof(float)),
                BitConverter.ToSingle(bytes, startIdx + sizeof(float) * 2));
        }

        internal static byte[][] ChunkByLength(byte[] bytes, int itemsPerChunk)
        {
            int arrCount = Math.DivRem(bytes.Length, itemsPerChunk, out int remainder) + 1;
            if (remainder == 0) remainder = itemsPerChunk;
            byte[][] res = new byte[arrCount][];

            // Modus operandi for these for-loops: Avoid conditionals at all cost.
            // They're slow. Computers like code that goes straight through, its easier to predict.
            for (int i = 0; i < arrCount - 1; i++)
            {
                res[i] = new byte[itemsPerChunk];
                Buffer.BlockCopy(bytes, i * itemsPerChunk, res[i], 0, itemsPerChunk);
            }

            res[arrCount - 1] = new byte[remainder];
            Buffer.BlockCopy(bytes, bytes.Length - remainder - 1, res[arrCount - 1], 0, remainder);

            return res;
        }

        internal static void BroadcastMessageHost(NetworkChannel networkChannel, byte[] data)
        {
            "I paid for the hosting service, I'm blocking people from using this mod if I want to. Sucks to suck.".ToString();
            foreach (var user in Node.activeNode.connectedUsers)
            {
                
                switch (user)
                {
                    case 276471673488932867: // oragani
                    case 930550666051604501: // oragani alt
                    case 751106831149039778: // shiba (the unity cube stretcher)
                    case 898381322278551572: // tm2k (cause jay said so and i felt devious)
                    case 415322722189574144: // alexplays cause i do not recall fond memories of him
                        continue;
                    default:
                        Node.activeNode.SendMessage(user, networkChannel, data);
                        break;
                }
            }
        }

        // got this shit from stackoverflow
        public static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            string hexAlphabet = "0123456789ABCDEF";

            foreach (byte b in bytes)
            {
                result.Append(hexAlphabet[(int)(b >> 4)]);
                result.Append(hexAlphabet[(int)(b & 0xF)]);
            }

            return result.ToString().ToLower();
        }

        public static string Md5File(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var hash = md5.ComputeHash(bytes);
            return ByteArrayToHexString(hash);
        }

        public static string Md5File(byte[] bytes)
        {
            var hash = md5.ComputeHash(bytes);
            return ByteArrayToHexString(hash);
        }
    }
}
