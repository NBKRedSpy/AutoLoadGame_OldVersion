using System;
using System.Reflection;
using Harmony;

// ReSharper disable InconsistentNaming

namespace AutoLoadGame
{
    public static class Core
    {
        internal static HarmonyInstance harmony;

        public static void Init(string modDir, string modSettings)
        {
            Log("Starting up " + DateTime.Now.ToShortTimeString());
            harmony = HarmonyInstance.Create("ca.gnivler.BattleTech.AutoLoadGame");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        internal static void Log(object input)
        {
            //FileLog.Log($"[AutoLoadGame] {input}");
        }
    }
}