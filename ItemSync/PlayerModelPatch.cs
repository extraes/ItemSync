using Entanglement.Compat.Playermodels;
using Entanglement.Network;
using Entanglement.Representation;
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
using UnityEngine;

namespace ItemSync
{
    [HarmonyPatch(typeof(PlayerSkinLoader), nameof(PlayerSkinLoader.ApplyPlayermodel))]
    public static class PlayerModelPatch
    {
        public static void Prefix(PlayerRepresentation rep, string path)
        {
            if (!DiscordIntegration.hasLobby)
                return;
            path = path.TrimStart('\\');
            path = path.TrimStart('/');
            ItemSync.Log("Player w/ id " + rep.playerId + " uses playermodel " + path); // Player w/ id 261631460727980033 uses playermodel PlayerModels\white suit blue glow ford.body
            CheckForPlayermodel(rep.playerId, path);
        }

        public static void CheckForPlayermodel(long user, string path)
        {
            string qualifiedPath = Path.Combine(MelonUtils.UserDataDirectory, path);
            if (File.Exists(qualifiedPath))
            {
                ItemSync.Log("We already have the playermodel " + Path.GetFileName(path) + " for " + user);
                return;
            }

            var npd = new NeedPlayermodelData()
            {
                modelPath = path
            };

            var msg = NetworkMessage.CreateMessage(ItemSyncMessageHandler.mIndex, npd);
            Node.activeNode.SendMessage(user, NetworkChannel.Reliable, msg.GetBytes());
        }
    }
}
