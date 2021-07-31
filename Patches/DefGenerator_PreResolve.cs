using HarmonyLib;
using RimWorld;
using Verse;

namespace Cain.MineShaft.Patches
{
    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public class DefGenerator_PreResolve
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            foreach (RecipeDef current3 in ThingDefGenerator.GetCoreMiningDefs())
            {
                current3.PostLoad();
                DefDatabase<RecipeDef>.Add(current3);
            }
        }
    }
}
