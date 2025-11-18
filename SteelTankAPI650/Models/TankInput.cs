using System.Collections.Generic;

namespace SteelTankAPI650.Models
{
    public class TankInput
    {
        public double Diameter { get; set; }  // m
        public double TotalHeight { get; set; }  // m

        public double? LiquidLevel { get; set; }  // m (default = TotalHeight)

        public double SpecificGravity { get; set; }  // design liquid SG
        public double CorrosionAllowance { get; set; }  // mm
        public double DesignTemperature { get; set; }  // °C

        public double JointEfficiency { get; set; } = 0.85;

        // Hydrostatic test parameters
        public double TestSpecificGravity { get; set; } = 1.0;  // usually water
        public double TestStressMultiplier { get; set; } = 0.9; // S_test = 0.9 * S @ 60°C

        public List<ShellCourse> ShellCourses { get; set; } = new();
    }
}
