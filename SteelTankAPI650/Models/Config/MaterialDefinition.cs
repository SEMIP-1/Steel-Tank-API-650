namespace SteelTankAPI650.Models.Config
{
    public class MaterialDefinition
    {
        public string Grade { get; set; } = null!;
        public double Sd_MPa { get; set; }
        public double StMultiplier { get; set; }
        public double Density { get; set; }
        public string? Note { get; set; }
    }
}
