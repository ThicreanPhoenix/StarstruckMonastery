using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Phoenix.AdventureMonsters;

public class Treasures
{
    public static ItemName LightHammer = ModManager.RegisterNewItemIntoTheShop("light hammer", itemName =>
    {
        return new Item(IllustrationName.Warhammer, "light hammer", new Trait[] { Trait.Melee, Trait.Martial, Trait.Hammer, Trait.Thrown20Feet, Trait.Agile })
        {
            ItemName = itemName
        }
        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning));
    });
}
