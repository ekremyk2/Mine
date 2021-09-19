using System.Linq;
namespace MinesAutomated {
    [RimWorld.DefOf]
    public static class DefOf {
        public static Verse.ThingDef MinesAutomated_ThingDef_Mine;
        public static Verse.ThingDef MinesAutomated_ThingDef_UnfinishedMineWork;
        public static Verse.ResearchProjectDef MinesAutomated_ResearchProjectDef_minecraft;
        public static Verse.SoundDef PickHit;
        static DefOf() {
            RimWorld.DefOfHelper.EnsureInitializedInCtor(typeof(DefOf));
        }
    }
    public class RecipesAndTheirResourceBlocks {
        public Verse.RecipeDef RecipeDef { get; }
        public Verse.ThingDef ResourceBlock { get; }
        public RecipesAndTheirResourceBlocks(Verse.RecipeDef recipeDef, Verse.ThingDef resourceBlock) {
            this.RecipeDef = recipeDef;
            this.ResourceBlock = resourceBlock;
        }
    }
    [Verse.StaticConstructorOnStartup]
    public static class CreateRecipeDefs {
        public static System.Collections.Generic.List<RecipesAndTheirResourceBlocks> MinesAutomatedRecipeDefs { get; set; }
        static CreateRecipeDefs() {
            MinesAutomatedRecipeDefs = new System.Collections.Generic.List<RecipesAndTheirResourceBlocks>();
            System.Collections.Generic.List<string> recipes = RecipeDefsNotToCreate();
            foreach (Verse.ThingDef resourceBlock in Verse.DefDatabase<Verse.ThingDef>.AllDefs.Where(td => td.mineable && td.building?.mineableThing != null &&
            (td.building.isResourceRock || td.building.isNaturalRock))) {
                //Adding them to the DefDatabase.
                if (Verse.DefDatabase<Verse.RecipeDef>.AllDefs.Where(e => e.defName == "MinesAutomated_RecipeDef_" + resourceBlock.building.mineableThing.label.Replace(" ", string.Empty)).Count() < 1) {
                    //Adding them to a list to make it easier to recalculate values with the values from the settings.
                    if (!recipes.Contains("MinesAutomated_RecipeDef_" + resourceBlock.defName)) {
                        Verse.RecipeDef recipeToAdd = DefineRecipeDef.FinishRecipeDef(DefineRecipeDef.CopyBaseRecipeDef(), resourceBlock);
                        MinesAutomatedRecipeDefs.Add(new RecipesAndTheirResourceBlocks(recipeToAdd, resourceBlock));
                        Verse.DefDatabase<Verse.RecipeDef>.Add(recipeToAdd);
                        recipeToAdd.ResolveReferences();
                        Verse.Log.Message("Mines 2.0-> RecipeDef named " + recipeToAdd.defName + " added to the mine.");
                    }
                }
                else
                    Verse.Log.Message("MinesAutomated -> CreateRecipeDefs" +
                        "\nA RecipeDef with the product" + resourceBlock.building.mineableThing + " has not been added because a Def with that product already exists.");
            }
            //Make sure the DefDatabase integrates the new RecipeDefs.
            Verse.DefDatabase<Verse.RecipeDef>.ResolveAllReferences();
        }
        //You can't edit RecipeDefs with PatchOperations if those RecipeDefs where created in C#.
        //So those PatchOperations are directly read and applied / warned about.
        private static System.Collections.Generic.List<string> RecipeDefsNotToCreate() {
            System.Collections.Generic.List<string> recipes = new System.Collections.Generic.List<string>();
            foreach (Verse.PatchOperation po in Verse.LoadedModManager.GetMod<MinesAutomatedSettings>().GetSettings<Settings>().Mod.Content.Patches) {
                System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
                xml.Load(po.sourceFile);
                foreach (System.Xml.XmlNode node in xml.GetElementsByTagName("operations")[0].ChildNodes) {
                    if (node.Attributes != null && node.Attributes.Count != 0) {
                        string attribute = node.Attributes[0].Value;
                        //Error message that PatchOperationReplace doesn't work on RecipeDefs.
                        if (attribute == "PatchOperationReplace" && node.InnerText.Contains("MinesAutomated_RecipeDef_")) {
                            Verse.Log.Error("Mines 2.0: PatchOperationReplace does not work for RecipeDefs of this mod. Please remove the recipe and add a new one with the values you need.");
                        //"Removing" RecipeDefs by preventing them from being created.
                        } else if (attribute == "PatchOperationRemove") {
                            string xpath = node.FirstChild.InnerText;
                            if (xpath.Contains("MinesAutomated_RecipeDef_")) {
                                int first = xpath.IndexOf('"', 0);
                                int second = xpath.IndexOf('"', ++first);
                                recipes.Add(xpath.Substring(first, second - first));
                            }
                        }
                    }
                }
            }
            return recipes;
        }
    }
    public static class DefineRecipeDef {
        //Give the recipe individual infos.
        public static Verse.RecipeDef FinishRecipeDef(Verse.RecipeDef recipeDef, Verse.ThingDef resourceBlock) {
            recipeDef.defName = "MinesAutomated_RecipeDef_" + resourceBlock.defName;
            recipeDef.label = "Mine for " + resourceBlock.building.mineableThing.label;
            recipeDef.jobString = "Mining for " + resourceBlock.building.mineableThing.label;
            recipeDef.description = "Mine for " + resourceBlock.building.mineableThing.label;
            recipeDef.descriptionHyperlinks = new System.Collections.Generic.List<Verse.DefHyperlink>() {
                new Verse.DefHyperlink() {def = resourceBlock.building.mineableThing}
            };
            recipeDef.products = new System.Collections.Generic.List<Verse.ThingDefCountClass>() {
                    new Verse.ThingDefCountClass() {
                        thingDef = resourceBlock.building.mineableThing,
                        count = 0
                    }
                };
            return recipeDef;
        }
        //The base recipe all other recipes are based off of.
        public static Verse.RecipeDef CopyBaseRecipeDef() {
            Verse.RecipeDef baseRecipe = new Verse.RecipeDef() {
                efficiencyStat = RimWorld.StatDefOf.MiningYield,
                workSpeedStat = RimWorld.StatDefOf.MiningSpeed,
                effectWorking = RimWorld.EffecterDefOf.Mine,
                workSkill = RimWorld.SkillDefOf.Mining,
                workSkillLearnFactor = 0.25f,
                recipeUsers = new System.Collections.Generic.List<Verse.ThingDef>() { DefOf.MinesAutomated_ThingDef_Mine },
                unfinishedThingDef = DefOf.MinesAutomated_ThingDef_UnfinishedMineWork,
                researchPrerequisite = DefOf.MinesAutomated_ResearchProjectDef_minecraft,
                soundWorking = DefOf.PickHit,
                modContentPack = DefOf.MinesAutomated_ThingDef_Mine.modContentPack,
                generated = true
            };
            return new Verse.RecipeDef() {
                efficiencyStat = baseRecipe.efficiencyStat,
                workSpeedStat = baseRecipe.workSpeedStat,
                effectWorking = baseRecipe.effectWorking,
                workSkillLearnFactor = baseRecipe.workSkillLearnFactor,
                workSkill = baseRecipe.workSkill,
                recipeUsers = baseRecipe.recipeUsers,
                unfinishedThingDef = baseRecipe.unfinishedThingDef,
                researchPrerequisite = baseRecipe.researchPrerequisite,
                soundWorking = baseRecipe.soundWorking
            };
        }
    }
}
