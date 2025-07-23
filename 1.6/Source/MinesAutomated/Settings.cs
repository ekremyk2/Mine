namespace MinesAutomated
{
    public class Settings : Verse.ModSettings
    {
        public bool disableLogging;
        public bool disableExtensions;
        public bool enableStoneChunks;
        //How much room each row of UI elements has.
        public float heightPerSetting = 25f;
        //The minimum and maximum values for the workamount and yield in %.
        public int minValue = 50;
        public int maxValue = 200;
        public System.Collections.Generic.List<SettingGlobalProperties> globalSettings = new System.Collections.Generic.List<SettingGlobalProperties>();
        public System.Collections.Generic.List<SettingindividualProperties> individualSettings = new System.Collections.Generic.List<SettingindividualProperties>();
        System.Xml.XmlNode locationOfSavefile = null;
        
        // VTR Optimization: Cache for frequently accessed settings
        private System.Collections.Generic.Dictionary<string, float> settingsCache = new System.Collections.Generic.Dictionary<string, float>();
        private int lastCacheUpdateTick = 0;
        private const int CACHE_UPDATE_INTERVAL = 60; // Update cache every 60 ticks
        
        public override void ExposeData()
        {
            base.ExposeData();
            Verse.Scribe_Values.Look(ref disableLogging, "disableLogging", defaultValue: false);
            Verse.Scribe_Values.Look(ref disableExtensions, "disableExtensions", defaultValue: false);
            Verse.Scribe_Values.Look(ref enableStoneChunks, "enableStoneChunks", defaultValue: false);
            //Save the global settings
            foreach (SettingGlobalProperties sp in globalSettings)
                Verse.Scribe_Values.Look(ref sp.value, sp.Scribe_Values_String);
            //Save the individual settings
            if (locationOfSavefile == null)
                locationOfSavefile = Verse.Scribe.loader.curXmlParent;
            foreach (SettingindividualProperties sp in individualSettings)
            {
                Verse.Scribe_Values.Look(ref sp.valueWorkamount, sp.Scribe_Values_Workamount, defaultValue: 100);
                Verse.Scribe_Values.Look(ref sp.valueYield, sp.Scribe_Values_Yield, defaultValue: 100);
            }
            
            // Clear cache when settings are saved/loaded
            settingsCache.Clear();
        }
        
        public void LoadSettings()
        {
            try
            {
                if (locationOfSavefile != null)
                {
                    Verse.Scribe.mode = Verse.LoadSaveMode.LoadingVars;
                    Verse.Scribe.loader.curXmlParent = locationOfSavefile;
                    foreach (SettingindividualProperties sp in individualSettings)
                    {
                        Verse.Scribe_Values.Look(ref sp.valueWorkamount, sp.Scribe_Values_Workamount, defaultValue: 100);
                        Verse.Scribe_Values.Look(ref sp.valueYield, sp.Scribe_Values_Yield, defaultValue: 100);
                    }
                    Verse.Scribe.mode = Verse.LoadSaveMode.Inactive;
                    
                    // Clear cache after loading
                    settingsCache.Clear();
                }
            }
            catch
            {
                Verse.Log.Error("MinesAutomated error -> LoadSettings: " + locationOfSavefile);
            }
        }
        
        // VTR Optimized: Gets called whenever the Recipes should be updated.
        public void UpdateRecipeDefs()
        {
            // Clear cache immediately when settings are updated
            ClearAllCaches();
            
            foreach (SettingindividualProperties sp in individualSettings)
            {
                Verse.RecipeDef rd = Verse.DefDatabase<Verse.RecipeDef>.GetNamed(sp.recipeDef.defName);
                if (rd != null && rd.products != null && rd.products.Count > 0)
                {
                    rd.products[0].count = (int)SettingsIndividual.CalculateValues(sp, this, false);
                    //Don't ask me where the 60 comes from, but it's needed for the calculation.
                    rd.workAmount = SettingsIndividual.CalculateValues(sp, this, true) * 60f;
                }
            }
        }
        
        // VTR Optimization: Cache for frequently accessed settings
        private void UpdateSettingsCache()
        {
            if (Verse.Find.TickManager?.TicksGame == null) return;
            
            int currentTick = Verse.Find.TickManager.TicksGame;
            if (currentTick - lastCacheUpdateTick < CACHE_UPDATE_INTERVAL) return;
            
            settingsCache.Clear();
            foreach (SettingindividualProperties sp in individualSettings)
            {
                string workKey = sp.Scribe_Values_Workamount + "_work";
                string yieldKey = sp.Scribe_Values_Yield + "_yield";
                
                settingsCache[workKey] = sp.valueWorkamount;
                settingsCache[yieldKey] = sp.valueYield;
            }
            
            lastCacheUpdateTick = currentTick;
        }
        
        // VTR Optimized: Get cached setting value
        public float GetCachedSetting(string key, float defaultValue = 100f)
        {
            UpdateSettingsCache();
            return settingsCache.TryGetValue(key, out float value) ? value : defaultValue;
        }
        
        // NEW: Clear all caches immediately when settings change
        public void ClearAllCaches()
        {
            settingsCache.Clear();
            lastCacheUpdateTick = 0;
            SettingsIndividual.ClearCalculationCache();
        }
        
        // NEW: Force immediate cache update
        public void ForceCacheUpdate()
        {
            lastCacheUpdateTick = 0;
            UpdateSettingsCache();
        }
        
        public Settings()
        {
            if (disableLogging)
                Verse.Log.Message("Mines 2.0: Logging has been disabled.");

            //The two global settings.
            globalSettings.Add(new SettingGlobalProperties("globalWorkamount", "Global workamount modifier"));
            globalSettings.Add(new SettingGlobalProperties("globalYield", "Global yield modifier"));
        }
    }
    public class MinesAutomatedSettings : Verse.Mod
    {
        //Updates the RecipeDefs with the correct values after saving the settings.
        public override void WriteSettings()
        {
            base.WriteSettings();
            // Clear caches immediately when settings are written
            Settings.ClearAllCaches();
            Settings.UpdateRecipeDefs();
        }
        public Settings Settings => GetSettings<Settings>();
        //Giving the Setting a name in the mod-setting window.
        public override string SettingsCategory()
        {
            return "Mines 2.0";
        }
        //The main method to draw the GUI.
        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            Verse.Listing_Standard listingStandard = new Verse.Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Disable logging", ref Settings.disableLogging);
            listingStandard.CheckboxLabeled("Disable Extensions", ref Settings.disableExtensions);
            listingStandard.CheckboxLabeled("Enable Stone Chunks", ref Settings.enableStoneChunks);
            listingStandard.Label("Warning! Stone Chunks may break wealth system if you modify work amount and yield. Game needs restart.");
            listingStandard.Label("All values are in %. Values can range between " + Settings.minValue + " and " + Settings.maxValue + ".");
            SettingsGlobal.DrawGlobalSettings(listingStandard, inRect.width, Settings);
            SettingsIndividual.DrawIndividualSettings(listingStandard, inRect.width, Settings, inRect);
            listingStandard.End();
        }
        public MinesAutomatedSettings(Verse.ModContentPack content) : base(content) { }
    }
}
