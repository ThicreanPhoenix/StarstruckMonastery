using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;

namespace Dawnsbury.Mods.Phoenix.AdventureMonsters;

public class AddMonsters
{
    public static QEffectId OpeningStatementTarget = ModManager.RegisterEnumMember<QEffectId>("OpeningStatementTarget");
    public static QEffectId ShrinePrayTarget = ModManager.RegisterEnumMember<QEffectId>("ShrinePrayTarget");
    public static QEffectId ShrinePrayPrayer = ModManager.RegisterEnumMember<QEffectId>("ShrinePrayPrayer");
    public static QEffectId VortexArcanaTarget = ModManager.RegisterEnumMember<QEffectId>("VortexArcanaTarget");
    public static QEffectId VortexArcanaActor = ModManager.RegisterEnumMember<QEffectId>("VortexArcanaActor");
    public static QEffectId VortexReligionTarget = ModManager.RegisterEnumMember<QEffectId>("VortexReligionTarget");
    public static QEffectId VortexReligionActor = ModManager.RegisterEnumMember<QEffectId>("VortexReligionActor");
    public static QEffectId SilverDoorTarget = ModManager.RegisterEnumMember<QEffectId>("SilverDoorTarget");
    public static QEffectId SilverDoorActor = ModManager.RegisterEnumMember<QEffectId>("SilverDoorActor");
    public static CreatureId AnimatedBroomId = ModManager.RegisterEnumMember<CreatureId>("AnimatedBroom");
    public static CreatureId DustMephitId = ModManager.RegisterEnumMember<CreatureId>("DustMephit");
    public static CreatureId NaiadId = ModManager.RegisterEnumMember<CreatureId>("Naiad");
    public static CreatureId HellHoundId = ModManager.RegisterEnumMember<CreatureId>("HellHound");
    public static CreatureId MonasteryGardenShrineId = ModManager.RegisterEnumMember<CreatureId>("MonasteryGardenShrine");
    public static CreatureId DraugrId = ModManager.RegisterEnumMember<CreatureId>("Draugr");
    public static CreatureId DrownedMinerId = ModManager.RegisterEnumMember<CreatureId>("DrownedMiner");
    public static CreatureId FireWispId = ModManager.RegisterEnumMember<CreatureId>("FireWisp");
    public static CreatureId AzerId = ModManager.RegisterEnumMember<CreatureId>("Azer");
    public static CreatureId SilverDoorId = ModManager.RegisterEnumMember<CreatureId>("SilverDoor");
    public static CreatureId HellboundAttorneyId = ModManager.RegisterEnumMember<CreatureId>("HellboundAttorney");
    public static CreatureId DemonicConflagrationId = ModManager.RegisterEnumMember<CreatureId>("DemonicConflagration");
    public static CreatureId DemonicBlobId = ModManager.RegisterEnumMember<CreatureId>("DemonicBlob");
    public static QEffect CreateSeasRevengeEffect(int value, int dc)
    {
        QEffect sick = new QEffect()
        {
            Name = "Sickened (Mariner's Curse)",
            Illustration = IllustrationName.Sickened,
            Description = "You take a status penalty equal to the value to all your checks and DCs.\n\nYou can't drink elixirs or potions, or be administered elixirs or potions unless you're unconscious.\n\nYou can't lower the value of this condition below 1.",
            Value = value,
            Id = QEffectId.Sickened,
            Tag = dc,
            BonusToAllChecksAndDCs = (QEffect qf) => new Bonus(-qf.Value, BonusType.Status, "sickened"),
            PreventTakingAction = (CombatAction ca) => (ca.ActionId != ActionId.Drink) ? null : "You're sickened.",
            ProvideContextualAction = delegate (QEffect qf)
            {
                if (qf.Value > 1)
                {
                    QEffect qf3 = qf;
                    return new ActionPossibility(new CombatAction(qf3.Owner, IllustrationName.Retch, "Retch", Array.Empty<Trait>(), "Make a fortitude save against DC " + dc + ". On a success, the sickened value is reduced by 1 (or by 2 on a critical success).", Target.Self()).WithActionId(ActionId.Retch).WithSavingThrow(new SavingThrow(Defense.Fortitude, dc)).WithEffectOnEachTarget(async delegate (CombatAction spell, Creature a, Creature cr, CheckResult ck)
                    {
                        if (ck >= CheckResult.Success)
                        {
                            qf3.Value -= ((ck != CheckResult.CriticalSuccess) ? 1 : 2);
                            if (qf3.Value <= 0)
                            {
                                qf3.Value = 1;
                            }
                        }
                    })).WithPossibilityGroup("Remove debuff");
                }
                else return null;
            },
            EndOfCombat = async delegate (QEffect effect, bool victory)
            {
                effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("SeasRevenge", "You are sickened and cannot lower the value below 1", dc));
            },
            LongTermEffectDuration = LongTermEffectDuration.UntilDowntime,
            CountsAsADebuff = true
        };
        sick.PreventTargetingBy = (CombatAction ca) => (ca.ActionId != ActionId.Administer || sick.Owner.HasEffect(QEffectId.Unconscious)) ? null : "sickened";
        return sick;
    }

    public static Creature CreateAnimatedBroom()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/AnimatedBroom.png"),
            "Animated Broom",
            new Trait[] { Trait.Small, Trait.Neutral, Trait.Construct, Trait.Mindless, Trait.AnimatedObject },
            -1, 3, 3,
            new Defenses(16, 3, 6, 3),
            6,
            new Abilities(0, 1, 0, -5, 0, -5),
            new Skills(athletics: 5))
            .WithTactics(Tactic.Mindless)
            .WithCharacteristics(speaksCommon: false, hasASkeleton: false)
            .WithCreatureId(AnimatedBroomId)
            .WithProficiency(Trait.Weapon, (Proficiency)5)
            .WithUnarmedStrike(new Item(new ModdedIllustration("PhoenixAssets/AnimatedBroom.png"), "bristles", new Trait[] { Trait.Agile, Trait.Finesse, Trait.Magical, Trait.Weapon })
                .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning)).WithSoundEffect(SfxName.Fist))
            .AddQEffect(new QEffect("Dust", "A creature hit by an animated broom's bristles must succeed at a DC 15 Fortitude save or be forced to spend its next action coughing. This effect doesn't stack with itself.")
            {
                AfterYouDealDamage = async delegate (Creature attacker, CombatAction action, Creature victim)
                {
                    CheckResult result = CommonSpellEffects.RollSavingThrow(victim, action, Defense.Fortitude, 15);
                    if (result <= CheckResult.Failure)
                    {
                        victim.AddQEffect(new QEffect()
                        {
                            Name = "Dusted",
                            Description = victim.Name + " must spend their next action coughing due to " + attacker.Name + "'s dust.",
                            Illustration = IllustrationName.AncientDust,
                            ExpiresAt = ExpirationCondition.Never,
                            PreventTakingAction = delegate (CombatAction action)
                            {
                                if (!((action.Name == "Cough") || (action.ActionId == ActionId.EndTurn)))
                                {
                                    return "must spend next action coughing";
                                }
                                else return null;
                            },
                            ProvideMainAction = delegate (QEffect qf)
                            {
                                return new ActionPossibility(new CombatAction(qf.Owner, IllustrationName.AncientDust, "Cough", new Trait[] { }, qf.Owner.Name + " coughs and sneezes.", Target.Self())
                                    .WithActionCost(1)
                                    .WithGoodness((_, _, _) => AIConstants.ALWAYS)
                                    .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                                    {
                                        caster.Overhead("Achoo!", Color.Black);
                                        qf.ExpiresAt = ExpirationCondition.Immediately;
                                    }));
                            },
                            Key = "Dusted"
                        });
                    }
                }
            })
            .WithHardness(2)
            .AddQEffect(QEffect.ArmorBreak(2));
    }

    public static Creature CreateDustMephit()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/DustMephit.PNG"),
            "Dust Mephit",
            new Trait[] { Trait.Small, Trait.Air, Trait.Earth, Trait.Elemental },
            1, 3, 7,
            new Defenses(16, 6, 9, 5),
            16,
            new Abilities(1, 4, 1, -2, 0, -1),
            new Skills(acrobatics: 7, stealth: 7))
            .WithCharacteristics(speaksCommon: false, hasASkeleton: false)
            .WithCreatureId(DustMephitId)
            .WithTactics(Tactic.Standard)
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .WithUnarmedStrike(new Item(IllustrationName.DragonClaws, "claws", new Trait[] { Trait.Finesse, Trait.Agile, Trait.Melee, Trait.Weapon })
                .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing))
                .WithSoundEffect(SfxName.Fist))
            .AddQEffect(QEffect.Flying())
            .AddMonsterInnateSpellcasting(7, Trait.Arcane, null, new SpellId[] { SpellId.Glitterdust })
            .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
            .AddQEffect(QEffect.DamageImmunity(DamageKind.Bleed))
            .AddQEffect(QEffect.DamageImmunity(DamageKind.Poison))
            .AddQEffect(QEffect.TraitImmunity(Trait.Sleep))
            //.AddQEffect(QEffect.BreathWeapon("a cloud of dust", Target.FifteenFootCone(), Defense.Reflex, 17, DamageKind.Slashing, DiceFormula.FromText("2d6", "Breath Weapon"), SfxName.AncientDust, null))
            .AddQEffect(new QEffect
            {
                ProvideMainAction = (QEffect qf) => new ActionPossibility(new CombatAction(qf.Owner, IllustrationName.BreathWeapon, "Breath Weapon", new Trait[]
                {
                        Trait.Arcane,
                        Trait.Air,
                        Trait.Earth,
                }, "Deal 2d6 slashing damage in a 15-foot cone (DC 17 basic Reflex save). You can't use Breath Weapon again for 1d4 rounds.", Target.FifteenFootCone()).WithActionCost(2).WithProjectileCone(IllustrationName.BreathWeapon, 25, ProjectileKind.Cone).WithSavingThrow(new SavingThrow(Defense.Reflex, (Creature? _) => 17))
                    .WithGoodnessAgainstEnemy((Target tg, Creature a, Creature d) => 7.5f)
                    .WithSoundEffect(SfxName.AncientDust)
                    .WithActionId(ActionId.BreathWeapon)
                    .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                    {
                        await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, "2d6", DamageKind.Slashing);
                    })
                    .WithEffectOnChosenTargets(async delegate (Creature a, ChosenTargets d)
                    {
                        a.AddQEffect(QEffect.Recharging("Breath Weapon"));
                    }))
            });
    }

    public static Creature CreateNaiad()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/Naiad.PNG"), "Naiad", new Trait[] { Trait.Chaotic, Trait.Fey, Trait.Water },
            1, 6, 5,
            new Defenses(16, 3, 6, 8),
            20,
            new Abilities(0, 3, 0, 1, 1, 4),
            new Skills(acrobatics: 6, athletics: 3, diplomacy: 7, nature: 6, stealth: 6, survival: 6))
        {
            SpawnAsFriends = true
        }
        .WithBasicCharacteristics()
        .WithCreatureId(NaiadId)
        .WithTactics(Tactic.Standard)
        .With(delegate (Creature cr)
        {
            cr.Characteristics.DeathSoundEffect = SfxName.FemaleDeath;
        })
        .WithProficiency(Trait.Weapon, Proficiency.Expert)
        .AddQEffect(QEffect.DamageResistance(DamageKind.Fire, 3))
        .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 3))
        .WithUnarmedStrike(new Item(IllustrationName.Fist, "aqueous fist", new Trait[] { Trait.Melee, Trait.Agile, Trait.Finesse, Trait.Water, Trait.Magical })
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning)))
        //.AddMonsterInnateSpellcasting(7, Trait.Primal, new SpellId[] { SpellId.TidalSurge })
        //Monsters don't seem able to use focus spells. Without that and without the Charm spell, the naiad's spellcasting isn't very useful.
        .AddQEffect(new QEffect("Wild Empathy", "The dryad can use its Diplomacy to convince an Animal to help it instead of fighting.")
        {
            ProvideActionIntoPossibilitySection = delegate (QEffect qf, PossibilitySection section)
            {
                if (section.PossibilitySectionId == PossibilitySectionId.SkillActions)
                {
                    return new ActionPossibility(new CombatAction(qf.Owner, IllustrationName.Command, "Request Aid", new Trait[] { Trait.Auditory, Trait.Concentrate, Trait.Linguistic, Trait.Mental },
                        "The naiad asks one adjacent creature with the animal trait to aid it, making a Diplomacy check against its Will DC. If the naiad succeeds, the animal joins the naiad's side in combat.",
                        Target.Touch().WithAdditionalConditionOnTargetCreature((a, b) => !b.HasTrait(Trait.Animal) ? Usability.NotUsableOnThisCreature("Not an animal") : Usability.Usable))
                        .WithActionCost(1)
                        .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Diplomacy), Checks.DefenseDC(Defense.Will)))
                        .WithGoodness((a, b, c) => AIConstants.EXTREMELY_PREFERRED)
                        .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                        {
                            if (result >= CheckResult.Success)
                            {
                                target.OwningFaction = caster.OwningFaction;
                                target.Overhead("convinced", Color.Yellow, target.Name + " is now fighting for " + caster.Name);
                            }
                        }));
                }
                else return null;
            }
        });
    }

    public static Creature CreateHellHound()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/HellHound.PNG"),
            "Hell Hound",
            new Trait[] { Trait.Lawful, Trait.Evil, Trait.Fiend, Trait.Beast, Trait.Fire },
            3, 9, 8,
            new Defenses(19, 9, 10, 7),
            40,
            new Abilities(4, 3, 2, -2, 2, -2),
            new Skills(acrobatics: 8, athletics: 9, stealth: 8, survival: 9))
            .WithCharacteristics(speaksCommon: false, hasASkeleton: true).WithTactics(Tactic.Standard)
            .WithCreatureId(HellHoundId)
            .With(delegate (Creature cr)
            {
                cr.Characteristics.DeathSoundEffect = SfxName.BeastDeath;
            })
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .WithUnarmedStrike(new Item(IllustrationName.DragonClaws, "jaws", new Trait[] { Trait.Melee, Trait.Weapon })
                .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing)
                    .WithAdditionalDamage("1d6", DamageKind.Fire)
                    .WithAdditionalDamage("1d6", DamageKind.Evil))
                .WithSoundEffect(SfxName.ZombieAttack2))
            .AddQEffect(QEffect.DamageImmunity(DamageKind.Fire))
            .AddQEffect(new QEffect
            {
                ProvideMainAction = (QEffect qf) => new ActionPossibility(new CombatAction(qf.Owner, IllustrationName.FireRay, "Breath Weapon", new Trait[]
                {
                        Trait.Divine,
                        Trait.Fire,
                        Trait.Evocation,
                }, "Deal 4d6 fire damage in a 15-foot cone (DC 19 basic Reflex save). You can't use Breath Weapon again for 1d4 rounds. The hound's breath weapon recharges immediately if it would take fire damage or be targeted by a fire effect.", Target.FifteenFootCone()).WithActionCost(2).WithProjectileCone(IllustrationName.BurningHands, 25, ProjectileKind.Cone).WithSavingThrow(new SavingThrow(Defense.Reflex, (Creature? _) => 19))
                    .WithGoodnessAgainstEnemy((Target tg, Creature a, Creature d) => 14f)
                    .WithSoundEffect(SfxName.FieryBurst)
                    .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                    {
                        await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, "4d6", DamageKind.Fire);
                    })
                    .WithEffectOnChosenTargets(async delegate (Creature a, ChosenTargets d)
                    {
                        a.AddQEffect(QEffect.Recharging("Breath Weapon"));
                    }))
            })
            .AddQEffect(new QEffect()
            {
                AfterYouAreTargeted = async delegate (QEffect qf, CombatAction action)
                {
                    if (action.HasTrait(Trait.Fire))
                    {
                        qf.Owner.RemoveAllQEffects((QEffect fct) => fct.Id == QEffectId.Recharging);
                    }
                },
                AfterYouTakeIncomingDamageEventEvenZero = async delegate (QEffect qf, DamageEvent damageEvent)
                {
                    if (damageEvent.KindedDamages.Any((KindedDamage kd) => kd.DamageKind == DamageKind.Fire))
                    {
                        qf.Owner.RemoveAllQEffects((QEffect fct) => fct.Id == QEffectId.Recharging);
                    }
                }
            });
    }

    public static Creature CreateBloomingShrine()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/EverbloomingFlowerShrine.png"), "Everblooming Shrine", new List<Trait>
            {
                Trait.Indestructible,
                Trait.Object
            }, 2, 0, 0, new Defenses(0, 0, 0, 0), 1, new Abilities(0, 0, 0, 0, 0, 0), new Skills()).WithHardness(1000).WithEntersInitiativeOrder(entersInitiativeOrder: false).WithTactics(Tactic.DoNothing)
            .AddQEffect(new QEffect
            {
                PreventTargetingBy = (CombatAction ca) => (!ca.HasTrait(Trait.Interact)) ? "Interact-only" : null
            })
            .AddQEffect(QEffect.OutOfCombat(null, null, true))
            .WithSpawnAsGaia()
            .WithCreatureId(MonasteryGardenShrineId)
            .AddQEffect(new QEffect().AddAllowActionOnSelf(ShrinePrayPrayer, ShrinePrayTarget, (creature) =>
            {
                return new ActionPossibility(new CombatAction(creature, IllustrationName.Heal, "Pray", new Trait[]
                {
                        Trait.BypassesOutOfCombat,
                        Trait.Manipulate,
                        Trait.Basic
                }, "Beseech the shrine of the Blooming Flower for aid.", Target.Touch())
                    .WithActionCost(2)
                    .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Religion), Checks.FlatDC(20)))
                    .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                    {
                        target.RemoveAllQEffects((qf) => qf.Id == ShrinePrayTarget);
                        switch (result)
                        {
                            case CheckResult.CriticalSuccess:
                                caster.AddQEffect(new QEffect()
                                {
                                    Name = "Flower's Gift",
                                    Illustration = IllustrationName.Bless,
                                    Description = "You have pleased the Blooming Flower. You gain a +1 status bonus to all checks for the rest of the encounter.",
                                    BonusToAttackRolls = delegate (QEffect qf, CombatAction action, Creature defender)
                                    {
                                        return new Bonus(1, BonusType.Status, "Flower's Gift");
                                    }
                                });
                                foreach (Creature c in caster.Battle.AllCreatures)
                                {
                                    if (c.CreatureId == CreatureId.VenomousSnake)
                                    {
                                        Faction f = caster.Battle.Factions.FirstOrDefault((Faction fa) => fa.IsGaiaFriends);
                                        c.OwningFaction = f;
                                    }
                                }
                                break;
                            case CheckResult.Success:
                                foreach (Creature c in caster.Battle.AllCreatures)
                                {
                                    if (c.CreatureId == CreatureId.VenomousSnake)
                                    {
                                        Faction f = caster.Battle.Factions.FirstOrDefault((Faction fa) => fa.IsGaiaFriends);
                                        c.OwningFaction = f;
                                    }
                                }
                                break;
                            case CheckResult.CriticalFailure:
                                caster.AddQEffect(new QEffect()
                                {
                                    Name = "Flower's Ire",
                                    Illustration = IllustrationName.Bane,
                                    Description = "You have offended the Blooming Flower. You suffer a -1 status penalty to all checks for the rest of the encounter.",
                                    BonusToAttackRolls = delegate (QEffect qf, CombatAction action, Creature defender)
                                    {
                                        return new Bonus(-1, BonusType.Status, "Flower's Ire");
                                    }
                                });
                                break;
                        }
                    })).WithPossibilityGroup("Interactions");
            }));
    }

    public static Creature CreateDrownedMiner()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/DrownedMiner.PNG"),
            "Drowned Miner",
            new Trait[] { Trait.Evil, Trait.Mindless, Trait.Undead, Trait.Zombie },
            0, 6, 5,
            new Defenses(14, 7, 5, 4),
            30,
            new Abilities(2, 1, 3, 0, 2, 0),
            new Skills(acrobatics: 3, athletics: 6, survival: 4))
        .WithCharacteristics(speaksCommon: false, hasASkeleton: true)
        .WithCreatureId(DrownedMinerId)
        .WithTactics(Tactic.Mindless)
        .WithProficiency(Trait.Weapon, Proficiency.Expert)
        .AddHeldItem(Items.CreateNew(ItemName.Pick))
        //Possibly add the Piton Pin action? It might not get used with a Mindless AI.
        .AddQEffect(QEffect.DamageWeakness(DamageKind.Positive, 5))
        .AddQEffect(QEffect.DamageWeakness(DamageKind.Slashing, 5))
        .AddQEffect(new QEffect()
        {
            Name = "Slowed",
            Id = QEffectId.Slowed,
            Illustration = IllustrationName.None,
            Description = "A zombie is permanently slowed 1.",
            ExpiresAt = ExpirationCondition.Never,
            Value = 1,
            Innate = true,
        })
        .With(delegate (Creature cr)
        {
            cr.Characteristics.DeathSoundEffect = SfxName.ZombieDeath;
        });
    }

    public static Creature CreateDraugr()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/Draugr.PNG"),
            "Draugr",
            new Trait[] { Trait.Chaotic, Trait.Evil, Trait.Undead, Trait.Water },
            2, 7, 5,
            new Defenses(17, 11, 6, 7),
            35,
            new Abilities(4, 2, 3, -1, 1, 1),
            new Skills(athletics: 10, stealth: 8))
        .WithCharacteristics(speaksCommon: false, hasASkeleton: true)
        .WithCreatureId(DraugrId)
        .WithTactics(Tactic.Standard)
        .WithProficiency(Trait.Weapon, Proficiency.Expert)
        .AddHeldItem(Items.CreateNew(ItemName.Greataxe))
        .WithUnarmedStrike(new Item(IllustrationName.Fist, "fist", new Trait[] { Trait.Agile, Trait.Weapon })
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Slashing)))
        .AddQEffect(QEffect.DamageWeakness(DamageKind.Positive, 5))
        .AddQEffect(QEffect.DamageResistance(DamageKind.Fire, 3))
        .AddQEffect(QEffect.Swimming())
        .AddQEffect(new QEffect()
        {
            Name = "Grotesque Gift",
            Description = "A creature damage by a draugr's Strike must succeed on a DC 15 Fortitude save or become sickened 1 (sickened 2 on a critical failure).",
            Innate = true,
            AfterYouDealDamage = async (Creature self, CombatAction action, Creature target) =>
            {
                if (action.HasTrait(Trait.Strike))
                {
                    CheckResult result = CommonSpellEffects.RollSavingThrow(target, CombatAction.CreateSimple(self, "Grotesque Gift", new Trait[] { Trait.Olfactory }), Defense.Fortitude, 15);
                    switch (result)
                    {
                        case CheckResult.Failure:
                            target.AddQEffect(QEffect.Sickened(1, 15));
                            break;
                        case CheckResult.CriticalFailure:
                            target.AddQEffect(QEffect.Sickened(2, 15));
                            break;
                    }
                }
            }
        })
        .WithFeat(FeatName.Swipe)
        .AddQEffect(new QEffect("The Sea's Revenge", "The creature that slays the draugr must make a saving throw against the {i}mariner's curse{/i} spell with a DC of 19.")
        {
            YouAreDealtLethalDamage = async (qf, attacker, stuff, defender) =>
            {
                CombatAction spell = AllSpells.CreateSpellInCombat(SpellId.MarinersCurse, defender, 2, Trait.None).WithActionCost(0).WithSavingThrow(new SavingThrow(Defense.Will, 19));
                spell.ChosenTargets = ChosenTargets.CreateSingleTarget(attacker);
                await spell.AllExecute();
                return null;
            }
        })
        .With(delegate (Creature cr)
        {
            cr.Characteristics.DeathSoundEffect = SfxName.ZombieDeath;
        });
    }

    public static Creature CreateAzer()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/Azer.PNG"),
            "Azer",
            new Trait[] { Trait.Lawful, Trait.Elemental, Trait.Fire, Trait.Humanoid },
            2, 8, 4,
            new Defenses(17, 10, 5, 8),
            45,
            new Abilities(3, 1, 4, 2, 2, 0),
            new Skills(athletics: 7, crafting: 10, intimidation: 4))
        .WithBasicCharacteristics()
        .WithCreatureId(AzerId)
        .WithTactics(Tactic.Standard)
        .AddQEffect(QEffect.DamageWeakness(DamageKind.Cold, 5))
        .AddQEffect(QEffect.DamageImmunity(DamageKind.Fire))
        .WithProficiency(Trait.Weapon, Proficiency.Expert)
        .With(delegate (Creature cr)
        {
            cr.Characteristics.DeathSoundEffect = SfxName.MaleDeath;
        })
        .AddHeldItem(Items.CreateNew(ItemName.Warhammer))
        .AddHeldItem(Items.CreateNew(Treasures.LightHammer))
        .AddQEffect(new QEffect("Heart of the Forge", "A creature who starts their turn within 10 feet of the azer must make a DC 16 Fortitude save or become fatigued from the radiating heat for as long as it stands in the azer's aura. Creatures with fire resistance are immune.")
        {
            StateCheck = delegate (QEffect qf)
            {
                foreach (Creature c in qf.Owner.Battle.AllCreatures.Where((Creature cr) => (cr.DistanceTo(qf.Owner) <= 2)))
                {
                    bool notresistanttofire = (c.WeaknessAndResistance.Resistances.FirstOrDefault((Resistance r) => r.DamageKind == DamageKind.Fire) == default);
                    bool notimmunetofire = !(c.WeaknessAndResistance.Immunities.Contains(DamageKind.Fire));
                    bool notimmunetofatigue = (c.QEffects.FirstOrDefault((QEffect qf) => qf.ImmuneToCondition == QEffectId.Fatigued) == default);
                    bool notself = (c != qf.Owner);
                    bool alreadyfatigued = c.HasEffect(QEffectId.Fatigued);
                    if (notresistanttofire && notimmunetofire && notimmunetofatigue && notself && !alreadyfatigued)
                    {
                        c.AddQEffect(new QEffect()
                        {
                            Name = "Heart of the Forge",
                            Illustration = IllustrationName.Fatigued,
                            Description = "You must make a DC 16 Fortitude save at the beginning of your turn or become fatigued due to " + qf.Owner.Name + "'s heat.",
                            ExpiresAt = ExpirationCondition.Ephemeral,
                            StartOfYourTurn = async delegate (QEffect fct, Creature creature)
                            {
                                if (!creature.HasEffect(QEffectId.Fatigued))
                                {
                                    CheckResult result = CommonSpellEffects.RollSavingThrow(creature, CombatAction.CreateSimple(fct.Owner, "Heart of the Forge"), Defense.Fortitude, 16);
                                    if (result <= CheckResult.Failure)
                                    {
                                        creature.AddQEffect(new QEffect("Fatigued (Heart of the Forge)", "You take a -1 status penalty to AC and saving throws while within " + qf.Owner.Name + "'s aura.", ExpirationCondition.Never, null, IllustrationName.Fatigued)
                                        {
                                            Key = "Fatigued (Heart of the Forge)",
                                            Id = QEffectId.Fatigued,
                                            BonusToDefenses = (QEffect qfSelf, CombatAction? combatAction, Defense defense) => (qfSelf.Owner.DistanceTo(qf.Owner) <= 2 && (defense == Defense.Reflex || defense == Defense.Will || defense == Defense.Fortitude || defense == Defense.AC)) ? new Bonus(-1, BonusType.Status, "Fatigued") : null,
                                            CountsAsADebuff = true
                                        });
                                    }
                                }
                            }
                        });
                    }
                }
            }
        })
        .AddQEffect(new QEffect("Burning Touch", "An azer deals 1d6 fire damage whenever it hits with a Strike or successfully performs a Grapple or Shove.")
        {
            AdditionalGoodness = delegate (QEffect qf, CombatAction action, Creature target)
            {
                if (action.ActionId == ActionId.Shove || action.ActionId == ActionId.Grapple || action.HasTrait(Trait.Strike))
                {
                    if (qf.Owner.WeaknessAndResistance.Immunities.Contains(DamageKind.Fire))
                    {
                        return 0f;
                    }
                    return 3.5f;
                }
                return 0f;
            },
            AfterYouTakeActionAgainstTarget = async delegate (QEffect qf, CombatAction action, Creature enemy, CheckResult result)
            {
                if (action.ActionId == ActionId.Shove || action.ActionId == ActionId.Grapple || action.HasTrait(Trait.Strike))
                {
                    if (result >= CheckResult.Success)
                    {
                        await CommonSpellEffects.DealDirectDamage(action, DiceFormula.FromText("1d6", "Burning Touch"), enemy, result, DamageKind.Fire);
                    }
                }
            }
        })
        .AddQEffect(new QEffect()
        {
            ProvideMainAction = delegate (QEffect qf)
            {
                if (qf.Owner.HeldItems.FirstOrDefault((Item i) => i.ItemName == Treasures.LightHammer) != default)
                {
                    return new ActionPossibility(new CombatAction(qf.Owner, IllustrationName.ScorchingRay, "Scorch", new Trait[] { Trait.Evocation, Trait.Fire, Trait.Primal }, "The azer shrouds a light hammer in flames and hurls it forward, dealing 2d6 fire damage to each creature in a 20-foot line (DC 16 basic Reflex save).",
                        Target.TwentyFootLine())
                        .WithGoodness((Target tg, Creature a, Creature d) => d.WeaknessAndResistance.Immunities.Contains(DamageKind.Fire) ? 0 : d.EnemyOf(a) ? 7.5f : 0f)
                        .WithSoundEffect(SfxName.FireRay)
                        .WithProjectileCone(new VfxStyle(1, ProjectileKind.Ray, IllustrationName.BurningHands))
                        .WithActionCost(2)
                        .WithSavingThrow(new SavingThrow(Defense.Reflex, 16))
                        .WithEffectOnSelf(async (Creature self) =>
                        {
                            Item hammer = self.HeldItems.FirstOrDefault(i => i.ItemName == Treasures.LightHammer);
                            hammer.Traits.Add(Trait.HandEphemeral);
                            self.DropItem(hammer);
                            self.AddQEffect(new QEffect()
                            {
                                ProvideContextualAction = delegate (QEffect qf)
                                {
                                    return new ActionPossibility(new CombatAction(self, IllustrationName.Warhammer, "Draw Light Hammer", new Trait[] { Trait.Manipulate }, "The azer draws a hammer from its stash.",
                                            Target.Self((Creature cr, AI ai) => 10f))
                                        .WithActionCost(1)
                                        .WithEffectOnSelf(self =>
                                        {
                                            qf.Owner.AddHeldItem(Items.CreateNew(Treasures.LightHammer));
                                            qf.ExpiresAt = ExpirationCondition.Immediately;
                                        }));
                                }
                            });
                        })
                        .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                        {
                            await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, "2d6", DamageKind.Fire);
                        }));
                }
                else return null;
            }
        });
    }

    public static Creature CreateFireWisp()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/FireWisp.png"), "Fire Wisp", new Trait[] { Trait.Elemental, Trait.Fire },
            0, 6, 3,
            new Defenses(16, 6, 7, 4),
            18,
            new Abilities(2, 3, 2, 0, 2, 0),
            new Skills(acrobatics: 5, stealth: 7))
        .WithCharacteristics(speaksCommon: false, hasASkeleton: false)
        .WithCreatureId(FireWispId)
        .WithTactics(Tactic.Standard)
        .WithProficiency(Trait.Weapon, Proficiency.Expert)
        .AddQEffect(QEffect.DamageImmunity(DamageKind.Fire))
        .AddQEffect(QEffect.DamageImmunity(DamageKind.Bleed))
        .AddQEffect(QEffect.DamageImmunity(DamageKind.Poison))
        .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
        .AddQEffect(QEffect.TraitImmunity(Trait.Poison))
        .AddQEffect(QEffect.TraitImmunity(Trait.Sleep))
        .AddQEffect(QEffect.DamageWeakness(DamageKind.Cold, 2))
        .AddQEffect(QEffect.DamageWeakness(Trait.Water, 2))
        .AddQEffect(QEffect.Flying())
        .WithUnarmedStrike(new Item(IllustrationName.FireRay, "tendril", new Trait[] { Trait.Reach, Trait.Melee })
            .WithSoundEffect(SfxName.FireRay)
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Fire).WithAdditionalPersistentDamage("1", DamageKind.Fire)))
        .AddQEffect(new QEffect("Resonance", "Creatures within 30 feet of the wisp gain a +1 status bonus to attack and damage rolls for fire effects. Creatures with the elemental and fire traits gain this bonus to all attack and damage rolls.")
        {
            StateCheck = delegate (QEffect qf)
            {
                foreach (Creature c in qf.Owner.Battle.AllCreatures.Where((Creature cr) => cr.DistanceTo(qf.Owner) <= 6))
                {
                    if (c.HasTrait(Trait.Fire) && c.HasTrait(Trait.Elemental) && (c != qf.Owner))
                    {
                        c.AddQEffect(new QEffect()
                        {
                            Name = "Resonance",
                            Key = "FireWispResonance",
                            Illustration = IllustrationName.FireShield,
                            Description = "This elemental has a +1 status bonus to all attack and damage rolls.",
                            ExpiresAt = ExpirationCondition.Ephemeral,
                            BonusToAttackRolls = delegate (QEffect fct, CombatAction action, Creature creature)
                            {
                                return new Bonus(1, BonusType.Status, "Resonance");
                            },
                            BonusToDamage = delegate (QEffect fct, CombatAction action, Creature creature)
                            {
                                return new Bonus(1, BonusType.Status, "Resonance");
                            }
                        });
                    }
                    else if (c.Name != qf.Owner.Name)
                    {
                        c.AddQEffect(new QEffect()
                        {
                            Name = "Resonance",
                            Key = "FireWispResonance",
                            Illustration = IllustrationName.FireShield,
                            Description = "This creature has a +1 status bonus to attack and damage rolls with fire effects.",
                            ExpiresAt = ExpirationCondition.Ephemeral,
                            BonusToAttackRolls = delegate (QEffect fct, CombatAction action, Creature creature)
                            {
                                if (action == null) return null;
                                if (action.HasTrait(Trait.Fire))
                                {
                                    return new Bonus(1, BonusType.Status, "Resonance");
                                }
                                else return null;
                            },
                            BonusToDamage = delegate (QEffect fct, CombatAction action, Creature creature)
                            {
                                if (action == null) return null;
                                if (action.HasTrait(Trait.Fire))
                                {
                                    return new Bonus(1, BonusType.Status, "Resonance");
                                }
                                else return null;
                            }
                        });
                    }
                }
            }
        });
        //Not sure if I should implement Accord Essence. I don't really want the wisps destroying themselves.
    }

    public static Creature CreateSilverDoor()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/SilverDoor.png"), "Silver Door", new List<Trait>
            {
                Trait.Indestructible,
                Trait.Object
            }, 2, 0, 0, new Defenses(0, 0, 0, 0), 1, new Abilities(0, 0, 0, 0, 0, 0), new Skills()).WithHardness(1000).WithEntersInitiativeOrder(entersInitiativeOrder: false).WithTactics(Tactic.DoNothing)
            .AddQEffect(new QEffect
            {
                PreventTargetingBy = (CombatAction ca) => (!ca.HasTrait(Trait.Interact)) ? "Interact-only" : null
            })
            .AddQEffect(QEffect.OutOfCombat(null, null, true))
            .WithSpawnAsGaia()
            .WithCreatureId(SilverDoorId)
            .AddQEffect(new QEffect().AddAllowActionOnSelf(SilverDoorActor, SilverDoorTarget, (creature) =>
            {
                return new ActionPossibility(new CombatAction(creature, IllustrationName.OpenLock, "Pick a Lock", new Trait[]
                {
                        Trait.BypassesOutOfCombat,
                        Trait.Manipulate,
                        Trait.Basic
                }, "Attempt to pick one of the locks on the silver door.", Target.Touch())
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Thievery), Checks.FlatDC(20)))
                    .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                    {
                        Creature azer = caster.Battle.AllSpawnedCreatures.FirstOrDefault((Creature c) => c.CreatureId == AzerId);
                        switch (result)
                        {
                            case CheckResult.CriticalSuccess:
                                target.RemoveAllQEffects(qf => qf.Id == SilverDoorTarget);
                                int i = 0;
                                List<Creature> list = new List<Creature>();
                                foreach (Creature c in target.Battle.AllCreatures)
                                {
                                    if (c.HasEffect(SilverDoorTarget))
                                    {
                                        list.Add(c);
                                        i++;
                                    }
                                }
                                if (i == 0 && azer != default)
                                {
                                    await azer.Battle.Cinematics.PlayCutscene(async delegate (Cinematics cinematics)
                                    {
                                        await cinematics.LineAsync(azer, "No! The doors cannot be opened! I'll--gaaah!");
                                    });
                                    azer.Die();
                                }
                                Creature extra = list.GetRandom();
                                extra.RemoveAllQEffects(qf => qf.Id == SilverDoorTarget);
                                i--;
                                if (i == 0 && azer != default)
                                {
                                    await azer.Battle.Cinematics.PlayCutscene(async delegate (Cinematics cinematics)
                                    {
                                        await cinematics.LineAsync(azer, "No! The doors cannot be opened! I'll--gaaah!");
                                    });
                                    azer.Die();
                                }
                                break;
                            case CheckResult.Success:
                                target.RemoveAllQEffects(qf => qf.Id == SilverDoorTarget);
                                int i2 = 0;
                                foreach (Creature c in target.Battle.AllCreatures)
                                {
                                    if (c.HasEffect(SilverDoorTarget))
                                    {
                                        i2++;
                                    }
                                }
                                if (i2 == 0 && azer != default)
                                {
                                    await azer.Battle.Cinematics.PlayCutscene(async delegate (Cinematics cinematics)
                                    {
                                        await cinematics.LineAsync(azer, "No! The doors cannot be opened! I'll--gaaah!");
                                    });
                                    azer.Die();
                                }
                                break;
                        }
                    })).WithPossibilityGroup("Interactions");
            }));
    }

    public static Creature CreateHellboundAttorney()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/HellboundAttorney.PNG"),
            "Hellbound Attorney",
            new Trait[] { Trait.Lawful, Trait.Evil, Trait.Fiend, Trait.Humanoid, Trait.Human, Trait.Devil },
            4, 11, 4,
            new Defenses(20, 9, 12, 13),
            60,
            new Abilities(1, 2, 0, 4, 1, 3),
            new Skills(acrobatics: 10, deception: 11, diplomacy: 11, intimidation: 11, society: 10))
            .WithBasicCharacteristics()
            .WithCreatureId(HellboundAttorneyId)
            .WithTactics(Tactic.Standard)
            .AddQEffect(QEffect.DamageResistance(DamageKind.Fire, 4))
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 2))
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .AddHeldItem(new Item(IllustrationName.Quarterstaff, "elegant cane", new Trait[] { Trait.Agile, Trait.Finesse, Trait.Shove, Trait.Club, Trait.Weapon })
                .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning)).WithMonsterWeaponSpecialization(2))
            .AddMonsterInnateSpellcasting(11, Trait.Divine, new SpellId[] { SpellId.BurningHands })
            .AddQEffect(new QEffect("Abrogation of Consequences", "When this creature rolls a critical failure or a success on a Will save against a linguistic effect, it can use its reaction to find a loophole, increasing its degree of success by one step.")
            {
                //Really, I wanted to use a AskToUseReaction prompt, but AdjustSavingThrowResult doesn't take async, and since this is a monster and would always choose to take reactions anyway, this works fine enough.
                AdjustSavingThrowCheckResult = delegate (QEffect qf, Defense defense, CombatAction action, CheckResult result)
                {
                    if (defense == Defense.Will && action.HasTrait(Trait.Linguistic))
                    {
                        if (result == CheckResult.CriticalFailure || result == CheckResult.Success)
                        {
                            if (qf.Owner.Actions.CanTakeReaction())
                            {
                                CheckResult result2 = result;
                                result2.ImproveByOneStep();
                                qf.Owner.Actions.UseUpReaction();
                                return result2;
                            }
                            else return result;
                        }
                        else return result;
                    }
                    else return result;
                }
            })
            .AddQEffect(new QEffect("Opening Statement", "When the attorney's turn begins, it chooses one enemy it can see and attempts a Society check against that creature's Will DC as the attorney lists its alleged crimes. If it succeeds, it deals 2d6 additional precision damage (4d6 on a critical success) to that creature on a Strike until the end of the attorney's turn.", ExpirationCondition.Never, null, null)
            {
                AdditionalGoodness = delegate (QEffect qf, CombatAction action, Creature target)
                {
                    if (action.HasTrait(Trait.Strike) && target.HasEffect(OpeningStatementTarget))
                    {
                        if (target.FindQEffect(OpeningStatementTarget).Name == "Criminal (Critical)")
                        {
                            return 14f;
                        }
                        else return 7.5f;
                    }
                    else return 0;
                },
                YouDealDamageWithStrike = delegate (QEffect qf, CombatAction action, DiceFormula formula, Creature defender)
                {
                    QEffect qf2 = defender.FindQEffect(OpeningStatementTarget);
                    if ((!(qf2 == null)) && (qf2.Source == qf.Owner) && (!defender.IsImmuneTo(Trait.PrecisionDamage)))
                    {
                        if (qf2.Name == "Criminal")
                        {
                            DiceFormula formula2 = DiceFormula.FromText("2d6", "Opening Statement");
                            return formula.Add(formula2);
                        }
                        else if (qf2.Name == "Criminal (Critical)")
                        {
                            DiceFormula formula2 = DiceFormula.FromText("4d6", "Opening Statement");
                            return formula.Add(formula2);
                        }
                        else return formula;
                    }
                    else return formula;
                },
                ProvideMainAction = delegate (QEffect qf)
                {
                    CombatAction action = new CombatAction(qf.Owner, IllustrationName.Rage, "Opening Statement", new Trait[] { Trait.Auditory, Trait.Concentrate }, qf.Owner.Name + " lists the alleged crimes of one of its foes. If it succeeds, it deals an additional 2d6 precision damage with its Strikes against that creature (4d6 on a crit success).",
                            Target.Ranged(20).WithAdditionalConditionOnTargetCreature((attacker, defender) => attacker.Actions.TakenAnyActionThisTurn ? Usability.NotUsable("not start of turn") : !attacker.CanSee(defender) ? Usability.NotUsableOnThisCreature("cannot see") : Usability.Usable))
                        .WithActionCost(0)
                        .WithProjectileCone(IllustrationName.Demoralize, 24, ProjectileKind.Cone)
                        .WithSoundEffect(SfxName.MaleIntimidate)
                        .WithGoodness((target, attacker, defender) => AIConstants.ALWAYS)
                        .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Society), Checks.DefenseDC(Defense.Will)))
                        .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                        {
                            if (result == CheckResult.Success)
                            {
                                target.AddQEffect(new QEffect()
                                {
                                    Name = "Criminal",
                                    Id = OpeningStatementTarget,
                                    Illustration = IllustrationName.Demoralize,
                                    Description = "You take an additional 2d6 precision damage from " + qf.Owner.Name + "'s attacks until the end of its turn.",
                                    ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn,
                                    Source = caster
                                });
                            }
                            else if (result == CheckResult.CriticalSuccess)
                            {
                                target.AddQEffect(new QEffect()
                                {
                                    Name = "Criminal (Critical)",
                                    Id = OpeningStatementTarget,
                                    Illustration = IllustrationName.Demoralize,
                                    Description = "You take an additional 4d6 precision damage from " + qf.Owner.Name + "'s attacks until the end of its turn.",
                                    ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn,
                                    Source = caster
                                });
                            }
                        });
                    return new ActionPossibility(action);
                }
            });
    }

    public static Creature CreateDemonicConflagration()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/DemonicConflagration.png"), "Demonic Conflagration", new List<Trait>
            {
                Trait.Indestructible,
                Trait.Object,
                Trait.NoPhysicalUnarmedAttack
            }, 2, 0, 0, new Defenses(0, 0, 0, 0), 1, new Abilities(0, 0, 0, 0, 0, 0), new Skills()).WithHardness(1000).WithEntersInitiativeOrder(entersInitiativeOrder: true).WithTactics(Tactic.DoNothing)
            .AddQEffect(new QEffect
            {
                PreventTargetingBy = (CombatAction ca) => (!ca.HasTrait(Trait.Interact)) ? "Interact-only" : null
            })
            .WithCreatureId(DemonicConflagrationId)
            .WithTactics(Tactic.DoNothing)
            .WithSpawnAsGaia()
            .AddQEffect(new QEffect("Demonic Gateway", "At the beginning of your turn, summon a demon. You can't summon another one for 1d4 rounds.")
            {
                StartOfYourPrimaryTurn = async delegate (QEffect qf, Creature self)
                {
                    if (!self.QEffects.Any((QEffect qf) => qf.Id == QEffectId.Recharging))
                    {
                        Creature c = CreateDemonicBlob();
                        self.Battle.SpawnCreature(c, self.Battle.Factions.FirstOrDefault(f => f.IsEnemy), self.Occupies);
                        self.AddQEffect(QEffect.Recharging("Summon"));
                    }
                }
            })
            .AddQEffect(new QEffect().AddAllowActionOnSelf(VortexArcanaActor, VortexArcanaTarget, (creature) =>
            {
                return new ActionPossibility(new CombatAction(creature, IllustrationName.CastASpell, "Close Vortex (Religion)", new Trait[]
                {
                        Trait.BypassesOutOfCombat,
                        Trait.Manipulate,
                        Trait.Basic
                }, "Use your knowledge of demons and their energies to close the vortex.", Target.Touch())
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Religion), Checks.FlatDC(20)))
                    .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                    {
                        if (result >= CheckResult.Success)
                        {
                            target.Die();
                        }
                    })).WithPossibilityGroup("Interactions");
            }))
            .AddQEffect(new QEffect().AddAllowActionOnSelf(VortexReligionActor, VortexReligionTarget, (creature) =>
            {
                return new ActionPossibility(new CombatAction(creature, IllustrationName.CastASpell, "Close Vortex (Arcana)", new Trait[]
                {
                        Trait.BypassesOutOfCombat,
                        Trait.Manipulate,
                        Trait.Basic
                }, "Use your knowledge of arcane gateways to close the vortex.", Target.Touch())
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Arcana), Checks.FlatDC(20)))
                    .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                    {
                        if (result >= CheckResult.Success)
                        {
                            target.Die();
                        }
                    })).WithPossibilityGroup("Interactions");
            }));
    }

    public static Creature CreateDemonicBlob()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/DemonicBlob.PNG"), "Demonic Blob", new Trait[] { Trait.Fiend, Trait.Demon, Trait.Ooze, Trait.Mindless, Trait.Evil, Trait.Chaotic },
            -1, 2, 3,
            new Defenses(7, 8, 0, 2), 5, new Abilities(1, -4, 3, -5, 0, -5), new Skills())
        .WithTactics(Tactic.Mindless)
        .WithCharacteristics(false, false)
        .WithCreatureId(DemonicBlobId)
        .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .WithUnarmedStrike(new Item(IllustrationName.Tentacle, "pseudopod", new Trait[] { Trait.Weapon })
                .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning)).WithSoundEffect(SfxName.Fist))
        .AddQEffect(QEffect.TraitImmunity(Trait.PrecisionDamage))
        .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 1))
        .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 1))
        .AddQEffect(new QEffect("Made of Malice", "A creature struck by this conglomeration of malice and evil must succeed at a DC 13 Will save or become irritated by the residual energy for 1 round.")
        {
            AfterYouDealDamage = async delegate (Creature you, CombatAction action, Creature defender)
            {
                CheckResult result = CommonSpellEffects.RollSavingThrow(defender, CombatAction.CreateSimple(you, "Made of Malice"), Defense.Will, 13);
                if (result <= CheckResult.Failure)
                {
                    defender.AddQEffect(new QEffect()
                    {
                        Name = "Aggravated",
                        Illustration = IllustrationName.Rage,
                        Description = "You have a -" + (result == CheckResult.CriticalFailure ? 2 : 1) + " status penalty to your AC.",
                        BonusToDefenses = delegate (QEffect qf, CombatAction thing, Defense defense)
                        {
                            if (defense == Defense.AC)
                            {
                                return new Bonus(-(result == CheckResult.CriticalFailure ? 2 : 1), BonusType.Status, "Made of Malice");
                            }
                            else return null;
                        }
                    }.WithExpirationOneRoundOrRestOfTheEncounter(you, false));
                }
            }
        });
    }

    public static void LoadMonsters()
    {
        ModManager.RegisterNewCreature("AnimatedBroom", (encounter) =>
        {
            return CreateAnimatedBroom();
        });

        ModManager.RegisterCodeHook("E2AvoidSpoilingBroomAllegiance", async delegate (TBattle battle)
        {
            Creature broom = battle.Cinematics.FindCreatureAtTile(8, 5);
            broom.OwningFaction = battle.Factions.First((Faction f) => f.IsGaiaPure);
            broom = battle.Cinematics.FindCreatureAtTile(10, 6);
            broom.OwningFaction = battle.Factions.First((Faction f) => f.IsGaiaPure);
        });

        ModManager.RegisterCodeHook("E2BroomsBecomeHostile", async delegate (TBattle battle)
        {
            Creature broom = battle.Cinematics.FindCreatureAtTile(8, 5);
            broom.OwningFaction = battle.Factions.First((Faction f) => f.IsEnemy);
            broom = battle.Cinematics.FindCreatureAtTile(10, 6);
            broom.OwningFaction = battle.Factions.First((Faction f) => f.IsEnemy);
        });

        ModManager.RegisterNewCreature("DustMephit", (encounter) =>
        {
            return CreateDustMephit();
        });

        ModManager.RegisterNewCreature("Naiad", (encounter) =>
        {
            return CreateNaiad();
        });

        ModManager.RegisterCodeHook("InitializeTheliphe", async delegate (TBattle battle)
        {
            Creature naiad = battle.Cinematics.FindCreatureAtTile(13,7);
            naiad.AddQEffect(new QEffect()
            {
                StateCheckWithVisibleChanges = async (qf) =>
                {
                    if (qf.Owner.DeathScheduledForNextStateCheck)
                    {
                        await qf.Owner.Battle.EndTheGame(false, qf.Owner.Name + " has died.");
                    }
                }
            });
        });

        ModManager.RegisterNewCreature("HellHound", (encounter) =>
        {
            return CreateHellHound();
        });

        ModManager.RegisterNewCreature("MonasteryGardenShrine", (encounter) =>
        {
            return CreateBloomingShrine();
        });

        ModManager.RegisterNewCreature("DrownedMiner", (encounter) =>
        {
            return CreateDrownedMiner();
        });
        
        LongTermEffects.EasyRegister("SeasRevenge", LongTermEffectDuration.UntilDowntime, (string _, int help) =>
        {
            return CreateSeasRevengeEffect(1, help);
        });
        
        ModManager.RegisterNewCreature("Draugr", (encounter) =>
        {
            return CreateDraugr();
        });

        ModManager.RegisterNewCreature("Azer", (encounter) =>
        {
            return CreateAzer();
        });

        ModManager.RegisterNewCreature("FireWisp", (encounter) =>
        {
            return CreateFireWisp();
        });

        ModManager.RegisterNewCreature("SilverDoor", (encounter) =>
        {
            return CreateSilverDoor();
        });

        ModManager.RegisterNewCreature("HellboundAttorney", (encounter) =>
        {
            return CreateHellboundAttorney();
        });

        ModManager.RegisterNewCreature("DemonicConflagration", (encounter) =>
        {
            return CreateDemonicConflagration();
        });

        ModManager.RegisterNewCreature("DemonicBlob", (encounter) =>
        {
            return CreateDemonicBlob();
        });
    }
}