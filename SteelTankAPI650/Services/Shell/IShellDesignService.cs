using SteelTankAPI650.Models;

namespace SteelTankAPI650.Services.Shell
{
    public interface IShellDesignService
    {
        ShellDesignResult CalculateShell(ShellDesignInput input);
    }
}
