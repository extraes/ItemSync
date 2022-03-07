using ItemSync.Networking;
using MelonLoader;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using Entanglement.Modularity;

[assembly: AssemblyTitle(ItemSync.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(ItemSync.BuildInfo.Company)]
[assembly: AssemblyProduct(ItemSync.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + ItemSync.BuildInfo.Author)]
[assembly: AssemblyTrademark(ItemSync.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(ItemSync.BuildInfo.Version)]
[assembly: AssemblyFileVersion(ItemSync.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(ItemSync.ItemSync), ItemSync.BuildInfo.Name, ItemSync.BuildInfo.Version, ItemSync.BuildInfo.Author, ItemSync.BuildInfo.DownloadLink)]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]
[assembly: MelonPriority(10000)]
[assembly: EntanglementModuleInfo(typeof(SyncModule), "Custom Item Sync", "1.0.0", "extraes", "CIS")]