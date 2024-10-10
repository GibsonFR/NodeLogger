global using BepInEx;
global using BepInEx.IL2CPP;
global using BepInEx.IL2CPP.Utils.Collections;
global using System;
global using System.IO;
global using System.Collections.Generic;
global using System.Collections;
global using HarmonyLib;
global using UnityEngine;
global using UnhollowerRuntimeLib;
global using static NodeLogger.Variables;
global using static NodeLogger.Utility;
global using static NodeLogger.ClientData;
global using static NodeLogger.ServerData;

namespace NodeLogger
{
    /// <summary>
    /// Main plugin class for NodeLogger. Initializes the plugin, registers custom classes, and applies Harmony patches.
    /// </summary>
    [BepInPlugin("gibson.node.logger", "NodeLogger", "1.0.0")]
    public class Plugin : BasePlugin
    {
        /// <summary>
        /// Called when the plugin is loaded. Registers types and applies Harmony patches.
        /// </summary>
        public override void Load()
        {
            // Register custom MonoBehaviour classes to work with IL2CPP
            ClassInjector.RegisterTypeInIl2Cpp<MainManager>();
            ClassInjector.RegisterTypeInIl2Cpp<NodeLoggingManager>();

            // Apply all Harmony patches in the Plugin class
            Harmony.CreateAndPatchAll(typeof(Plugin));

            // Create necessary folders and reset log files
            CreateFolder(mainFolderPath, logFilePath);
            CreateFolder(nodeMapFolderPath, logFilePath);
            CreateFile(logFilePath, logFilePath);
            ResetFile(logFilePath, logFilePath);

            Log.LogInfo("Mod created by Gibson, discord: gib_son, github: GibsonFR");
        }

        /// <summary>
        /// Harmony patch applied to the Awake method of GameUI. 
        /// Adds the Main and NodeLoggingController components to the GameUI object.
        /// </summary>
        [HarmonyPatch(typeof(GameUI), "Awake")]
        [HarmonyPostfix]
        public static void UIAwakePatch(GameUI __instance)
        {
            GameObject plugin = new();
            plugin.AddComponent<MainManager>();
            plugin.AddComponent<NodeLoggingManager>();

            plugin.transform.SetParent(__instance.transform); // Attach to GameUI's transform
        }

        /// <summary>
        /// Harmony patches for bypassing anti-cheat checks. 
        /// Disables specific methods to avoid interference with the plugin's functionality.
        /// </summary>
        [HarmonyPatch(typeof(EffectManager), "Method_Private_Void_GameObject_Boolean_Vector3_Quaternion_0")]
        [HarmonyPatch(typeof(LobbyManager), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(MonoBehaviourPublicVesnUnique), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(LobbySettings), "Method_Public_Void_PDM_2")]
        [HarmonyPatch(typeof(MonoBehaviourPublicTeplUnique), "Method_Private_Void_PDM_32")]
        [HarmonyPrefix]
        public static bool Prefix(System.Reflection.MethodBase __originalMethod)
        {
            // Prevent the original method from executing
            return false;
        }
    }
}
