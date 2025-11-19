using SteelTankAPI650.Models;
using SteelTankAPI650.Services.Config;
using SteelTankAPI650.Models.Config;


namespace SteelTankAPI650.Services.Shell
{
    public class ShellDesignService : IShellDesignService
    {
        private readonly IDesignDataRepository _repo;

        public ShellDesignService(IDesignDataRepository repo)
        {
            _repo = repo;
        }

        // ============================================================
        //  MATERIAL ACCESS HELPER (Fixes all null warnings)
        // ============================================================
        private MaterialDefinition Mat(ShellCourse c)
        {
            if (c.Material == null)
                throw new Exception($"Material not loaded for course {c.CourseNumber}");

            return c.Material;
        }

        // ============================================================
        // PUBLIC MAIN FUNCTION (entire tank)
        // ============================================================
        public ShellDesignResult CalculateShell(ShellDesignInput input)
        {
            var result = new ShellDesignResult();

            // Resolve material from Excel DB
            foreach (var c in input.ShellCourses)
            {
                var m = _repo.GetMaterial(c.MaterialGrade);
                if (m == null)
                    throw new Exception($"Material '{c.MaterialGrade}' not found in Excel.");

                c.Material = m;
            }

            var courses = input.ShellCourses
                .OrderBy(c => c.CourseNumber)
                .ToList();

            ComputeCourseElevations(courses);

            // FIRST COURSE
            var r1 = CalculateBottomCourse(courses[0], input);
            result.Courses.Add(r1);

            // SECOND COURSE
            if (courses.Count > 1)
            {
                var r2 = CalculateSecondCourse(courses[1], r1, input);
                result.Courses.Add(r2);
            }

            // UPPER COURSES
            for (int i = 2; i < courses.Count; i++)
            {
                var r = CalculateUpperCourse(courses[i], result.Courses[i - 1], input);
                result.Courses.Add(r);
            }

            return result;
        }

        // ============================================================
        //  ELEVATIONS
        // ============================================================
        private void ComputeCourseElevations(List<ShellCourse> courses)
        {
            double current = 0.0;

            foreach (var c in courses)
            {
                c.BottomElevation = current;
                c.TopElevation = current + c.Height;
                current = c.TopElevation;
            }
        }

        // ============================================================
        //  FIRST COURSE — One-Foot + VDP
        // ============================================================
        private ShellCourseResult CalculateBottomCourse(ShellCourse c, ShellDesignInput input)
        {
            var m = Mat(c);

            var result = new ShellCourseResult
            {
                CourseNumber = c.CourseNumber,
                Height = c.Height,
                Material = m.Grade
            };

            double td_1f = Td_OneFoot(input, c);
            double tt_1f = Tt_OneFoot(input, c);

            double td_vdp = Td_VDP_Bottom(input, c);
            double tt_vdp = Tt_VDP_Bottom(input, c);

            result.Td_OneFoot = td_1f;
            result.Tt_OneFoot = tt_1f;
            result.Td_Variable = td_vdp;
            result.Tt_Variable = tt_vdp;

            double td_req = Math.Max(td_1f, td_vdp);
            double tt_req = Math.Max(tt_1f, tt_vdp);

            result.RequiredThickness = td_req;
            result.TestThickness = tt_req;

            result.GoverningMethod =
                (td_req == td_1f ? "One-Foot Method" : "Variable Design Point (VDP)");

            ApplyMinThickness(input, c, result);
            ApplyRounding(result);
            DetermineFinalAdoption(result);

            return result;
        }

        // ============================================================
        //  SECOND COURSE — Ratio Rule
        // ============================================================
        private ShellCourseResult CalculateSecondCourse(
            ShellCourse c, ShellCourseResult bottom, ShellDesignInput input)
        {
            var m = Mat(c);

            var result = new ShellCourseResult
            {
                CourseNumber = c.CourseNumber,
                Height = c.Height,
                Material = m.Grade
            };

            double D = input.Diameter;
            double r_mm = (D / 2.0) * 1000.0;
            double h1_mm = bottom.Height * 1000.0;

            double CA = input.CorrosionAllowance;

            double t1d = Math.Max(bottom.RequiredThickness - CA, 0);
            double t1t = bottom.TestThickness;

            double R = h1_mm / Math.Sqrt(r_mm * t1t);

            double t2a_d = ComputeUpperCoursePrelimDesign(input, t1d, c);
            double t2a_t = ComputeUpperCoursePrelimTest(input, t1t, c);

            double t2d, t2t;

            if (R <= 1.375)
            {
                t2d = t1d;
                t2t = t1t;
            }
            else if (R >= 2.625)
            {
                t2d = t2a_d;
                t2t = t2a_t;
            }
            else
            {
                double denom = 1.25 * Math.Sqrt(r_mm * t1t);
                double factor = 2.1 - (h1_mm / denom);

                t2d = t2a_d + (t1d - t2a_d) * factor;
                t2t = t2a_t + (t1t - t2a_t) * factor;
            }

            result.Td_Variable = Math.Round(t2d, 2);
            result.Tt_Variable = Math.Round(t2t, 2);

            result.RequiredThickness = Math.Round(t2d + CA, 2);
            result.TestThickness = Math.Round(t2t, 2);

            result.GoverningMethod = "Second-course ratio rule (5.6.4.5)";

            ApplyMinThickness(input, c, result);
            ApplyRounding(result);
            DetermineFinalAdoption(result);

            return result;
        }

