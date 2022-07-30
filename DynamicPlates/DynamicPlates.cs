using ABI_RC.Core.Player;
using HarmonyLib;
using MelonLoader;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(DynamicPlates.DynamicPlates), "DynamicPlates", "1.0.0", "ImTiara", "https://github.com/ImTiara/CVRMods")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace DynamicPlates
{
    public class DynamicPlates : MelonMod
    {
        public static MelonPreferences_Entry<bool> ENABLE;
        public static MelonPreferences_Entry<float> HEIGHT;

        public override void OnApplicationStart()
        {
            var category = MelonPreferences.CreateCategory("DynamicPlates", "Dynamic Plates");
            ENABLE = category.CreateEntry("Enabled", true, "Enable Dynamic Plates");
            HEIGHT = category.CreateEntry("Height", 0.4f, "Height Offset");

            ENABLE.OnValueChanged += (editedValue, defaultValue) =>
            {
                if (ENABLE.Value)
                {
                    var playerNameplate = Resources.FindObjectsOfTypeAll<PlayerNameplate>();
                    for (int i = 0; i < playerNameplate.Length; i++)
                    {
                        OnPlayerNameplateStart(ref playerNameplate[i]);

                        playerNameplate[i].GetComponentInChildren<CanvasGroup>().alpha = 1.0f;
                    }
                }
                else
                {
                    var dynamicPlate = Resources.FindObjectsOfTypeAll<DynamicPlate>();
                    for (int i = 0; i < dynamicPlate.Length; i++)
                    {    
                        dynamicPlate[i].transform.localScale = Vector3.one;

                        Object.DestroyImmediate(dynamicPlate[i]);
                    }

                    DynamicPlate.dynamicPlates.Clear();
                }
            };

            HarmonyInstance.Patch(typeof(PlayerNameplate).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance),
                new HarmonyMethod(typeof(DynamicPlates).GetMethod("OnPlayerNameplateStart", BindingFlags.Public | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(PuppetMaster).GetMethod("AvatarInstantiated", BindingFlags.Public | BindingFlags.Instance), null,
                new HarmonyMethod(typeof(DynamicPlates).GetMethod("OnAvatarInstantiated", BindingFlags.Public | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(PlayerNameplate).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance),
                new HarmonyMethod(typeof(DynamicPlates).GetMethod("OnPlayerNameplateUpdate", BindingFlags.Public | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(PlayerNameplate).GetMethod("SetNameplatePos", BindingFlags.Public | BindingFlags.Instance),
                new HarmonyMethod(typeof(DynamicPlates).GetMethod("OnSetNameplatePos", BindingFlags.Public | BindingFlags.Static)));
        }

        public static void OnPlayerNameplateStart(ref PlayerNameplate __instance)
        {
            if (!ENABLE.Value) return;

            var dynamicPlate = __instance.gameObject.AddComponent<DynamicPlate>();
            dynamicPlate.puppetMaster = __instance.transform.parent.GetComponent<PuppetMaster>();
            DynamicPlate.dynamicPlates.Add(dynamicPlate.puppetMaster, dynamicPlate);

            dynamicPlate.OnAvatarChanged();
        }

        public static void OnAvatarInstantiated(ref PuppetMaster __instance)
        {
            DynamicPlate.dynamicPlates[__instance].OnAvatarChanged();
        }

        public static bool OnPlayerNameplateUpdate() => !ENABLE.Value;
        public static bool OnSetNameplatePos() => !ENABLE.Value;
    }
}