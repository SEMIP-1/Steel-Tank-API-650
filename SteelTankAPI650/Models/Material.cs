namespace SteelTankAPI650.Models
{
    public class Material
    {
        public string Grade { get; set; }=null!;   // e.g., A36, A516 Gr70
        public double AllowableStress { get; set; }  // at design temp
        public double Density { get; set; } // kg/m3 (optional now)

        // Future: we can add temperature curves, weld efficiency, etc.
    }
}
