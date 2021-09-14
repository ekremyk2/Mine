namespace MinesAutomated {
    //Updates the RecipeDefs with the correct values after loading into the game.
    public class GameComponent : Verse.GameComponent {
        public Settings Settings => Verse.LoadedModManager.GetMod<MinesAutomatedSettings>().GetSettings<Settings>();
        public override void FinalizeInit() {
            Settings.UpdateRecipeDefs();
        }
        public GameComponent(Verse.Game game) : base() { }
    }
}
