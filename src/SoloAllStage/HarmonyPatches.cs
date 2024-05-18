using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using GameDataEditor;
using HarmonyLib;
using I2.Loc;
using UnityEngine;
using UseItem;

namespace SoloAllStage
{
    public static class HarmonyPatches
    {

        public const string name = "MiniMapQoL";


        public static Harmony harmony;

        public static void PatchAll()
        {
            if (harmony == null)
                harmony = new Harmony(VersionInfo.GUID);

            harmony.PatchAll();
        }

        public static void UnpatchAll()
        {
            if (harmony != null)
                harmony.UnpatchAll(VersionInfo.GUID);
        }
        
        [HarmonyPatch(typeof(SR_Solo), nameof(SR_Solo.GameSetting))]
        class SoloSettingPatch
        {
            static void Postfix(SR_Solo __instance)
            {
                __instance.RuleChange.EndisStage4 = false;
            }
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ClearUnlock))]
        class AchievementPatch
        {
            static private SpecialRule SpRuleOldValue = null;

            static void Prefix()
            {
                if (PlayData.TSavedata.SpRule != null)
                {
                    if (!(PlayData.TSavedata.SpRule is SR_Solo))
                        PlayData.TSavedata.SpRule.ChellangeClear();
                    SpRuleOldValue = PlayData.TSavedata.SpRule;
                }
                else
                {
                    SpRuleOldValue = null;
                }
            }

            static void Postfix()
            {
                if (SpRuleOldValue != null && PlayData.TSavedata.SpRule == null)
                {
                    PlayData.TSavedata.SpRule = SpRuleOldValue;
                }
            }
        }

        [HarmonyPatch(typeof(FieldSystem), nameof(FieldSystem.StageStart))]
        class StageStartPatch
        {
            static private EPlayMode recordedPlayMode = EPlayMode.StoryMode;

            static void Prefix()
            {
                recordedPlayMode = PlayData.TSavedata.NowPlayMode;
            }

            static void Postfix()
            {
                if (PlayData.TSavedata.SpRule != null && !string.IsNullOrEmpty(PlayData.TSavedata.SpRule.Key))
                {
                    PlayData.TSavedata.NowPlayMode = recordedPlayMode;
                }
            }
        }

        [HarmonyPatch(typeof(Camp), "Start")]
        class CampPatch
        {
            private static string KeyOldValue = null;
            
            static void Prefix()
            {
                Debug.Log("Enter Camp: MainStory " + SaveManager.NowData.storydata.MainStoryProgress + " Stage:" + PlayData.TSavedata.StageNum);
                Debug.Log("NowPlayMode" + PlayData.TSavedata.NowPlayMode);
                /*
                if (SaveManager.NowData.storydata.MainStoryProgress >= 9 &&
                    PlayData.TSavedata.NowPlayMode == EPlayMode.FreeMode)
                {
                    //打扎师傅
                    PlayData.TSavedata.NowPlayMode = EPlayMode.StoryMode;
                }
                */
                if (PlayData.TSavedata.SpRule != null &&
                    PlayData.TSavedata.SpRule.Key != GDEItemKeys.SpecialRule_Story_AzarSolo)
                {
                    KeyOldValue = PlayData.TSavedata.SpRule.Key;
                    PlayData.TSavedata.SpRule.Key = null;
                    Debug.Log("Replaced SpRule Key:" + KeyOldValue);
                }
                else
                {
                    KeyOldValue = null;
                }
            }

            static void Postfix(Camp __instance)
            {
                if (KeyOldValue != null)
                {
                    PlayData.TSavedata.SpRule.Key = KeyOldValue;
                    Debug.Log("Recovered SpRule Key:" + KeyOldValue);
                }
                if (SaveManager.NowData.storydata.MainStoryProgress >= 9 && __instance.UseNecklaceObj != null)
                {
                    __instance.UseNecklaceObj.SetActive(true);
                }
            }
        }

        [HarmonyPatch(typeof(CampUI), "Update")] //Private Method
        class CampBloodMistPatch
        {
            public static GameObject BtnBackup = null;

            static void Prefix(CampUI __instance)
            {
                BtnBackup = __instance.Button_BloodyMist;
                __instance.Button_BloodyMist = null;
            }
            
            static void Postfix(CampUI __instance)
            {
                __instance.Button_BloodyMist = BtnBackup;
                if (__instance.Button_BloodyMist != null)
                {
                    if (PlayData.TSavedata.SpRule != null)
                    {
                        __instance.Button_BloodyMist.gameObject.SetActive(true);
                    }
                    if (__instance.MainCampScript.BMist)
                    {
                        __instance.Button_BloodyMist.gameObject.SetActive(false);
                    }
                    if (SaveManager.NowData.GameOptions.CasualMode)
                    {
                        __instance.Button_BloodyMist.gameObject.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ArkCode), nameof(ArkCode.GoArk))]
        class GoArkFix
        {
            static bool Prefix(ArkCode __instance)
            {
                if (SaveManager.NowData.storydata.MainStoryProgress == 8 && !SaveManager.NowData.storydata.IsStory5Init)
                {
                    return true;
                }
                FieldSystem.instance.StartCoroutine(_GoarkFix());
                return true;
            }

            public static IEnumerator _GoarkFix()
            {
                yield return new WaitForSecondsRealtime(2.0f);
                if (PlayData.TSavedata.SpRule != null)
                {
                    PlayData.TSavedata.CantStoryAndAchievethisrun = false;
                }
                Debug.Log("Go Ark Exec complete.");
                yield break;
            }
        }

        [HarmonyPatch(typeof(StartPartySelect), nameof(StartPartySelect.Apply)),]
        class StartPartyApplyPatch
        {
            static void Postfix()
            {
                if (PlayData.TSavedata.SpRule != null)
                {
                    PlayData.TSavedata.CantStoryAndAchievethisrun = false;
                }
                Debug.Log("Start Party Fix Complete.");
            }
        }
        
        [HarmonyPatch(typeof(StartPartySelect), nameof(StartPartySelect.SpecialApply)),]
        class StartPartyApplyPatch2
        {
            static void Postfix()
            {
                if (PlayData.TSavedata.SpRule != null)
                {
                    PlayData.TSavedata.CantStoryAndAchievethisrun = false;
                }
                Debug.Log("Start Party Fix2 Complete.");
            }
        }
        
        [HarmonyPatch(typeof(CharSelectMainUIV2), nameof(CharSelectMainUIV2.SpecialApply)),]
        class CharSelectUIApplyPatch
        {
            static void Postfix()
            {
                if (PlayData.TSavedata.SpRule != null)
                {
                    PlayData.TSavedata.CantStoryAndAchievethisrun = false;
                }
                Debug.Log("CharSelectMainUIV2 Fix Complete.");
            }
        }
        
        [HarmonyPatch(typeof(CharSelectMainUIV2), nameof(CharSelectMainUIV2.Apply)),]
        class CharSelectUIApplyPatch2
        {
            static void Postfix()
            {
                if (PlayData.TSavedata.SpRule != null)
                {
                    PlayData.TSavedata.CantStoryAndAchievethisrun = false;
                }
                Debug.Log("CharSelectMainUIV2 Fix2 Complete.");
            }
        }

        [HarmonyPatch(typeof(B_S4_King_minion_0_0_T), nameof(B_S4_King_minion_0_0_T.Init))]
        class KingTighteningChainFix
        {
            static void Postfix(B_S4_King_minion_0_0_T __instance)
            {
                if (PlayData.TSavedata.SpRule != null && __instance.BChar.MyTeam.AliveChars.Count < 2)
                {
                    __instance.PlusStat.Stun = false;
                    __instance.PlusStat.spd = -10;
                    __instance.PlusStat.dod = -50f;
                    __instance.PlusStat.cri = -100f;
                }
            }
        }
        
        [HarmonyPatch(typeof(B_ProgramMaster_1_Select_T), nameof(B_ProgramMaster_1_Select_T.Init))]
        class ProgramMasterPauseFix
        {
            static void Postfix(B_ProgramMaster_1_Select_T __instance)
            {
                if (PlayData.TSavedata.SpRule != null && __instance.BChar.MyTeam.AliveChars.Count < 2)
                {
                    __instance.PlusStat.Stun = false;
                    __instance.PlusStat.dod = -50f;
                    __instance.PlusStat.cri = -100f;
                }
            }
        }

        [HarmonyPatch(typeof(BattleSystem), nameof(BattleSystem.BattleEnd))]
        class BattleEndPatch
        {
            static void Prefix(SR_Solo __instance)
            {
                if (PlayData.TSavedata.StageNum == 5 && PlayData.TSavedata.SpRule != null && PlayData.TSavedata.Party.Count == 1)
                {
                    PlayData.TSavedata.SpRule.ChellangeClear();
                    PlayData.TSavedata.SpRule.ChellangeClearUnlockAndReward();
                }
            }
        }

        [HarmonyPatch(typeof(SkillBookCharacter_Rare), nameof(SkillBookCharacter_Rare.Use))]
        class RareSkillBookPatch
        {
            static void Postfix(SkillBookCharacter_Rare __instance, ref bool __result)
            {
                if (__result == false && PlayData.TSavedata.SpRule != null && PlayData.TSavedata.Party.Count == 1)
                {
                    List<string> list = new List<string>();
                    list.Add(PlayData.LucyRandomSkill());
                    list.Add(PlayData.LucyRandomSkill());
                    list.Add(PlayData.LucyRandomSkill());
                    List<Skill> list2 = new List<Skill>();
                    foreach (string key in list)
                    {
                        list2.Add(Skill.TempSkill(key, PlayData.BattleLucy, PlayData.TempBattleTeam));
                    }
                    FieldSystem.DelayInput(BattleSystem.I_OtherSkillSelect(list2, new SkillButton.SkillClickDel(__instance.SkillAdd), ScriptLocalization.System_Item.SkillAdd, false, true, true, true, false));
                    MasterAudio.PlaySound("BookFlip", 1f, null, 0f, null, null, false, false);
                    __result = true;
                }
            }
        }
        /*
        [HarmonyPatch(typeof(BattleSystem), nameof(BattleSystem.CheatChack))]
        class KillAll
        {
            static bool Prefix(BattleSystem __instance)
            {
                bool keyDown = Input.GetKeyDown(KeyCode.C);
                if (keyDown)
                {
                    __instance.CheatEnabled();
                    foreach (BattleEnemy battleEnemy3 in __instance.EnemyList)
                    {
                        battleEnemy3.Info.Hp = 0;
                        battleEnemy3.Dead(false);
                    }
                }

                {
                    bool keyDown2 = Input.GetKeyDown(KeyCode.F);
                    if (keyDown2)
                    {
                        __instance.CheatEnabled();
                        using (List<BattleAlly>.Enumerator enumerator = __instance.AllyList.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                BattleAlly battleAlly = enumerator.Current;
                                battleAlly.Recovery = battleAlly.GetStat.maxhp;
                                battleAlly.Heal(battleAlly, 99f, false, true, null);
                            }
                        }
                    }
                }

                return false;
            }
        }
        */
        
        
    }
}