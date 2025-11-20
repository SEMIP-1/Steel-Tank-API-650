namespace SteelTankAPI650.Models.Bottom
{
    /// <summary>Result for annular + bottom plate design.</summary>
    public class BottomPlateResult
    {
        // --------- Annular ring thickness (Table 5.1a) ---------
        public double EffectiveDesignStress { get; set; }   // MPa (Sd,1)
        public double EffectiveTestStress { get; set; }     // MPa (St,1)
        public double GoverningShellStress { get; set; }    // MPa (max of above)

        public double MinAnnularCorrodedThickness { get; set; }  // mm (from Table 5.1a)
        public double MinAnnularRequiredThickness { get; set; }  // mm (incl. CA if required)
        public double UsedAnnularThickness { get; set; }         // mm (nominal used)

        // --------- Annular radial width ---------
        public double MinRadialWidthFromFormula { get; set; }    // mm (2 t_b √(Fy / (2 γ G H)))
        public double MinRadialWidthAtShell { get; set; }        // mm (e.g. 600 mm from Excel)
        public double RequiredRadialWidth { get; set; }          // mm (max of above)
        public double UsedRadialWidth { get; set; }              // mm (what you actually choose)

        // --------- Flat bottom (non-annular) ---------
        public double MinBottomCorrodedThickness { get; set; }   // mm (e.g. 6 mm)
        public double MinBottomRequiredThickness { get; set; }   // mm
        public double UsedBottomThickness { get; set; }          // mm
    }
}
