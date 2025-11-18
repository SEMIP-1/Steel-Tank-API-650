using SteelTankAPI650.Models;

namespace SteelTankAPI650.Services.Shell
{
    public class ShellDesignService : IShellDesignService
    {
        // -----------------------------------------------------------
        // PUBLIC COSTRUCTOR
        #region CONSTRUCTOR
        public ShellDesignResult CalculateShell(ShellDesignInput input)
        {
            var result = new ShellDesignResult();

            // Sort courses from bottom to top
            var courses = input.ShellCourses
                               .OrderBy(c => c.CourseNumber)
                               .ToList();

            // Compute elevations
            ComputeCourseElevations(courses);

            // FIRST COURSE (SPECIAL: One-Foot AND VDP)
            var firstCourse = courses.First();
            var firstResult = CalculateBottomCourse(firstCourse, input);
            result.Courses.Add(firstResult);

            // SECOND COURSE (Uses ratio rule 5.6.4.5)
            if (courses.Count > 1)
            {
                var secondCourse = courses[1];
                var secondResult = CalculateSecondCourse(secondCourse, firstResult, input);
                result.Courses.Add(secondResult);
            }

            // UPPER COURSES (5.6.4.6 & 5.6.4.7)
            for (int i = 2; i < courses.Count; i++)
            {
                var upperResult = CalculateUpperCourse(courses[i], result.Courses[i - 1], input);
                result.Courses.Add(upperResult);
            }

            return result;
        }
        #endregion
        
        // -----------------------------------------------------------
        // INTERNAL CALCULATION CORE
        #region CALCULATION CORE
        private void ComputeCourseElevations(List<ShellCourse> courses)
        {
            double current = 0.0;
            foreach (var c in courses)
            {
                c.BottomElevation = current;           // store in new property
                c.TopElevation = current + c.Height;   // store in new property
                current = c.TopElevation;
            }
        }
        #endregion
        
        // -----------------------------------------------------------
        // COURSE CALCULATIONS
        #region COURSE CALCULATIONS
        // FIRST COURSE
        private ShellCourseResult CalculateBottomCourse(ShellCourse c, ShellDesignInput input)
        {
            var result = new ShellCourseResult
            {
                CourseNumber = c.CourseNumber,
                Height = c.Height,
                Material = c.Material.Grade
            };

            // ONE-FOOT method td & tt
            double td_1f = Td_OneFoot(input, c);
            double tt_1f = Tt_OneFoot(input, c);

            // VDP method td & tt
            double td_vdp = Td_VDP_Bottom(input, c);
            double tt_vdp = Tt_VDP_Bottom(input, c);

            result.Td_OneFoot = td_1f;
            result.Tt_OneFoot = tt_1f;

            result.Td_Variable = td_vdp;
            result.Tt_Variable = tt_vdp;

            // Governing design thickness
            result.RequiredThickness = Math.Max(td_1f, td_vdp);
            result.TestThickness = Math.Max(tt_1f, tt_vdp);

            result.GoverningMethod =
                (result.RequiredThickness == td_1f) ? "One-Foot Method" : "Variable Design Point (VDP)";

            return result;
        }

