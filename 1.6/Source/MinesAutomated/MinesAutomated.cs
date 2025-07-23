using System.Linq;

namespace MinesAutomated
{
    //Updates the RecipeDefs with the correct values after loading into the game.
    public class GameComponent : Verse.GameComponent
    {
        public override void FinalizeInit()
        {
            Verse.LoadedModManager.GetMod<MinesAutomatedSettings>().GetSettings<Settings>().UpdateRecipeDefs();
            base.FinalizeInit();
        }
        public GameComponent(Verse.Game game) : base() { }
    }

    // VTR System optimized component for the automated mine workbench
    public class CompAutomatedMine : Verse.ThingComp
    {
        private int lastActivityCheck = 0;
        private const int ACTIVITY_CHECK_INTERVAL = 60; // Check activity every 60 ticks
        private bool isCurrentlyBeingUsed = false;
        
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            
            // Only check activity periodically to avoid performance impact
            if (Verse.Find.TickManager?.TicksGame == null) return;
            
            int currentTick = Verse.Find.TickManager.TicksGame;
            if (currentTick - lastActivityCheck < ACTIVITY_CHECK_INTERVAL) return;
            
            lastActivityCheck = currentTick;
            
            // Check if the workbench is being used
            isCurrentlyBeingUsed = IsWorkbenchBeingUsed();
            
            // Log activity status for debugging (optional)
            if (!Verse.LoadedModManager.GetMod<MinesAutomatedSettings>().GetSettings<Settings>().disableLogging)
            {
                Verse.Log.Message($"MinesAutomated: Workbench at {parent?.Position} is {(isCurrentlyBeingUsed ? "being used" : "idle")}");
            }
        }
        
        private bool IsWorkbenchBeingUsed()
        {
            if (parent?.Map == null) return false;
            
            // Check if any pawn is currently working at this workbench
            var workGivers = parent.Map.lordManager.lords
                .Where(l => l.LordJob is RimWorld.LordJob_VoluntarilyJoinable)
                .SelectMany(l => l.ownedPawns)
                .Where(p => p.CurJob?.targetA.Thing == parent);
                
            return workGivers.Any();
        }
        
        // Provide a method to check if workbench is currently being used
        public bool IsBeingUsed => isCurrentlyBeingUsed;
    }
}