        // ============================================================
        //  UPPER COURSES — API 650 5.6.4.6–7
        // ============================================================
        private ShellCourseResult CalculateUpperCourse(
            ShellCourse c, ShellCourseResult lower, ShellDesignInput input)
        {
            var m = Mat(c);

            var result = new ShellCourseResult
            {
                CourseNumber = c.CourseNumber,
                Height = c.Height,
                Material = m.Grade
            };

            double D = input.Diameter;
            double r_mm = (D / 2.0) * 1000.0;
            double CA = input.CorrosionAllowance;

            double Sd = m.Sd_MPa;
            double St = Sd * m.StMultiplier;

            double H = input.LiquidLevel - c.TopElevation;
            if (H < 0) H = 0;

            double tL_d = Math.Max(lower.RequiredThickness - CA, 0);
            double tL_t = lower.TestThickness;

            double tu_d = tL_d;
            for (int i = 0; i < 3; i++)
                tu_d = UpperCourseIter(D, r_mm, H, tu_d, tL_d, input.SpecificGravity, Sd);

            double tu_t = tL_t;
            for (int i = 0; i < 3; i++)
                tu_t = UpperCourseIter(D, r_mm, H, tu_t, tL_t, input.TestSpecificGravity, St);

            result.Td_Variable = Math.Round(tu_d, 2);
            result.Tt_Variable = Math.Round(tu_t, 2);

            result.RequiredThickness = Math.Round(tu_d + CA, 2);
            result.TestThickness = Math.Round(tu_t, 2);

            result.GoverningMethod = "Upper course VDP (5.6.4.6–7)";

            ApplyMinThickness(input, c, result);
            ApplyRounding(result);
            DetermineFinalAdoption(result);

            return result;
        }

        private double UpperCourseIter(double D, double r_mm, double H, double tu,
            double tL, double SG, double S)
        {
            double K = tL / tu;
            double C = (Math.Sqrt(K) * (K - 1)) / (1 + Math.Pow(K, 1.5));

            double x1 = 0.61 * Math.Sqrt(r_mm * tu) + 320 * C * H;
            double x2 = 1000 * C * H;
            double x3 = 1.22 * Math.Sqrt(r_mm * tu);

            double x = Math.Min(x1, Math.Min(x2, x3));

            return (4.9 * D * (H - x / 1000.0) * SG) / S;
        }

        // ============================================================
        //  FORMULA HELPERS (One-Foot + VDP)
        // ============================================================
        private double Td_OneFoot(ShellDesignInput input, ShellCourse c)
        {
            var m = Mat(c);

            double H = input.LiquidLevel;
            double Hred = Math.Max(H - 0.3048, 0.0);

            return (4.9 * input.Diameter * Hred * input.SpecificGravity) / m.Sd_MPa
                   + input.CorrosionAllowance;
        }

        private double Tt_OneFoot(ShellDesignInput input, ShellCourse c)
        {
            var m = Mat(c);

            double H = input.LiquidLevel;
            double Hred = Math.Max(H - 0.3048, 0.0);

            return (4.9 * input.Diameter * Hred * input.TestSpecificGravity)
                   / (m.Sd_MPa * m.StMultiplier);
        }

        private double Td_VDP_Bottom(ShellDesignInput input, ShellCourse c)
        {
            var m = Mat(c);

            double D = input.Diameter;
            double H = input.LiquidLevel;

            double A = 1.06 - (0.0696 * (D / H) *
                     Math.Sqrt((H * input.SpecificGravity) / m.Sd_MPa));

            double B = (4.9 * H * D * input.SpecificGravity) / m.Sd_MPa;

            return A * B + input.CorrosionAllowance;
        }

