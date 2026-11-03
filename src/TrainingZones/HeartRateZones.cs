namespace TrainingZones;

/// <summary>Formule d'estimation de la FC maximale à partir de l'âge.</summary>
public enum MaxHeartRateFormula
{
    /// <summary>Tanaka, Monahan & Seals (2001) : 208 − 0,7 × âge. Défaut : erreur ~2× plus faible.</summary>
    Tanaka,
    /// <summary>Haskell & Fox (1971) : 220 − âge. Écart-type de ±10 à 12 bpm.</summary>
    Fox
}

/// <summary>Méthode de calcul des bornes de zones cardiaques.</summary>
public enum HeartRateMethod
{
    /// <summary>Pourcentage de la FC maximale.</summary>
    PercentMax,
    /// <summary>Karvonen : pourcentage de la réserve cardiaque (FC max − FC repos).</summary>
    Karvonen
}

/// <summary>Zone cardiaque. <see cref="MaxBpm"/> est nul pour la zone 5, qui n'a pas de borne haute.</summary>
/// <param name="Zone">Numéro de zone, de 1 à 5.</param>
/// <param name="MinBpm">Borne basse (bpm).</param>
/// <param name="MaxBpm">Borne haute (bpm), ou <c>null</c> pour la zone 5.</param>
public sealed record HeartRateZone(int Zone, int MinBpm, int? MaxBpm);

/// <summary>Estimation de la FC maximale et calcul des cinq zones cardiaques.</summary>
public static class HeartRateZones
{
    /// <summary>Bornes des zones 1 à 5, en fraction de la FC max (ou de la réserve pour Karvonen).</summary>
    private static readonly (double Low, double High)[] Bands =
    [
        (0.50, 0.60),
        (0.60, 0.70),
        (0.70, 0.80),
        (0.80, 0.90),
        (0.90, 1.00)
    ];

    /// <summary>FC maximale estimée (bpm). Une FC max <em>mesurée</em> reste toujours prioritaire.</summary>
    public static int EstimateMaxHeartRate(int age, MaxHeartRateFormula formula = MaxHeartRateFormula.Tanaka) =>
        formula switch
        {
            MaxHeartRateFormula.Tanaka => (int)Math.Round(208 - 0.7 * age, MidpointRounding.AwayFromZero),
            MaxHeartRateFormula.Fox => 220 - age,
            _ => throw new ArgumentOutOfRangeException(nameof(formula))
        };

    /// <summary>
    /// Vrai si la FC de repos permet un calcul de Karvonen utile. Sans FC de repos (0), la formule
    /// se réduit au pourcentage de FC max : l'athlète croit choisir une méthode et reçoit l'autre.
    /// Une FC de repos ≥ FC max donne une réserve négative et des zones inversées.
    /// </summary>
    public static bool IsValidKarvonenInput(int maxHr, int restingHr) =>
        restingHr > 0 && restingHr < maxHr;

    /// <summary>
    /// Cinq zones cardiaques, ou <c>null</c> si Karvonen est demandé avec une FC de repos inutilisable.
    /// Mieux vaut n'afficher aucune zone que de fausses zones.
    /// </summary>
    public static IReadOnlyList<HeartRateZone>? Compute(int maxHr, HeartRateMethod method, int restingHr = 0)
    {
        if (method == HeartRateMethod.Karvonen && !IsValidKarvonenInput(maxHr, restingHr))
        {
            return null;
        }

        var reserve = maxHr - restingHr;
        var zones = new List<HeartRateZone>(Bands.Length);

        for (var i = 0; i < Bands.Length; i++)
        {
            var (low, high) = Bands[i];
            var isTop = i == Bands.Length - 1;

            var (min, max) = method switch
            {
                HeartRateMethod.PercentMax => (
                    Round(maxHr * low),
                    Round(maxHr * high)),
                HeartRateMethod.Karvonen => (
                    restingHr + Round(reserve * low),
                    restingHr + Round(reserve * high)),
                _ => throw new ArgumentOutOfRangeException(nameof(method))
            };

            zones.Add(new HeartRateZone(i + 1, min, isTop ? null : max));
        }

        return zones;
    }

    private static int Round(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);
}
