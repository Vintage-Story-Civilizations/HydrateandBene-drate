using System;
using Vintagestory.API.Common;
using System.Linq;
using HydrateOrDiedrate;

namespace HydrateandBene_drate;



public class HydrateandBene_drateModSystem : ModSystem
{
    float MaxThirst;
    float CurrentThirst;
    float HydrationPercent; // = thirst.CurrentThirst / thirst.MaxThirst; 

    float HBHungerDelta = -0.1f;
    float HBMoveDelta = 0.1f;
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
            api.Logger.Debug("Current Thirst: " + CurrentThirst + "Max Thirst: " + MaxThirst + "Hydration Percent: " + HydrationPercent);
        //3. Apply Behaviour
            if (HydrationPercent >= 0.5f)
            {
                // Character is Hydrated, Set 10% Hunger Reduction and 10% Walkspeed
                entity.Stats.Set("hungerrate", "hydrationMod", HBHungerDelta, true);
                entity.Stats.Set("walkspeed", "hydrationMod", HBMoveDelta, true);
                //Debug
                api.Logger.Debug("Applied hunger Delta: " + HBHungerDelta + "and Walkspeed: " + HBMoveDelta);
            }
            else 
            {
                // Character is in a bad state or is less than Hydrated and get nothing.
                entity.Stats.Set("hungerrate", "hydrationMod", 0, true);
                entity.Stats.Set("walkspeed", "hydrationMod", 0, true);
                //Debug
                api.Logger.Debug("Applied hunger Delta: " + 0 + "and Walkspeed: " + 0);
            }

        }
        
    }


    public override void Start(ICoreAPI api)
    {
        this.api = api;
        api.World.RegisterGameTickListener(OnGameTick, 2000);
    }

}

