namespace MinesAutomated {
    public static class SettingsIndividual {
        static UnityEngine.Vector2 scrollbar = new UnityEngine.Vector2();
        //Takes care of the individual settings area.
        public static void DrawIndividualSettings(Verse.Listing_Standard listingStandard, float width, Settings settings, UnityEngine.Rect mainRect) {
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
            foreach (SettingindividualProperties sp in settings.individualSettings) {
                Verse.Widgets.DrawLineHorizontal(rect.x, currentY, rect.width);
                newIndividualSetting(currentY, sp, settings, xWorkamount, xYield);
                currentY += settings.heightPerSetting;
            }
            Verse.Widgets.EndScrollView();
        }
        //Create a new "line" with an image, a label and two textboxes for numbers.
        private static void newIndividualSetting(float currentY, SettingindividualProperties sp, Settings settings, float xWorkamount, float xYield) {
            Verse.WidgetRow wr = new Verse.WidgetRow(0, currentY);
            //The image
            wr.DefIcon(sp.resource.building.mineableThing);
            //The label
            UnityEngine.Rect tempRect = wr.Label(sp.label);
            wr.Gap(xWorkamount - tempRect.x - tempRect.width- 10f);
            tempRect = wr.Label("", width: 80);
            //Numeric Textbox for the workamount
            Verse.Widgets.TextFieldNumeric(tempRect, ref sp.valueWorkamount, ref sp.bufferWorkamount, settings.minValue, settings.maxValue);
            //A label to show how much workamount this recipe is gonna need
            float width = wr.Label("-> " + CalculateValues(sp, settings, true)).width;
            wr.Gap(xYield - tempRect.x - tempRect.width - width - 10f);
            tempRect = wr.Label("", width: 80);
            //Numeric Textbox for the yield
            Verse.Widgets.TextFieldNumeric(tempRect, ref sp.valueYield, ref sp.bufferYield, settings.minValue, settings.maxValue);
            //A label to show how much thid recipe yields
            wr.Label("-> " + CalculateValues(sp, settings, false));
        }
        public static float CalculateValues(SettingindividualProperties sp, Settings settings, bool workAmount) {
            float globalPercent = 0;
            float individualPercent = 0;
            if (workAmount) {
                globalPercent = (float)settings.globalSettings.Find(e => e.label == "Global workamount modifier").value / 100;
                individualPercent = (float)sp.valueWorkamount / 100;
                return (float)System.Math.Ceiling((float)sp.resource.BaseMaxHitPoints / 80 * globalPercent * individualPercent);
            } else {
                globalPercent = (float)settings.globalSettings.Find(e => e.label == "Global yield modifier").value / 100;
                individualPercent = (float)sp.valueYield / 100;
                return sp.resource.building.mineableYield * globalPercent * individualPercent;
            }
        }
    }
    //The Properties each individual settings needs.
    public class SettingindividualProperties {
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
        public SettingindividualProperties(Verse.RecipeDef recipeDef, Verse.ThingDef resource) {
            foreach (char c in resource.building.mineableThing.label + "Workamount")
                if (char.IsLetterOrDigit(c))
                    this.Scribe_Values_Workamount += c;
            foreach (char c in resource.building.mineableThing.label + "Yield")
                if (char.IsLetterOrDigit(c))
                    this.Scribe_Values_Yield += c;
            this.label = resource.building.mineableThing.label;
            this.label = this.label.CapitalizeFirst();
            this.recipeDef = recipeDef;
            this.resource = resource;
        }
    }
}
