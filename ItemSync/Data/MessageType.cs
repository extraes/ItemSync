using Entanglement.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync.Data
{
    internal enum MessageType : byte
    {
        NotifySpawnItem,
        HaveSpawnItem,
        NeedSpawnItem,
        SpawnItemData,
        NeedPlayermodel,
        PlayermodelFileData,
    }
}
