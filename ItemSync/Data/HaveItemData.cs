using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync.Data
{
    internal class HaveItemData : ItemSyncData
    {
        public override MessageType MsgType => MessageType.HaveSpawnItem;
        public string itemName;

        public override void Deserialize(byte[] data)
        {
            itemName = Encoding.ASCII.GetString(data);
        }

        public override byte[] Serialize()
        {
            return Encoding.ASCII.GetBytes(itemName);
        }
    }
}
