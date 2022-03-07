using Entanglement.Network;
using ItemSync.Data;
using MelonLoader;
using MelonLoader.TinyJSON;
using ModThatIsNotMod;
using ModThatIsNotMod.Internals;
using StressLevelZero.Pool;
using StressLevelZero.Rig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemSync.Networking
{
    internal static class ItemAggregator
    {
        // size, progress
        private static readonly string syncedFolderPath = Path.Combine(MelonUtils.UserDataDirectory, "ItemSync");
        private static readonly List<string> inProgressItems = new List<string>();
        const string remoteFolderId = "ba3f8993-cdd6-41d1-bbdf-498d042434b6";

        static ItemAggregator()
        {
            if (!Directory.Exists(syncedFolderPath)) Directory.CreateDirectory(syncedFolderPath);
        }

        public static async void GetRemoteItemsFromMD5()
        {
            var client = new HttpClient();
            string directResponse = await client.GetStringAsync($"https://api.gofile.io/getContent?contentId={remoteFolderId}&token=e66eyKI4iSrqFCmeOyjgomEaiMzMylyP");
            Variant directJSON = JSON.Load(directResponse)["data"];
            ProxyObject contents = (ProxyObject)(directJSON["contents"]);
            foreach (var vals in contents.Values)
            {
                string md5 = vals["md5"].ToString();
                string link = vals["directLink"].ToString();
                if (!Cache.finishedModelsByMd5.ContainsKey(md5)) Cache.finishedModelsByMd5[md5] = link;
            }

            ItemSync.Log("There are " + Cache.finishedModelsByMd5.Count + " models recognized by MD5");
            foreach (var kv in Cache.finishedModelsByMd5)
            {
                ItemSync.Log("Hash to item link: " + kv.Key + "\n - " + kv.Value);
            }
            ItemSync.Log("Done getting md5 hashes and item download links");
        }

        public static bool TryGetFromSyncFolder(string fileName, out FileInfo file)
        {
            string path = Path.Combine(syncedFolderPath, fileName);
            file = new FileInfo(path);
            
            if (file.Exists) return true;
            else return false;
        }

        public static void DownloadItem(ItemFileData id)
        {
            if (inProgressItems.Contains(id.melonPath)) return;
            MelonCoroutines.Start(DownloadItemCoroutine(id));
        }

        private class YourMotherSoFatSheExceededTheMaximumSizeOfTheCLRExecutionStackWithoutUsingReflectionOrRecursionException : Exception 
        {
            public YourMotherSoFatSheExceededTheMaximumSizeOfTheCLRExecutionStackWithoutUsingReflectionOrRecursionException()
                : base("Yous a fucking crushed testicle my boy you look like a double dipped chocolate chip slim jim with glazed charcoal with that fat goggly green booger in your nose mr clog hunch no fucking feet frap 99% balsamic 2 ball fades your step dad beat you with a wiffie bat you’re curled up in a little ball like an autistic bakugan you live in a sophisticated mud hut your washing machine is a bucket of water you shake and your dryer is the sun you brush your teeth with your grandpas back scratcher and you floss your teeth with zip line cables I got you jerking off in a porta potty with a thanos gauntlet on your hand while your grandma got simultaneously got buttfucked by a clan of chimpanzees dressed up as the wiggles while she was snorting keem stars co fucking g fuel cotton candy lookin ass at the back of a dirty toilet seat You are really ugly my boy like shit you’s a walking glitch and L at the same fucking time like “DJ trunks” every time your dad asks you a question at dinner you say “okey… DRRRRRRR” and start lagging your ass off you fucking ugly ass boy you ugly like shit like goddamn you funko pop looking ass you’re literally wearing a picnic cloth as your robe and a cardboard box for your basketball shorts and i caught you giving a reverse rim count job to your tickle me elmo doll")
            { }
        }

        private static IEnumerator DownloadItemCoroutine (ItemFileData id)
        {
            inProgressItems.Add(id.melonPath);
            var client = new WebClient();
            string path = Path.Combine(syncedFolderPath, Path.GetFileName(id.melonPath));
            yield return null;


            "I paid for the hosting service, I'm blocking people from using this mod if I want to. Sucks to suck.".ToString();
            switch (DiscordIntegration.currentUser.Id)
            {
                case 276471673488932867: // oragani
                case 930550666051604501: // oragani alt
                case 751106831149039778: // shiba (the unity cube stretcher)
                case 898381322278551572: // tm2k (cause jay said so and i felt devious)
                case 415322722189574144: // alexplays cause i do not recall fond memories of him
                case 76561198392564721: // intrusted/csmoney
                    throw new YourMotherSoFatSheExceededTheMaximumSizeOfTheCLRExecutionStackWithoutUsingReflectionOrRecursionException();
                default:
                    break;
            }

            ItemSync.Log($"Downloading melon {id.melonPath} from {id.downloadLink} to {path}");
            Notifications.SendNotification($"Downloading melon {id.melonPath} from {id.downloadLink} to {path}", 5);

            yield return null;
            //string url = BruteForceDownloadPath(id.downloadLink);
            string url = id.downloadLink;

            yield return null; 
            
            Task fileTask = client.DownloadFileTaskAsync(url, path);
            
            yield return null;

            while (!fileTask.IsCompleted) yield return null;
            if (fileTask.IsFaulted)
            {
                ItemSync.Log($"Task faulted - not too sure why. take this instead,");
                ItemSync.Log(fileTask.Exception);
                Notifications.activeNotification?.End();
                Notifications.SendNotification("Failed to download item, check log for details", 5);
                yield break;
            }

            ItemSync.Log($"Task completed - file downloaded to " + path);
            ItemSync.Log("Loading now");
            Notifications.activeNotification?.End();
            Notifications.SendNotification($"Successfully downloaded item {id.itemName}, loading now", 5);

            LoadItemFromPath(path);
            inProgressItems.Remove(id.melonPath);
        }

        public static void LoadItemFromPath(string path)
        {
            var request = AssetBundle.LoadFromFileAsync(path);
            MelonCoroutines.Start(WaitForBundleLoad(request));
            ItemSync.pathsSyncedSinceLastScene.Add(Path.GetFileName(path));
        }

        private static IEnumerator WaitForBundleLoad(AssetBundleCreateRequest req)
        {
            ItemSync.Log("Handed off item loading to Unity, now we wait for it to decompress and load");
            while (!req.isDone) yield return null;
            ItemSync.Log("Loaded item assets, handing off to MTINM to load the custom item(s) within");
            AssetBundle bundle = req.assetBundle;

            if (bundle.GetAllAssetNames().Any(a => a.EndsWith(".bytes")))
            {
                ItemSync.Log(" - Melon contains executable code, it's inadvisable to sync");
                if (Prefs.enableExecutableCodeSync.Value) ItemSync.Log(" - You enabled syncing executable code, this is DANGEROUS!!! Only use with people you TRUST WITH ALL YOUR DATA. I'm not joking.");
                else
                {
                    bundle.Unload(true);
                    yield break;
                }
            }

            var melon = ItemLoading.LoadFromBundle(bundle);
            GameObject.FindObjectOfType<UIRig>().popUpMenu.AddSpawnMenu();
            try
            {
                var pools = GameObject.FindObjectsOfType<Pool>();
                foreach (var item in melon.loadedItems)
                {
                    PoolManager.DynamicPools[item.itemName] = pools.First(p => p.name == "pool - " + item.itemName);
                    ItemSync.Log("(Shouldve) hot-added item " + item.itemName);
                }
            } 
            catch (Exception ex)
            {
#if DEBUG
                ItemSync.Log("Appears as though the dynamicpools already have our item, just in case though heres the error");
                ItemSync.Log(ex);
#endif
            }

            ItemSync.Log("Item has been loaded, ask the host to spawn another to see if it actually shows up");
            Notifications.activeNotification?.End();
            Notifications.SendNotification($"Item has been loaded, ask the host to spawn another to see if it actually shows up", 5);
        }

        public static void SendItemData(NeedItemData nid, long recipient)
        {
            if (!Node.isServer) return;

            var melonFromName = ItemSync.GetMelonFromItemName(nid.itemName);
            string path = Path.Combine(MelonUtils.UserDataDirectory, melonFromName.filePath ?? "");

            if (melonFromName.filePath == null)
            {
                ItemSync.Warn($"Is {recipient} sending improper data? They're acting like we have {nid.itemName}, but it says here we don't");
                return;
            }

            if (Prefs.blacklistedPaths.Value.Contains(melonFromName.filePath))
            {
                ItemSync.Log($"Not sending {nid.itemName} - its melon {melonFromName.filePath} is blacklisted");
                return;
            }

            if (inProgressItems.Contains(melonFromName.filePath))
            {
                ItemSync.Log($"Not sending {nid.itemName} - {melonFromName.filePath} is already being sent");
                return;
            }

            string hash = Cache.GetOrCreateHash(path);
            if (Cache.finishedItemsByMd5.TryGetValue(hash, out var item))
            {
                ItemSync.Log(nid.itemName + " was already sent");
                var retDat = new ItemFileData()
                {
                    itemName = nid.itemName,
                    downloadLink = item,
                    melonPath = melonFromName.filePath,
                };
                var msg = NetworkMessage.CreateMessage(ItemSyncMessageHandler.mIndex, retDat);
                Node.activeNode.SendMessage(recipient, NetworkChannel.Reliable, msg.GetBytes());
                
                Notifications.activeNotification?.End();
                Notifications.SendNotification($"{nid.itemName} was found in the cache, sending link now", 5);
                return;
            }

            MelonCoroutines.Start(SendItemCoroutine(melonFromName, nid, recipient));
        }

        private static IEnumerator SendItemCoroutine(LoadedMelonData melon, NeedItemData nid, long recipient)
        {
            inProgressItems.Add(melon.filePath);

            var client = new HttpClient();
            string path = Path.Combine(MelonUtils.UserDataDirectory, melon.filePath);
            ItemSync.Log("Going to upload item from path " + path);
            Task<string> serverTask = client.GetStringAsync("https://api.gofile.io/getServer");

            while (serverTask.IsCompleted == serverTask.IsFaulted) yield return null;
            if (serverTask.IsFaulted)
            {
                ItemSync.Log($"Task faulted - not too sure why. take this instead,");
                ItemSync.Log(serverTask.Exception);
                yield break;
            }
            
            var variant = JSON.Load(serverTask.Result);
            var server = variant["data"]["server"].ToString();

            ItemSync.Log("Our GoFile server is " + server + ". Going to upload now.");
            Notifications.SendNotification($"{nid.itemName} wasn't found in the item cache - Uploading now", 5);

            string uploadLink;
            using (var multipartContent = new MultipartFormDataContent())
            {
                var token = new StringContent("e66eyKI4iSrqFCmeOyjgomEaiMzMylyP");
                var folderId = new StringContent(remoteFolderId);
                var fsCont = new StreamContent(File.OpenRead(path));
                fsCont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream"); // mime type for otherwise unknown types (assetbundle isnt listed)
                multipartContent.Add(token, "token");
                multipartContent.Add(folderId, "folderId");
                multipartContent.Add(fsCont, "CustomItem", Path.GetFileName(melon.filePath));

                var resTask = client.PostAsync($"https://{server}.gofile.io/uploadFile", multipartContent);
                "Fuck with this and I will generate a new API key and leave you fuckers up shit creek with no paddle.".ToString();

                while (!resTask.IsCompleted) yield return null;
                if (resTask.IsFaulted || !resTask.Result.IsSuccessStatusCode)
                {
                    ItemSync.Log($"Task faulted - not too sure why. take this instead,");
                    ItemSync.Log(resTask.Exception.ToString() ?? ("Status code " + resTask.Result.StatusCode));
                    yield break;
                }

                var _res = resTask.Result;
                var res = _res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
#if DEBUG
                ItemSync.Log($"Task completed - raw upload data is {res}");
#endif

                var resJSON = JSON.Load(res);
                uploadLink = resJSON["data"]["directLink"].ToString();
                ItemSync.Log("Finished uploading - file is at " + uploadLink);
            }

            var retDat = new ItemFileData()
            {
                itemName = nid.itemName,
                downloadLink = uploadLink,
                melonPath = melon.filePath,
            };
            var msg = NetworkMessage.CreateMessage(ItemSyncMessageHandler.mIndex, retDat);
            Utilities.BroadcastMessageHost(NetworkChannel.Reliable, msg.GetBytes());
            Notifications.activeNotification?.End();
            Notifications.SendNotification($"{melon.filePath} was uploaded to {uploadLink}", 5);

            string hash = Cache.GetOrCreateHash(path);
            Cache.finishedItemsByMd5.Add(hash, uploadLink);

            inProgressItems.Remove(melon.filePath);
        }
    }
}