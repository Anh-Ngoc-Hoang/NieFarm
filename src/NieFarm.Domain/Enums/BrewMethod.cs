namespace NieFarm.Domain.Enums;

/// <summary>Brewing methods a batch can carry a guide for (R-26). Stored as int; never renumber.</summary>
public enum BrewMethod
{
    Phin = 1,
    Espresso = 2,
    PourOver = 3,
    ColdBrew = 4,
    FrenchPress = 5,
    Moka = 6
}
