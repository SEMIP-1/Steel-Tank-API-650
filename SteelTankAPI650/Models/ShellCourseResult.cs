namespace SteelTankAPI650.Models
{
    public class ShellCourseResult
    {
        public int CourseNumber { get; set; }
        public double Height { get; set; }
        public string Material { get; set; } = null!;

        // Heads
        public double HydrostaticHead { get; set; }
        public double HydrostaticHead_OneFoot { get; set; }

        // Thicknesses
        public double Td_Variable { get; set; }
        public double Td_OneFoot { get; set; }

        public double Tt_Variable { get; set; }
        public double Tt_OneFoot { get; set; }

        public double RequiredThickness { get; set; }    // governing td
        public double TestThickness { get; set; }        // governing tt
        public double AdoptedThickness { get; set; }

        public string GoverningMethod { get; set; } = null!;
    }
}
