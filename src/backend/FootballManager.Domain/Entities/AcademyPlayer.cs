using FootballManager.Domain.Common;
using FootballManager.Domain.Enums;

namespace FootballManager.Domain.Entities;

public sealed class AcademyPlayer
{
    private AcademyPlayer()
    {
    }

    internal AcademyPlayer(
        string firstName,
        string lastName,
        PlayerPosition position,
        int age,
        int attack,
        int defense,
        int passing,
        int fitness,
        int morale,
        int potential,
        int developmentProgress,
        string trainingFocus,
        Club club)
    {
        Id = Guid.NewGuid();
        FirstName = Guard.AgainstNullOrWhiteSpace(firstName, nameof(firstName));
        LastName = Guard.AgainstNullOrWhiteSpace(lastName, nameof(lastName));
        Position = position;
        Age = Guard.AgainstOutOfRange(age, 15, 19, nameof(age));
        Attack = Guard.AgainstOutOfRange(attack, 1, 100, nameof(attack));
        Defense = Guard.AgainstOutOfRange(defense, 1, 100, nameof(defense));
        Passing = Guard.AgainstOutOfRange(passing, 1, 100, nameof(passing));
        Fitness = Guard.AgainstOutOfRange(fitness, 1, 100, nameof(fitness));
        Morale = Guard.AgainstOutOfRange(morale, 1, 100, nameof(morale));
        Potential = Guard.AgainstOutOfRange(potential, 1, 100, nameof(potential));
        DevelopmentProgress = Guard.AgainstOutOfRange(developmentProgress, 0, 100, nameof(developmentProgress));
        TrainingFocus = Guard.AgainstNullOrWhiteSpace(trainingFocus, nameof(trainingFocus));
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

    public int Attack { get; private set; }

    public int Defense { get; private set; }

    public int Passing { get; private set; }

    public int Fitness { get; private set; }

    public int Morale { get; private set; }

    public int Potential { get; private set; }

    public int DevelopmentProgress { get; private set; }

    public string TrainingFocus { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public Guid ClubId { get; private set; }

    public Club? Club { get; private set; }

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

    public int GetPromotionReadiness()
    {
        var readinessScore = (GetOverallRating() * 0.58) +
                             (Potential * 0.22) +
                             (DevelopmentProgress * 0.15) +
                             (Morale * 0.05);

        return (int)Math.Round(readinessScore, MidpointRounding.AwayFromZero);
    }

    public bool IsPromotionReady() => GetPromotionReadiness() >= 72;

    public string GetTrainingStatus() =>
        DevelopmentProgress switch
        {
            >= 85 => "Knocking on the first-team door",
            >= 68 => "Accelerating through training",
            >= 50 => "Settling into the programme",
            _ => "Building the base level"
        };

    public void AdvanceDevelopment()
    {
        DevelopmentProgress = Math.Clamp(DevelopmentProgress + Random.Shared.Next(2, 6), 0, 100);
        Fitness = Math.Clamp(Fitness + Random.Shared.Next(0, 3), 1, 100);
        Morale = Math.Clamp(Morale + Random.Shared.Next(0, 3), 1, 100);

        if (Potential <= GetOverallRating())
        {
            return;
        }

        var improvementBudget = Random.Shared.Next(0, 3);
        if (improvementBudget == 0)
        {
            return;
        }

        switch (TrainingFocus)
        {
            case "Finishing":
                Attack = ImproveAttribute(Attack, improvementBudget + 1);
                Passing = ImproveAttribute(Passing, 1);
                break;
            case "Ball retention":
                Passing = ImproveAttribute(Passing, improvementBudget + 1);
                Attack = ImproveAttribute(Attack, 1);
                break;
            case "Defensive timing":
                Defense = ImproveAttribute(Defense, improvementBudget + 1);
                Passing = ImproveAttribute(Passing, 1);
                break;
            case "Tempo control":
                Passing = ImproveAttribute(Passing, improvementBudget + 1);
                Defense = ImproveAttribute(Defense, 1);
                break;
            default:
                Attack = ImproveAttribute(Attack, 1);
                Defense = ImproveAttribute(Defense, 1);
                Passing = ImproveAttribute(Passing, 1);
                break;
        }
    }

    private int ImproveAttribute(int currentValue, int maxGain)
    {
        var ceiling = Math.Max(currentValue, Potential - 2);
        if (currentValue >= ceiling)
        {
            return currentValue;
        }

        return Math.Min(ceiling, currentValue + Random.Shared.Next(1, maxGain + 1));
    }
}
