namespace Velarium;

public static class Combat
{
    public static Gladiator MakeFoe(Random rng, Armatura armatura, int daysPlayed)
    {
        var taken = new HashSet<string>();
        var g = new Gladiator
        {
            Id = -1,
            Name = Content.UniqueName(rng, taken),
            Origin = Content.Origins[rng.Next(Content.Origins.Length)],
            Armatura = armatura,
            VigorMax = rng.Next(13, 19),
            Virtus = Math.Clamp(rng.Next(4, 8) + daysPlayed / 6, 3, 16),
            Fama = rng.Next(0, 4),
            Palmae = Math.Max(0, rng.Next(-2, 4)),
            Pugnat = rng.Next(0, 6),
            Status = GladiatorStatus.Validus,
            Source = rng.Next(6) == 0 ? "damnatus" : "servus"
        };
        g.Vigor = g.VigorMax;
        return g;
    }

    public static CombatReport Fight(Random rng, Gladiator player, Gladiator foe)
    {
        var report = new CombatReport();
        int pv = player.Vigor;
        int fv = foe.Vigor;
        int rounds = 0;
        int totalSwing = 0;

        int P() => Score(rng, player, foe, pv);
        int F() => Score(rng, foe, player, fv, applyEquipment: false);

        while (pv > 0 && fv > 0 && rounds < 6)
        {
            rounds++;
            int ps = P();
            int fs = F();
            int diff = ps - fs;
            totalSwing += Math.Abs(diff);

            if (diff >= 4)
            {
                int dmg = rng.Next(2, 5);
                fv -= dmg;
                report.Beats.Add(Content.Beat(rng, player.Armatura, true));
                if (diff >= 7)
                    report.Beats.Add($"{player.Name} drives {foe.Name} toward the podium. The crowd rises.");
            }
            else if (diff <= -4)
            {
                int dmg = rng.Next(2, 5);
                pv -= dmg;
                report.Beats.Add(Content.Beat(rng, player.Armatura, false));
                report.Beats.Add(Content.Beat(rng, foe.Armatura, true));
            }
            else
            {
                pv -= 1;
                fv -= 1;
                report.Beats.Add(Content.Pick(rng,
                    "Bronze on bronze. The crowd draws breath as one.",
                    "They lock, break, lock again. Sand sprays the first rows.",
                    "Neither yields. The velarium snaps in the wind above.",
                    "A shout from the podium: the editor leans forward."));
            }
        }

        pv = Math.Max(0, pv);
        fv = Math.Max(0, fv);
        report.PlayerVigorAfter = pv;
        report.PlayerDown = pv <= 0;
        report.FoeDown = fv <= 0;
        report.Spectacular = totalSwing >= 14 || rounds >= 5;
        report.CrowdBloodlust = (report.Spectacular ? 2 : 0) + (player.Fama < 2 ? 1 : 0) + rng.Next(0, 3);

        if (pv <= 0 && fv <= 0)
        {
            report.Outcome = FightOutcome.Stans;
            report.Beats.Add("Both fall in the same breath. The trumpets falter.");
        }
        else if (pv <= 0)
        {
            report.Outcome = FightOutcome.Missio; // actual missio/mors decided by crowd/editor later
            report.Beats.Add($"{player.Name} is down. His {KitWord(player.Armatura)} lies in the sand.");
        }
        else if (fv <= 0)
        {
            report.Outcome = FightOutcome.Victoria;
            report.Beats.Add($"{foe.Name} drops. {player.Name} turns to the podium, waiting on the editor's hand.");
        }
        else
        {
            // time called — compare remaining vigor
            int gap = pv - fv;
            if (Math.Abs(gap) <= 2)
            {
                report.Outcome = FightOutcome.Stans;
                report.Beats.Add("The trumpets cut them apart. Neither has the other. Stans.");
            }
            else if (gap > 0)
            {
                report.Outcome = FightOutcome.Victoria;
                report.Beats.Add($"The editor has seen enough. {foe.Name} lowers his weapon. Palma to {player.Name}.");
            }
            else
            {
                report.Outcome = FightOutcome.Missio;
                report.Beats.Add($"{player.Name} is outmatched. He looks to the podium for missio.");
            }
        }

        return report;
    }

    // Additive kit knobs. Sheet: production/economy.md § Forum equipment → Combat additives.
    // Do not retune virtus / vigor / palmae / rounds / Ville. Foe loadouts stay unknobbed (v1).
    public const int GearTier1Bonus = 0;
    public const int GearTier2Bonus = 1;
    public const int GearTier3Bonus = 2;
    public const int GearBonusCap = 4;
    public const int RetiariusMissingRomanWeapon = 2;
    public const int CultureMismatchDock = 1;

    public static int Score(Random rng, Gladiator self, Gladiator other, int currentVigor, bool applyEquipment = true)
    {
        int n = self.Virtus
            + currentVigor / 4
            + rng.Next(1, 7)
            + rng.Next(1, 7);
        if (self.Status == GladiatorStatus.Vulneratus) n -= 3;
        if (self.Status == GladiatorStatus.Fessus) n -= 2;
        if (self.FoughtToday) n -= 2;
        if (self.Pugnat == 0) n -= 1; // tiro
        if (Content.ClassicPair(self.Armatura, other.Armatura)) n += 1;
        if (self.Source == "auctoratus") n += 1;
        if (applyEquipment) n += EquipmentScore(self);
        return n;
    }

    public static int TierBonus(EquipmentTier tier) => tier switch
    {
        EquipmentTier.T1 => GearTier1Bonus,
        EquipmentTier.T2 => GearTier2Bonus,
        EquipmentTier.T3 => GearTier3Bonus,
        _ => 0
    };

    public static bool HasRomanWeapon(Gladiator g)
        => g.Weapon is { Slot: EquipmentSlot.Weapon, Culture: EquipmentCulture.Roman };

    // Natural culture is the armatura, not Origin. Thraex: Punic exotic ok.
    // Retiarius weapon is the −2 track (only Roman Weapon clears it); mismatch is helm / armour.
    public static bool PieceMatchesAffinity(Armatura armatura, EquipmentItem item) => armatura switch
    {
        Armatura.Murmillo or Armatura.Secutor => item.Culture == EquipmentCulture.Roman,
        Armatura.Thraex => item.Culture is EquipmentCulture.Greek or EquipmentCulture.Punic,
        Armatura.Retiarius when item.Slot == EquipmentSlot.Weapon => true,
        Armatura.Retiarius => item.Culture == EquipmentCulture.Roman,
        _ => true
    };

    public static bool CultureMismatch(Gladiator g)
    {
        foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
        {
            var item = g.Equipped(slot);
            if (item != null && !PieceMatchesAffinity(g.Armatura, item))
                return true;
        }
        return false;
    }

    public static int EquipmentScore(Gladiator g)
    {
        int bonus = 0;
        foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
        {
            var item = g.Equipped(slot);
            if (item != null)
                bonus += TierBonus(item.Tier);
        }
        int n = Math.Min(GearBonusCap, bonus);
        if (g.Armatura == Armatura.Retiarius && !HasRomanWeapon(g))
            n -= RetiariusMissingRomanWeapon;
        if (CultureMismatch(g))
            n -= CultureMismatchDock;
        return n;
    }

    static string KitWord(Armatura a) => a switch
    {
        Armatura.Murmillo => "scutum",
        Armatura.Thraex => "parmula",
        Armatura.Retiarius => "trident",
        Armatura.Secutor => "helm",
        _ => "weapon"
    };
}
