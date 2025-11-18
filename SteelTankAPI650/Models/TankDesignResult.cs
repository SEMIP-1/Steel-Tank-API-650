using System.Collections.Generic;

namespace SteelTankAPI650.Models
{
    public class TankDesignResult
    {
        public List<ShellCourseResult> ShellCourses { get; set; } = new();
        public double BottomThickness { get; set; }
        public double RoofThickness { get; set; }
        public List<string> Notes { get; set; } = new();
        
    }
}
