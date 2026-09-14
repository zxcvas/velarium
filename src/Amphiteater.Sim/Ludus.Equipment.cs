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

    // Armory → living man. Occupied slot: previous piece returns to the rack first.
    // Retiarius may wear Helmet / Armor / Weapon of any culture; Shield is N/A (`SlotUsable`).
    public static string? Assign(GameState s, Gladiator g, int itemId)
    {
        if (!g.Alive || !s.Familia.Contains(g)) return "gone";
        s.Armory ??= new();
        int idx = s.Armory.FindIndex(i => i.Id == itemId);
        if (idx < 0) return "gone";
        var item = s.Armory[idx];
        if (!EquipmentItem.SlotUsable(g.Armatura, item.Slot)) return "slot";

        var previous = g.Equipped(item.Slot);
        s.Armory.RemoveAt(idx);
        if (previous != null)
            s.Armory.Add(previous);
        g.SetEquipped(item.Slot, item);
        return null;
    }

    public static string? Unequip(GameState s, Gladiator g, EquipmentSlot slot)
    {
        if (!g.Alive || !s.Familia.Contains(g)) return "gone";
        var item = g.Equipped(slot);
        if (item == null) return "gone";
        s.Armory ??= new();
        s.Armory.Add(item);
        g.SetEquipped(slot, null);
        return null;
    }

    // House keeps the bronze: every equipped piece back to the Armory. Used on death and rudis.
    public static void ReturnLoadout(GameState s, Gladiator g)
    {
        s.Armory ??= new();
        foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
        {
            var item = g.Equipped(slot);
            if (item == null) continue;
            s.Armory.Add(item);
            g.SetEquipped(slot, null);
        }
    }
}
