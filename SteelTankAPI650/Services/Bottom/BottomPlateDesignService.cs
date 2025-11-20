using System;
using System.Collections.Generic;
using System.Linq;
using SteelTankAPI650.Models.Bottom;

namespace SteelTankAPI650.Services.Bottom
{
    public class BottomPlateDesignService : IBottomPlateDesignService
    {
        // Represents one row of Table 5.1a (SI).
        private record AnnularThicknessRow(
            double MinShellThk, double MaxShellThk,
            double T_le190, double T_le210, double T_le220, double T_le250);

        // TODO: Fill this table from your Excel "Table 5.1a – Annular bottom-plate thicknesses (SI)".
        // Numbers below are placeholders so the code compiles.
        //                shell t range (mm)      Sd<=190  Sd<=210  Sd<=220  Sd<=250
        private static readonly List<AnnularThicknessRow> Table5_1a = new()
        {
            new AnnularThicknessRow(  0.0, 19.0,  6,  7,  8,  9),
            new AnnularThicknessRow( 19.0, 25.0,  6,  7,  9, 10),
            new AnnularThicknessRow( 25.0, 32.0,  6,  8, 10, 11),
            new AnnularThicknessRow( 32.0, 40.0,  7,  9, 11, 12),
            new AnnularThicknessRow( 40.0, 45.0,  9, 13, 16, 19),
        };

        public BottomPlateResult DesignBottom(BottomPlateInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var result = new BottomPlateResult();

            // ---------------- First shell effective stresses ----------------
            double CA = input.CorrosionAllowance;              // mm
            double t_nom = input.FirstShellNominalThickness;   // mm
            double td_req = input.FirstShellRequiredTd;        // mm
            double tt_req = input.FirstShellRequiredTt;        // mm

            double t_corroded = Math.Max(t_nom - CA, 0.0);     // "Corroded t (Nominal t - CA)" in Excel

            // Sd1 = [(td - CA) / corroded t] * Sd
            double Sd1 = (td_req - CA) / t_corroded * input.FirstShellSd;

            // St1 = [tt / nominal t] * St
            double St1 = tt_req / t_nom * input.FirstShellSt;

            double Sg = Math.Max(Sd1, St1);                    // "Max stress in 1st shell"
            result.EffectiveDesignStress = Sd1;
            result.EffectiveTestStress = St1;
            result.GoverningShellStress = Sg;

            // ---------------- Annular plate thickness (Table 5.1a) ---------
            double t_ann_corroded_min = LookupAnnularThickness(
                t_nom, Sg);

            result.MinAnnularCorrodedThickness = t_ann_corroded_min;
            // If you want to add CA to annular plate, uncomment next line:
            // result.MinAnnularRequiredThickness = t_ann_corroded_min + CA;
            result.MinAnnularRequiredThickness = t_ann_corroded_min; // like your screenshot (no CA)
            result.UsedAnnularThickness = input.AnnularNominalThickness;

            // ---------------- Radial width of annular ring -------------------
            double Fy = input.AnnularYieldStrength;            // MPa
            double H = input.LiquidHeight;                     // m
            double G = input.SpecificGravity;
            double gamma = input.WaterDensityFactor ?? 0.0098; // MPa/m (from your sheet)

            // RadialWidth = 2 * t_b * sqrt(Fy / (2 * γ * G * H))
            // t_b must be corroded thickness used in width calculation
            double t_b = t_ann_corroded_min;                   // mm

            double radialWidthFormula =
                2.0 * t_b * Math.Sqrt(Fy / (2.0 * gamma * G * H));

            result.MinRadialWidthFromFormula = radialWidthFormula;

            // From your sheet: fixed minimum 600 mm near shell + lap
            const double minShellLapWidth = 600.0;             // mm
            result.MinRadialWidthAtShell = minShellLapWidth;

            // Required radial width = max(Formula, 600 mm)
            result.RequiredRadialWidth = Math.Max(radialWidthFormula, minShellLapWidth);

            // For now, assume user adopts required value (you can change later)
            result.UsedRadialWidth = result.RequiredRadialWidth;

            // ---------------- Flat bottom thickness --------------------------
            // From your sheet: minimum bottom plate corroded thickness = 6 mm
            const double minBottomCorroded = 6.0;              // mm

            result.MinBottomCorrodedThickness = minBottomCorroded;
            result.MinBottomRequiredThickness = minBottomCorroded; // no CA in your screenshot
            result.UsedBottomThickness = minBottomCorroded;        // can be adjusted from UI

            return result;
        }

        /// <summary>
        /// Looks up minimum annular corroded plate thickness from Table 5.1a
        /// based on nominal first-shell thickness and governing shell stress.
        /// </summary>
        private static double LookupAnnularThickness(double shellThk, double governingStress)
        {
            // Find row for shell thickness range
            var row = Table5_1a.FirstOrDefault(r =>
                shellThk > r.MinShellThk && shellThk <= r.MaxShellThk);

            if (row == null)
            {
                // Outside table range → fallback to lowest row
                row = Table5_1a.Last();
            }

            // Choose column by stress
            if (governingStress <= 190.0) return row.T_le190;
            if (governingStress <= 210.0) return row.T_le210;
            if (governingStress <= 220.0) return row.T_le220;
            // <= 250 or above
            return row.T_le250;
        }
    }
}
