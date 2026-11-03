# Training Zones

[Lire en français](./README.md)

## What it is for

A small C# (.NET 10) library to frame cycling and running training intensity:

- **My maximum heart rate**, estimated with Tanaka or Fox (a measured maximum always takes priority);
- **My heart rate zones**, as a percentage of maximum HR or of heart-rate reserve (Karvonen);
- **My FTP**, estimated from a 20-minute, 8-minute, ramp or 60-minute test;
- **My power zones**, using Coggan's 7-zone model, in watts.

It is extracted from the HR-zone and FTP calculators of my iOS app, Allure+. It has no external dependencies.

## Functions

| Function | Role |
|---|---|
| `HeartRateZones.EstimateMaxHeartRate` | Maximum HR by Tanaka (default) or Fox. |
| `HeartRateZones.Compute` | Five heart-rate zones, as a percentage of maximum HR or by Karvonen. |
| `HeartRateZones.IsValidKarvonenInput` | Checks that the resting HR allows a meaningful calculation. |
| `FtpEstimator.Estimate` / `Coefficient` | FTP from a test, with the coefficient for the test and the level. |
| `FtpEstimator.WattsPerKg` | Relative power. |
| `PowerZones.RangesFor` | Coggan's seven zones, in watts, for a given FTP. |

## Usage

```csharp
using TrainingZones;

// Maximum HR of a 35-year-old athlete, estimated by Tanaka
var max = HeartRateZones.EstimateMaxHeartRate(35);            // 184

// Karvonen zones: max HR 185, resting HR 50
var zones = HeartRateZones.Compute(185, HeartRateMethod.Karvonen, restingHr: 50);
// Z2: 131 to 145 bpm

// FTP: 280 W average over 20 minutes, intermediate level
var ftp = FtpEstimator.Estimate(280, FtpTest.Test20Min, AthleteLevel.Intermediate);   // 257.6 W

// Power zones for this FTP
var ranges = PowerZones.RangesFor(ftp);
```

## The 20-minute test coefficient

The 0.95 coefficient is the most common convention, but a 2023 study (Sitko, n = 120) shows it overestimates FTP for less-trained athletes by about 7 percentage points. The module uses a level-specific value: 0.88 for beginners, 0.92 for intermediate, 0.95 for advanced. Without a level, it keeps 0.95.

## Why Karvonen rejects some inputs

Without a resting HR (0), the Karvonen formula reduces exactly to a percentage of maximum HR: the athlete thinks they chose one method and gets the other. A resting HR equal to or above maximum HR gives inverted zones. In these cases the library returns `null` rather than wrong zones.

## Limits

- Zones are **guidelines**. Cardiac drift (heat, fatigue) shifts HR at constant pace.
- HR responds with lag: it is unsuited to very short efforts.
- An FTP estimated from a 20- or 8-minute test remains an estimate. The 60-minute test is the direct measure.
- The FTP estimation thresholds come from the literature; the 0.95 coefficient for the 20-minute test is not established by any peer-reviewed publication.

## Tests

```bash
dotnet test
```
