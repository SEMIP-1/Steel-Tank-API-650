using ClosedXML.Excel;
using SteelTankAPI650.Models.Config;

namespace SteelTankAPI650.Services.Config
{
    public class ExcelDesignDataRepository : IDesignDataRepository
    {
        // ============================================================
        // INTERNAL (editable) LISTS – used by CRUD
        // ============================================================
        public List<MaterialDefinition> MaterialsInternal => _materials;
        public List<PlateSize> PlateSizesInternal => _plateSizes;
        public List<MinShellThicknessRule> MinShellThicknessInternal => _minShellThickness;

        // Private storage
        private readonly List<MaterialDefinition> _materials = new();
        private readonly List<PlateSize> _plateSizes = new();
        private readonly List<MinShellThicknessRule> _minShellThickness = new();
        private readonly Dictionary<string, string> _settings = new(StringComparer.OrdinalIgnoreCase);

        private readonly string _excelPath;

        // ============================================================
        // PUBLIC READ ONLY ACCESS (for design calcs)
        // ============================================================
        public IReadOnlyList<MaterialDefinition> Materials => _materials;
        public IReadOnlyList<PlateSize> PlateSizes => _plateSizes;
        public IReadOnlyList<MinShellThicknessRule> MinShellThickness => _minShellThickness;

        // ============================================================
        // CONSTRUCTOR — loads Excel on startup
        // ============================================================
        public ExcelDesignDataRepository(IWebHostEnvironment env)
        {
            _excelPath = Path.Combine(env.ContentRootPath, "Data", "API650_Data.xlsx");

            if (!File.Exists(_excelPath))
                throw new FileNotFoundException("API650_Data.xlsx not found", _excelPath);

            using var wb = new XLWorkbook(_excelPath);

            LoadMaterials(wb.Worksheet("Materials"));
            LoadPlateSizes(wb.Worksheet("PlateThicknesses"));
            LoadMinShellThickness(wb.Worksheet("MinShellThickness"));
            LoadSettings(wb.Worksheet("Settings"));
        }

        // ============================================================
        // LOAD SHEETS
        // ============================================================
        private void LoadMaterials(IXLWorksheet ws)
        {
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                _materials.Add(new MaterialDefinition
                {
                    Grade        = row.Cell(1).GetString(),
                    Sd_MPa       = row.Cell(2).GetDouble(),
                    StMultiplier = row.Cell(3).GetDouble(),
                    Density      = row.Cell(4).GetDouble(),
                    Note         = row.Cell(5).GetString()
                });
            }
        }

        private void LoadPlateSizes(IXLWorksheet ws)
        {
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                _plateSizes.Add(new PlateSize
                {
                    ThicknessMM = row.Cell(1).GetDouble()
                });
            }
        }

        private void LoadMinShellThickness(IXLWorksheet ws)
        {
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                _minShellThickness.Add(new MinShellThicknessRule
                {
                    CourseNumber   = (int)row.Cell(1).GetDouble(),
                    MaxDiameterM   = row.Cell(2).GetDouble(),
                    MinThicknessMM = row.Cell(3).GetDouble()
                });
            }
        }

        private void LoadSettings(IXLWorksheet ws)
        {
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                var key   = row.Cell(1).GetString();
                var value = row.Cell(2).GetString();

                if (!string.IsNullOrWhiteSpace(key))
                    _settings[key] = value;
            }
        }

        // ============================================================
        // LOOKUPS
        // ============================================================
        public MaterialDefinition? GetMaterial(string grade) =>
            _materials.FirstOrDefault(m =>
                m.Grade.Equals(grade, StringComparison.OrdinalIgnoreCase));

        public double GetDefaultCorrosionAllowanceMM()
        {
            if (_settings.TryGetValue("DefaultCorrosionMM", out var v)
                && double.TryParse(v, out var val))
                return val;

            return 2.0;
        }

        public double GetDefaultTestMultiplier()
        {
            if (_settings.TryGetValue("DefaultTestMultiplier", out var v)
                && double.TryParse(v, out var val))
                return val;

            return 0.6;
        }

        // ============================================================
        // CRUD (MaterialController)
        // ============================================================
        public IEnumerable<MaterialDefinition> GetAllMaterials() => _materials;

        public void AddMaterial(MaterialDefinition material)
        {
            if (material == null)
                return;

            if (_materials.Any(m => m.Grade.Equals(material.Grade, StringComparison.OrdinalIgnoreCase)))
                throw new Exception($"Material '{material.Grade}' already exists.");

            _materials.Add(material);
        }

        public bool UpdateMaterial(string grade, MaterialDefinition updated)
        {
            var existing = _materials.FirstOrDefault(m =>
                m.Grade.Equals(grade, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
                return false;

            existing.Sd_MPa       = updated.Sd_MPa;
            existing.StMultiplier = updated.StMultiplier;
            existing.Density      = updated.Density;
            existing.Note         = updated.Note;

            return true;
        }

        public bool DeleteMaterial(string grade)
        {
            var existing = _materials.FirstOrDefault(m =>
                m.Grade.Equals(grade, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
                return false;

            _materials.Remove(existing);
            return true;
        }

        // ============================================================
        // SAVE BACK TO EXCEL (Materials Sheet Only)
        // ============================================================
        public void SaveChanges()
        {
            using var wb = new XLWorkbook(_excelPath);

            // ================== MATERIALS SHEET ==================
            var wsMat = wb.Worksheet("Materials");

            // Delete all rows except header
            var matRows = wsMat.RowsUsed().Skip(1).ToList();
            foreach (var row in matRows)
                row.Delete();

            int rowMat = 2;
            foreach (var m in _materials)
            {
                wsMat.Cell(rowMat, 1).Value = m.Grade;
                wsMat.Cell(rowMat, 2).Value = m.Sd_MPa;
                wsMat.Cell(rowMat, 3).Value = m.StMultiplier;
                wsMat.Cell(rowMat, 4).Value = m.Density;
                wsMat.Cell(rowMat, 5).Value = m.Note;
                rowMat++;
            }

            // ================== PLATE THICKNESSES SHEET ==================
            // (Sheet name must match your Excel: "PlateThicknesses")
            var wsPlate = wb.Worksheet("PlateThicknesses");

            var plateRows = wsPlate.RowsUsed().Skip(1).ToList();
            foreach (var row in plateRows)
                row.Delete();

            int rowPl = 2;
            foreach (var p in _plateSizes)
            {
                wsPlate.Cell(rowPl, 1).Value = p.ThicknessMM;
                rowPl++;
            }

            wb.Save();
        }

    }
}
