using Entanglement.Network;
using HarmonyLib;
using ItemSync.Data;
using ItemSync.Networking;
using MelonLoader;
using ModThatIsNotMod.Internals;
using StressLevelZero.Data;
using StressLevelZero.Pool;
using StressLevelZero.Props.Weapons;
using System.Collections;
using System.IO;
using System.Linq;

namespace ItemSync
{
    [HarmonyPatch(typeof(SpawnGun), nameof(SpawnGun.OnFire))]
    public static class SpawnGunPatch
    {
        public static void Postfix(SpawnGun __instance)
        {
            if (!DiscordIntegration.hasLobby)
                return;
            if (!Node.isServer)
                return;
            MelonCoroutines.Start(SpawnGunFire(__instance));
        }

        public static IEnumerator SpawnGunFire(SpawnGun __instance)
        {
            SpawnableObject spawnable = __instance._selectedSpawnable;

            if (__instance._selectedMode != UtilityModes.SPAWNER || !spawnable)
                yield break;

            yield return null;

            yield return null;

            Pool objPool = PoolManager.GetPool(spawnable.title);

            if (!objPool)
                yield break;

            Poolee lastSpawn = objPool._lastSpawn;
            if (!lastSpawn)
                yield break;

            LoadedMelonData melon = ItemSync.GetMelonFromItemName(spawnable.title);
            if (melon.filePath == null) yield break;
            ItemSync.Log($"Found melon {melon.filePath} from spawned item {spawnable.title}");
            if (Prefs.blacklistedPaths.Value.Contains(melon.filePath))
            {
                ItemSync.Log(" - Melon is blacklisted from sync - refusing to send!");
                yield break;
            }
            if (melon.bundle.GetAllAssetNames().Any(a => a.EndsWith(".bytes")))
            {
                ItemSync.Log(" - Melon contains executable code, it's inadvisable to sync");
                if (Prefs.enableExecutableCodeSync.Value) ItemSync.Log(" - You enabled syncing executable code, clients must have this enabled as well. This is DANGEROUS!!!");
                else yield break;
            }

            // type, name, size
            NotifySpawnItemData nsid = new NotifySpawnItemData()
            {
                itemName = spawnable.title,
                melonPath = Path.GetFileName(melon.filePath),
                sizeKB = (int)(new FileInfo(Path.Combine(MelonUtils.UserDataDirectory, melon.filePath)).Length / 1024),
                pos = lastSpawn.transform.position,
                rot = lastSpawn.transform.rotation.eulerAngles,
            };

            NetworkMessage msg = NetworkMessage.CreateMessage(ItemSyncMessageHandler.mIndex, nsid);
            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msg.GetBytes());
        }
    }
}