        // SECOND COURSE (ratio method)
        private ShellCourseResult CalculateSecondCourse(ShellCourse c, ShellCourseResult bottom, ShellDesignInput input)
        {
            var result = new ShellCourseResult
            {
                CourseNumber = c.CourseNumber,
                Height = c.Height,
                Material = c.Material.Grade
            };

            // --- Common geometric values ---
            double D = input.Diameter;                  // m
            double r_mm = (D / 2.0) * 1000.0;           // mm
            double h1_mm = bottom.Height * 1000.0;      // bottom course height in mm

            // Bottom course corroded thicknesses
            double CA = input.CorrosionAllowance;

            // Design corroded thickness t1 (bottom)
            double t1d_corroded = Math.Max(bottom.RequiredThickness - CA, 0.0);

            // Test corroded thickness t1t (bottom) – already no CA
            double t1t_corroded = bottom.TestThickness;

            // Ratio R = h1 / sqrt(r * t1t)
            double R_design = h1_mm / Math.Sqrt(r_mm * t1t_corroded);

            // ---------------- DESIGN CONDITION ----------------
            // Preliminary second-course corroded thickness t2a (design)
            double t2a_d_corroded = ComputeUpperCoursePrelimDesign(
                input, lowerCorroded: t1d_corroded, course: c);

            double t2d_corroded;

            if (R_design <= 1.375)
            {
                t2d_corroded = t1d_corroded;
            }
            else if (R_design >= 2.625)
            {
                t2d_corroded = t2a_d_corroded;
            }
            else
            {
                double denom = 1.25 * Math.Sqrt(r_mm * t1t_corroded);
                double factor = 2.1 - (h1_mm / denom);
                t2d_corroded = t2a_d_corroded + (t1d_corroded - t2a_d_corroded) * factor;
            }

            // Add CA back to get required design thickness
            double t2d_required = t2d_corroded + CA;

            // ---------------- TEST CONDITION ----------------
            double t2a_t_corroded = ComputeUpperCoursePrelimTest(
                input, lowerTestCorroded: t1t_corroded, course: c);

            double t2t_corroded;

            if (R_design <= 1.375)
            {
                t2t_corroded = t1t_corroded;
            }
            else if (R_design >= 2.625)
            {
                t2t_corroded = t2a_t_corroded;
            }
            else
            {
                double denom = 1.25 * Math.Sqrt(r_mm * t1t_corroded);
                double factor = 2.1 - (h1_mm / denom);
                t2t_corroded = t2a_t_corroded + (t1t_corroded - t2a_t_corroded) * factor;
            }

            double t2t_required = t2t_corroded; // no CA for test thickness

            // Fill result object
            result.Td_Variable = Math.Round(t2d_corroded, 2);
            result.Tt_Variable = Math.Round(t2t_corroded, 2);
            result.RequiredThickness = Math.Round(t2d_required, 2);
            result.TestThickness = Math.Round(t2t_required, 2);
            result.GoverningMethod = "Second-course ratio method (API 650 5.6.4.5)";

            return result;
        }


        // UPPER COURSES (x1,x2,x3)
        private ShellCourseResult CalculateUpperCourse(ShellCourse c, ShellCourseResult lower, ShellDesignInput input)
        {
            var result = new ShellCourseResult
            {
                CourseNumber = c.CourseNumber,
                Height = c.Height,
                Material = c.Material.Grade
            };

            // FULL API 650 §5.6.4.6–5.6.4.7 logic will be inserted here

            return result;
        }
        #endregion
        
        // -----------------------------------------------------------
        // FORMULA HELPERS (PLACEHOLDERS — WE WILL FILL NEXT)
        #region FORMULAHELPERS
        // --------------------------------------------
        // API 650 §5.6.3.2 — ONE-FOOT METHOD (SI Units)
        // --------------------------------------------
        // Design thickness using One-Foot method
        private double Td_OneFoot(ShellDesignInput input, ShellCourse c)
        {
            // H = liquid level above bottom (meters)
            double H = input.LiquidLevel;

            // Reduced head = (H - 0.3 m)
            double H_reduced = Math.Max(0.0, H - 0.3048);

            // D = tank diameter (meters)
            double D = input.Diameter;

            // G = specific gravity
            double G = input.SpecificGravity;

            // S = allowable design stress (MPa)
            double Sd = c.Material.AllowableStress;

            // CA = corrosion allowance (mm)
            double CA = input.CorrosionAllowance;

            // API equation:
            // td = [4.9 · D · (H−0.3) · G] / Sd   + CA
            double td = (4.9 * D * H_reduced * G) / Sd;

            // Result in mm
            return td + CA;
        }
        // Hydrostatic test thickness using One-Foot method
        private double Tt_OneFoot(ShellDesignInput input, ShellCourse c)
        {
            double H = input.LiquidLevel;
            double H_reduced = Math.Max(0.0, H - 0.3048);
            double D = input.Diameter;

            // Test SG
            double Gt = input.TestSpecificGravity;

            // Test stress = Sd × multiplier (per Annex M)
            double Sd = c.Material.AllowableStress;
            double St = Sd * input.TestStressMultiplier;

            // tt = [4.9 · D · (H−0.3) · Gt] / St
            double tt = (4.9 * D * H_reduced * Gt) / St;

            return tt; // no CA added for test thickness
        }
        // -----------------------------------------------------------
        // API 650 §5.6.4.4 — VARIABLE DESIGN POINT METHOD (BOTTOM COURSE)
        // -----------------------------------------------------------
        // Design case VDP thickness
        private double Td_VDP_Bottom(ShellDesignInput input, ShellCourse c)
        {
            double D = input.Diameter;           // tank diameter in m
            double H = input.LiquidLevel;        // liquid height in m
            double G = input.SpecificGravity;    // design SG
            double Sd = c.Material.AllowableStress; // allowable design stress (MPa)
            double CA = input.CorrosionAllowance;   // corrosion allowance (mm)

            if (H <= 0) return 0;

            // Term A = 1.06 - (0.0696 * D/H * sqrt(H*G/Sd))
            double A = 1.06 - (0.0696 * (D / H) * Math.Sqrt((H * G) / Sd));

            // Term B = (4.9 * H * D * G) / Sd
            double B = (4.9 * H * D * G) / Sd;

            // Final thickness
            double td = A * B + CA;

            return td;
        }
        // Hydrostatic test case VDP thickness
        private double Tt_VDP_Bottom(ShellDesignInput input, ShellCourse c)
        {
            double D = input.Diameter;
            double H = input.LiquidLevel;
            double Gt = input.TestSpecificGravity;   // test SG
            double Sd = c.Material.AllowableStress;
            double St = Sd * input.TestStressMultiplier; // test allowable stress

            if (H <= 0) return 0;

            // Term A = 1.06 - (0.0696 * D/H * sqrt(H*Gt/St))
            double A = 1.06 - (0.0696 * (D / H) * Math.Sqrt((H * Gt) / St));

            // Term B = (4.9 * H * D * Gt) / St
            double B = (4.9 * H * D * Gt) / St;

            // No CA added in test case
            double tt = A * B;

            return tt;
        }
        
