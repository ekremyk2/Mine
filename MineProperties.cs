using RimWorld;
using System;
using Verse;

namespace Cain.MineShaft
{
    class MineProperties
    {
        private const float workPerHitpoint = 2f;
        private const float densityFactor = 40f;
        private const float commonalityFactor = 2000;

        private const float minScatterCommonality = 0.001f;

        private static float WorkPerHitpoint => workPerHitpoint * MineParameters.MineWork.Value;
        private static float DensityFactor => densityFactor / MineParameters.Density.Value;
        private static float CommonalityFactor => commonalityFactor * MineParameters.Commonality.Value;

        private static GlobalMineShaftParameters _params;

        private static GlobalMineShaftParameters MineParameters => _params ?? (_params = LoadedModManager.GetMod<MineShaftMod>().Settings.GlobalParameters);

        protected readonly float _hitpointsPerLump;

        private readonly BuildingProperties _buildingProps;

        public MineProperties(float hitpointsPerLump, BuildingProperties buildingProperties)
        {
            _hitpointsPerLump = hitpointsPerLump;
            _buildingProps = buildingProperties;
        }

        public float Work => _hitpointsPerLump * WorkPerHitpoint +
            (_buildingProps.isResourceRock ?
            (float)(CommonalityFactor / Math.Sqrt(Math.Max(_buildingProps.mineableScatterCommonality, minScatterCommonality))) : 0);

        public float Yield => _buildingProps.isResourceRock
            ? (_buildingProps.mineableScatterLumpSizeRange.Average * _buildingProps.mineableYield) / DensityFactor
            : 1f;
    }
}
