using SteelTankAPI650.Models.Config;

namespace SteelTankAPI650.Services.Config
{
    public interface IDesignDataRepository
    {
        // READ-ONLY PUBLIC LISTS
        IReadOnlyList<MaterialDefinition> Materials { get; }
        IReadOnlyList<PlateSize> PlateSizes { get; }
        IReadOnlyList<MinShellThicknessRule> MinShellThickness { get; }

        // INTERNAL EDITABLE LISTS (needed by Controller for CRUD)
        List<MaterialDefinition> MaterialsInternal { get; }
        List<PlateSize> PlateSizesInternal { get; }
        List<MinShellThicknessRule> MinShellThicknessInternal { get; }

        // LOOKUP
        MaterialDefinition? GetMaterial(string grade);
        double GetDefaultCorrosionAllowanceMM();
        double GetDefaultTestMultiplier();

        // CRUD
        IEnumerable<MaterialDefinition> GetAllMaterials();
        void AddMaterial(MaterialDefinition material);
        bool UpdateMaterial(string grade, MaterialDefinition updated);
        bool DeleteMaterial(string grade);

        // SAVE TO EXCEL
        void SaveChanges();
    }
}
