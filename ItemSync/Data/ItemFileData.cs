using ItemSync.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync.Data
{
    internal class ItemFileData : ItemSyncData
    {
        public override MessageType MsgType => MessageType.SpawnItemData;
        public string itemName;
        public string melonPath;
        public string downloadLink;

        public override void Deserialize(byte[] data)
        {
            var bytes = Utilities.SplitBytes(data);

            itemName = Encoding.ASCII.GetString(bytes[0]);
            melonPath = Encoding.ASCII.GetString(bytes[1]);
            downloadLink = Encoding.ASCII.GetString(bytes[2]);
        }

        public override byte[] Serialize()
        {
            byte[][] bytess = new byte[][]
            {
                Encoding.ASCII.GetBytes(itemName),
                Encoding.ASCII.GetBytes(melonPath),
                Encoding.ASCII.GetBytes(downloadLink),
            };
            return Utilities.JoinBytes(bytess);
        }
    }
}
