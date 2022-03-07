using Entanglement.Modularity;
using Entanglement.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync.Networking
{
    public class SyncModule : EntanglementModule
    {
        public enum ItemNetChannel : byte
        {
            Blob1 = 5,
            Blob2 = 6,
            Blob3 = 7,
            Blob4 = 8,
            Blob5 = 9,
            Blob6 = 10,
            Blob7 = 11,
            Blob8 = 12,
        }
        public override void OnModuleLoaded()
        {
            // set secure connection settings so that GoFile requests will go through
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            NetworkMessage.RegisterHandler<ItemSyncMessageHandler>();
            ModuleLogger.Msg("Loaded module successfully");
        }
    }
}
