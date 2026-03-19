using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

public sealed class MatchSimulationService(FootballManagerDbContext dbContext) : IMatchSimulationService
{
    public async Task<SimulatedMatchResultDto?> SimulateNextMatchAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var gameSave = await dbContext.GameSaves
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.League)
                    .ThenInclude(league => league!.Clubs)
                        .ThenInclude(club => club.Players)
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.League)
                    .ThenInclude(league => league!.Clubs)
                        .ThenInclude(club => club.AcademyPlayers)
            .Include(save => save.Lineup)
                .ThenInclude(lineup => lineup!.Formation)
            .Include(save => save.Season)
                .ThenInclude(season => season!.Fixtures)
            .SingleOrDefaultAsync(save => save.Id == gameId, cancellationToken);

        if (gameSave?.SelectedClub?.League is null || gameSave.Season is null)
        {
            return null;
        }

        var selectedClub = gameSave.SelectedClub;
        var selectedLineup = await LineupPlanner.EnsureLineupAsync(dbContext, gameSave, cancellationToken);
        var defaultFormation = await LineupPlanner.GetDefaultFormationAsync(dbContext, cancellationToken);
        var seniorSnapshots = CaptureSeniorSnapshots(selectedClub.Players);
        var academySnapshots = CaptureAcademySnapshots(selectedClub.AcademyPlayers);

        var managedFixture = gameSave.Season.Fixtures
            .Where(fixture => !fixture.IsPlayed &&
                              (fixture.HomeClubId == selectedClub.Id || fixture.AwayClubId == selectedClub.Id))
            .OrderBy(fixture => fixture.RoundNumber)
            .ThenBy(fixture => fixture.ScheduledAt)
            .FirstOrDefault();

        if (managedFixture is null)
        {
            throw new InvalidOperationException("No upcoming fixture is available for this club.");
        }

        var roundFixtures = gameSave.Season.Fixtures
            .Where(fixture => !fixture.IsPlayed && fixture.RoundNumber == managedFixture.RoundNumber)
            .OrderBy(fixture => fixture.ScheduledAt)
            .ThenBy(fixture => fixture.HomeClubId)
            .ToList();

        var leagueClubs = selectedClub.League.Clubs.ToList();
        var clubLookup = leagueClubs.ToDictionary(club => club.Id);
        var clubNames = leagueClubs.ToDictionary(club => club.Id, club => club.Name);
        var matchEvents = new List<MatchEventDto>();
        var managedStarterIds = new HashSet<Guid>();
        var resultClock = DateTime.UtcNow;

        foreach (var fixture in roundFixtures)
        {
            var homeClub = clubLookup[fixture.HomeClubId];
            var awayClub = clubLookup[fixture.AwayClubId];
            var homeSelection = BuildTeamSelection(
                homeClub,
                homeClub.Id == selectedClub.Id ? selectedLineup.Formation ?? defaultFormation : defaultFormation,
                homeClub.Id == selectedClub.Id ? selectedLineup.GetStarterPlayerIds() : null);
            var awaySelection = BuildTeamSelection(
                awayClub,
                awayClub.Id == selectedClub.Id ? selectedLineup.Formation ?? defaultFormation : defaultFormation,
                awayClub.Id == selectedClub.Id ? selectedLineup.GetStarterPlayerIds() : null);

            var simulation = SimulateFixture(
                homeSelection,
                awaySelection,
                captureEvents: fixture.Id == managedFixture.Id);

            fixture.Complete(simulation.HomeGoals, simulation.AwayGoals, resultClock.AddMinutes(fixture.RoundNumber));
            ApplyPostMatchState(homeSelection, simulation.HomeGoals, simulation.AwayGoals, simulation.HomeScorers, simulation.HomeInjuries, matchEvents, fixture.Id == managedFixture.Id);
            ApplyPostMatchState(awaySelection, simulation.AwayGoals, simulation.HomeGoals, simulation.AwayScorers, simulation.AwayInjuries, matchEvents, fixture.Id == managedFixture.Id);
            ApplyMatchdayFinances(homeClub, awayClub, fixture.RoundNumber, resultClock.AddMinutes(fixture.RoundNumber));

            if (fixture.Id == managedFixture.Id)
            {
                var managedSelection = fixture.HomeClubId == selectedClub.Id ? homeSelection : awaySelection;
                managedStarterIds = managedSelection.Starters
                    .Select(player => player.Id)
                    .ToHashSet();
                matchEvents.AddRange(simulation.Events);
            }
        }

        AdvanceAcademyDevelopment(leagueClubs);
        gameSave.Season.RefreshCurrentRound();
        await dbContext.SaveChangesAsync(cancellationToken);

        var leagueTable = LeagueTableCalculator.BuildTable(leagueClubs, gameSave.Season.Fixtures.ToList());
        var clubStanding = leagueTable.Single(entry => entry.ClubId == selectedClub.Id);
        var nextFixture = gameSave.Season.Fixtures
            .Where(fixture => !fixture.IsPlayed &&
                              (fixture.HomeClubId == selectedClub.Id || fixture.AwayClubId == selectedClub.Id))
            .OrderBy(fixture => fixture.RoundNumber)
            .ThenBy(fixture => fixture.ScheduledAt)
            .FirstOrDefault();

        matchEvents = matchEvents
            .OrderBy(matchEvent => matchEvent.Minute)
            .ThenBy(matchEvent => matchEvent.Type)
            .ToList();
        var seniorPlayerDevelopment = BuildSeniorPlayerDevelopment(selectedClub, seniorSnapshots, managedStarterIds);
        var academyDevelopment = BuildAcademyDevelopment(selectedClub, academySnapshots);

        return new SimulatedMatchResultDto(
            clubNames[managedFixture.HomeClubId],
            clubNames[managedFixture.AwayClubId],
            new MatchScoreDto(managedFixture.HomeGoals ?? 0, managedFixture.AwayGoals ?? 0),
            matchEvents,
            seniorPlayerDevelopment,
            academyDevelopment,
            clubStanding,
            nextFixture is null
                ? null
                : new NextFixtureDto(
                    clubNames[nextFixture.HomeClubId],
                    clubNames[nextFixture.AwayClubId],
                    nextFixture.ScheduledAt,
                    nextFixture.RoundNumber),
            BuildSummary(selectedClub, clubStanding, managedFixture));
    }

    private static TeamSelection BuildTeamSelection(
        Club club,
        Formation formation,
        IEnumerable<Guid>? preferredStarterIds)
    {
        var starters = preferredStarterIds is null
            ? LineupPlanner.SelectDefaultStarters(club, formation).ToList()
            : LineupPlanner.SelectPreferredStarters(club, formation, preferredStarterIds).ToList();
        var tacticalProfile = ResolveTacticalProfile(formation);

        return new TeamSelection(
            club,
            formation,
            tacticalProfile,
            starters,
            Average(starters.Select(player => player.GetOverallRating())),
            Average(starters.Select(player => player.Fitness)),
            Average(starters.Select(player => player.Morale)),
            starters.Average(player => (player.Attack * GetAttackWeight(player.Position)) + (player.Passing * 0.35)),
            starters.Average(player => (player.Defense * GetDefenseWeight(player.Position)) + (player.Passing * 0.15)));
    }

    private static FixtureSimulation SimulateFixture(TeamSelection home, TeamSelection away, bool captureEvents)
    {
        var homeExpectedGoals = CalculateExpectedGoals(home, away, isHome: true);
        var awayExpectedGoals = CalculateExpectedGoals(away, home, isHome: false);
        var homeGoals = SampleGoals(homeExpectedGoals);
        var awayGoals = SampleGoals(awayExpectedGoals);

        var events = new List<MatchEventDto>();
        var homeScorers = new Dictionary<Guid, int>();
        var awayScorers = new Dictionary<Guid, int>();
        var homeInjuries = new List<PlayerInjury>();
        var awayInjuries = new List<PlayerInjury>();

        if (captureEvents)
        {
            events.Add(
                new MatchEventDto(
                    1,
                    "Kickoff",
                    $"{home.Club.Name} open with a {home.Tactics.Style.ToLowerInvariant()} shape while {away.Club.Name} answer in a {away.Tactics.Style.ToLowerInvariant()} setup."));
            events.Add(BuildChanceEvent(home, away, homeExpectedGoals, awayExpectedGoals, homeGoals + awayGoals));
        }

        foreach (var goal in BuildGoalHighlights(home, homeGoals))
        {
            homeScorers[goal.Scorer.Id] = homeScorers.GetValueOrDefault(goal.Scorer.Id) + 1;

            if (captureEvents)
            {
                events.Add(new MatchEventDto(goal.Minute, "Goal", $"{home.Club.Name} score through {goal.Scorer.FullName}."));
            }
        }

        foreach (var goal in BuildGoalHighlights(away, awayGoals))
        {
            awayScorers[goal.Scorer.Id] = awayScorers.GetValueOrDefault(goal.Scorer.Id) + 1;

            if (captureEvents)
            {
                events.Add(new MatchEventDto(goal.Minute, "Goal", $"{away.Club.Name} hit back through {goal.Scorer.FullName}."));
            }
        }

        if (captureEvents && homeGoals == 0 && awayGoals == 0)
        {
            events.Add(new MatchEventDto(74, "Chance", $"{away.Club.Name} force one late save, but the breakthrough never comes."));
        }

        homeInjuries.AddRange(GenerateInjuries(home));
        awayInjuries.AddRange(GenerateInjuries(away));

        if (captureEvents)
        {
            events.Add(new MatchEventDto(90, "FullTime", $"Full time: {home.Club.Name} {homeGoals}-{awayGoals} {away.Club.Name}."));
        }

        return new FixtureSimulation(
            homeGoals,
            awayGoals,
            homeScorers,
            awayScorers,
            homeInjuries,
            awayInjuries,
            events);
    }

    private static MatchEventDto BuildChanceEvent(
        TeamSelection home,
        TeamSelection away,
        double homeExpectedGoals,
        double awayExpectedGoals,
        int totalGoals)
    {
        if (homeExpectedGoals >= awayExpectedGoals)
        {
            return new MatchEventDto(
                totalGoals > 2 ? 22 : 17,
                "Chance",
                $"{home.Club.Name} settle first and drag the game into their rhythm.");
        }

        return new MatchEventDto(
            totalGoals > 2 ? 24 : 19,
            "Chance",
            $"{away.Club.Name} break the first line and make the home side scramble.");
    }

    private static IReadOnlyCollection<GoalHighlight> BuildGoalHighlights(TeamSelection selection, int goalCount)
    {
        var highlights = new List<GoalHighlight>();
        var usedMinutes = new HashSet<int>();

        for (var index = 0; index < goalCount; index++)
        {
            highlights.Add(new GoalHighlight(
                NextUniqueMinute(usedMinutes, 8, 88),
                PickGoalScorer(selection)));
        }

        return highlights;
    }

    private static IReadOnlyCollection<PlayerInjury> GenerateInjuries(TeamSelection selection)
    {
        var injuries = new List<PlayerInjury>();
        var usedMinutes = new HashSet<int>();

        foreach (var player in selection.Starters)
        {
            var injuryChance = 0.008 + Math.Max(0d, (68 - player.Fitness) / 450d);
            if (Random.Shared.NextDouble() >= injuryChance)
            {
                continue;
            }

            injuries.Add(new PlayerInjury(
                player,
                NextUniqueMinute(usedMinutes, 18, 84),
                Random.Shared.Next(1, 4)));
        }

        return injuries;
    }

    private static void ApplyPostMatchState(
        TeamSelection selection,
        int goalsFor,
        int goalsAgainst,
        IReadOnlyDictionary<Guid, int> scorerTallies,
        IReadOnlyCollection<PlayerInjury> injuries,
        ICollection<MatchEventDto> managedFixtureEvents,
        bool captureEvents)
    {
        var injuryLookup = injuries.ToDictionary(injury => injury.Player.Id);
        var starterIds = selection.Starters.Select(player => player.Id).ToHashSet();
        var resultMoraleShift = goalsFor.CompareTo(goalsAgainst) switch
        {
            > 0 => 6,
            < 0 => -5,
            _ => 1
        };

        foreach (var player in selection.Starters)
        {
            var fitnessLoss = Random.Shared.Next(6, 11) + (player.Position is PlayerPosition.Midfielder or PlayerPosition.Forward ? 1 : 0);
            player.AdjustFitness(-fitnessLoss);
            player.AdjustMorale(resultMoraleShift + Math.Min(4, scorerTallies.GetValueOrDefault(player.Id) * 2));
            player.ApplyAgeBasedMatchDevelopment(playedMatch: true);

            if (!injuryLookup.TryGetValue(player.Id, out var injury))
            {
                continue;
            }

            player.RecordInjury(injury.MatchesToMiss);
            player.AdjustFitness(-Random.Shared.Next(4, 9));

            if (captureEvents)
            {
                managedFixtureEvents.Add(
                    new MatchEventDto(
                        injury.Minute,
                        "Injury",
                        $"{player.FullName} pulls up for {selection.Club.Name} and could miss {injury.MatchesToMiss} matchday(s)."));
            }
        }

        foreach (var player in selection.Club.Players.Where(player => !starterIds.Contains(player.Id)))
        {
            if (player.IsInjured)
            {
                player.RecoverFromMissedMatch();
                player.AdjustFitness(3);
            }
            else
            {
                player.AdjustFitness(2);
            }

            player.AdjustMorale(resultMoraleShift / 2);
            player.ApplyAgeBasedMatchDevelopment(playedMatch: false);
        }
    }

    private static double CalculateExpectedGoals(TeamSelection team, TeamSelection opponent, bool isHome)
    {
        var randomShift = (Random.Shared.NextDouble() * 0.36) - 0.18;

        var expectedGoals = 0.55
            + ((team.AttackRating / 100d) * 0.85)
            + ((team.TeamStrength / 100d) * 0.45)
            + ((team.AverageFitness / 100d) * 0.22)
            + ((team.AverageMorale / 100d) * 0.18)
            + team.Tactics.AttackBias
            + (isHome ? 0.18 : 0d)
            - ((opponent.DefenseRating / 100d) * 0.72)
            - opponent.Tactics.DefenseBias
            + randomShift;

        return Math.Clamp(expectedGoals, 0.15, 3.6);
    }

    private static int SampleGoals(double lambda)
    {
        var threshold = Math.Exp(-Math.Clamp(lambda, 0.05, 4.5));
        var product = 1d;
        var count = 0;

        do
        {
            count++;
            product *= Random.Shared.NextDouble();
        }
        while (product > threshold && count < 8);

        return Math.Min(count - 1, 6);
    }

    private static Player PickGoalScorer(TeamSelection selection)
    {
        var weightedPlayers = selection.Starters
            .Select(player => new
            {
                Player = player,
                Weight = (player.Attack * 1.25) + (player.Passing * 0.35) + GetGoalScorerPositionBonus(player.Position)
            })
            .ToList();
        var roll = Random.Shared.NextDouble() * weightedPlayers.Sum(entry => entry.Weight);
        var cursor = 0d;

        foreach (var entry in weightedPlayers)
        {
            cursor += entry.Weight;
            if (roll <= cursor)
            {
                return entry.Player;
            }
        }

        return weightedPlayers[^1].Player;
    }

    private static TacticalProfile ResolveTacticalProfile(Formation formation)
    {
        if (formation.Forwards >= 3)
        {
            return new TacticalProfile("Attacking", 0.22, -0.06);
        }

        if (formation.Forwards <= 1)
        {
            return new TacticalProfile("Defensive", -0.08, 0.18);
        }

        if (formation.Midfielders >= 5)
        {
            return new TacticalProfile("Balanced", 0.12, 0.08);
        }

        return new TacticalProfile("Balanced", 0.04, 0.04);
    }

    private static double GetAttackWeight(PlayerPosition position) =>
        position switch
        {
            PlayerPosition.Goalkeeper => 0.15,
            PlayerPosition.Defender => 0.55,
            PlayerPosition.Midfielder => 0.95,
            PlayerPosition.Forward => 1.30,
            _ => 0.75
        };

    private static double GetDefenseWeight(PlayerPosition position) =>
        position switch
        {
            PlayerPosition.Goalkeeper => 1.30,
            PlayerPosition.Defender => 1.15,
            PlayerPosition.Midfielder => 0.75,
            PlayerPosition.Forward => 0.35,
            _ => 0.75
        };

    private static double GetGoalScorerPositionBonus(PlayerPosition position) =>
        position switch
        {
            PlayerPosition.Forward => 42,
            PlayerPosition.Midfielder => 24,
            PlayerPosition.Defender => 9,
            _ => 3
        };

    private static int NextUniqueMinute(ISet<int> usedMinutes, int minInclusive, int maxInclusive)
    {
        var minute = Random.Shared.Next(minInclusive, maxInclusive + 1);

        while (!usedMinutes.Add(minute))
        {
            minute = minute == maxInclusive ? minInclusive : minute + 1;
        }

        return minute;
    }

    private static int Average(IEnumerable<int> values)
    {
        var materialized = values.ToList();
        return materialized.Count == 0
            ? 0
            : (int)Math.Round(materialized.Average(), MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyDictionary<Guid, PlayerSnapshot> CaptureSeniorSnapshots(IEnumerable<Player> players) =>
        players.ToDictionary(
            player => player.Id,
            player => new PlayerSnapshot(
                player.Attack,
                player.Defense,
                player.Passing,
                player.Fitness,
                player.Morale,
                player.GetOverallRating()));

    private static IReadOnlyDictionary<Guid, AcademySnapshot> CaptureAcademySnapshots(IEnumerable<AcademyPlayer> players) =>
        players.ToDictionary(
            player => player.Id,
            player => new AcademySnapshot(
                player.Attack,
                player.Defense,
                player.Passing,
                player.Fitness,
                player.Morale,
                player.DevelopmentProgress,
                player.GetOverallRating()));

    private static IReadOnlyCollection<PlayerDevelopmentChangeDto> BuildSeniorPlayerDevelopment(
        Club club,
        IReadOnlyDictionary<Guid, PlayerSnapshot> snapshots,
        IReadOnlySet<Guid> managedStarterIds)
    {
        return club.Players
            .OrderBy(player => managedStarterIds.Contains(player.Id) ? 0 : 1)
            .ThenBy(player => player.SquadNumber)
            .ThenBy(player => player.LastName)
            .ThenBy(player => player.FirstName)
            .Select(player =>
            {
                var snapshot = snapshots[player.Id];
                var overallDelta = CalculateSeniorReportOverallDelta(player, snapshot);
                var overallRating = Math.Clamp(snapshot.OverallRating + overallDelta, 1, 100);

                return new PlayerDevelopmentChangeDto(
                    player.Id,
                    player.FullName,
                    player.Position.ToString(),
                    player.Age,
                    player.SquadNumber,
                    player.IsCaptain,
                    managedStarterIds.Contains(player.Id),
                    overallRating,
                    overallDelta,
                    player.Attack,
                    player.Attack - snapshot.Attack,
                    player.Defense,
                    player.Defense - snapshot.Defense,
                    player.Passing,
                    player.Passing - snapshot.Passing,
                    player.Fitness,
                    player.Fitness - snapshot.Fitness,
                    player.Morale,
                    player.Morale - snapshot.Morale);
            })
            .ToList();
    }

    private static IReadOnlyCollection<AcademyDevelopmentChangeDto> BuildAcademyDevelopment(
        Club club,
        IReadOnlyDictionary<Guid, AcademySnapshot> snapshots)
    {
        return club.AcademyPlayers
            .OrderByDescending(player =>
            {
                var snapshot = snapshots[player.Id];
                return (player.DevelopmentProgress - snapshot.DevelopmentProgress) * 1000 +
                       (player.GetOverallRating() - snapshot.OverallRating);
            })
            .ThenByDescending(player => player.GetOverallRating())
            .ThenBy(player => player.LastName)
            .ThenBy(player => player.FirstName)
            .Select(player =>
            {
                var snapshot = snapshots[player.Id];
                var overallDelta = CalculateAcademyReportOverallDelta(player, snapshot);
                var overallRating = Math.Clamp(snapshot.OverallRating + overallDelta, 1, 100);

                return new AcademyDevelopmentChangeDto(
                    player.Id,
                    player.FullName,
                    player.Position.ToString(),
                    player.Age,
                    player.TrainingFocus,
                    overallRating,
                    overallDelta,
                    player.Attack,
                    player.Attack - snapshot.Attack,
                    player.Defense,
                    player.Defense - snapshot.Defense,
                    player.Passing,
                    player.Passing - snapshot.Passing,
                    player.Fitness,
                    player.Fitness - snapshot.Fitness,
                    player.Morale,
                    player.Morale - snapshot.Morale,
                    player.DevelopmentProgress,
                    player.DevelopmentProgress - snapshot.DevelopmentProgress);
            })
            .ToList();
    }

    private static void AdvanceAcademyDevelopment(IEnumerable<Club> clubs)
    {
        foreach (var academyPlayer in clubs.SelectMany(club => club.AcademyPlayers))
        {
            academyPlayer.AdvanceDevelopment();
        }
    }

    private static int CalculateSeniorReportOverallDelta(Player player, PlayerSnapshot snapshot)
    {
        var rawDelta = CalculateTechnicalDelta(
                           player.Position,
                           player.Attack - snapshot.Attack,
                           player.Defense - snapshot.Defense,
                           player.Passing - snapshot.Passing)
                       + ((player.Fitness - snapshot.Fitness) * 0.10d)
                       + ((player.Morale - snapshot.Morale) * 0.05d);

        return NormalizeReportDelta(
            rawDelta,
            player.Fitness - snapshot.Fitness,
            player.Attack - snapshot.Attack,
            player.Passing - snapshot.Passing,
            player.Defense - snapshot.Defense,
            player.Morale - snapshot.Morale);
    }

    private static int CalculateAcademyReportOverallDelta(AcademyPlayer player, AcademySnapshot snapshot)
    {
        var rawDelta = CalculateTechnicalDelta(
                           player.Position,
                           player.Attack - snapshot.Attack,
                           player.Defense - snapshot.Defense,
                           player.Passing - snapshot.Passing)
                       + ((player.Fitness - snapshot.Fitness) * 0.05d)
                       + ((player.Morale - snapshot.Morale) * 0.05d)
                       + ((player.DevelopmentProgress - snapshot.DevelopmentProgress) * 0.12d);

        return NormalizeReportDelta(
            rawDelta,
            player.DevelopmentProgress - snapshot.DevelopmentProgress,
            player.Attack - snapshot.Attack,
            player.Passing - snapshot.Passing,
            player.Defense - snapshot.Defense,
            player.Fitness - snapshot.Fitness,
            player.Morale - snapshot.Morale);
    }

    private static double CalculateTechnicalDelta(
        PlayerPosition position,
        int attackDelta,
        int defenseDelta,
        int passingDelta) =>
        position switch
        {
            PlayerPosition.Goalkeeper => (attackDelta * 0.05d) + (defenseDelta * 0.75d) + (passingDelta * 0.20d),
            PlayerPosition.Defender => (attackDelta * 0.15d) + (defenseDelta * 0.60d) + (passingDelta * 0.25d),
            PlayerPosition.Midfielder => (attackDelta * 0.30d) + (defenseDelta * 0.25d) + (passingDelta * 0.45d),
            PlayerPosition.Forward => (attackDelta * 0.60d) + (defenseDelta * 0.15d) + (passingDelta * 0.25d),
            _ => (attackDelta + defenseDelta + passingDelta) / 3d
        };

    private static int NormalizeReportDelta(double rawDelta, params int[] fallbackSignals)
    {
        var roundedDelta = (int)Math.Round(rawDelta, MidpointRounding.AwayFromZero);
        if (roundedDelta != 0)
        {
            return roundedDelta;
        }

        if (rawDelta > 0d)
        {
            return 1;
        }

        if (rawDelta < 0d)
        {
            return -1;
        }

        foreach (var signal in fallbackSignals)
        {
            if (signal != 0)
            {
                return Math.Sign(signal);
            }
        }

        return 0;
    }

    private static string BuildSummary(Club selectedClub, LeagueTableEntryDto clubStanding, Fixture managedFixture)
    {
        var goalsFor = managedFixture.HomeClubId == selectedClub.Id
            ? managedFixture.HomeGoals ?? 0
            : managedFixture.AwayGoals ?? 0;
        var goalsAgainst = managedFixture.HomeClubId == selectedClub.Id
            ? managedFixture.AwayGoals ?? 0
            : managedFixture.HomeGoals ?? 0;

        var resultLead = goalsFor.CompareTo(goalsAgainst) switch
        {
            > 0 => $"{selectedClub.Name} take the points",
            < 0 => $"{selectedClub.Name} are beaten",
            _ => $"{selectedClub.Name} grind out a draw"
        };

        return $"{resultLead} and move to {ToOrdinal(clubStanding.Position)} on {clubStanding.Points} point(s).";
    }

    private static string ToOrdinal(int number)
    {
        var value = number % 100;
        if (value is >= 11 and <= 13)
        {
            return $"{number}th";
        }

        return (number % 10) switch
        {
            1 => $"{number}st",
            2 => $"{number}nd",
            3 => $"{number}rd",
            _ => $"{number}th"
        };
    }

    private void ApplyMatchdayFinances(Club homeClub, Club awayClub, int roundNumber, DateTime occurredAt)
    {
        var matchIncome = FinanceCalculator.CalculateMatchIncome(homeClub, awayClub, roundNumber);
        var homeWageBill = FinanceCalculator.CalculateWageBill(homeClub.Players);
        var awayWageBill = FinanceCalculator.CalculateWageBill(awayClub.Players);

        homeClub.AdjustTransferBudget(matchIncome);
        homeClub.AdjustTransferBudget(-homeWageBill, allowNegative: true);
        awayClub.AdjustTransferBudget(-awayWageBill, allowNegative: true);

        dbContext.FinanceEntries.AddRange(
            new FinanceEntry(
                homeClub,
                FinanceEntryType.MatchIncome,
                matchIncome,
                $"{homeClub.Name} collected gate receipts for hosting {awayClub.Name}.",
                occurredAt),
            new FinanceEntry(
                homeClub,
                FinanceEntryType.WageExpense,
                homeWageBill,
                $"{homeClub.Name} covered the weekly wage bill for round {roundNumber}.",
                occurredAt.AddSeconds(1)),
            new FinanceEntry(
                awayClub,
                FinanceEntryType.WageExpense,
                awayWageBill,
                $"{awayClub.Name} covered the weekly wage bill for round {roundNumber}.",
                occurredAt.AddSeconds(2)));
    }

    private sealed record TeamSelection(
        Club Club,
        Formation Formation,
        TacticalProfile Tactics,
        IReadOnlyCollection<Player> Starters,
        int TeamStrength,
        int AverageFitness,
        int AverageMorale,
        double AttackRating,
        double DefenseRating);

    private sealed record TacticalProfile(string Style, double AttackBias, double DefenseBias);

    private sealed record GoalHighlight(int Minute, Player Scorer);

    private sealed record PlayerInjury(Player Player, int Minute, int MatchesToMiss);

    private sealed record PlayerSnapshot(
        int Attack,
        int Defense,
        int Passing,
        int Fitness,
        int Morale,
        int OverallRating);

    private sealed record AcademySnapshot(
        int Attack,
        int Defense,
        int Passing,
        int Fitness,
        int Morale,
        int DevelopmentProgress,
        int OverallRating);

    private sealed record FixtureSimulation(
        int HomeGoals,
        int AwayGoals,
        IReadOnlyDictionary<Guid, int> HomeScorers,
        IReadOnlyDictionary<Guid, int> AwayScorers,
        IReadOnlyCollection<PlayerInjury> HomeInjuries,
        IReadOnlyCollection<PlayerInjury> AwayInjuries,
        IReadOnlyCollection<MatchEventDto> Events);
}
