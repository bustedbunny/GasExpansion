using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GasExpansion.Gas.GasTrackers
{
    public class WeatherTracker : IExposable
    {
        private float windDirectionTarget = Rand.Range(0, 360f);
        private float windDirection;
        public float windDirectionFast = 0;
        public float windStrength = 0;
        public float WindDirection => windDirection;
        private static readonly string[] windDirections = new string[8] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        public readonly float[] windOffset = new float[4];

        public byte[] pipeGrid;

        private const float rangeMultiplier = 1f;
        public bool direction = false;
        public Vector2 winddir = new();
        public GasMapComponent parent;

        public WeatherTracker(GasMapComponent parent)
        {
            this.parent = parent;
        }
        public void UpdateTracker()
        {
            UpdateWind();
            CalculateWindOffsets();

        }
        private void UpdateWind()
        {

            float dif = (windDirectionTarget - windDirection);
            if (dif > 0)
            {
                if (dif < 180)
                {
                    windDirection += Rand.Range(0f, 5f) * rangeMultiplier;
                }
                else
                {
                    windDirection -= Rand.Range(0f, 5f) * rangeMultiplier;
                }
            }
            else
            {
                if (dif < -180)
                {
                    windDirection += Rand.Range(0f, 5f) * rangeMultiplier;
                }
                else
                {
                    windDirection -= Rand.Range(0f, 5f) * rangeMultiplier;
                }
            }
            if (windDirection > 360f)
            {
                windDirection -= 360f;
            }
            if (windDirection < 0f)
            {
                windDirection += 360f;
            }

            while (Math.Abs(windDirectionTarget - windDirection) < 1f)
            {
                while (Math.Abs(windDirectionTarget - windDirection) < 30f)
                {

                    windDirectionTarget = Rand.Range(0, 360);
                }
            }
            windDirectionFast = Mathf.Deg2Rad * windDirection;
        }
        private const double minOffset = 0.5;
        private void CalculateWindOffsets()
        {
            double angle = windDirectionFast;
            double strength = (double)Math.Min(parent.map.windManager.WindSpeed * 0.67, 6);
            double N = minOffset + Math.Pow(1.0 + Math.Cos(angle), strength);
            double E = minOffset + Math.Pow(1.0 + Math.Sin(angle), strength);
            double S = minOffset + Math.Pow(1.0 + Math.Cos(angle + Math.PI), strength);
            double W = minOffset + Math.Pow(1.0 + Math.Sin(angle + Math.PI), strength);
            double sum = minOffset * 4 * Math.Max(1, strength) / (N + E + S + W);
            windOffset[0] = (float)(N * sum);
            windOffset[1] = (float)(E * sum);
            windOffset[2] = (float)(S * sum);
            windOffset[3] = (float)(W * sum);
        }

        public void DoWindGUI(float xPos, ref float yPos)
        {
            float num = 100f;
            float width = 200f + num;
            float num2 = 26f;
            Rect rect = new(xPos - num, yPos - num2, width, num2);
            Text.Anchor = TextAnchor.MiddleRight;
            rect.width -= 15f;
            Text.Font = GameFont.Small;
            Widgets.Label(rect, WindStrengthText + WindDirectionText);
            TooltipHandler.TipRegion(rect, "GE_Wind_Tooltip".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            yPos -= num2;
        }

        private string WindDirectionText
        {
            get
            {
                if (BeaufortScale == 0)
                {
                    return "";
                }
                float direction = windDirection + 22.5f;
                if (direction > 360)
                {
                    direction -= 360;
                }
                int num = (int)(direction / 45f);
                return ", " + ("GE_Wind_Direction_" + windDirections[num]).Translate();
            }
        }
        private float WindStrength => Mathf.Min(3f * parent.map.windManager.WindSpeed, 9f);
        private int BeaufortScale => Mathf.FloorToInt(WindStrength);
        private string WindStrengthText => ("GE_Wind_Beaufort" + BeaufortScale).Translate();

        public void ExposeData()
        {
            Scribe_Values.Look(ref windDirection, "windDirection");
            Scribe_Values.Look(ref windDirectionTarget, "windDirectionTarget");
            Scribe_Values.Look(ref winddir, "winddir");
        }
    }
}
