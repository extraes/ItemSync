using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync.Data
{
    internal class NeedPlayermodelData : ItemSyncData
    {
        public override MessageType MsgType => MessageType.NeedPlayermodel;
        public string modelPath;

        public override void Deserialize(byte[] data)
        {
            modelPath = Encoding.ASCII.GetString(data);
        }

        public override byte[] Serialize()
        {
            return Encoding.ASCII.GetBytes(modelPath);
        }
    }
}
