﻿#undef CONSTRUCTIONFEATURES 

using BepInEx;
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
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public partial class Plugin : BaseUnityPlugin
    {
        private static Harmony _harmony;
        internal static ManualLogSource Log; // static copy of the BaseUnityPligin.Logger object so it can be accessed in static methods, initilized in constructor

        private static ConfigEntry<bool> _debugLogging;
        private static ConfigEntry<bool> _dumpItemListOnStart;
        private static ConfigEntry<int>  _dispensorStackSize;
        private static ConfigEntry<int> _itemStackSize;
        private static ConfigEntry<int> _agingBarrelStackSize;
        private static ConfigEntry<float> _moveSpeed;
        private static ConfigEntry<float> _moveRunMult;
        private static ConfigEntry<bool> _soilStaysWatered;
        private static ConfigEntry<bool> _soilWet3DaysRain;
        private static ConfigEntry<bool> _recipesNoFuel;
        private static ConfigEntry<int> _recipesQuickCook;
        private static ConfigEntry<bool> _recipesNoFragments;
        private static ConfigEntry<bool> _fireplaceNoFuelUse;
        private static ConfigEntry<bool> _dumpRecipeListOnStart;
        private static ConfigEntry<bool> _dumpCropListOnStart;
        private static ConfigEntry<bool> _dumpStaffGenDataOnStart;
        private static ConfigEntry<bool> _dumpReputationListOnStart;
        private static ConfigEntry<bool> _dumpIngredientGroupListOnStart;
        private static ConfigEntry<bool> _CropFastGrow;
        private static ConfigEntry<bool> _CropFastRegrow;
        private static ConfigEntry<bool> _staffNoNeg;
        private static ConfigEntry<bool> _staffRefreshOnOpen;
        private static ConfigEntry<bool> _staffAlways3Perks;
        private static ConfigEntry<int> _staffLevel;
        private static ConfigEntry<int> _moreTiles;
        private static ConfigEntry<int> _moreZones;
        private static ConfigEntry<int> _moreRooms;
        private static ConfigEntry<int> _moreCustomers;
        private static ConfigEntry<int> _moreDisponible;
        private static ConfigEntry<bool> _wilsonOneCoin;
        private static ConfigEntry<float> _moreValuableFish;
        private static ConfigEntry<float> _moreValuableMeat;
        private static ConfigEntry<float> _moreValuableVege;
        private static ConfigEntry<float> _moreValuableGrain;
        private static ConfigEntry<float> _moreValuableFruit;
        private static ConfigEntry<bool> _allSeasonCrops;
        private static ConfigEntry<bool> _badBirdIsFunny;
        private static ConfigEntry<bool> _walkThroughCrops;
        private static ConfigEntry<float> _xpMult;
        private static ConfigEntry<KeyCode> _hotkeyGrowCrops;
        private static ConfigEntry<KeyCode> _hotKeyBirdTalk;
        private static ConfigEntry<KeyCode> _hotkeyGrowTrees;
        private static ConfigEntry<KeyCode> _whatIsThatTree;
        private static ConfigEntry<KeyCode> _regrowRegrowables;

#if CONSTRUCTIONFEATURES
        private static ConfigEntry<bool> _buildNoMatsUsed;
        private static ConfigEntry<bool> _buildNoMatsUsedFarm;
#endif
        private static ConfigEntry<bool> _endlessWater;
        private static bool setupDoneItems = false;
        private static bool setupDoneRecipes = false;
        private static bool setupDoneCrops = false;
        private static bool setupDoneStaffManager = false;
        private static bool setupDoneCustomerInfo = false;

        private static ConfigEntry<int> _cowLootExtra;
        private static ConfigEntry<int> _chickenLootExtra;
        private static ConfigEntry<int> _pigLootExtra;
        private static ConfigEntry<int> _sheepLootExtra;
        private static ConfigEntry<bool> _GrowTreesTypeFix;
        private static ConfigEntry<int> _moreMilk;
        public static ConfigEntry<bool> _custCleanRooms;
        public static ConfigEntry<bool> _custCleanFloor;
        public static ConfigEntry<bool> _custCleanTable;
        public static ConfigEntry<float> _custFastEating;
        public static ConfigEntry<bool> _custNeverAngry;
        public static ConfigEntry<bool> _custCanCalm;
        public static ConfigEntry<bool> _custMorePatient;
        public static ConfigEntry<bool> _custNeverLeave;
        public static ConfigEntry<bool> _custAlwaysLeave;
        public static ConfigEntry<bool> _custIgnoreDirt;
        private static ConfigEntry<bool> _AnimalsNoNeeds;
        private static ConfigEntry<bool> _AnimalsNoSick;
        private static ConfigEntry<bool> _fasterMilk;


        public Plugin()
        {
            // bind to config settings
            _debugLogging = Config.Bind("Debug", "Debug Logging", false, "Logs additional information to console");

            _dispensorStackSize = Config.Bind("Stacks", "Tap/Keg Stack Size", -1, "Change the amount of drinks you can store in taps/kegs; set to -1 to disable, set to 0 to use item stack size");
            _agingBarrelStackSize = Config.Bind("Stacks", "Aging Barrel Stack Size", -1, "Change the amount of drinks you can store in aging barrels; set to -1 to disable, set to 0 to use item stack size");
            _itemStackSize = Config.Bind("Stacks", "Item Stack Size", -1, "Change the stack size of any item that normally stacks to 99; set to -1 to disable");

            _dumpItemListOnStart = Config.Bind("Database", "List Items on start", false, "set to true to print a list of all items to console on startup");
            _dumpRecipeListOnStart = Config.Bind("Database", "List Recipes on start", false, "set to true to print a list of all recipes to console on startup");
            _dumpReputationListOnStart = Config.Bind("Database", "List Reputation milestones on start", false, "set to true to print a list of all reputation milestones to console on startup");
            _dumpStaffGenDataOnStart = Config.Bind("Database", "List staff generation data on start", false, "set to true to print a list of staff generation data on startup");
            _dumpIngredientGroupListOnStart = Config.Bind("Database", "List Ingredient Group data on start", false, "set to true to print a list of ingredient Groups on startup");
            _dumpCropListOnStart = Config.Bind("Database", "List Crops on start", false, "set to true to print a list of all crops to console on startup");

            _moveSpeed = Config.Bind("Movement", "Walking Speed", 2.5f, "walking speed; set to 2.5 for default speed ");
            _moveRunMult = Config.Bind("Movement", "Run Speed Multiplier", 1.6f, "run speed multiplier; set to 1.6 for default speed ");

            _soilStaysWatered = Config.Bind("Farming", "Soil Stays Wet", false, "Soil stays watered");
            _soilWet3DaysRain = Config.Bind("Farming", "Rain Fully waters soil", false, "rain will make soil wet for the next 3 days, like watering");
            _CropFastGrow = Config.Bind("Farming", "Fast Growing Crops", false, "All crops advance one growth stage per day");
            _CropFastRegrow = Config.Bind("Farming", "Fast Regrowing Crops", false, "Crops that allow multiple harvests can be harvested every day");
            _walkThroughCrops = Config.Bind("Farming", "Walk Through Crops", false, "Lets you walk through your crops.");
            _hotkeyGrowCrops = Config.Bind("Farming", "grow all crops hotkey", KeyCode.None, "Press to instantly grow planted crops");
            _GrowTreesTypeFix = Config.Bind("Farming", "grow trees type fix", true, "workarpund for tree growth key changeing type");
            _hotkeyGrowTrees = Config.Bind("Farming", "grow all trees hotkey", KeyCode.None, "Press to instantly grow all trees");
            _allSeasonCrops = Config.Bind("Farming", "All-season crops", false, "All crops can be grown in any season.");
            _whatIsThatTree = Config.Bind("Farming", "What is that tree", KeyCode.None, "For Troubleshooting - lists details of crop trees");
            _regrowRegrowables = Config.Bind("Farming", "Regrow Regrowales", KeyCode.None, "NOT DONE makes re-harvestable crops & trees ready to harvest");


            _cowLootExtra = Config.Bind("Animals", "Cow Bonus Loot", 0, "Increase Cow loot by this amount; set to 0 to disable");
            _pigLootExtra = Config.Bind("Animals", "Pig Bonus Loot", 0, "Increase Pig loot by this amount; set to 0 to disable");
            _chickenLootExtra = Config.Bind("Animals", "Chicken Bonus Loot", 0, "Increase Chicken loot by this amount; set to 0 to disable");
            _sheepLootExtra = Config.Bind("Animals", "Sheep Bonus Loot", 0, "Increase Sheep loot by this amount; set to 0 to disable");
            _moreMilk = Config.Bind("Animals", "More Milk", 0, "NOT DONE Higher Milk Yield");
            _fasterMilk = Config.Bind("Animals", "Milk Every Day", false, "Animals provide milk every day");
            _AnimalsNoNeeds = Config.Bind("Animals", "No Needs", false, "Animals don't need food/water/walking");
            _AnimalsNoSick = Config.Bind("Animals", "No Sickness", false, "NOT DONE Animals never get sick");


            _recipesNoFuel = Config.Bind("Recipes", "No Fuel", false, "Recipes no longer require fuel");
            _recipesNoFragments = Config.Bind("Recipes", "No Fragment Cost", false, "Cave Recipies only cost one fragment");
            _recipesQuickCook = Config.Bind("Recipes", "Quick Crafting", -1, "Sets the maximum time recipes take to craft in minutes; set to -1 to disable");

            _staffNoNeg = Config.Bind("Staff", "No Negative Perks", false, "New Staff will not have any negative perks");
            _staffRefreshOnOpen = Config.Bind("Staff", "Refresh Applicants on Open", false, "Refresh the list of new staff available to hire every time the hiring interface is opened");
            _staffAlways3Perks = Config.Bind("Staff", "Always Three Perks", false, "NOT WORKING New hires will always have three positive perks");
            _staffLevel = Config.Bind("Staff", "Starting Level", -1, "Starting level for new hires; set to -1 to disable, set to 31 for all three skills at level 5");

            _moreTiles = Config.Bind("Milestones", "More Zone Tiles", -1, "increase number of tiles for crafting/dining zone; set to -1 to disable");
            _moreZones = Config.Bind("Milestones", "More Crafting Zones", -1, "NOT WELL TESTED increase number of zones for crafting; set to -1 to disable");
            _moreRooms = Config.Bind("Milestones", "More Rentable Rooms", -1, "increase number of rooms for rent; set to -1 to disable");
            _moreCustomers = Config.Bind("Milestones", "More Customers", -1, "increase customer capacity; set to -1 to disable");
            _moreDisponible = Config.Bind("Milestones", "More Floor Tiles", -1, "increase total number of floor tiles allowed; set to -1 to disable");



            _badBirdIsFunny = Config.Bind("Misc", "Naughty Bird is Funny", false, "Patrons like a naughty bird, so everything your bird says causes reputation gain instead of loss");
            _fireplaceNoFuelUse = Config.Bind("Misc", "Fireplace does not consume fuel", false, "fireplace no longer consumes fuel");
            _xpMult             = Config.Bind("Misc", "XP Multiplier", 1.0f, "NOT WORKING increase the amopunt of reputation earned; set to 1.0 to disable");
            _endlessWater = Config.Bind("Misc", "Endless Water", false, "DO NOT USE DURING TUTORIAL. PREVENTS FILLING BUCKETS AT WELL. Buckets of water do not empty when used");
            _hotKeyBirdTalk = Config.Bind("Misc", "All Birds Talk", KeyCode.None, "Make your birds say something nice");

            _wilsonOneCoin = Config.Bind("Prices", "Wilson Price Reduction", false, "Wilson only charges 1 coin per item");
            //Setting sellPrice doesn't work for objects where the price gets determined (at least partly) by the stuff it is made of.
            _moreValuableFish = Config.Bind("Prices", "Fish price increase", 1.0f, "increase the value of fish/shellfish; set to 1.0 to disable");
            _moreValuableMeat = Config.Bind("Prices", "Meat price increase", 1.0f, "increase the value of meat; set to 1.0 to disable");
            _moreValuableVege = Config.Bind("Prices", "Vege price increase", 1.0f, "increase the value of Vegetables/Legumes; set to 1.0 to disable");
            _moreValuableFruit = Config.Bind("Prices", "Fruit price increase", 1.0f, "increase the value of Fruit/Berries; set to 1.0 to disable");
            _moreValuableGrain = Config.Bind("Prices", "Grain price increase", 1.0f, "increase the value of Grains; set to 1.0 to disable");



            _custCleanRooms = Config.Bind("Customers", "Clean Rooms", false, "rented rooms are kept clean");
            _custCleanFloor = Config.Bind("Customers", "Clean Floors", false, "customers do not make mess on floor (NOTE: staff can still make a mess)");
            _custCleanTable = Config.Bind("Customers", "Clean Tables", false, "customers do not make mess on tables");
            _custFastEating = Config.Bind("Customers", "Fast Eating", 1.0f, "customers eat faster - set to 1.0f to disable, higher to eat faster, lower to eat slower");
            _custNeverAngry = Config.Bind("Customers", "Clean Rooms", false, "customer do not get angry");
            _custCanCalm = Config.Bind("Customers", "Clean Rooms", false, "angry customers can always be calmed down");
            _custMorePatient = Config.Bind("Customers", "More Patience", false, "customers don't mind waiting");
            _custNeverLeave = Config.Bind("Customers", "Customers never leave", false, "customers just keep ordering more food/drink until last call");
            _custAlwaysLeave = Config.Bind("Customers", "Customers always leave", false, "customers leave after one meal/drink (overrides never leave)");
            _custIgnoreDirt = Config.Bind("Customers", "Customers ignore dirt, cold, dark", false, "customers ignore dirt, cold and darkness");

#if CONSTRUCTIONFEATURES
            _buildNoMatsUsed = Config.Bind("Building", "No Materials used", false, "Building materials not consumed by construction (you still need enough to do the construction)");
            _buildNoMatsUsedFarm = Config.Bind("Building", "Farm Construction No Materials used", false, "TEST WITH BARN/COOP CONSTRUCTION");
#endif

        }


        // ----------------------------------------------------
        // Some Accessor objects
        // -------------
        // This horendous pile of jank is because TavernReputation has three private int fields and the one with the repvalue has a random name.  
        //
        // I wonder if this could be replaced with using ILCode to steal the location/name of value from TavernReputation.GetReputationExp()

        private static FieldInfo myRepLocation;
        private static bool foundRepLocation=false;
        private static TavernReputation myTavernReputation = null;
        public static int repAccessor
        {
            get
            {
                return TavernReputation.GetReputationExp(); 
            }
            set
            {
                if (myTavernReputation is null) myTavernReputation = UnityEngine.Object.FindObjectOfType<TavernReputation>();

                if (foundRepLocation)
                {
                    myRepLocation.SetValue(myTavernReputation, value);
                }
                else
                {
                    TryFindTavernReputationValue();
                    if (foundRepLocation)
                    {
                        myRepLocation.SetValue(myTavernReputation, value);
                    }
                    else
                    {
                        DebugLog($"Still Looking for TavernReputation rep value, unable to set to {value}"); ;
                    }
                }
            }
        }
        public static bool TryFindTavernReputationValue() //RestfulTweaks.Plugin.TryFindTavernReputationValue()
        {
            DebugLog($"TryFindTavernReputationValue(): Looking for reputation location...");
            TavernReputation t = UnityEngine.Object.FindObjectOfType<TavernReputation>();
            int found = 0;
            int foundCorrect = 0;
            FieldInfo[] tavRepFieldInfo = t.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance); //all private fields.
            foreach (FieldInfo fi in tavRepFieldInfo)
            {
                if (fi.FieldType == typeof(int))
                {
                    found++;
                    if ((int)fi.GetValue(t) == TavernReputation.GetReputationExp())
                    {
                        DebugLog($"TryFindTavernReputationValue(): found private int field with value: {TavernReputation.GetReputationExp()} name: \"{fi.Name}\"  hash: {fi.GetHashCode()}");
                        myRepLocation =fi;
                        foundCorrect++;
                    }
                }
            }
            DebugLog($"TryFindTavernReputationValue(): Found {found} ints, {foundCorrect} with correct value");
            if (foundCorrect == 1)
            {
                foundRepLocation = true;
                return true;
            }
            return false;
        }

        // -------------
        private static CommonReferences myCommonReferences;
        public static CommonReferences commonReferences
        {
            get
            {
                if (myCommonReferences == null) myCommonReferences = UnityEngine.Object.FindObjectOfType<CommonReferences>();
                return myCommonReferences;
            }
        }
        // -------------
        private static ItemDatabaseAccessor myItemDatabaseAccessor;
        public static ItemDatabaseAccessor itemDatabaseAccessor
        {
            get
            {
                if (myItemDatabaseAccessor == null) myItemDatabaseAccessor = UnityEngine.Object.FindObjectOfType<ItemDatabaseAccessor>();
                return myItemDatabaseAccessor;
            }
        }
        // -------------
        private static ItemDatabase myitemDatabaseSO;
        public static ItemDatabase itemDatabaseSO
        {
            get
            {
                if (myitemDatabaseSO == null) myitemDatabaseSO = Traverse.Create(itemDatabaseAccessor).Field("itemDatabaseSO").GetValue<ItemDatabase>();
                return myitemDatabaseSO;
            }
        }
        // -------------
        private static RecipeDatabaseAccessor myRecipeDatabaseAccessor;
        public static RecipeDatabaseAccessor recipeDatabaseAccessor
        {
            get
            {
                if (myRecipeDatabaseAccessor == null) myRecipeDatabaseAccessor = RecipeDatabaseAccessor.GetInstance();
                return myRecipeDatabaseAccessor;
            }
        }
        // -------------
        private static RecipeDatabase myRecipeDatabaseSO;
        public static RecipeDatabase recipeDatabaseSO
        {
            get
            {
                if (myRecipeDatabaseSO == null) myRecipeDatabaseSO = Traverse.Create(recipeDatabaseAccessor).Field("recipeDatabaseSO").GetValue<RecipeDatabase>();
                return myRecipeDatabaseSO;
            }
        }
        // -------------



        // ----------------------------------------------------


        //public static RecipeDatabaseAccessor recipeDatabaseAccessor; //Not needed since RecipeDatabaseAccessor is full of useful static functions


        private void Awake()
        {
            // Plugin startup logic
            Log = Logger;
            _harmony = Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} is loaded!");
        }
        private void Update()
        {
            if (Input.GetKeyDown(_hotKeyBirdTalk.Value))
            {
                AllBirdsTalk();
            }
            else if (Input.GetKeyDown(_hotkeyGrowCrops.Value))
            {
                GrowAllCrops();
            }
            else if (Input.GetKeyDown(_hotkeyGrowTrees.Value))
            {
                GrowAllTrees();
            }
            else if (Input.GetKeyDown(_whatIsThatTree.Value))
            {
                WhatIsThatTree();
            }
            else if (Input.GetKeyDown(_regrowRegrowables.Value))
            {
                RegrowRegrowables();
            }
        }
        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
 
        public static void DebugLog(string message)
        {
            // Log a message to console only if debug is enabled in console
            if (_debugLogging.Value)
            {
                Log.LogInfo(string.Format("DEBUG: {0}", message));
            }
        }

        // //////////////////////////////////////////////////////////////////////
        // A bunch of functions for converting things to text to spit out 
        public static int Price2Copper(Price x)
        {
            return x.gold * 100000 + x.silver * 100 + x.copper;
        }

        public static Price Copper2Price(int x)
        {
            Price p = new Price();
            p.gold = Mathf.FloorToInt((float)(x / 10000));
            x -= p.gold * 10000;
            p.silver = Mathf.FloorToInt((float)(x / 100));
            x -= p.silver * 100;
            p.copper = x;
            return p;
        }

        public static Price PriceXFloat(Price p, float x)
        {
            return Copper2Price(Mathf.FloorToInt(x * Price2Copper(p)));
        }

        public static string Tags2String(Tag[] x)
        {
            return string.Join(":", x);
        }

        public static string Item2String(Item x)
        {
            if (x == null) return "nullItem";
            int xId = Traverse.Create(x).Field("id").GetValue<int>();
            string xName = (x.translationByID) ? LocalisationSystem.Get("Items/item_name_" + xId.ToString()) : x.nameId;
            return String.Format("{0}:{1}",xId, xName);
        }
        public static string ItemMod2String(ItemMod x)
        {
            if (x.item == null) return "-";
            string a = Item2String(x.item); //The base item
            String b = (x.mod == null) ? "" : Item2String(x.mod); //the modifier item, or empty string if no modifier
            return String.Format("[{0}({1})]", a,b ); 
        }
        public static string ItemModList2String(List<ItemMod> x)
        {
            string s = "";
            foreach (ItemMod itemMod in x)
            {
                s += ItemMod2String(itemMod);
            }
            return s;
        }

        public static string RecipeIngredients2String(RecipeIngredient[] x)
        {
            string result = string.Empty;
            for (int i = 0; i < x.Length; i++)
            {
                int id= Traverse.Create(x[i].item).Field("id").GetValue<int>();
                int modId = Traverse.Create(x[i].mod).Field("id").GetValue<int>();
                result += String.Format("[{0}:{1}:{2}]", id, x[i].amount, modId);
            }
            return result;
        }

        public static string IngredientTypes2String(IngredientType[] x)
        {
            string result = string.Empty;
            for (int i = 0; i < x.Length; i++)
            {
                result += String.Format("[{0}]", x[i]);
            }
            return result;
        }



        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // XP Mult
 
        [HarmonyPatch(typeof(TavernReputation), "ChangeReputation")]
        [HarmonyPrefix]
        private static void TavernReputationChangeReputationPrefix(object[] __args)
        {
            if (_xpMult.Value != 1.0f)
            {

                int addedXP = (int)__args[0];

                int extraNeeded = Mathf.FloorToInt((_xpMult.Value - 1.0f) * addedXP);
                DebugLog($"TavernReputation.ChangeReputation.Prefix(): Adding an extra {extraNeeded} rep");
                repAccessor = repAccessor + extraNeeded;
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Reputation Milestone Stuff

        [HarmonyPatch(typeof(ReputationDBAccessor), "Awake")]
        [HarmonyPrefix] //Has to be a prefix so our changes are done before this.SetUpDatabase(); 

        private static void ReputationDBAccessorAwakePrefix(ReputationDBAccessor __instance)
        {
            DebugLog("ReputationDBAccessor.Awake.Prefix");
            ReputationInfo[] repDB = ReputationDBAccessor.GetAllReputations();
            if(_dumpReputationListOnStart.Value) Log.LogInfo("repNumber, craftingTiles, craftingZonesNumber, customersCapacity, diningTiles, diningZonesNumber, floorDisponible, rentedRoomsNumber, repMax");

                         
            for (int i = 0; i < repDB.Length; i++)
            {
                if (_moreTiles.Value > 0) {repDB[i].craftingTiles += _moreTiles.Value; repDB[i].diningTiles += _moreTiles.Value;}
                if (_moreZones.Value > 0) repDB[i].craftingZonesNumber += _moreZones.Value;
                if (_moreRooms.Value > 0) repDB[i].rentedRoomsNumber += _moreRooms.Value;
                if (_moreCustomers.Value > 0) repDB[i].customersCapacity += _moreCustomers.Value;
                if (_moreDisponible.Value > 0) repDB[i].floorDisponible += _moreDisponible.Value;

                if (_dumpReputationListOnStart.Value) Log.LogInfo(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}",
                    repDB[i].repNumber, repDB[i].craftingTiles, repDB[i].craftingZonesNumber, repDB[i].customersCapacity, repDB[i].diningTiles, 
                    repDB[i].diningZonesNumber, repDB[i].floorDisponible, repDB[i].rentedRoomsNumber, repDB[i].repMax));
            }
        }






        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Player Speed

        [HarmonyPatch(typeof(PlayerController), "Awake")]
        [HarmonyPostfix]
        private static void setPlayerSpeed(PlayerController __instance)
        {
            __instance.speed = _moveSpeed.Value;
            __instance.sprintMultiplier = _moveRunMult.Value;
        }




        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Fireplace does not consume fuel

        [HarmonyPatch(typeof(Fireplace), "Update")]
        [HarmonyPrefix]
        static bool FireplaceUpdatePrefix(Fireplace __instance)
        {
            if (_fireplaceNoFuelUse.Value)
            {
                return false; //just disable the update so fuel is never checvked
            }
            else
            {
                return true; // flow thorugh to normal Update
            }
        }

    }
}
