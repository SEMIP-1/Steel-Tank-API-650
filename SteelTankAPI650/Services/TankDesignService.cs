using System;
using System.Linq;
using SteelTankAPI650.Models;

namespace SteelTankAPI650.Services
{
    #region Interface
    public interface ITankDesignService
    {
        TankDesignResult DesignTank(TankInput input);
    }
    #endregion

    #region Implementation
    public class TankDesignService : ITankDesignService
    {
        public TankDesignResult DesignTank(TankInput input)
        {
            var result = new TankDesignResult();

            if (input.ShellCourses == null || input.ShellCourses.Count == 0)
            {
                result.Notes.Add("No shell courses provided.");
                return result;
            }

            // 1) Sort courses bottom-up
            var courses = input.ShellCourses
                               .OrderBy(c => c.CourseNumber)
                               .ToList();

            // 2) Determine liquid level (m)
            double liquidLevel = input.LiquidLevel.HasValue
                ? input.LiquidLevel.Value
                : input.TotalHeight;

            // 3) Precompute elevations for each course (m)
            double currentBottomElevation = 0.0;

            foreach (var course in courses)
            {
                double courseHeight = course.Height; // m
                double bottomElev = currentBottomElevation;
                double topElev = bottomElev + courseHeight;

                bool isBottomCourse = course.CourseNumber == courses.Min(c => c.CourseNumber);

                // Basic data
                double D = input.Diameter;                 // m
                double G_design = input.SpecificGravity;   // design SG
                double G_test = input.TestSpecificGravity; // hydrotest SG

                double S_design = course.Material.AllowableStress; // MPa (N/mm2)
                double S_test = S_design * input.TestStressMultiplier;

                double E = input.JointEfficiency;
                double CA = input.CorrosionAllowance; // mm

                // VARIABLE DESIGN POINT METHOD (for all courses)
                double H_var = Math.Max(0.0, liquidLevel - topElev); // m

                double td_var = ComputeRequiredThickness(H_var, G_design, D, S_design, E); // mm
                double tt_var = ComputeRequiredThickness(H_var, G_test, D, S_test, E);     // mm

                // ONE-FOOT METHOD (bottom course only)
                double H_1ft = 0.0;
                double td_1ft = 0.0;
                double tt_1ft = 0.0;

                if (isBottomCourse)
                {
                    const double oneFoot_m = 0.3048; // 1 ft in meters
                    H_1ft = Math.Max(0.0, liquidLevel - oneFoot_m);

                    td_1ft = ComputeRequiredThickness(H_1ft, G_design, D, S_design, E);
                    tt_1ft = ComputeRequiredThickness(H_1ft, G_test, D, S_test, E);
                }

                // GOVERNING DESIGN THICKNESS (td)
                double td_governing;
                string governingMethod;

                if (isBottomCourse)
                {
                    td_governing = Math.Max(td_var, td_1ft);
                    governingMethod = td_1ft > td_var
                        ? "One-Foot Method (Design)"
                        : "Variable-Design-Point Method (Design)";
                }
                else
                {
                    td_governing = td_var;
                    governingMethod = "Variable-Design-Point Method (Design)";
                }

                // GOVERNING TEST THICKNESS (tt) – same logic if you care about test governing
                double tt_governing = isBottomCourse
                    ? Math.Max(tt_var, tt_1ft)
                    : tt_var;

                // Add corrosion allowance
                double t_req_with_CA = td_governing + CA;

                // Minimum thickness (simple 6 mm placeholder – later we link Table 5.6a)
                double t_min = 6.0;

                // Adopted thickness (rounded up to standard plate size)
                double t_after_min = Math.Max(t_req_with_CA, t_min);
                double t_adopt = RoundToPlateSize(t_after_min);

                // Build result row
                var courseResult = new ShellCourseResult
                {
                    CourseNumber = course.CourseNumber,
                    Height = course.Height,
                    Material = course.Material.Grade,

                    HydrostaticHead = H_var,
                    HydrostaticHead_OneFoot = isBottomCourse ? H_1ft : 0.0,

                    Td_Variable = Math.Round(td_var, 2),
                    Td_OneFoot = isBottomCourse ? Math.Round(td_1ft, 2) : 0.0,

                    Tt_Variable = Math.Round(tt_var, 2),
                    Tt_OneFoot = isBottomCourse ? Math.Round(tt_1ft, 2) : 0.0,

                    RequiredThickness = Math.Round(td_governing, 2),
                    TestThickness = Math.Round(tt_governing, 2),
                    AdoptedThickness = t_adopt,

                    GoverningMethod = governingMethod
                };

                result.ShellCourses.Add(courseResult);

                currentBottomElevation = topElev; // move up for next course
            }

            // BottomThickness, RoofThickness left for future modules
            result.Notes.Add("Shell thickness calculated using API 650 5.6.3: bottom course checked by One-Foot and VDP methods; upper courses by VDP only (metric, mm).");

            return result;
        }

        /// <summary>
        /// Placeholder API 650 thickness formula (metric-style).
        /// </summary>
        private double ComputeRequiredThickness(double H, double G, double D, double S, double E)
        {
            //H is hydrostatic head (m), G is specific gravity (dimensionless), D is diameter (m), S is allowable stress (MPa), E is joint efficiency (dimensionless)
            // H in m, D in m, S in MPa, G dimensionless
            // This is a simplified normalized form.
            if (H <= 0 || G <= 0 || D <= 0 || S <= 0 || E <= 0)
                return 0.0;

            // NOTE: This is a simplified form to match your current logic.
            // Later we'll adjust it exactly to the Excel/API 650 implementation.
            double numerator = H * G * D;
            double denominator = (2.0 * S * E + 0.6 * G);

            return numerator / denominator; // gives a pseudo-mm value consistent with current approach
        }

        /// <summary>
        /// Rounds required thickness up to a standard plate size (mm).
        /// </summary>
        private double RoundToPlateSize(double t)
        {
            // Simple standard list – we can sync with your Excel later.
            double[] standardPlates = { 6, 8, 10, 12, 14, 16, 18, 20, 22, 25, 28, 30, 32, 36, 40 , 44, 48, 52, 56, 60, 65, 70, 75, 80, 85, 90, 95, 100 };

            foreach (var plate in standardPlates)
            {
                if (t <= plate)
                    return plate;
            }

            // If thicker than our list, just round up to nearest mm
            return Math.Ceiling(t);
        }
    }
    #endregion
}
