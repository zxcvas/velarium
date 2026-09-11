using Velarium;

namespace Amphiteater.Sim.Tests;

public class CareerSimTests
{
    [Fact]
    public void ChooseStallDish_picks_moretum_when_the_purse_can_boil_it()
    {
        var rng = new Random(8);
        var s = Ludus.Start(rng, 8, "Lucius", "Atinius", "Strabo");
        s.FoodRumorActive = false;
        s.Denarii = 400;
        Ludus.RoomOf(s, RoomKind.Kitchen)!.Level = 3;
        var w = Ludus.MakeWorker(s, rng, new HashSet<string>());
        s.Household.Add(w);
        Assert.Null(Ludus.AssignWorker(s, w.Id, RoomKind.Kitchen));
        Assert.Equal(DishKind.Moretum, CareerSim.ChooseStallDish(s));
    }

    [Fact]
    public void ChooseStallDish_follows_a_fat_food_rumor()
    {
        var rng = new Random(9);
        var s = Ludus.Start(rng, 9, "Lucius", "Atinius", "Strabo");
        s.Denarii = 400;
        Ludus.RoomOf(s, RoomKind.Kitchen)!.Level = 3;
        var w = Ludus.MakeWorker(s, rng, new HashSet<string>());
        s.Household.Add(w);
        Assert.Null(Ludus.AssignWorker(s, w.Id, RoomKind.Kitchen));
        s.FoodRumorActive = true;
        s.FoodRumorDish = DishKind.Lentil;
        s.FoodRumorPrice = 3;
        s.FoodRumorDemand = 2;
        Assert.Equal(DishKind.Lentil, CareerSim.ChooseStallDish(s));
    }

    [Fact]
    public void ChooseStallDish_sells_cheap_bowls_when_grain_is_thin()
    {
        var rng = new Random(10);
        var s = Ludus.Start(rng, 10, "Lucius", "Atinius", "Strabo");
        s.FoodRumorActive = false;
        s.Denarii = 3;
        Ludus.RoomOf(s, RoomKind.Kitchen)!.Level = 3;
        var w = Ludus.MakeWorker(s, rng, new HashSet<string>());
        s.Household.Add(w);
        Assert.Null(Ludus.AssignWorker(s, w.Id, RoomKind.Kitchen));
        var dish = CareerSim.ChooseStallDish(s);
        Assert.Equal(1, Content.GetDish(dish).Cost);
    }

    [Fact]
    public void RunCareer_upgrade_policy_opens_stall_unlike_locatio_only()
    {
        const int n = 24;
        const int days = 20;
        var stall = CareerSim.RunMany(n, days, CareerKitchenPolicy.UpgradeStall);
        var staff = CareerSim.RunMany(n, days, CareerKitchenPolicy.StaffOnly);
        var locatio = CareerSim.RunMany(n, days, CareerKitchenPolicy.LocatioOnly);

        Assert.True(stall.StaffedKitchenBy30 > 0, "upgrade policy should still hire a cook");
        Assert.True(stall.StallKitchenBy30 > 0, "upgrade policy should raise culina to lv2 in some careers");
        Assert.True(stall.CareersWithStallIncome > 0, "upgrade policy should take stall denarii");
        Assert.True(stall.TotalStallProfit > 0);
        Assert.True(stall.MeanPeakKitchenLevel >= 2, "mean peak kitchen should reach the stall");

        Assert.True(staff.StaffedKitchenBy30 > 0, "staff-only still hires a cook");
        Assert.Equal(0, staff.StallKitchenBy30);
        Assert.Equal(0, staff.CareersWithStallIncome);
        Assert.Equal(0, staff.TotalStallProfit);

        Assert.Equal(0, locatio.StaffedKitchenBy30);
        Assert.Equal(0, locatio.StallKitchenBy30);
        Assert.Equal(0, locatio.CareersWithStallIncome);
        Assert.Equal(0, locatio.TotalStallProfit);
        Assert.True(locatio.MeanPeakKitchenLevel <= 1);
    }

