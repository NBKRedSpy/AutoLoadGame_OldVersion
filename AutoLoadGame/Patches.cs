using System;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using UnityEngine;
using static AutoLoadGame.Core;

// ReSharper disable InconsistentNaming

namespace AutoLoadGame
{
    public class Patches
    {
        private static bool done;

        [HarmonyPatch(typeof(MainMenu), "ShowRefreshingSaves")]
        public static class MainMenu_ShowRefreshingSaves_Patch
        {
            public static bool Prepare()
            {
                try
                {
                    return !(GetCommandLineOpenMainMenu() || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
                }
                catch (Exception ex)
                {

                    Logger.Log(ex);
                    return true;
                }

                
            }

            public static void Postfix(MainMenu __instance)
            {
                try
                {
                    if (done)
                    {
                        return;
                    }

                    done = true;
                    var saveManager = UnityGameInstance.BattleTechGame.SaveManager;
                    var campaignSaveTime = saveManager.GameInstanceSaves.MostRecentCampaignSave.SaveTime;
                    var careerSaveTime = saveManager.GameInstanceSaves.MostRecentCareerSave.SaveTime;
                    if (campaignSaveTime > careerSaveTime)
                    {
                        var saveSlot = LazySingletonBehavior<UnityGameInstance>.Instance.Game.SaveManager.GameInstanceSaves.MostRecentCampaignSave;
                        Traverse.Create(__instance).Method("BeginResumeSave", saveSlot).GetValue();
                    }
                    else
                    {
                        var saveSlot = LazySingletonBehavior<UnityGameInstance>.Instance.Game.SaveManager.GameInstanceSaves.MostRecentCareerSave;
                        Traverse.Create(__instance).Method("BeginResumeSave", saveSlot).GetValue();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
        }

        /// <summary>
        /// Returns true if the command line was provided.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool GetCommandLineOpenMainMenu()
        {
            string[] args = Environment.GetCommandLineArgs();

            return args.Any(x => String.Compare(x, "-AutoLoadMainMenu", true) == 0);
        }
    }
}