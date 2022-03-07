using Entanglement.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync.Data
{
    internal abstract class ItemSyncData : NetworkMessageData
    {
        public abstract MessageType MsgType { get; }

        public abstract byte[] Serialize();
        
        public abstract void Deserialize(byte[] data);
    }
}
