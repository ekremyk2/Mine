using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Cain.MineShaft
{
    public static class ThingDefGenerator
    {
        private static IDictionary<ThingDef, RecipeDef> _miningRecipes;

        private static IDictionary<ThingDef, MineProperties> _mineProperties;

        private static IDictionary<ThingDef, string> _sourceLabels;

        public static ICollection<RecipeDef> GetCoreMiningDefs()
        {
            if (_miningRecipes == null)
            {
                _miningRecipes = new Dictionary<ThingDef, RecipeDef>();
                _mineProperties = new Dictionary<ThingDef, MineProperties>();
                _sourceLabels = new Dictionary<ThingDef, string>();

                foreach (ThingDef source in DefDatabase<ThingDef>.AllDefs.Where(td => td.mineable && td.building?.mineableThing != null))
                {
                    BuildingProperties b;

                    MineProperties props = CalculateMineProperties(source, out b);
                    if (props == null)
                    {
                        Log.Warning($"Cannot determine hitpoints for {source.defName} - skipping resource excavation recipe.");
                        continue;
                    }

                    ThingDef t = b.mineableThing;

                    var recipe = new RecipeDef
                    {
                        efficiencyStat = StatDefOf.MiningYield,
                        workSpeedStat = StatDefOf.MiningSpeed,
                        effectWorking = EffecterDefOf.Mine,
                        workSkillLearnFactor = 0.25f,
                        workSkill = SkillDefOf.Mining,
                        defName = $"MS_Mine{t.defName}",
                        label = LanguageKeys.ms_label.Translate(t.LabelCap),
                        jobString = LanguageKeys.ms_jobString.Translate(t.LabelCap),
                        products = new List<ThingDefCountClass>
                        {
                            new ThingDefCountClass
                            {
                                thingDef = t,
                                count = 0
                            }
                        },
                        recipeUsers = new List<ThingDef>
                        {
                            DefDatabase<ThingDef>.GetNamed("Mine")
                        },
                        unfinishedThingDef = DefDatabase<ThingDef>.GetNamed("UnfinishedMineWork")
                    };

                    UpdateGeneratedDef(t, recipe, props, source.LabelCap);

                    _miningRecipes[t] = recipe;
                    _mineProperties[t] = props;
                    _sourceLabels[t] = source.LabelCap;
                }
            }
            return _miningRecipes.Values;
        }

        private static MineProperties CalculateMineProperties(ThingDef source, out BuildingProperties b)
        {
            b = source.building;

            float? hitpointsPerLump = source.statBases.FirstOrDefault(sm => sm.stat == StatDefOf.MaxHitPoints)?.value;

            if (hitpointsPerLump == null)
            {
                return null;
            }

            return new MineProperties(hitpointsPerLump.Value, b);
        }

        public static void UpdateGeneratedDef(ThingDef material)
        {
            UpdateGeneratedDef(material, _miningRecipes[material], _mineProperties[material], _sourceLabels[material]);
        }

        private static void UpdateGeneratedDef(ThingDef material, RecipeDef recipe, MineProperties props, string sourceLabel)
        {
            float yield = props.Yield;
            float work = props.Work;

            MaterialParameters mp = Current.Game?.GetComponent<GameMaterialParameters>()[material];
            if (mp != null)
            {
                yield *= mp.Yield.Value;
                work *= mp.Work.Value;
            }

            if (work < 0 || work == Int32.MaxValue || work == Int32.MinValue)
            {
                work = 10000;
            }

            if (yield < 1f)
            {
                work = work / yield;
                yield = 1f;
            }


            int iYield = (int)Math.Round(yield);

            recipe.workAmount = work;
            recipe.description = LanguageKeys.ms_description.Translate(new object[] { sourceLabel, iYield, material.LabelCap });
            recipe.products.First().count = iYield;
        }

        public static void UpdateAllGeneratedDefs()
        {
            foreach (var miningRecipe in _miningRecipes)
            {
                UpdateGeneratedDef(miningRecipe.Key, miningRecipe.Value, _mineProperties[miningRecipe.Key], _sourceLabels[miningRecipe.Key]);
            }
        }

        public static IEnumerable<KeyValuePair<ThingDef, RecipeDef>> AllMineRecipes => _miningRecipes;
    }
}
