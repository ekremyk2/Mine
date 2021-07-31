using System.ComponentModel;
using Verse;

namespace Cain.MineShaft
{
    class GlobalMineShaftParameters : IExposable
    {
        private Multiplier _mineWork;
        private Multiplier _density;
        private Multiplier _commonality;

        public GlobalMineShaftParameters()
        {
            _mineWork = new Multiplier();
            _density = new Multiplier();
            _commonality = new Multiplier();

            _mineWork.PropertyChanged += Component_PropertyChanged;
            _density.PropertyChanged += Component_PropertyChanged;
            _commonality.PropertyChanged += Component_PropertyChanged;
        }

        public Multiplier MineWork => _mineWork;

        public Multiplier Density => _density;

        public Multiplier Commonality => _commonality;

        private void Component_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Multiplier.Value):
                    ThingDefGenerator.UpdateAllGeneratedDefs();
                    break;
            }
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                _mineWork.PropertyChanged += Component_PropertyChanged;
                _density.PropertyChanged += Component_PropertyChanged;
                _commonality.PropertyChanged += Component_PropertyChanged;
            }

            Scribe_Deep.Look(ref _mineWork, "mineWork");
            Scribe_Deep.Look(ref _density, "density");
            Scribe_Deep.Look(ref _commonality, "commonality");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                _mineWork.PropertyChanged += Component_PropertyChanged;
                _density.PropertyChanged += Component_PropertyChanged;
                _commonality.PropertyChanged += Component_PropertyChanged;
            }
        }
    }
}
