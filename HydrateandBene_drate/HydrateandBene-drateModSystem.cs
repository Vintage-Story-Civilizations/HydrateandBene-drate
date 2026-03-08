using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using HydrateOrDiedrate;

namespace HydrateandBene_drate;

public class HydrateandBene_drateModSystem : ModSystem
{
    MyConfigData? config;
    ICoreServerAPI? sapi;
    Dictionary<string, long> lastApplyTime = new();

    public override void StartServerSide(ICoreServerAPI api)
    {
        this.sapi = api;
        TryToLoadConfig(api);
        api.Event.RegisterGameTickListener(OnGameTick, 2000); // Check every second.
    }

    private void TryToLoadConfig(ICoreServerAPI api)
    {
        //It is important to surround the LoadModConfig function in a try-catch. 
        //If loading the file goes wrong, then the 'catch' block is run.
        try
        {
            config = api.LoadModConfig<MyConfigData>("hydrateandbenedrate.json");
            if (config == null) //if the 'MyConfigData.json' file isn't found...
            {
                config = new MyConfigData();
            }
            //Save a copy of the mod config.
            api.StoreModConfig<MyConfigData>(config, "hydrateandbenedrate.json");
        }
        catch (Exception e)
        {
            //Couldn't load the mod config... Create a new one with default settings, but don't save it.
            Mod.Logger.Error("Could not load config! Loading default settings instead.");
            Mod.Logger.Error(e);
            config = new MyConfigData();
        }
    }

    private void OnGameTick(float obj)
    {
        if (sapi == null || config == null) return;

        long now = sapi.World.ElapsedMilliseconds;
        int durationMs = (int)(config.BuffDurationMinutes * 60 * 1000);

        foreach (var player in sapi.World.AllOnlinePlayers)
        {
            var entity = player.Entity;
            if (entity == null) continue;

            // Get Thirst
            var thirst = entity.GetBehavior<EntityBehaviorThirst>();
            if (thirst == null) continue;

            float hydrationPercent = thirst.CurrentThirst / thirst.MaxThirst;

            // Get Hunger
            float hungerPercent = entity.WatchedAttributes.GetFloat("hunger") / entity.WatchedAttributes.GetFloat("maxhunger");

            // Check if both > 50%
            if (hydrationPercent > config.Threshold && hungerPercent > config.Threshold)
            {
                // Check if buff not recently applied
                if (!lastApplyTime.ContainsKey(player.PlayerUID) || now - lastApplyTime[player.PlayerUID] >= durationMs)
                {
                    // Apply lowered rates for 10 mins
                    entity.Stats.Set("hungerrate", "hydrationMod", config.HungerRateMultiplier, false);
                    entity.Stats.Set(HoDStats.ThirstRateMul, "hydrationMod", config.ThirstRateMultiplier, false);
                    lastApplyTime[player.PlayerUID] = now;
                }
            }
            else
            {
                // Reset to normal
                entity.Stats.Set("hungerrate", "hydrationMod", 1f, false);
                entity.Stats.Set(HoDStats.ThirstRateMul, "hydrationMod", 1f, false);
                // Remove from lastApplyTime if exists
                lastApplyTime.Remove(player.PlayerUID);
            }
        }
    }
}

    public class MyConfigData
    {
        public float Threshold = 0.5f;
        public float HungerRateMultiplier = 0.5f; // Lower hunger rate to half
        public float ThirstRateMultiplier = 0.5f; // Lower thirst rate to half
        public float BuffDurationMinutes = 10f;
    }

