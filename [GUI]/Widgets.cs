using System;
using UnityEngine;

namespace Cain.MineShaft
{
    public class ExtraWidgets
    {
        public static float LogarithmicScaleSlider(Rect rect, float value, float minVal, float maxVal, Func<float, string> valueFormatter, string leftAlignedLabel = null, string rightAlignedLabel = null)
        {
            return (float)Math.Exp(Verse.Widgets.HorizontalSlider(rect, (float)Math.Log(value), minVal, maxVal, true, valueFormatter(value), leftAlignedLabel, rightAlignedLabel, -1f));
        }
    }
}
