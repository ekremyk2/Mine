using Verse;

namespace Cain.MineShaft
{
    public class UnfinishedMineWork : UnfinishedThing
    {
        public override string LabelNoCount => LanguageKeys.ms_unfinished.Translate(Recipe.products[0].thingDef.label);
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (Recipe == null)
            {
                Log.Warning("Spawned an UnfinishedMineWork without a Recipe - removing!");
                this.Destroy();
            }
        }
    }
}
