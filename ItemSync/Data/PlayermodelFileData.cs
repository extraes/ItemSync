using ItemSync.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync.Data
{
    internal class PlayermodelFileData : ItemSyncData
    {
        public override MessageType MsgType => MessageType.PlayermodelFileData;
        public string modelPath;
        public string downloadLink;

        public override void Deserialize(byte[] data)
        {
            var bytes = Utilities.SplitBytes(data);

            modelPath = Encoding.ASCII.GetString(bytes[0]);
            downloadLink = Encoding.ASCII.GetString(bytes[1]);
        }

        public override byte[] Serialize()
        {
            byte[][] bytess = new byte[][]
            {
                Encoding.ASCII.GetBytes(modelPath),
                Encoding.ASCII.GetBytes(downloadLink),
            };
            return Utilities.JoinBytes(bytess);
        }
    }
}
