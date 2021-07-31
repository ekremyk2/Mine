using Verse;

namespace Cain.MineShaft
{
    class Settings : ModSettings
    {
        private GlobalMineShaftParameters _props = new GlobalMineShaftParameters();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref _props, "props");
        }

        public GlobalMineShaftParameters GlobalParameters => _props;
    }
}
