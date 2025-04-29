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
    //public static Trait SilverTrait = ModManager.RegisterTrait("Silver");
    public static ItemName LightHammer = ModManager.RegisterNewItemIntoTheShop("light hammer", itemName =>
    {
        return new Item(IllustrationName.Warhammer, "light hammer", new Trait[] { Trait.Melee, Trait.Martial, Trait.Hammer, Trait.Thrown20Feet, Trait.Agile })
        {
            ItemName = itemName
        }
        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning));
    });
    /*
    public static ItemName WeaponSilver = ModManager.RegisterNewItemIntoTheShop("weapons-grade silver", itemName =>
    {
        return new Item(IllustrationName.ColdIron, "weapons-grade silver", new Trait[] { Trait.Material, SilverTrait })
        {
            Level = 2,
            Price = 40
        }.WithRuneProperties(new RuneProperties("silver", RuneKind.WeaponMaterial, "Silver has few magical properties of its own, but savvy adventurers and wealthy collectors alike coat their weapons with it.", "Silver weapons deal additional damage to creatures with a weakness to silver, and bypass the resistances of some other creatures.", delegate (Item item)
        {
            item.Traits.Add(SilverTrait);
        }));
    });
    */
}
