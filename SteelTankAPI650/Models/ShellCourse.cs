namespace SteelTankAPI650.Models
{
    public class ShellCourse
    {
        public int CourseNumber { get; set; }  // 1 = bottom course
        public double Height { get; set; }  // m
        public Material Material { get; set; } = null!;
        public double BottomElevation { get; set; }
        public double TopElevation { get; set; }

    }
}
