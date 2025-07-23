using Verse;

namespace MinesAutomated
{
    public static class SettingsIndividual
    {
        static UnityEngine.Vector2 scrollbar = new UnityEngine.Vector2();

        // VTR Optimization: Cache for calculated values
        private static System.Collections.Generic.Dictionary<string, float> calculatedValuesCache = new System.Collections.Generic.Dictionary<string, float>();
        private static int lastCalculationTick = 0;
        private const int CALCULATION_CACHE_INTERVAL = 30; // Cache calculations for 30 ticks

        //Takes care of the individual settings area.
        public static void DrawIndividualSettings(Verse.Listing_Standard listingStandard, float width, Settings settings, UnityEngine.Rect mainRect)
        {
            //Header
            Verse.Text.Font = Verse.GameFont.Medium;
            listingStandard.Label("Individual settings");
            listingStandard.GapLine();
            Verse.Text.Font = Verse.GameFont.Small;
            UnityEngine.Rect newRow = listingStandard.Label(""); //For some stupid ass reason Listing_Standard doesn't make room for a WidgetRow. Need to put an empty label here.
            float columnWidth = (listingStandard.ColumnWidth - 50) / 10;
            Verse.WidgetRow wr = new Verse.WidgetRow(newRow.x, newRow.y);
            wr.Label("Resource");
            wr.Gap(columnWidth * 5.8f);
            float xWorkamount = wr.Label("Work amount").x;
            wr.Gap(columnWidth * 0.8f);
            float xYield = wr.Label("Yield").x;
            //Content
            UnityEngine.Rect rect = new UnityEngine.Rect() { y = newRow.y, width = width - 30, height = settings.heightPerSetting * settings.individualSettings.Count };

            Verse.Widgets.BeginScrollView(listingStandard.GetRect(mainRect.height - listingStandard.CurHeight), ref scrollbar, rect);
            Verse.Widgets.DrawMenuSection(rect);
            float currentY = rect.y;
            foreach (SettingindividualProperties sp in settings.individualSettings)
            {
                Verse.Widgets.DrawLineHorizontal(rect.x, currentY, rect.width);
                newIndividualSetting(currentY, sp, settings, xWorkamount, xYield);
                currentY += settings.heightPerSetting;
            }
            Verse.Widgets.EndScrollView();
        }

        //Create a new "line" with an image, a label and two textboxes for numbers.
        private static void newIndividualSetting(float currentY, SettingindividualProperties sp, Settings settings, float xWorkamount, float xYield)
        {
            Verse.WidgetRow wr = new Verse.WidgetRow(0, currentY);
            //The image
            wr.DefIcon(sp.resource.building.mineableThing);
            //The label
            UnityEngine.Rect tempRect = wr.Label(sp.label);
            wr.Gap(xWorkamount - tempRect.x - tempRect.width - 10f);
            tempRect = wr.Label("", width: 80);

            // Store old values to detect changes
            int oldWorkAmount = sp.valueWorkamount;
            int oldYield = sp.valueYield;

            //Numeric Textbox for the workamount
            Verse.Widgets.TextFieldNumeric(tempRect, ref sp.valueWorkamount, ref sp.bufferWorkamount, settings.minValue, settings.maxValue);

            // Check if work amount changed and clear cache immediately
            if (oldWorkAmount != sp.valueWorkamount)
            {
                ClearCalculationCache();
            }

            //A label to show how much workamount this recipe is gonna need
            float width = wr.Label("-> " + CalculateValues(sp, settings, true)).width;
            wr.Gap(xYield - tempRect.x - tempRect.width - width - 10f);
            tempRect = wr.Label("", width: 80);

            //Numeric Textbox for the yield
            Verse.Widgets.TextFieldNumeric(tempRect, ref sp.valueYield, ref sp.bufferYield, settings.minValue, settings.maxValue);

            // Check if yield changed and clear cache immediately
            if (oldYield != sp.valueYield)
            {
                ClearCalculationCache();
            }

            //A label to show how much thid recipe yields
            wr.Label("-> " + CalculateValues(sp, settings, false));
        }

        // VTR Optimized calculation with caching
        public static float CalculateValues(SettingindividualProperties sp, Settings settings, bool workAmount)
        {
            // Check cache first
            string cacheKey = sp.Scribe_Values_Workamount + "_" + (workAmount ? "work" : "yield") + "_" +
                             sp.valueWorkamount + "_" + sp.valueYield;

            if (TryGetCachedValue(cacheKey, out float cachedValue))
                return cachedValue;

            float globalPercent = 0;
            float individualPercent = 0;
            float result = 0;

            if (workAmount)
            {
                globalPercent = (float)settings.globalSettings.Find(e => e.label == "Global workamount modifier").value / 100;
                individualPercent = (float)sp.valueWorkamount / 100;
                result = (float)System.Math.Ceiling((float)sp.resource.BaseMaxHitPoints / 80 * globalPercent * individualPercent);
            }
            else
            {
                globalPercent = (float)settings.globalSettings.Find(e => e.label == "Global yield modifier").value / 100;
                individualPercent = (float)sp.valueYield / 100;
                result = sp.resource.building.mineableYield * globalPercent * individualPercent;
            }

            // Cache the result
            CacheValue(cacheKey, result);
            return result;
        }

        // VTR Optimization: Cache management
        private static bool TryGetCachedValue(string key, out float value)
        {
            if (Current.Game is null || Verse.Find.TickManager?.TicksGame == null)
            {
                value = 0f;
                return false;
            }

            int currentTick = Verse.Find.TickManager.TicksGame;
            if (currentTick - lastCalculationTick > CALCULATION_CACHE_INTERVAL)
            {
                calculatedValuesCache.Clear();
                lastCalculationTick = currentTick;
                value = 0f;
                return false;
            }

            return calculatedValuesCache.TryGetValue(key, out value);
        }

        private static void CacheValue(string key, float value)
        {
            calculatedValuesCache[key] = value;
        }

        // Clear cache when settings change
        public static void ClearCalculationCache()
        {
            calculatedValuesCache.Clear();
            lastCalculationTick = 0;
        }
    }
    //The Properties each individual settings needs.
    public class SettingindividualProperties
    {
        public Verse.RecipeDef recipeDef;
        public Verse.ThingDef resource;
        //Dunno, something the TextFieldNumeric needs to properly assign and/or save the value
        public string bufferWorkamount;
        public string bufferYield;
        //A unique string Rimworld needs to save the settings.
        public string Scribe_Values_Workamount = "";
        public string Scribe_Values_Yield = "";
        //The label that is displayed in the settings menu for the line this instance is used in.
        public Verse.TaggedString label;
        //The value for Workamount that is displayed and saved.
        public int valueWorkamount = 100;
        //The value for Yield that is displayed and saved.
        public int valueYield = 100;
        public SettingindividualProperties(Verse.RecipeDef recipeDef, Verse.ThingDef resource)
        {
            this.Scribe_Values_Workamount = RemoveSpecialCharacters(resource.building.mineableThing.label + "Workamount");
            this.Scribe_Values_Yield = RemoveSpecialCharacters(resource.building.mineableThing.label + "Yield");
            this.label = resource.building.mineableThing.label;
            this.label = this.label.CapitalizeFirst();
            this.recipeDef = recipeDef;
            this.resource = resource;
        }
        private string RemoveSpecialCharacters(string s)
        {
            string returnString = "";
            foreach (char c in s)
                if (char.IsLetterOrDigit(c))
                    returnString += c;
            return returnString;
        }
    }
}