        // --------------------------------------------------------------------
        
        // PRELIMINARY UPPER-COURSE THICKNESS FOR SECOND COURSE (DESIGN/TEST)
        // Approximate implementation of API 650 5.6.4.6–5.6.4.7
        // --------------------------------------------------------------------

        // Design condition preliminary thickness (corroded) for upper course
        private double ComputeUpperCoursePrelimDesign(ShellDesignInput input, double lowerCorroded, ShellCourse course)
        {
            double D = input.Diameter;                   // m
            double H = input.LiquidLevel - course.BottomElevation; // approximation
            if (H <= 0) H = input.LiquidLevel;

            double G = input.SpecificGravity;
            double Sd = course.Material.AllowableStress;

            double r_mm = (D / 2.0) * 1000.0;            // mm

            // Start with lower course thickness as initial guess
            double tu = lowerCorroded;

            for (int i = 0; i < 3; i++) // 3 trial iterations
            {
                double K = lowerCorroded / tu;
                double C = (Math.Sqrt(K) * (K - 1.0)) / (1.0 + Math.Pow(K, 1.5));

                double x1 = 0.61 * Math.Sqrt(r_mm * tu) + 320.0 * C * H;
                double x2 = 1000.0 * C * H;
                double x3 = 1.22 * Math.Sqrt(r_mm * tu);

                double x = Math.Min(x1, Math.Min(x2, x3)); // mm

                // tdx (corroded) from 5.6.4.7 (SI) without CA
                double tdx_corroded = (4.9 * D * (H - x / 1000.0) * G) / Sd;

                tu = tdx_corroded; // next trial
            }

            return tu;
        }

        // Test condition preliminary thickness (corroded) for upper course
        private double ComputeUpperCoursePrelimTest(ShellDesignInput input, double lowerTestCorroded, ShellCourse course)
        {
            double D = input.Diameter;
            double H = input.LiquidLevel - course.BottomElevation;
            if (H <= 0) H = input.LiquidLevel;

            double Gt = input.TestSpecificGravity;

            double Sd = course.Material.AllowableStress;
            double St = Sd * input.TestStressMultiplier;

            double r_mm = (D / 2.0) * 1000.0;

            double tu = lowerTestCorroded;

            for (int i = 0; i < 3; i++)
            {
                double K = lowerTestCorroded / tu;
                double C = (Math.Sqrt(K) * (K - 1.0)) / (1.0 + Math.Pow(K, 1.5));

                double x1 = 0.61 * Math.Sqrt(r_mm * tu) + 320.0 * C * H;
                double x2 = 1000.0 * C * H;
                double x3 = 1.22 * Math.Sqrt(r_mm * tu);

                double x = Math.Min(x1, Math.Min(x2, x3));

                double ttx_corroded = (4.9 * D * (H - x / 1000.0) * Gt) / St;

                tu = ttx_corroded;
            }

            return tu;
        }

        #endregion
    }
}
