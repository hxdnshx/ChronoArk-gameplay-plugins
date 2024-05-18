using System;
using System.Linq;
using ChronoArkMod;
using ChronoArkMod.Plugin;
using UnityEngine;

namespace SoloAllStage
{
    public class WorkshopLoader
    {
        [PluginConfig(VersionInfo.GUID, VersionInfo.name, VersionInfo.version)]
        public class WorkshopPlugin : ChronoArkPlugin
        {

            public static GameObject attachObject;

     
            public override void Initialize()
            {

                //if (!LoadedByBepinex())
                //{
                Debug.Log($"Loading {VersionInfo.GUID} from workshop");
                //}
                HarmonyPatches.PatchAll();
            }

            public override void Dispose()
            {
                HarmonyPatches.UnpatchAll();
                //UnityEngine.Object.Destroy(attachObject);
            }

            bool LoadedByBepinex()
            {

                // there's gotta be a better way
                if (AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.GetName().Name == "BepInEx") != null)
                {
                    //return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(VersionInfo.GUID);
                }
                return false;
            }

        }
    }
}