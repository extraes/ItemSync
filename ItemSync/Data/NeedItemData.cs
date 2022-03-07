using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync.Data
{
    internal class NeedItemData : ItemSyncData
    {
        public override MessageType MsgType => MessageType.NeedSpawnItem;
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
