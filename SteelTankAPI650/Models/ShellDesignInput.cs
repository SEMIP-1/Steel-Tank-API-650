namespace SteelTankAPI650.Models
{
    public class ShellDesignInput
    {
        public double Diameter { get; set; }
        public double LiquidLevel { get; set; } // from tank bottom
        public double CorrosionAllowance { get; set; } = 0.0;
        public double SpecificGravity { get; set; } = 1.0; // default to water
        public double TestSpecificGravity { get; set; }= 1.0; // default to water
        public double JointEfficiency { get; set; }= 0.85;
        public double TestStressMultiplier { get; set; } = 0.6;
        public List<ShellCourse> ShellCourses { get; set; } = new();
    }
}