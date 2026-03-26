using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SharedFishiesnDishes
{
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += OnSync;
            helper.Events.GameLoop.DayEnding += OnSync;

            helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived;

            Monitor.Log("Shared Fishies 'n Dishes mod loaded !", LogLevel.Info);
        }

        private void OnSync(object sender, System.EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            var data = new SyncData();

            //Checking Cooking Recipes
            foreach (string recipe in Game1.player.recipesCooked.Keys)
            {
                if (Game1.player.recipesCooked[recipe] > 0)
                {
                    data.CookedRecipes.Add(recipe);
                }
            }

            //Checking Fish Caught
            foreach (string fishId in Game1.player.fishCaught.Keys)
            {
                int[] fishData = Game1.player.fishCaught[fishId];

                if (fishData != null && fishData.Length > 0 && fishData[0] > 0)
                {
                    data.CaughtFish.Add(fishId);
                }
            }

            Helper.Multiplayer.SendMessage(
                data,
                "SyncProgress",
                new[] { ModManifest.UniqueID }
            );

        }

        private void OnMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModManifest.UniqueID || e.Type != "SyncProgress")
                return;

            var data = e.ReadAs<SyncData>();

            int recipesAdded = 0;
            int fishAdded = 0;

            //Syncing Cooking Recipes
            foreach (string recipe in data.CookedRecipes)
            {
                if (!Game1.player.recipesCooked.ContainsKey(recipe) || Game1.player.recipesCooked[recipe] == 0)
                {
                    Game1.player.recipesCooked[recipe] = 1;
                    recipesAdded++;
                }
            }

            //Syncing Fish Caught
            foreach (string fishId in data.CaughtFish)
            {
                if (!Game1.player.fishCaught.ContainsKey(fishId))
                {
                    Game1.player.fishCaught[fishId] = new int[] { 1, 0 };
                    fishAdded++;
                }
                else
                {
                    int[] fishData = Game1.player.fishCaught[fishId];

                    if (fishData != null && fishData.Length > 0 && fishData[0] == 0)
                    {
                        fishData[0] = 1;
                        fishAdded++;
                    }
                }
            }

            if (recipesAdded > 0 || fishAdded > 0)
            {
                Monitor.Log($"[Apply] Added Recipes={recipesAdded}, Fish={fishAdded}", LogLevel.Info);
            }
        }

        private class SyncData
        {
            public List<string> CookedRecipes { get; set; } = new();
            public List<string> CaughtFish { get; set; } = new();
        }
    }
}