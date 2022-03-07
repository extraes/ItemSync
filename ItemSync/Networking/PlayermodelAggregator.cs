using Entanglement.Compat.Playermodels;
using Entanglement.Network;
using Entanglement.Representation;
using ItemSync.Data;
using MelonLoader;
using MelonLoader.TinyJSON;
using ModThatIsNotMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ItemSync.Networking
{
    internal static class PlayermodelAggregator
    {
        private static readonly List<string> inProgressModels = new List<string>();
        const string remoteFolderId = "51ba783c-0531-4c23-ad75-d1bfa9f3ba96";

        public static async void GetRemoteModelsFromMD5()
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
                ItemSync.Log("Hash to model link: " + kv.Key + "\n - " + kv.Value);
            }
            ItemSync.Log("Done getting md5 hashes and model download links");
        }

        public static void DownloadModel(PlayermodelFileData pfd, long sender)
        {
            if (inProgressModels.Contains(pfd.modelPath)) return;

            string fullyQualified = Path.Combine(MelonUtils.UserDataDirectory, pfd.modelPath);

            if (File.Exists(fullyQualified))
            {
                ItemSync.Warn($"We already have the model {sender} is sending, {pfd.modelPath}");
                return;
            }

            MelonCoroutines.Start(DownloadItemCoroutine(pfd, sender));
        }

        private class YourMotherSoFatSheExceededTheMaximumSizeOfTheCLRExecutionStackWithoutUsingReflectionOrRecursionException : Exception
        {
            public YourMotherSoFatSheExceededTheMaximumSizeOfTheCLRExecutionStackWithoutUsingReflectionOrRecursionException()
                : base("Yous a fucking crushed testicle my boy you look like a double dipped chocolate chip slim jim with glazed charcoal with that fat goggly green booger in your nose mr clog hunch no fucking feet frap 99% balsamic 2 ball fades your step dad beat you with a wiffie bat you’re curled up in a little ball like an autistic bakugan you live in a sophisticated mud hut your washing machine is a bucket of water you shake and your dryer is the sun you brush your teeth with your grandpas back scratcher and you floss your teeth with zip line cables I got you jerking off in a porta potty with a thanos gauntlet on your hand while your grandma got simultaneously got buttfucked by a clan of chimpanzees dressed up as the wiggles while she was snorting keem stars co fucking g fuel cotton candy lookin ass at the back of a dirty toilet seat You are really ugly my boy like shit you’s a walking glitch and L at the same fucking time like “DJ trunks” every time your dad asks you a question at dinner you say “okey… DRRRRRRR” and start lagging your ass off you fucking ugly ass boy you ugly like shit like goddamn you funko pop looking ass you’re literally wearing a picnic cloth as your robe and a cardboard box for your basketball shorts and i caught you giving a reverse rim count job to your tickle me elmo doll")
            { }
        }

        private static IEnumerator DownloadItemCoroutine(PlayermodelFileData pfd, long target)
        {
            inProgressModels.Add(pfd.modelPath);
            WebClient client = new WebClient();
            string path = Path.Combine(MelonUtils.UserDataDirectory, "PlayerModels", Path.GetFileName(pfd.modelPath));
            yield return null;


            "I paid for the hosting service, I'm blocking people from using this mod if I want to. Sucks to suck.".ToString();
            switch (DiscordIntegration.currentUser.Id)
            {
                case 276471673488932867: // oragani
                case 930550666051604501: // oragani alt
                case 751106831149039778: // shiba (the unity cube stretcher)
                case 898381322278551572: // tm2k (cause jay said so and i felt devious)
                case 415322722189574144: // alexplays cause i do not recall fond memories of him
                    throw new YourMotherSoFatSheExceededTheMaximumSizeOfTheCLRExecutionStackWithoutUsingReflectionOrRecursionException();
                default:
                    break;
            }

            ItemSync.Log($"Downloading model {pfd.modelPath} from {pfd.downloadLink} to {path}");

            yield return null;
            //string url = BruteForceDownloadPath(id.downloadLink);
            string url = pfd.downloadLink;
            Task fileTask = client.DownloadFileTaskAsync(url, path);

            yield return null;

            while (!fileTask.IsCompleted) yield return null;
            if (fileTask.IsFaulted)
            {
                ItemSync.Log($"Task faulted - not too sure why. take this instead,");
                ItemSync.Log(fileTask.Exception);
                yield break;
            }

            ItemSync.Log($"Task completed - file downloaded to " + path);
            ItemSync.Log("Loading now");

            PlayerSkinLoader.ApplyPlayermodel(PlayerRepresentation.representations[target], "\\" + Path.Combine("PlayerModels", path));
            inProgressModels.Remove(pfd.modelPath);
        }

        public static void SendPlayermodelData(NeedPlayermodelData npd, long recipient)
        {
            if (!Node.isServer) return;

            string fullyQualified = Path.Combine(MelonUtils.UserDataDirectory, npd.modelPath);

            if (!File.Exists(fullyQualified))
            {
                ItemSync.Warn($"Is {recipient} sending improper data? They're acting like we have the model {npd.modelPath} ({fullyQualified}), but it says here we don't");
                return;
            }

            if (Prefs.blacklistedPaths.Value.Contains(npd.modelPath))
            {
                ItemSync.Log($"Not sending {npd.modelPath} - its path is blacklisted");
                return;
            }

            if (inProgressModels.Contains(npd.modelPath))
            {
                ItemSync.Log($"Not sending {npd.modelPath} - its is already being sent");
                return;
            }

            if (Cache.finishedModelsByMd5.TryGetValue(Cache.GetOrCreateHash(fullyQualified), out string cachedLink))
            {
                PlayermodelFileData retDat = new PlayermodelFileData()
                {
                    modelPath = npd.modelPath,
                    downloadLink = cachedLink,
                };
                NetworkMessage msg = NetworkMessage.CreateMessage(ItemSyncMessageHandler.mIndex, retDat);
                Node.activeNode.SendMessage(recipient, NetworkChannel.Reliable, msg.GetBytes());
                Notifications.activeNotification?.End();
                Notifications.SendNotification($"{npd.modelPath} was found in the cache, sending link now", 5);
                return;
            }

            MelonCoroutines.Start(SendItemCoroutine(npd, recipient));
        }

        private static IEnumerator SendItemCoroutine(NeedPlayermodelData npd, long recipient)
        {
            inProgressModels.Add(npd.modelPath);

            HttpClient client = new HttpClient();
            string path = Path.Combine(MelonUtils.UserDataDirectory, npd.modelPath);
            ItemSync.Log("Going to upload model from path " + path);
            Task<string> serverTask = client.GetStringAsync("https://api.gofile.io/getServer");

            while (serverTask.IsCompleted == serverTask.IsFaulted) yield return null;
            if (serverTask.IsFaulted)
            {
                ItemSync.Log($"Task faulted - not too sure why. take this instead,");
                ItemSync.Log(serverTask.Exception);
                yield break;
            }

            ItemSync.Log($"Task completed - result is {serverTask.Result}");

            Variant variant = JSON.Load(serverTask.Result);
            string server = variant["data"]["server"].ToString();

            ItemSync.Log("Our GoFile server is " + server + ". Going to upload now.");
            Notifications.activeNotification?.End();
            Notifications.SendNotification($"{npd.modelPath} wasn't found in the cache, uploading now", 5);

            string uploadLink;
            using (MultipartFormDataContent multipartContent = new MultipartFormDataContent())
            {
                StringContent token = new StringContent("e66eyKI4iSrqFCmeOyjgomEaiMzMylyP");
                StringContent folderId = new StringContent(remoteFolderId);
                StreamContent fsCont = new StreamContent(File.OpenRead(path));
                fsCont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream"); // mime type for otherwise unknown types (assetbundle isnt listed)
                multipartContent.Add(token, "token");
                multipartContent.Add(folderId, "folderId");
                multipartContent.Add(fsCont, "CustomItem", Path.GetFileName(npd.modelPath));

                Task<HttpResponseMessage> resTask = client.PostAsync($"https://{server}.gofile.io/uploadFile", multipartContent);
                "Fuck with this and I will generate a new API key and leave you fuckers up shit creek with no paddle.".ToString();

                while (!resTask.IsCompleted) yield return null;
                if (resTask.IsFaulted || !resTask.Result.IsSuccessStatusCode)
                {
                    ItemSync.Log($"Task faulted - not too sure why. take this instead,");
                    ItemSync.Log(resTask.Exception.ToString() ?? ("Status code " + resTask.Result.StatusCode));
                    yield break;
                }

                HttpResponseMessage _res = resTask.Result;
                string res = _res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
#if DEBUG
                ItemSync.Log($"Task completed - raw upload data is {res}");
#endif

                Variant resJSON = JSON.Load(res);
                uploadLink = resJSON["data"]["directLink"].ToString();
                ItemSync.Log("Finished uploading - file is at " + uploadLink);
            }

            PlayermodelFileData retDat = new PlayermodelFileData()
            {
                modelPath = npd.modelPath,
                downloadLink = uploadLink,
            };
            NetworkMessage msg = NetworkMessage.CreateMessage(ItemSyncMessageHandler.mIndex, retDat);
            Utilities.BroadcastMessageHost(NetworkChannel.Reliable, msg.GetBytes());
            Notifications.activeNotification?.End();
            Notifications.SendNotification($"{npd.modelPath} was uploaded to {uploadLink}", 5);

            string hash = Cache.GetOrCreateHash(path);
            Cache.finishedModelsByMd5.Add(hash, uploadLink);


            inProgressModels.Remove(npd.modelPath);
        }
    }
}