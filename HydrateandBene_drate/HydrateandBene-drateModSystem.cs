using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;  
using HydrateOrDiedrate;
namespace HydrateandBene_drate;




public class HydrateandBene_drateModSystem : ModSystem
{
    float MaxThirst;
    float CurrentThirst;
    float HydrationPercent; // = thirst.CurrentThirst / thirst.MaxThirst; 
    MyConfigData config;
   
    float CheckTimer;
    ICoreAPI api;



    void OnGameTick(float dt)
    {
        CheckTimer = CheckTimer + dt;
        if (CheckTimer >= 10f) 
        {
        //0. Reset the Clock
            CheckTimer = 0f; // Reset the clock.
        //1. Get the Player
            var player = api.World.AllOnlinePlayers.FirstOrDefault(); // Get You.
            if (player == null) // If there is no player Exit.
                {
                    return; 
                }
            var entity = player.Entity;
        //2. Get Thirst
            var thirst = entity.GetBehavior<EntityBehaviorThirst>();
            if (thirst == null) // If you have no thirst then exit.
                { 
                    api.Logger.Debug("No thirst behavior found."); 
                    return; 
                }
            CurrentThirst = thirst.CurrentThirst;
            MaxThirst = thirst.MaxThirst;
            HydrationPercent = CurrentThirst/ MaxThirst;
            //Debug
            //api.Logger.Debug("Current Thirst: " + CurrentThirst + "Max Thirst: " + MaxThirst + "Hydration Percent: " + HydrationPercent);
        //3. Apply Behaviour
            if (HydrationPercent >= config.HydratedThreshold)
            {
                // Character is Hydrated, Set 10% Hunger Reduction and 10% Walkspeed
                entity.Stats.Set("hungerrate", "hydrationMod", config.HungerBuff, true);
                entity.Stats.Set("walkspeed", "hydrationMod", config.MoveBuff, true);
                //Debug
                //api.Logger.Debug("Applied hunger Delta: " + HBHungerDelta + "and Walkspeed: " + HBMoveDelta);
            }
            else 
            {
                // Character is in a bad state or is less than Hydrated and get nothing.
                entity.Stats.Set("hungerrate", "hydrationMod", 0, true);
                entity.Stats.Set("walkspeed", "hydrationMod", 0, true);
                //Debug
                //api.Logger.Debug("Applied hunger Delta: " + 0 + "and Walkspeed: " + 0);
            }

        }
        
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        this.api = api;
        TryToLoadConfig(api);
        api.World.RegisterGameTickListener(OnGameTick, 2000);
    }

    public class MyConfigData
        {
        public float HungerBuff = -0.1f;
        public float MoveBuff = 0.1f;
        public float HydratedThreshold = 0.5f;

        }

    private void TryToLoadConfig(ICoreAPI api) 
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

}