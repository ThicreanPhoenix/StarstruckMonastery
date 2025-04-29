using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Phoenix.AdventureMonsters;

public class LoadMod
{
    [DawnsburyDaysModMainMethod]

    public static void BootMod()
    {
        AddMonsters.LoadMonsters();
    }
}