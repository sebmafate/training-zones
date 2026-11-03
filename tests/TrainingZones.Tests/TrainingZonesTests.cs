namespace TrainingZones.Tests;

public class HeartRateZonesTests
{
    [Theory]
    [InlineData(35, 184)]
    [InlineData(55, 170)]
    public void Tanaka_MatchesDocumentedValues(int age, int expected)
    {
        Assert.Equal(expected, HeartRateZones.EstimateMaxHeartRate(age, MaxHeartRateFormula.Tanaka));
    }

    [Fact]
    public void Fox_IsTwoTwentyMinusAge()
    {
        Assert.Equal(185, HeartRateZones.EstimateMaxHeartRate(35, MaxHeartRateFormula.Fox));
    }

    [Fact]
    public void PercentMax_Max185_Z2AndZ4()
    {
        var zones = HeartRateZones.Compute(185, HeartRateMethod.PercentMax)!;

        Assert.Equal(111, zones[1].MinBpm);
        Assert.Equal(130, zones[1].MaxBpm);
        Assert.Equal(148, zones[3].MinBpm);
        Assert.Equal(167, zones[3].MaxBpm);
    }

    [Fact]
    public void Karvonen_Max185Rest50_Z2IsDocumented131To145()
    {
        var zones = HeartRateZones.Compute(185, HeartRateMethod.Karvonen, restingHr: 50)!;

        Assert.Equal(131, zones[1].MinBpm);
        Assert.Equal(145, zones[1].MaxBpm);
    }

    [Fact]
    public void Karvonen_IsShiftedUpForFitterAthlete_ComparedToPercentMax()
    {
        var percent = HeartRateZones.Compute(185, HeartRateMethod.PercentMax)![1].MinBpm;
        var karvonen = HeartRateZones.Compute(185, HeartRateMethod.Karvonen, restingHr: 50)![1].MinBpm;

        Assert.True(karvonen > percent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(185)]
    [InlineData(200)]
    public void Karvonen_UnusableRestingHr_ReturnsNull(int restingHr)
    {
        Assert.Null(HeartRateZones.Compute(185, HeartRateMethod.Karvonen, restingHr));
    }

    [Fact]
    public void Zone5_HasNoUpperBound()
    {
        var zones = HeartRateZones.Compute(185, HeartRateMethod.PercentMax)!;

        Assert.Null(zones[4].MaxBpm);
        Assert.Equal(5, zones.Count);
    }
}

public class FtpEstimatorTests
{
    [Fact]
    public void Test20Min_WithoutLevel_UsesConventionalCoefficient()
    {
        // 280 W × 0,95 = 266 W
        Assert.Equal(266, FtpEstimator.Estimate(280, FtpTest.Test20Min), precision: 9);
    }

    [Fact]
    public void Test20Min_IntermediateLevel_UsesLevelCoefficient()
    {
        // 280 W × 0,92 = 257,6 W
        Assert.Equal(257.6, FtpEstimator.Estimate(280, FtpTest.Test20Min, AthleteLevel.Intermediate), precision: 9);
    }

    [Fact]
    public void Test8Min_IsNinetyPercent()
    {
        Assert.Equal(261, FtpEstimator.Estimate(290, FtpTest.Test8Min), precision: 9);
    }

    [Fact]
    public void Ramp_UsesPeakPowerTimesThreeQuarters()
    {
        Assert.Equal(262.5, FtpEstimator.Estimate(350, FtpTest.Ramp), precision: 9);
    }

    [Fact]
    public void Test60Min_IsDirectMeasure()
    {
        Assert.Equal(250, FtpEstimator.Estimate(250, FtpTest.Test60Min), precision: 9);
    }

    [Fact]
    public void Coefficient_20Min_IncreasesWithLevel()
    {
        var beginner = FtpEstimator.Coefficient(FtpTest.Test20Min, AthleteLevel.Beginner);
        var intermediate = FtpEstimator.Coefficient(FtpTest.Test20Min, AthleteLevel.Intermediate);
        var advanced = FtpEstimator.Coefficient(FtpTest.Test20Min, AthleteLevel.Advanced);

        Assert.True(beginner < intermediate);
        Assert.True(intermediate < advanced);
    }

    [Fact]
    public void WattsPerKg_IsFtpOverWeight()
    {
        Assert.Equal(3.8, FtpEstimator.WattsPerKg(266, 70), precision: 9);
    }

    [Fact]
    public void NonPositivePower_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FtpEstimator.Estimate(0, FtpTest.Test20Min));
    }
}

public class PowerZonesTests
{
    [Fact]
    public void Ftp250_Z4Range_Is228To263()
    {
        // 91 % = 227,5 → 228 ; 105 % = 262,5 → 263 (arrondi loin de zéro)
        var z4 = PowerZones.RangesFor(250)[3];

        Assert.Equal("Z4 · Seuil", z4.Zone.Name);
        Assert.Equal(228, z4.LowerW);
        Assert.Equal(263, z4.UpperW);
    }

    [Fact]
    public void Ftp250_OpenZone_HasNoUpperBound()
    {
        var z7 = PowerZones.RangesFor(250)[6];

        Assert.Equal(378, z7.LowerW);
        Assert.Null(z7.UpperW);
    }

    [Fact]
    public void SevenZones_AreReturnedInOrder()
    {
        Assert.Equal(7, PowerZones.RangesFor(250).Count);
    }

    [Fact]
    public void NonPositiveFtp_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PowerZones.RangesFor(0));
    }
}
