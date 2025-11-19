using SteelTankAPI650.Models.Config;

namespace SteelTankAPI650.Models
{
    public class ShellCourse
    {
        public int CourseNumber { get; set; }     // 1 = bottom course
        public double Height { get; set; }        // m

        // Material grade entered by user
        public string MaterialGrade { get; set; } = "";

        // Computed internally
        public double BottomElevation { get; set; }
        public double TopElevation { get; set; }

        // Loaded from Excel
        public MaterialDefinition? Material { get; set; }
    }
}
