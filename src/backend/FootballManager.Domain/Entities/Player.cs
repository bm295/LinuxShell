using FootballManager.Domain.Common;
using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Entities;

public sealed class Player
{
    private Player()
    {
    }

    internal Player(
        string firstName,
        string lastName,
        PlayerPosition position,
        int age,
        int squadNumber,
        int attack,
        int defense,
        int passing,
        int fitness,
        int morale,
        Club club)
    {
        Id = Guid.NewGuid();
        FirstName = Guard.AgainstNullOrWhiteSpace(firstName, nameof(firstName));
        LastName = Guard.AgainstNullOrWhiteSpace(lastName, nameof(lastName));
        Position = position;
        Age = Guard.AgainstOutOfRange(age, 15, 39, nameof(age));
        SquadNumber = Guard.AgainstOutOfRange(squadNumber, 1, 99, nameof(squadNumber));
        Attack = Guard.AgainstOutOfRange(attack, 1, 100, nameof(attack));
        Defense = Guard.AgainstOutOfRange(defense, 1, 100, nameof(defense));
        Passing = Guard.AgainstOutOfRange(passing, 1, 100, nameof(passing));
        Fitness = Guard.AgainstOutOfRange(fitness, 1, 100, nameof(fitness));
        Morale = Guard.AgainstOutOfRange(morale, 1, 100, nameof(morale));
        Club = club ?? throw new ArgumentNullException(nameof(club));
        ClubId = club.Id;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    public PlayerPosition Position { get; private set; }

    public int Age { get; private set; }

    public int SquadNumber { get; private set; }

    public int Attack { get; private set; }

    public int Defense { get; private set; }

    public int Passing { get; private set; }

    public int Fitness { get; private set; }

    public int Morale { get; private set; }

    public bool IsCaptain { get; private set; }

    public int InjuryMatchesRemaining { get; private set; }

    public int AttributeProgress { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Guid ClubId { get; private set; }

    public Club? Club { get; private set; }

    public bool IsInjured => InjuryMatchesRemaining > 0;

    public int GetOverallRating()
    {
        var weightedScore = Position switch
        {
            PlayerPosition.Goalkeeper => (Attack * 0.05) + (Defense * 0.75) + (Passing * 0.20),
            PlayerPosition.Defender => (Attack * 0.15) + (Defense * 0.60) + (Passing * 0.25),
            PlayerPosition.Midfielder => (Attack * 0.30) + (Defense * 0.25) + (Passing * 0.45),
            PlayerPosition.Forward => (Attack * 0.60) + (Defense * 0.15) + (Passing * 0.25),
            _ => (Attack + Defense + Passing) / 3d
        };

        return (int)Math.Round(weightedScore, MidpointRounding.AwayFromZero);
    }

    public int GetReadinessScore()
    {
        var readinessScore = (Fitness * 0.6) + (Morale * 0.4);
        return (int)Math.Round(readinessScore, MidpointRounding.AwayFromZero);
    }

    public bool IsAvailableForSelection() => !IsInjured;

    public void AdjustFitness(int delta)
    {
        Fitness = Math.Clamp(Fitness + delta, 1, 100);
    }

    public void AdjustMorale(int delta)
    {
        Morale = Math.Clamp(Morale + delta, 1, 100);
    }

    public void RecordInjury(int matchesToMiss)
    {
        if (matchesToMiss <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(matchesToMiss), "Injury duration must be greater than zero.");
        }

        InjuryMatchesRemaining = Math.Max(InjuryMatchesRemaining, matchesToMiss);
    }

    public void RecoverFromMissedMatch()
    {
        if (InjuryMatchesRemaining == 0)
        {
            return;
        }

        InjuryMatchesRemaining--;
    }

    public void Rename(string firstName, string lastName)
    {
        FirstName = Guard.AgainstNullOrWhiteSpace(firstName, nameof(firstName));
        LastName = Guard.AgainstNullOrWhiteSpace(lastName, nameof(lastName));
    }

    public void SetAge(int age)
    {
        Age = Guard.AgainstOutOfRange(age, 15, 39, nameof(age));
    }

    public void ChangePosition(PlayerPosition position)
    {
        Position = position;
    }

    public void AssignCaptaincy()
    {
        IsCaptain = true;
    }

    public void RemoveCaptaincy()
    {
        IsCaptain = false;
    }

    public void ApplyAgeBasedMatchDevelopment(bool playedMatch)
    {
        var overallBefore = GetOverallRating();
        var progressShift = ResolveAgeBasedProgressShift(playedMatch);
        var improvedCoreAttribute = false;

        AttributeProgress += progressShift;

        while (AttributeProgress >= 3)
        {
            ImproveCoreAttribute();
            AttributeProgress -= 3;
            improvedCoreAttribute = true;
        }

        while (AttributeProgress <= -3)
        {
            ReduceCoreAttribute();
            AttributeProgress += 3;
        }

        if (playedMatch && progressShift > 0 && improvedCoreAttribute)
        {
            ApplyVisibleOverallBoost(overallBefore);
        }
    }

    public void TransferTo(Club club, int squadNumber)
    {
        if (club is null)
        {
            throw new ArgumentNullException(nameof(club));
        }

        if (club.Players.Any(player => player.Id != Id && player.SquadNumber == squadNumber))
        {
            throw new InvalidOperationException($"Squad number '{squadNumber}' is already assigned at club '{club.Name}'.");
        }

        Club?.Players.Remove(this);
        Club = club;
        ClubId = club.Id;
        SquadNumber = Guard.AgainstOutOfRange(squadNumber, 1, 99, nameof(squadNumber));
        IsCaptain = false;
        club.Players.Add(this);
    }

    private int ResolveAgeBasedProgressShift(bool playedMatch) =>
        Age switch
        {
            <= 18 => playedMatch ? 5 : 2,
            <= 21 => playedMatch ? 4 : 1,
            <= 24 => playedMatch ? 2 : 0,
            <= 29 => 0,
            <= 32 => playedMatch ? -1 : 0,
            _ => playedMatch ? -2 : -1
        };

    private void ImproveCoreAttribute() => UpdateWeightedAttribute(1);

    private void ReduceCoreAttribute() => UpdateWeightedAttribute(-1);

    private void UpdateWeightedAttribute(int delta)
    {
        var roll = Random.Shared.NextDouble();
        var target = Position switch
        {
            PlayerPosition.Goalkeeper => roll < 0.72 ? "Defense" : "Passing",
            PlayerPosition.Defender => roll < 0.60 ? "Defense" : roll < 0.82 ? "Passing" : "Attack",
            PlayerPosition.Midfielder => roll < 0.46 ? "Passing" : roll < 0.74 ? "Attack" : "Defense",
            _ => roll < 0.60 ? "Attack" : roll < 0.85 ? "Passing" : "Defense"
        };

        switch (target)
        {
            case "Attack":
                Attack = Math.Clamp(Attack + delta, 1, 100);
                break;
            case "Defense":
                Defense = Math.Clamp(Defense + delta, 1, 100);
                break;
            default:
                Passing = Math.Clamp(Passing + delta, 1, 100);
                break;
        }
    }

    private void ApplyVisibleOverallBoost(int overallBefore)
    {
        var bonusGrowthSteps = ResolveVisibleGrowthBoostSteps();

        if (bonusGrowthSteps == 0 || GetOverallRating() > overallBefore)
        {
            return;
        }

        for (var step = 0; step < bonusGrowthSteps && GetOverallRating() <= overallBefore; step++)
        {
            ApplyGrowth(ResolveBestGrowthAttribute(), 1);
        }
    }

    private int ResolveVisibleGrowthBoostSteps() =>
        Age switch
        {
            <= 21 => 2,
            <= 24 => 1,
            _ => 0
        };

    private CoreAttribute ResolveBestGrowthAttribute()
    {
        foreach (var attribute in ResolveGrowthPriority())
        {
            if (GetAttributeValue(attribute) < 100)
            {
                return attribute;
            }
        }

        return ResolveGrowthPriority()[0];
    }

    private IReadOnlyList<CoreAttribute> ResolveGrowthPriority() =>
        Position switch
        {
            PlayerPosition.Goalkeeper => [CoreAttribute.Defense, CoreAttribute.Passing, CoreAttribute.Attack],
            PlayerPosition.Defender => [CoreAttribute.Defense, CoreAttribute.Passing, CoreAttribute.Attack],
            PlayerPosition.Midfielder => [CoreAttribute.Passing, CoreAttribute.Attack, CoreAttribute.Defense],
            _ => [CoreAttribute.Attack, CoreAttribute.Passing, CoreAttribute.Defense]
        };

    private int GetAttributeValue(CoreAttribute attribute) =>
        attribute switch
        {
            CoreAttribute.Attack => Attack,
            CoreAttribute.Defense => Defense,
            _ => Passing
        };

    private void ApplyGrowth(CoreAttribute attribute, int delta)
    {
        switch (attribute)
        {
            case CoreAttribute.Attack:
                Attack = Math.Clamp(Attack + delta, 1, 100);
                break;
            case CoreAttribute.Defense:
                Defense = Math.Clamp(Defense + delta, 1, 100);
                break;
            default:
                Passing = Math.Clamp(Passing + delta, 1, 100);
                break;
        }
    }

    private enum CoreAttribute
    {
        Attack,
        Defense,
        Passing
    }
}