    [Fact]
    public void Format_names_the_kitchen_upgrade_policy()
    {
        var r = CareerSim.RunMany(3, maxDays: 4);
        string text = CareerSim.Format(r);
        Assert.Contains("upgrade culina", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Kitchen stall", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Mean stall profit", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Format_names_the_locatio_only_policy()
    {
        var r = CareerSim.RunMany(3, maxDays: 4, CareerKitchenPolicy.LocatioOnly);
        string text = CareerSim.Format(r, CareerKitchenPolicy.LocatioOnly);
        Assert.Contains("locatio-only", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hosts when unlocked", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("upgrade culina", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldTakeLocatio_skips_a_tired_man_when_the_purse_is_fat()
    {
        var s = FreshOffer();
        s.Denarii = 400;
        var pick = s.Living.First(g => g.Armatura == s.Offer!.Requested);
        pick.Status = GladiatorStatus.Fessus;
        pick.Vigor = 8;
        Assert.False(CareerSim.ShouldTakeLocatio(s, pick));
    }

    [Fact]
    public void ShouldTakeLocatio_sends_a_tired_man_when_broke()
    {
        var s = FreshOffer();
        s.Denarii = 40;
        var pick = s.Living.First(g => g.CanFight);
        pick.Status = GladiatorStatus.Fessus;
        pick.Vigor = 8;
        Assert.True(CareerSim.ShouldTakeLocatio(s, pick));
    }

    [Fact]
    public void ShouldTakeLocatio_skips_a_cheap_wrong_type()
    {
        var s = FreshOffer();
        s.Denarii = 400;
        var pick = s.Living.First();
        pick.Status = GladiatorStatus.Validus;
        pick.Vigor = pick.VigorMax;
        s.Offer!.Requested = pick.Armatura == Armatura.Secutor ? Armatura.Murmillo : Armatura.Secutor;
        s.Offer.PaySudore = 22;
        Assert.False(CareerSim.ShouldTakeLocatio(s, pick));
        s.Offer.PaySudore = 36;
        Assert.True(CareerSim.ShouldTakeLocatio(s, pick));
    }

    [Fact]
    public void ShouldHost_when_unlocked_two_fresh_and_cushioned()
    {
        var s = FreshOffer();
        s.Fama = Ludus.HostFamaNeed;
        s.Denarii = Ludus.HostCost + CareerSim.HostCushion;
        foreach (var g in s.Living)
        {
            g.Palmae = 1;
            g.Status = GladiatorStatus.Validus;
            g.Vigor = g.VigorMax;
        }
        Assert.True(s.HostingUnlocked);
        Assert.True(CareerSim.ShouldHost(s));
        s.Denarii = Ludus.HostCost + CareerSim.HostCushion - 1;
        Assert.False(CareerSim.ShouldHost(s));
    }

    [Fact]
    public void RunCareer_locatio_only_rarely_ruins_by_day_21()
    {
        const int n = 40;
        const int days = 21;
        var r = CareerSim.RunMany(n, days, CareerKitchenPolicy.LocatioOnly);
        Assert.True(r.RuinedPct < 50,
            $"locatio-only ruin by day 21 was {r.RuinedPct:0.0}% (prior baseline ~100%)");
        Assert.True(r.MeanDays > 16, $"mean days {r.MeanDays:0.0} still hugs the old ~21 death march");
        Assert.True(r.LocatioSkipped > 0, "locatio AI should skip some tired/wrong-type offers");
    }

    [Fact]
    public void RunCareer_locatio_only_takes_the_fama_host_path()
    {
        var r = CareerSim.RunMany(24, maxDays: 40, CareerKitchenPolicy.LocatioOnly);
        Assert.True(r.HostedBouts > 0, "locatio-only should host once fama unlocks and the purse can pay the gate");
        Assert.Equal(0, r.StaffedKitchenBy30);
    }

    static GameState FreshOffer()
    {
        var rng = new Random(3);
        var s = Ludus.Start(rng, 3, "Lucius", "Atinius", "Strabo");
        var pick = s.Living.First();
        s.Offer = new Contract
        {
            EditorName = "Gaius",
            Requested = pick.Armatura,
            PaySudore = 32,
            PayOccisus = 400
        };
        return s;
    }
}
