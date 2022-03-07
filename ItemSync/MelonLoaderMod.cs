using Entanglement.Patching;
using ItemSync.Networking;
using MelonLoader;
using MelonLoader.Assertions;
using ModThatIsNotMod;
using ModThatIsNotMod.Internals;
using Steamworks;
using StressLevelZero.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ItemSync
{
    public static class BuildInfo
    {
        public const string Name = "ItemSync"; // Name of the Mod.  (MUST BE SET)
        public const string Author = "extraes"; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class ItemSync : MelonMod
    {
        internal static ItemSync instance;
        internal static List<LoadedMelonData> itemList;
        internal static bool alreadyLogged = false;
        internal static List<string> pathsSyncedSinceLastScene = new List<string>();

        public override unsafe void OnApplicationStart()
        {
            instance = this;
            Entanglement.Modularity.ModuleHandler.SetupModule(Assembly);
            PlayermodelAggregator.GetRemoteModelsFromMD5();
            ItemAggregator.GetRemoteItemsFromMD5();
        }

        public override unsafe void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            pathsSyncedSinceLastScene.Clear();
            itemList = ItemLoading.loadedMelons;
            if (itemList?.Count != 0 && !alreadyLogged)
            {
                alreadyLogged = true;
                Log("Logging loaded melons");
                foreach (var loadedMelon in itemList)
                {
                    Log("Loaded melon from - " + loadedMelon.filePath + " with " + loadedMelon.itemsLoaded + " items");
                    if (Prefs.blacklistedPaths.Value.Contains(loadedMelon.filePath)) Log("     --===     This melon will NOT be synced     ===--");
                    foreach (var loadedItemData in loadedMelon.loadedItems)
                    {
                        LemonAssert.IsNotNull(loadedItemData.itemName, "loadedItemData.itemName");
                        Log(" - " + loadedItemData.itemName);
                    }
                }
            }
        }

        public override void OnPreferencesLoaded()
        {
            Prefs.CheckMaxSize();
        }

        public static bool DoWeHaveItem(string itemName) => PoolManager.GetPool(itemName) != null;
        public static LoadedMelonData GetMelonFromItemName(string name)
        {
            return itemList.Find(loadedMelon => loadedMelon.loadedItems.Any(item => item.itemName == name));
        }

        #region MelonLogger replacements

        internal static void Log(string str) => instance.LoggerInstance.Msg(str);
        internal static void Log(object obj) => instance.LoggerInstance.Msg(obj?.ToString() ?? "null");
        internal static void Warn(string str) => instance.LoggerInstance.Warning(str);
        internal static void Warn(object obj) => instance.LoggerInstance.Warning(obj?.ToString() ?? "null");
        internal static void Error(string str) => instance.LoggerInstance.Error(str);
        internal static void Error(object obj) => instance.LoggerInstance.Error(obj?.ToString() ?? "null");

        #endregion
    }
}
