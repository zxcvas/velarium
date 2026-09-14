namespace Velarium;

// Closed v1 stall: 3 cultures × 4 slots × 3 tiers. Buy prices match production/economy.md
// (Forum equipment). Resale is floor(buy/2). Rows are templates; CreateInstance mints Armory items.
public sealed record EquipmentTemplate(
    EquipmentSlot Slot,
    EquipmentCulture Culture,
    EquipmentTier Tier,
    int Buy);

public static class EquipmentCatalog
{
    public static readonly EquipmentTemplate[] Templates =
    {
        new(EquipmentSlot.Helmet, EquipmentCulture.Punic, EquipmentTier.T1, 35),
        new(EquipmentSlot.Helmet, EquipmentCulture.Punic, EquipmentTier.T2, 55),
        new(EquipmentSlot.Helmet, EquipmentCulture.Punic, EquipmentTier.T3, 80),
        new(EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T1, 40),
        new(EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T2, 60),
        new(EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T3, 90),
        new(EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1, 45),
        new(EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T2, 70),
        new(EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T3, 100),

        new(EquipmentSlot.Armor, EquipmentCulture.Punic, EquipmentTier.T1, 50),
        new(EquipmentSlot.Armor, EquipmentCulture.Punic, EquipmentTier.T2, 80),
        new(EquipmentSlot.Armor, EquipmentCulture.Punic, EquipmentTier.T3, 120),
        new(EquipmentSlot.Armor, EquipmentCulture.Greek, EquipmentTier.T1, 55),
        new(EquipmentSlot.Armor, EquipmentCulture.Greek, EquipmentTier.T2, 90),
        new(EquipmentSlot.Armor, EquipmentCulture.Greek, EquipmentTier.T3, 130),
        new(EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T1, 60),
        new(EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T2, 100),
        new(EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T3, 150),

        new(EquipmentSlot.Shield, EquipmentCulture.Punic, EquipmentTier.T1, 25),
        new(EquipmentSlot.Shield, EquipmentCulture.Punic, EquipmentTier.T2, 40),
        new(EquipmentSlot.Shield, EquipmentCulture.Punic, EquipmentTier.T3, 60),
        new(EquipmentSlot.Shield, EquipmentCulture.Greek, EquipmentTier.T1, 30),
        new(EquipmentSlot.Shield, EquipmentCulture.Greek, EquipmentTier.T2, 45),
        new(EquipmentSlot.Shield, EquipmentCulture.Greek, EquipmentTier.T3, 70),
        new(EquipmentSlot.Shield, EquipmentCulture.Roman, EquipmentTier.T1, 35),
        new(EquipmentSlot.Shield, EquipmentCulture.Roman, EquipmentTier.T2, 55),
        new(EquipmentSlot.Shield, EquipmentCulture.Roman, EquipmentTier.T3, 85),

        new(EquipmentSlot.Weapon, EquipmentCulture.Punic, EquipmentTier.T1, 30),
        new(EquipmentSlot.Weapon, EquipmentCulture.Punic, EquipmentTier.T2, 50),
        new(EquipmentSlot.Weapon, EquipmentCulture.Punic, EquipmentTier.T3, 75),
        new(EquipmentSlot.Weapon, EquipmentCulture.Greek, EquipmentTier.T1, 35),
        new(EquipmentSlot.Weapon, EquipmentCulture.Greek, EquipmentTier.T2, 55),
        new(EquipmentSlot.Weapon, EquipmentCulture.Greek, EquipmentTier.T3, 85),
        new(EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T1, 40),
        new(EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T2, 60),
        new(EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T3, 95),
    };

    public static EquipmentTemplate Template(EquipmentSlot slot, EquipmentCulture culture, EquipmentTier tier)
        => Templates.FirstOrDefault(t => t.Slot == slot && t.Culture == culture && t.Tier == tier)
           ?? throw new ArgumentOutOfRangeException(nameof(slot), slot, $"No catalog row for {culture} {slot} {tier}.");

    public static int BuyPrice(EquipmentSlot slot, EquipmentCulture culture, EquipmentTier tier)
        => Template(slot, culture, tier).Buy;

    public static int BuyPrice(EquipmentItem item)
        => BuyPrice(item.Slot, item.Culture, item.Tier);

    public static int ResalePrice(EquipmentSlot slot, EquipmentCulture culture, EquipmentTier tier)
        => BuyPrice(slot, culture, tier) / 2;

    public static int ResalePrice(EquipmentItem item)
        => ResalePrice(item.Slot, item.Culture, item.Tier);

    public static EquipmentItem CreateInstance(int id, EquipmentSlot slot, EquipmentCulture culture, EquipmentTier tier)
    {
        _ = Template(slot, culture, tier);
        return new EquipmentItem
        {
            Id = id,
            Slot = slot,
            Culture = culture,
            Tier = tier
        };
    }
}
