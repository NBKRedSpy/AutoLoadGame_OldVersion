using System;
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
                return !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
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
                    Log(ex);
                }
            }
        }
    }
}