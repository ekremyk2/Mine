using System.Collections.Generic;
using Verse;

namespace MinesAutomatedExtension
{
    public class MineableSettings : DefModExtension
    {
        public List<ResearchProjectDef> researchPrerequisites;

        public int workOffset = 0;

        public float workMultiplier = 1.0f;

        public int mineableYield = -1;
    }
}
