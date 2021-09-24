namespace MinesAutomated {
    //Updates the RecipeDefs with the correct values after loading into the game.
    public class GameComponent : Verse.GameComponent {
        Settings Settings => Verse.LoadedModManager.GetMod<MinesAutomatedSettings>().GetSettings<Settings>();
        public override void FinalizeInit() {
            Verse.LoadedModManager.GetMod<MinesAutomatedSettings>().GetSettings<Settings>().UpdateRecipeDefs();
            base.FinalizeInit();
        }
        public GameComponent(Verse.Game game) : base() {}
    }
}
