using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BattleTech;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using Harmony;
using HBS;
using UnityEngine;
using static AutoLoadGame.Core;

// ReSharper disable InconsistentNaming

namespace AutoLoadGame
{
    public enum Mode
    {
        Save,
        MechBay
    }

    public class Patches
    {
        [HarmonyPatch(typeof(MainMenu), "ShowRefreshingSaves")]
        public static class MainMenu_ShowRefreshingSaves_Patch
        {
            private static bool doneMode;
            private static bool doneAbort;
            private static Mode mode;
            private static string saveFile = @"Mods/AutoLoadGame/operatingMode.txt";

            public static bool Prepare()
            {
                try
                {
                    if (!doneMode)
                    {
                        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                        {
                            if (File.Exists(saveFile))
                            {
                                mode = (Mode) Enum.Parse(typeof(Mode), File.ReadAllText(saveFile));
                                Log("Read mode: " + mode);
                            }

                            switch (mode)
                            {
                                case Mode.Save:
                                    mode = Mode.MechBay;
                                    Log("Mode is MechBay");
                                    break;
                                case Mode.MechBay:
                                    mode = Mode.Save;
                                    Log("Mode is Save");
                                    break;
                            }

                            Log("Writing mode: " + mode);
                            File.WriteAllText(saveFile, mode.ToString());
                            Log("Read back: " + (Mode) Enum.Parse(typeof(Mode), File.ReadAllText(saveFile)));
                            doneMode = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(ex);
                }

                return !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            }


            public static void Postfix(MainMenu __instance, SaveGameStructure ____saveStructure)
            {
                try
                {
                    doneAbort = false;
                    if (doneAbort)
                    {
                        return;
                    }

                    doneAbort = true;

                    if (mode == Mode.MechBay)
                    {
                        LazySingletonBehavior<UIManager>.Instance.GetOrCreateUIModule<SkirmishMechBayPanel>().SetData();
                    }
                    else
                    {
                        var saveManager = UnityGameInstance.BattleTechGame.SaveManager;
                        var campaignSaveTime = saveManager.GameInstanceSaves.MostRecentCampaignSave.SaveTime;
                        var careerSaveTime = saveManager.GameInstanceSaves.MostRecentCareerSave.SaveTime;

                        var mostRecentSaveSlot = ____saveStructure.GetAllSlots().OrderByDescending(x => x.SaveTime).FirstOrDefault();
                        Traverse.Create(__instance).Method("BeginResumeSave", mostRecentSaveSlot).GetValue();
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