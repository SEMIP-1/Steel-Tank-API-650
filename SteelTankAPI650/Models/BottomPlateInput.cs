using System.Collections.Generic;

namespace SteelTankAPI650.Models.Bottom
{
    /// <summary>
    /// Input data for bottom-plate & annular-plate design (API 650 5.5 & 5.1).
    /// For annular ring we follow your Excel "Bottom Plates" sheet.
    /// </summary>
    public class BottomPlateInput
    {
        // --- Global tank data ---
        public double Diameter { get; set; }           // m
        public double LiquidHeight { get; set; }       // m (design liquid level H)
        public double SpecificGravity { get; set; } = 1.0;
        public double CorrosionAllowance { get; set; } = 2.0; // mm

        // --- First shell course data (from shell design result) ---
        public double FirstShellNominalThickness { get; set; }   // mm (nominal t)
        public double FirstShellRequiredTd { get; set; }         // mm (design td req.)
        public double FirstShellRequiredTt { get; set; }         // mm (test tt req.)
        public double FirstShellSd { get; set; }                 // MPa (design stress Sd)
        public double FirstShellSt { get; set; }                 // MPa (test stress St)

        // --- Annular plate properties ---
        /// <summary>Nominal annular plate thickness, mm (trial/selected).</summary>
        public double AnnularNominalThickness { get; set; }      // mm (t_b nominal)

        /// <summary>Minimum yield strength of annular plate, MPa (Fy in your sheet).</summary>
        public double AnnularYieldStrength { get; set; }         // MPa

        /// <summary>
        /// Optional override for density factor γ (MPa/m). If null we use 0.0098 (your Excel).
        /// </summary>
        public double? WaterDensityFactor { get; set; }          // MPa/m
    }
}
