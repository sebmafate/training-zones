namespace TrainingZones;

/// <summary>Protocole de test de puissance utilisé pour estimer le FTP.</summary>
public enum FtpTest
{
    /// <summary>Test de 20 minutes : puissance moyenne × coefficient selon le niveau.</summary>
    Test20Min,
    /// <summary>Test de 8 minutes : puissance moyenne × 0,90.</summary>
    Test8Min,
    /// <summary>Ramp test : puissance de pointe × 0,75.</summary>
    Ramp,
    /// <summary>Test de 60 minutes : mesure directe, coefficient 1,00.</summary>
    Test60Min
}

/// <summary>Niveau de l'athlète, qui modifie le coefficient du test de 20 minutes.</summary>
public enum AthleteLevel
{
    Beginner,
    Intermediate,
    Advanced
}

/// <summary>Estimation du FTP (puissance de seuil fonctionnelle, en W).</summary>
public static class FtpEstimator
{
    /// <summary>
    /// Coefficient du test de 20 minutes. Il dépend du niveau (Sitko 2023) : un 0,95 universel
    /// surestime le FTP d'un athlète peu entraîné d'environ 7 points de pourcentage.
    /// Sans niveau connu, 0,95 reste la convention la plus répandue.
    /// </summary>
    public static double Coefficient(FtpTest test, AthleteLevel? level = null) => test switch
    {
        FtpTest.Test20Min => level switch
        {
            AthleteLevel.Beginner => 0.88,
            AthleteLevel.Intermediate => 0.92,
            AthleteLevel.Advanced => 0.95,
            null => 0.95,
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        },
        FtpTest.Test8Min => 0.90,
        FtpTest.Ramp => 0.75,
        FtpTest.Test60Min => 1.00,
        _ => throw new ArgumentOutOfRangeException(nameof(test))
    };

    /// <summary>FTP estimé (W) à partir de la puissance mesurée (W) pendant le test.</summary>
    public static double Estimate(double measuredPowerW, FtpTest test, AthleteLevel? level = null)
    {
        if (measuredPowerW <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(measuredPowerW), "La puissance doit être strictement positive.");
        }

        return measuredPowerW * Coefficient(test, level);
    }

    /// <summary>Puissance relative (W/kg).</summary>
    public static double WattsPerKg(double ftpW, double weightKg) => weightKg > 0 ? ftpW / weightKg : 0;
}

/// <summary>Zone de puissance en pourcentage du FTP, à la manière de Coggan.</summary>
/// <param name="Name">Nom de la zone.</param>
/// <param name="LowerPercent">Borne basse, en % du FTP (entier).</param>
/// <param name="UpperPercent">Borne haute, en % du FTP, ou <c>null</c> pour la zone ouverte.</param>
public sealed record PowerZone(string Name, int LowerPercent, int? UpperPercent);

/// <summary>Plage de puissance d'une zone, en watts.</summary>
/// <param name="Zone">La zone concernée.</param>
/// <param name="LowerW">Borne basse (W).</param>
/// <param name="UpperW">Borne haute (W), ou <c>null</c> pour la zone ouverte.</param>
public sealed record PowerRange(PowerZone Zone, int LowerW, int? UpperW);

/// <summary>Modèle de Coggan à 7 zones de puissance.</summary>
public static class PowerZones
{
    public static readonly IReadOnlyList<PowerZone> Coggan7 =
    [
        new("Z1 · Récupération active", 0, 55),
        new("Z2 · Endurance", 56, 75),
        new("Z3 · Tempo", 76, 90),
        new("Z4 · Seuil", 91, 105),
        new("Z5 · VO₂max", 106, 120),
        new("Z6 · Capacité anaérobie", 121, 150),
        new("Z7 · Puissance neuromusculaire", 151, null)
    ];

    /// <summary>Plages en watts pour un FTP donné, arrondies au watt le plus proche.</summary>
    public static IReadOnlyList<PowerRange> RangesFor(double ftpW)
    {
        if (ftpW <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ftpW), "Le FTP doit être strictement positif.");
        }

        return Coggan7
            .Select(zone => new PowerRange(
                zone,
                Watts(ftpW, zone.LowerPercent),
                zone.UpperPercent is { } upper ? Watts(ftpW, upper) : null))
            .ToList();
    }

    private static int Watts(double ftp, int percent) =>
        (int)Math.Round(ftp * percent / 100.0, MidpointRounding.AwayFromZero);
}
