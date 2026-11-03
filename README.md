# Training Zones

[Read in English](./README.en.md)

## À quoi ça sert

Une petite bibliothèque C# (.NET 10) pour cadrer l'intensité de l'entraînement cycliste et course :

- **Ma FC max**, estimée par Tanaka ou Fox (une FC max mesurée reste prioritaire) ;
- **Mes zones cardiaques**, en % de la FC max ou en réserve cardiaque (Karvonen) ;
- **Mon FTP**, estimé à partir d'un test de 20 minutes, 8 minutes, d'un ramp test ou de 60 minutes ;
- **Mes zones de puissance**, selon le modèle de Coggan à 7 niveaux, en watts.

Elle est tirée des calculateurs de zones et de FTP de mon application Allure+. Elle n'a aucune dépendance externe.

## Fonctions

| Fonction | Rôle |
|---|---|
| `HeartRateZones.EstimateMaxHeartRate` | FC max par Tanaka (défaut) ou Fox. |
| `HeartRateZones.Compute` | Cinq zones cardiaques, en % de la FC max ou par Karvonen. |
| `HeartRateZones.IsValidKarvonenInput` | Vérifie que la FC de repos permet un calcul utile. |
| `FtpEstimator.Estimate` / `Coefficient` | FTP à partir d'un test, avec le coefficient du test et du niveau. |
| `FtpEstimator.WattsPerKg` | Puissance relative. |
| `PowerZones.RangesFor` | Sept zones de Coggan, en watts, pour un FTP donné. |

## Comment l'utiliser

```csharp
using TrainingZones;

// FC max d'un athlète de 35 ans, estimée par Tanaka
var max = HeartRateZones.EstimateMaxHeartRate(35);            // 184

// Zones de Karvonen : FC max 185, FC de repos 50
var zones = HeartRateZones.Compute(185, HeartRateMethod.Karvonen, restingHr: 50);
// Z2 : 131 à 145 bpm

// FTP : 280 W de moyenne sur 20 minutes, niveau intermédiaire
var ftp = FtpEstimator.Estimate(280, FtpTest.Test20Min, AthleteLevel.Intermediate);   // 257,6 W

// Zones de puissance pour ce FTP
var ranges = PowerZones.RangesFor(ftp);
```

## Le coefficient du test de 20 minutes

Le coefficient 0,95 est la convention la plus répandue, mais une étude de 2023 (Sitko, n = 120) montre qu'il surestime le FTP des athlètes peu entraînés d'environ 7 points de pourcentage. Le module applique un coefficient par niveau : 0,88 pour un débutant, 0,92 pour un intermédiaire, 0,95 pour un avancé. Sans niveau, il garde 0,95.

## Pourquoi Karvonen refuse certaines entrées

Sans FC de repos (0), la formule Karvonen se réduit exactement au pourcentage de FC max : l'athlète croit choisir une méthode et reçoit l'autre. Une FC de repos égale ou supérieure à la FC max donne des zones inversées. Dans ces cas, la bibliothèque renvoie `null` plutôt que de fausses zones.

## Limites

- Les zones sont des **repères**. La dérive cardiaque (chaleur, fatigue) décale la FC à allure constante.
- La FC répond avec inertie : elle ne convient pas aux efforts très courts.
- Le FTP estimé par un test de 20 minutes ou de 8 minutes reste une estimation. Le test de 60 minutes est la mesure directe.
- Les seuils de l'estimation FTP sont issus de la littérature ; le coefficient 0,95 pour un test de 20 minutes n'est établi par aucune publication à comité de lecture.

## Tests

```bash
dotnet test
```