        private double Tt_VDP_Bottom(ShellDesignInput input, ShellCourse c)
        {
            var m = Mat(c);

            double D = input.Diameter;
            double H = input.LiquidLevel;

            double A = 1.06 - (0.0696 * (D / H) *
                     Math.Sqrt((H * input.TestSpecificGravity)
                     / (m.Sd_MPa * m.StMultiplier)));

            double B = (4.9 * H * D * input.TestSpecificGravity)
                      / (m.Sd_MPa * m.StMultiplier);

            return A * B;
        }

        private double ComputeUpperCoursePrelimDesign(
            ShellDesignInput input, double lowerCorroded, ShellCourse c)
        {
            var m = Mat(c);

            double D = input.Diameter;
            double H = input.LiquidLevel - c.BottomElevation;
            if (H <= 0) H = input.LiquidLevel;

            double G = input.SpecificGravity;
            double Sd = m.Sd_MPa;
            double r_mm = (D / 2.0) * 1000.0;

            double tu = lowerCorroded;

            for (int i = 0; i < 3; i++)
            {
                double K = lowerCorroded / tu;
                double C = (Math.Sqrt(K) * (K - 1)) /
                           (1.0 + Math.Pow(K, 1.5));

                double x1 = 0.61 * Math.Sqrt(r_mm * tu) + 320 * C * H;
                double x2 = 1000 * C * H;
                double x3 = 1.22 * Math.Sqrt(r_mm * tu);

                double x = Math.Min(x1, Math.Min(x2, x3));

                tu = (4.9 * D * (H - x / 1000.0) * G) / Sd;
            }

            return tu;
        }

        private double ComputeUpperCoursePrelimTest(
            ShellDesignInput input, double lowerCorroded, ShellCourse c)
        {
            var m = Mat(c);

            double D = input.Diameter;
            double H = input.LiquidLevel - c.BottomElevation;
            if (H <= 0) H = input.LiquidLevel;

            double Gt = input.TestSpecificGravity;
            double Sd = m.Sd_MPa;
            double St = Sd * m.StMultiplier;
            double r_mm = (D / 2.0) * 1000.0;

            double tu = lowerCorroded;

            for (int i = 0; i < 3; i++)
            {
                double K = lowerCorroded / tu;
                double C = (Math.Sqrt(K) * (K - 1)) /
                           (1.0 + Math.Pow(K, 1.5));

                double x1 = 0.61 * Math.Sqrt(r_mm * tu) + 320 * C * H;
                double x2 = 1000 * C * H;
                double x3 = 1.22 * Math.Sqrt(r_mm * tu);

                double x = Math.Min(x1, Math.Min(x2, x3));

                tu = (4.9 * D * (H - x / 1000.0) * Gt) / St;
            }

            return tu;
        }

        // ============================================================
        //   MINIMUM THICKNESS (Table 5.6a)
        // ============================================================
        private void ApplyMinThickness(
            ShellDesignInput input, ShellCourse c, ShellCourseResult r)
        {
            var rule = _repo.MinShellThickness
                .FirstOrDefault(x =>
                    x.CourseNumber == c.CourseNumber &&
                    input.Diameter <= x.MaxDiameterM);

            if (rule != null)
            {
                if (r.RequiredThickness < rule.MinThicknessMM)
                {
                    r.Notes.Add($"Minimum thickness applied from Table 5.6a: {rule.MinThicknessMM} mm");
                    r.RequiredThickness = rule.MinThicknessMM;
                }
            }
        }

        // ============================================================
        //   PLATE ROUNDING
        // ============================================================
        private void ApplyRounding(ShellCourseResult r)
        {
            double[] plates = _repo.PlateSizes
                .Select(p => p.ThicknessMM)
                .OrderBy(x => x)
                .ToArray();

            double Round(double t)
            {
                foreach (var p in plates)
                    if (t <= p) return p;

                return Math.Ceiling(t);
            }

            r.RequiredThickness = Round(r.RequiredThickness);
            r.TestThickness = Round(r.TestThickness);
        }

        // ============================================================
        //   GOVERN FINAL THICKNESS
        // ============================================================
        private void DetermineFinalAdoption(ShellCourseResult r)
        {
            r.AdoptedThickness =
                Math.Max(r.RequiredThickness, r.TestThickness);

            if (r.TestThickness > r.RequiredThickness)
                r.Notes.Add("Hydrostatic test thickness governed.");
        }
    }
}
