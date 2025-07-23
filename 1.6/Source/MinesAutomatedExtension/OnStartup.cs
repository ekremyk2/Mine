using MinesAutomated;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MinesAutomatedExtension
{
    [StaticConstructorOnStartup]
    public static class OnStartup
    {
        static OnStartup()
        {
            Settings minesAutomatedSettings = Verse.LoadedModManager.GetMod<MinesAutomatedSettings>().GetSettings<Settings>();

            if (!minesAutomatedSettings.disableExtensions)
            {
                List<RecipeDef> existingRecipes = new List<RecipeDef>(DefDatabase<RecipeDef>.AllDefs.Where(rd => rd.defName.StartsWith("MinesAutomated_RecipeDef_")));
                List<RecipeDef> newRecipes = new List<RecipeDef>();

                foreach (ThingDef mineable in DefDatabase<ThingDef>.AllDefs.Where(td => td.HasModExtension<MinesAutomatedExtension.MineableSettings>()))
                {
                    RecipeDef existing = null;
                    MineableSettings settings = mineable.GetModExtension<MinesAutomatedExtension.MineableSettings>();

                    foreach (RecipeDef recipe in existingRecipes)
                    {
                        if (recipe.products.First().thingDef.Equals(mineable))
                        {
                            existing = recipe;
                        }
                    }

                    if (existing == null)
                    {
                        RecipeDef newRecipe = DefineRecipeDef.CopyBaseRecipeDef();

                        newRecipe.defName = "MinesAutomatedExtension_RecipeDef_" + mineable.defName;
                        newRecipe.label = "Mine for " + mineable.label;
                        newRecipe.jobString = "Mining for " + mineable.label;
                        newRecipe.description = "Mine for " + mineable.label;
                        newRecipe.descriptionHyperlinks = new List<DefHyperlink>()
                        {
                            new DefHyperlink() { def = mineable }
                        };
                        newRecipe.products = new List<ThingDefCountClass>()
                        {
                            new ThingDefCountClass()
                            {
                                thingDef = mineable,
                                count = (settings.mineableYield >= 1) ? settings.mineableYield : 1
                            }
                        };

                        if (settings.researchPrerequisites.Any())
                        {
                            newRecipe.researchPrerequisite = null;
                            newRecipe.researchPrerequisites =
                                new List<ResearchProjectDef>(settings.researchPrerequisites);
                        }

                        newRecipe.workAmount = settings.workOffset;
                        newRecipe.workAmount *= settings.workMultiplier;

                        newRecipes.Add(newRecipe);
                        if (!minesAutomatedSettings.disableLogging)
                            Log.Message("Mines 2.0 Extension -> RecipeDef named " + newRecipe.defName + " added to the mine. This recipe will not be effected by any mod settings!!");
                    }
                }

                DefDatabase<RecipeDef>.Add(newRecipes);
                DefDatabase<RecipeDef>.ResolveAllReferences();
                DefOf.MinesAutomated_ThingDef_Mine.ResolveReferences();
            }
        }
    }
}
