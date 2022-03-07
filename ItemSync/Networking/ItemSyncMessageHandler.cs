using Entanglement.Modularity;
using Entanglement.Network;
using ItemSync.Data;
using StressLevelZero.Pool;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ItemSync.Networking
{

    internal class ItemSyncMessageHandler : NetworkMessageHandler<ItemSyncData>
    {
        public const byte mIndex = 100; // cuz chaos was 99
        public override byte? MessageIndex => mIndex;
        public override NetworkMessage CreateMessage(ItemSyncData data)
        {
            NetworkMessage message = new NetworkMessage();
            // message.messageType = MessageIndex.Value; unnecessary assignment - entanglement does it already

#if DEBUG
            ModuleLogger.Msg($"Creating a network message with MessageType {data.MsgType}");
#endif
            byte[] ser = data.Serialize();
            byte[] serb = new byte[ser.Length + 1];
            ser.CopyTo(serb, 1);
            serb[0] = (byte)data.MsgType;
            // SERBIA NUMBER ONE 
            message.messageData = serb;

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            try
            {
                MessageType msgType = (MessageType)message.messageData[0];
                byte[] data = new byte[message.messageData.Length - 1];
                Buffer.BlockCopy(message.messageData, 1, data, 0, data.Length);

                switch (msgType)
                {
                    case MessageType.NotifySpawnItem:
                        if (Prefs.dontSyncPeople.Value.Contains(sender))
                        {
                            ModuleLogger.Msg("Sender " + sender + " is blocked - not syncing items from this user");
                            return;
                        }

                        var nsid = new NotifySpawnItemData();
                        nsid.Deserialize(data);
                        ModuleLogger.Msg($"Spawned item {nsid.itemName} is {nsid.sizeKB}KB.");
                        if (nsid.sizeKB > Prefs.maxMelonSizeKB.Value)
                        {
                            ModuleLogger.Msg(" - That's larger than the max acceptable size " + Prefs.maxMelonSizeKB.Value);
                            return;
                        }
                        NotifiedSpawnItem(nsid, sender);
                        break;

                    case MessageType.HaveSpawnItem:
                        var hid = new HaveItemData();
                        hid.Deserialize(data);
                        ModuleLogger.Msg("Client " + sender + " already has the item " + hid.itemName);
                        break;

                    case MessageType.NeedSpawnItem:
                        if (Prefs.dontSyncPeople.Value.Contains(sender))
                        {
                            ModuleLogger.Msg("Sender " + sender + " is blocked - not syncing items to this user");
                            return;
                        }

                        var nid = new NeedItemData();
                        nid.Deserialize(data);
                        ModuleLogger.Msg("Client " + sender + " is requesting we upload and send " + nid.itemName);
                        ItemAggregator.SendItemData(nid, sender);
                        break;

                    case MessageType.SpawnItemData:
                        var ifd = new ItemFileData();
                        ifd.Deserialize(data);
                        if (ItemSync.DoWeHaveItem(ifd.itemName))
                        {
                            ModuleLogger.Msg("We already have " + ifd.itemName + ", ignoring itemdata");
                            return;
                        }
                        ItemAggregator.DownloadItem(ifd);
                        break;

                    case MessageType.NeedPlayermodel:
                        var npd = new NeedPlayermodelData();
                        npd.Deserialize(data);
                        PlayermodelAggregator.SendPlayermodelData(npd, sender);
                        break;

                    case MessageType.PlayermodelFileData:
                        var pfd = new PlayermodelFileData();
                        pfd.Deserialize(data);
                        PlayermodelAggregator.DownloadModel(pfd, sender);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ModuleLogger.Error("ERROR!!! While handling message: " + ex.ToString());
            }
        }

        private void NotifiedSpawnItem(NotifySpawnItemData data, long sender)
        {
            if (ItemSync.DoWeHaveItem(data.itemName))
            {
                ModuleLogger.Msg("We don't need the item, we already have it");

                var retDat = new HaveItemData()
                {
                    itemName = data.itemName,
                };
                var msg = NetworkMessage.CreateMessage(MessageIndex.Value, retDat);
                Node.activeNode.SendMessage(sender, NetworkChannel.Reliable, msg.GetBytes());

                ModuleLogger.Msg("Items synced since last scene reload:");
                foreach (var mPath in ItemSync.pathsSyncedSinceLastScene) ModuleLogger.Msg(" - " + mPath);
                ModuleLogger.Msg("Just spawned item path: " + data.melonPath);

                if (ItemSync.pathsSyncedSinceLastScene.Contains(data.melonPath))
                {
                    ModuleLogger.Msg($"We haven't loaded a new scene since the melon {data.melonPath} (which contains the spawned item {data.itemName}) was loaded, force-spawning {data.itemName} using our position and location");
                    var pool = PoolManager.GetPool(data.itemName);
                    var poolee = pool.InstantiatePoolee(data.pos, Quaternion.Euler(data.rot));
                    poolee.gameObject.SetActive(true);
                }

                return;
            }


            if (ItemAggregator.TryGetFromSyncFolder(data.melonPath, out FileInfo fileInfo))
            {
                ModuleLogger.Msg($"Item was found in the sync folder under the name {fileInfo.Name}, loading now");
                ModuleLogger.Msg(" - If this is item has executable code and you want it to be synced, put it in your normal CustomItems folder to let MTINM handle it automatically");
                ItemAggregator.LoadItemFromPath(fileInfo.FullName);

                var retDat = new HaveItemData()
                {
                    itemName = data.itemName,
                };
                var msg = NetworkMessage.CreateMessage(MessageIndex.Value, retDat);
                Node.activeNode.SendMessage(sender, NetworkChannel.Reliable, msg.GetBytes());
                return;
            }

            ModuleLogger.Msg("Item wasn't found in the sync folder - telling " + sender + " to upload it now");

            var nid = new NeedItemData()
            {
                itemName = data.itemName,
            };
            var msg_ = NetworkMessage.CreateMessage(MessageIndex.Value, nid);
            Node.activeNode.SendMessage(sender, NetworkChannel.Reliable, msg_.GetBytes());

        }
    }
}