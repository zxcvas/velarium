namespace Velarium;

public static partial class Ludus
{
    public static string? BuyEquipment(GameState s, EquipmentTemplate template)
        => BuyEquipment(s, template.Slot, template.Culture, template.Tier);

    public static string? BuyEquipment(GameState s, EquipmentSlot slot, EquipmentCulture culture, EquipmentTier tier)
    {
        int price = EquipmentCatalog.BuyPrice(slot, culture, tier);
        if (s.Denarii < price) return "coin";
        s.Armory ??= new();
        s.Denarii -= price;
        s.Armory.Add(EquipmentCatalog.CreateInstance(s.NextId++, slot, culture, tier));
        return null;
    }

    public static string? SellEquipment(GameState s, int armoryIndex)
    {
        s.Armory ??= new();
        if (armoryIndex < 0 || armoryIndex >= s.Armory.Count) return "gone";
        var item = s.Armory[armoryIndex];
        s.Denarii += EquipmentCatalog.ResalePrice(item);
        s.Armory.RemoveAt(armoryIndex);
        return null;
    }
}
