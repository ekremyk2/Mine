namespace MinesAutomated {
    static class SettingsGlobal {
        //Takes care of the global settings area.
        public static void DrawGlobalSettings(Verse.Listing_Standard listingStandard, float width, Settings settings) {
            //Header
            Verse.Text.Font = Verse.GameFont.Medium;
            listingStandard.Label("Global settings");
            listingStandard.GapLine();
            Verse.Text.Font = Verse.GameFont.Small;
            //Content
            Verse.Listing_Standard listingStandardGlobal = listingStandard.BeginSection_NewTemp(settings.heightPerSetting * settings.globalSettings.Count, 5f, 0f);
            UnityEngine.Rect rect = new UnityEngine.Rect() { width = width, height = settings.heightPerSetting };

            foreach (SettingGlobalProperties sp in settings.globalSettings)
                rect.y += newGlobalSetting(rect, sp, settings);

            listingStandard.EndSection(listingStandardGlobal);
        }
        //Create a new "line" with a label and a textbox for numbers.
        private static float newGlobalSetting(UnityEngine.Rect rect, SettingGlobalProperties sp, Settings settings) {
            Verse.Listing_Standard listingStandard = new Verse.Listing_Standard();
            listingStandard.Begin(rect);
            UnityEngine.Rect tempRect = listingStandard.Label("");
            Verse.WidgetRow wr = new Verse.WidgetRow(0f, tempRect.y);
            tempRect = wr.Label(sp.label);
            wr.Gap(listingStandard.ColumnWidth - tempRect.width - 80);
            tempRect = wr.Label("", width: 80);
            Verse.Widgets.TextFieldNumeric(tempRect, ref sp.value, ref sp.buffer, settings.minValue, settings.maxValue);
            listingStandard.End();
            return rect.height;
        }
    }
    //The Properties each global settings needs.
    public class SettingGlobalProperties {
        //Dunno, something the TextFieldNumeric needs to properly assign and/or save the value
        public string buffer;
        //A unique string Rimworld needs to save the settings.
        public string Scribe_Values_String;
        //The label that is displayed in the settings menu for the line this instance is used in.
        public string label;
        //The value that is displayed and saved.
        public int value = 100;
        public SettingGlobalProperties(string Scribe_Values_String, string label) {
            this.Scribe_Values_String = Scribe_Values_String;
            this.label = label;
        }
    }
}
