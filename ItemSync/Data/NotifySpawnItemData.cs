using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemSync.Data
{
    internal class NotifySpawnItemData : ItemSyncData
    {
        public override MessageType MsgType => MessageType.NotifySpawnItem;
        public string itemName;
        public string melonPath;
        public int sizeKB;
        public Vector3 pos;
        public Vector3 rot;

        public override void Deserialize(byte[] data)
        {
            var splitted = Utilities.SplitBytes(data);
            sizeKB = BitConverter.ToInt32(splitted[0], 0);
            itemName = Encoding.ASCII.GetString(splitted[1]);
            melonPath = Encoding.ASCII.GetString(splitted[2]);
            pos = Utilities.DebyteV3(splitted[3]);
            rot = Utilities.DebyteV3(splitted[4]);
        }

        public override byte[] Serialize()
        {
            byte[][] data = new byte[][]
            {
                BitConverter.GetBytes(sizeKB),
                Encoding.ASCII.GetBytes(itemName),
                Encoding.ASCII.GetBytes(melonPath),
                pos.ToBytes(),
                rot.ToBytes(),
            };
            return Utilities.JoinBytes(data);
        }
    }
}
