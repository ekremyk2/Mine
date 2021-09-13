namespace MinesAutomated {
    public class Settings : Verse.ModSettings {
        //How much room each row of UI elements has.
        public float heightPerSetting = 25f;
        //The minimum and maximum values for the workamount and yield in %.
        public int minValue = 50;
        public int maxValue = 200;
        public System.Collections.Generic.List<SettingGlobalProperties> globalSettings = new System.Collections.Generic.List<SettingGlobalProperties>();
        public System.Collections.Generic.List<SettingindividualProperties> individualSettings = new System.Collections.Generic.List<SettingindividualProperties>();
        public override void ExposeData() {
            //Save the global settings
            foreach (SettingGlobalProperties sp in globalSettings)
                Verse.Scribe_Values.Look(ref sp.value, sp.Scribe_Values_String);
            //Save the individual settings
            foreach (SettingindividualProperties sp in individualSettings) {
                Verse.Scribe_Values.Look(ref sp.valueWorkamount, sp.Scribe_Values_Workamount, defaultValue: 100);
                Verse.Scribe_Values.Look(ref sp.valueYield, sp.Scribe_Values_Yield, defaultValue: 100);
            }
            base.ExposeData();
        }
        //Fill the lists which gets used for the UI.
        public Settings() {
            //The two global settings.
            globalSettings.Add(new SettingGlobalProperties("globalWorkamount", "Global workamount modifier"));
            globalSettings.Add(new SettingGlobalProperties("globalYield", "Global yield modifier"));
            //The individual settings for each mineable resource.
            foreach(RecipesAndTheirResourceBlocks rd in CreateRecipeDefs.MinesAutomatedRecipeDefs)
                individualSettings.Add(new SettingindividualProperties(rd.RecipeDef, rd.ResourceBlock));
        }
        //Gets called whenever the Recipes should be updated.
        public void UpdateRecipeDefs() {
            foreach (SettingindividualProperties sp in individualSettings) {
                sp.recipeDef.products = new System.Collections.Generic.List<Verse.ThingDefCountClass>() {
                    new Verse.ThingDefCountClass() {
                        thingDef = sp.resource.building.mineableThing,
                        count = (int)SettingsIndividual.CalculateValues(sp, this, false)
                    }
                };
                Verse.RecipeDef rd = Verse.DefDatabase<Verse.RecipeDef>.GetNamed(sp.recipeDef.defName);
                //Don't ask me where the 60 comes from, but it's needed for the calculation.
                rd.workAmount = SettingsIndividual.CalculateValues(sp, this, true) * 60f;
                rd.ResolveReferences();
            }
            base.ExposeData();
        }
    }
    public class MinesAutomatedSettings : Verse.Mod {
        //Updates the RecipeDefs with the correct values after saving the settings.
        public override void WriteSettings() {
            Settings.UpdateRecipeDefs();
            base.WriteSettings();
        }
        public Settings Settings => GetSettings<Settings>();
        //Giving the Setting a name in the mod-setting window.
        public override string SettingsCategory() {
            return "Mines - Automated";
        }
        //The main method to draw the GUI.
        public override void DoSettingsWindowContents(UnityEngine.Rect inRect) {
            Verse.Listing_Standard listingStandard = new Verse.Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("All values are in %. Values can range between " + Settings.minValue + " and " + Settings.maxValue + ".");
            SettingsGlobal.DrawGlobalSettings(listingStandard, inRect.width, Settings);
            SettingsIndividual.DrawIndividualSettings(listingStandard, inRect.width, Settings, inRect);
            listingStandard.End();
        }
        public MinesAutomatedSettings(Verse.ModContentPack content) : base(content) { }
    }
}
