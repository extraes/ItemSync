using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSync
{
    internal static class Prefs
    {
        private static MelonPreferences_Category category = MelonPreferences.CreateCategory("CustomItemSync");
        public static MelonPreferences_Entry<string[]> blacklistedPaths = category.CreateEntry("blacklistedItemPaths", new string[0]);
        public static MelonPreferences_Entry<long[]> dontSyncPeople = category.CreateEntry("dontSyncUserIDs", new long[0]);
        //public static MelonPreferences_Entry<long[]> syncCodePeople = category.CreateEntry("syncCodePeople", new long[0]);
        public static MelonPreferences_Entry<bool> enableExecutableCodeSync = category.CreateEntry("enableExecutableCodeSync", false, description: "THIS IS DANGEROUS!!! THIS LETS SOMEONE EXECUTE MALICIOUS CODE ON YOUR PC!!!");
        public static MelonPreferences_Entry<int> maxMelonSizeKB = category.CreateEntry("maxMelonSizeKB", 1024 * 100); // 100mb
        // (64*128)kbpbs = 8mbps
        // thats probably way more than what we need lolmao
        // (16*32)kbps = 0.5mbps
        // slower than id like but i dont think ive got much of a choice - if someone wants it faster, they can spam the item

        // this runs after field init
        static Prefs ()
        {
            category.SaveToFile();
            category.LoadFromFile();
            CheckMaxSize();
        }

        public static void CheckMaxSize()
        {
            if (maxMelonSizeKB.Value > 1024 * 1024) // 1gb
            {
                "I'm limiting it to 1gb because im a cheap bastard and dont feel like paying $100/mo for this shit. If I hit 500gb direct link traffic then thats a sign to me that things are gonna go tits up.".ToString();
                try { ItemSync.Log($"{nameof(maxMelonSizeKB)} is over 1GB!!! We're not trying to get banned from this file host! Resetting to default!"); } catch { }
                maxMelonSizeKB.Value = 1024 * 100; // set back to 100mb
            }
        }
    }
}
