﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UIRAtlasAllocator;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Text;
using UnityEngine.Playables;
using static CropsDatabase;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {




        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Animal  Loot increase
        // Drop specific number extra of whatever the first item in the Animal's loot table is

        [HarmonyPatch(typeof(AnimalNPC), "Hit")]
        [HarmonyPostfix]
        private static void AnimalNPCHitPostFix(AnimalNPC __instance)
        {
            DebugLog($"AnimalNPC.Hit.PostFix: {__instance.GetType().ToString()} with {__instance.lives} health remaining");
            if (__instance.lives > 0) return; // Not dead
            int extraItems = 0;
            if (_cowLootExtra.Value > 0 && __instance.GetType() == typeof(CowNPC)) extraItems = _cowLootExtra.Value;
            if (_pigLootExtra.Value > 0 && __instance.GetType() == typeof(PigNPC)) extraItems = _pigLootExtra.Value;
            if (_sheepLootExtra.Value > 0 && __instance.GetType() == typeof(SheepNPC)) extraItems = _sheepLootExtra.Value;
            if (_chickenLootExtra.Value > 0 && __instance.GetType() == typeof(ChickenNPC)) extraItems = _chickenLootExtra.Value;
            if (extraItems == 0) return;
            DebugLog($"AnimalNPC.Hit.PostFix: Spawning {extraItems} extra items");
            Animal animalItem = __instance.placeable.itemSetup.item as Animal;
            ItemProduction[] reflectedSacrificeItems = Traverse.Create(animalItem).Field("sacrificeItems").GetValue<ItemProduction[]>();
            Food food = reflectedSacrificeItems[0].item as Food;
            DroppedItem.SpawnDroppedItem(__instance.gameObject.transform.position, food, extraItems, false, false, 0);
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Animal  No needs
        
        [HarmonyPatch(typeof(AnimalNPC), "Update")]
        [HarmonyPrefix]
        private static void AnimalNPCUpdatePrefix(AnimalNPC __instance)
        {
            if (_AnimalsNoNeeds.Value)
            {
                __instance.hasWalked = true;
                __instance.hasWater = true;
                __instance.hasFood = true;
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Milk Everyday

        [HarmonyPatch(typeof(AnimalNPC), "IncrementProduction")]  //Called once per day, by the barn?
        [HarmonyPrefix]
        private static void AnimalNPCIncrementProductionPrefix(AnimalNPC __instance)
        {
            if (_fasterMilk.Value)
            {
                Animal a = __instance.placeable.itemSetup.item as Animal;
                if (a.productionLimitedToOnce) __instance.productionProgress = Mathf.Max(1f, __instance.productionProgress);
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Infinite Milk

        [HarmonyPatch(typeof(CowNPC), "MouseHold")]  
        [HarmonyPostfix]
        private static void CowNPCMouseHoldPostfix(CowNPC __instance)
        {
            DebugLog($"CowNPCMouseHoldPostfix()");
            if (_infiniteMilk.Value)
            {
                Animal a = __instance.placeable.itemSetup.item as Animal;
                __instance.productionProgress = 1f;
            }
        }

        [HarmonyPatch(typeof(SheepNPC), "MouseHold")]
        [HarmonyPostfix]
        private static void SheepNPCMouseHoldPostfix(SheepNPC __instance)
        {
            DebugLog($"SheepNPCMouseHoldPostfix()");
            if (_infiniteMilk.Value)
            {
                Animal a = __instance.placeable.itemSetup.item as Animal;
                __instance.productionProgress = 1f;
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // More Eggs
        [HarmonyPatch(typeof(ChickenNPC), "IncrementProduction")]  //Called once per day, by the barn?
        [HarmonyPrefix]
        private static void ChickenNPCIncrementProductionPrefix(ChickenNPC __instance)
        {
            if (_moreEggs.Value > 0)
            {
                HenHouse h = __instance.currentBuilding as HenHouse;
                h.IncrementEggsAmount(_moreEggs.Value);

            }

        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Never Sick

        [HarmonyPatch(typeof(AnimalNPC), "Sick")]  //Called once per day, by the barn?
        [HarmonyPrefix]
        private static bool AnimalNPCSickPrefix(AnimalNPC __instance)
        {
            return !_AnimalsNoSick.Value;

        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Rapid Growth

        [HarmonyPatch(typeof(AnimalNPC), "IncrementLevel")]  //Called once per day, by the barn?
        [HarmonyPrefix]
        private static void AnimalNPCIncrementLevelPrefix(AnimalNPC __instance)
        {
            if (_AnimalsFastGrow.Value)
            {
                __instance.level = __instance.maxLevel;
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Rapid Growth

        [HarmonyPatch(typeof(AnimalNPC), "IncrementLevel")]  //Called once per day, by the barn?
        [HarmonyPostfix]
        private static void AnimalNPCIncrementLevelPostfix(AnimalNPC __instance)
        {
            if (_AnimalsNoNeeds.Value)
            {
                __instance.hasWalked = true;
                __instance.hasWater = true;
                __instance.hasFood = true;
            }
        }

    }

}
