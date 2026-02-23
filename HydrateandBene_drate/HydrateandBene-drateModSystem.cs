using System;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using HydrateOrDiedrate;
namespace HydrateandBene_drate;



public class HydrateandBene_drateModSystem : ModSystem
{
    MyConfigData? config;

    ICoreServerAPI? sapi;

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
        if (sapi == null) return;

        foreach (var player in sapi.World.AllOnlinePlayers)
        {
            var entity = player.Entity;
            if (entity == null) continue;

            // Get Thirst
            var thirst = entity.GetBehavior<EntityBehaviorThirst>();
            if (thirst == null) continue;

            float hydrationPercent = thirst.CurrentThirst / thirst.MaxThirst;

            // Apply Behaviour
            if (hydrationPercent >= config.HydratedThreshold)
            {
                // Character is hydrated: apply hunger reduction and walk speed buff
                entity.Stats.Set("hungerrate", "hydrationMod", config.HungerBuff, true);
                entity.Stats.Set("walkspeed", "hydrationMod", config.MoveBuff, true);
            }
            else
            {
                // Character is not hydrated: remove buffs
                entity.Stats.Set("hungerrate", "hydrationMod", 0, true);
                entity.Stats.Set("walkspeed", "hydrationMod", 0, true);
            }
        }
    }
}

    public class MyConfigData
    {
        public float HungerBuff = -0.1f;
        public float MoveBuff = 0.1f;
        public float HydratedThreshold = 0.5f;

    }

