using SteelTankAPI650.Models.Bottom;

namespace SteelTankAPI650.Services.Bottom
{
    public interface IBottomPlateDesignService
    {
        /// <summary>
        /// Calculates annular-plate thickness, radial width, and minimum bottom plate thickness
        /// according to API 650 5.5 / Table 5.1a (SI), using the same logic as your Excel.
        /// </summary>
        BottomPlateResult DesignBottom(BottomPlateInput input);
    }
}
