using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Godot;
using FootballManagerSim.Models;
using FootballManagerSim.Simulation;

namespace FootballManagerSim;

public partial class Main : Control
{
    private const int ReputationBoostCost = 200_000;
    private const int SponsorBonus = 500_000;

    private League _league = null!;
    private MatchSimulator _simulator = null!;
    private RichTextLabel _output = null!;
    private Control _outputPanel = null!;
    private Label _subtitle = null!;
    private LineEdit _clubNameInput = null!;
    private Club _myClub = null!;
    private int _myClubIndex;
    private List<Club> _clubs = new();
    private Control _startMenuPanel = null!;
    private Label _startMenuStatus = null!;
    private Control _mainLayout = null!;
    private Control _myClubPanel = null!;
    private FileDialog _saveDialog = null!;
    private FileDialog _loadDialog = null!;
    private List<ClubStanding> _standings = new();
    private int _currentRound;
    private readonly Random _random = new();

    private static readonly JsonSerializerOptions SaveOptions = new()
    {
        WriteIndented = true
    };

    public override void _Ready()
    {
        _simulator = new MatchSimulator();
        _output = GetNode<RichTextLabel>("Root/Card/Layout/OutputPanel/Output");
        _outputPanel = GetNode<Control>("Root/Card/Layout/OutputPanel");
        _subtitle = GetNode<Label>("Root/Card/Layout/Subtitle");
        _clubNameInput = GetNode<LineEdit>("Root/Card/Layout/MyClubPanel/MyClubContent/RenameRow/ClubNameInput");
        _startMenuPanel = GetNode<Control>("Root/Card/StartMenuPanel");
        _startMenuStatus = GetNode<Label>("Root/Card/StartMenuPanel/StartMenu/StatusLabel");
        _mainLayout = GetNode<Control>("Root/Card/Layout");
        _myClubPanel = GetNode<Control>("Root/Card/Layout/MyClubPanel");

        _saveDialog = BuildSaveDialog();
        _loadDialog = BuildLoadDialog();
        AddChild(_saveDialog);
        AddChild(_loadDialog);

        GetNode<Button>("Root/Card/StartMenuPanel/StartMenu/NewGameButton").Pressed += OnNewGamePressed;
        GetNode<Button>("Root/Card/StartMenuPanel/StartMenu/LoadGameButton").Pressed += OnLoadGamePressed;

        GetNode<Button>("Root/Card/Layout/MenuPanel/Menu/SimulateMatchButton").Pressed += OnSimulateMatchPressed;
        GetNode<Button>("Root/Card/Layout/MenuPanel/Menu/LeagueTableButton").Pressed += OnLeagueTablePressed;
        GetNode<Button>("Root/Card/Layout/MenuPanel/Menu/MyClubButton").Pressed += OnMyClubPressed;
        GetNode<Button>("Root/Card/Layout/MenuPanel/Menu/SaveGameButton").Pressed += OnSaveGamePressed;

        GetNode<Button>("Root/Card/Layout/MyClubPanel/MyClubContent/MyClubActions/MyClubInfoButton").Pressed += OnMyClubInfoPressed;
        GetNode<Button>("Root/Card/Layout/MyClubPanel/MyClubContent/MyClubActions/ImproveReputationButton").Pressed += OnImproveReputationPressed;
        GetNode<Button>("Root/Card/Layout/MyClubPanel/MyClubContent/MyClubActions/SponsorButton").Pressed += OnSponsorPressed;
        GetNode<Button>("Root/Card/Layout/MyClubPanel/MyClubContent/RenameRow/RenameClubButton").Pressed += OnRenameClubPressed;

        GetNode<Button>("Root/Card/Layout/ExitButton").Pressed += OnExitPressed;

        ShowStartMenu();
    }

    private void CreateNewLeague()
    {
        var sample = League.Sample();
        _clubs = sample.Clubs.ToList();
        _league = new League(sample.Name, _clubs);
        _myClubIndex = 0;
        _myClub = _clubs[_myClubIndex];
        _standings = InitializeStandings(_clubs.Count);
        _currentRound = 0;
        _subtitle.Text = $"League: {_league.Name}";
        _output.Text = string.Empty;
        _clubNameInput.Text = _myClub.Name;
    }

    private void ShowWelcome()
    {
        AddOutput($"Chao mung den voi {_league.Name}!");
        AddOutput($"Ban dang quan ly CLB: {_myClub.Name}.");
        AddOutput("Chon mot tuy chon de bat dau quan ly doi bong.");
    }

    private void ShowStartMenu()
    {
        _startMenuStatus.Text = "Chon New Game hoac Load Game.";
        _startMenuPanel.Visible = true;
        _mainLayout.Visible = false;
    }

    private void StartGameSession()
    {
        _startMenuPanel.Visible = false;
        _mainLayout.Visible = true;
        ShowMyClubScreen(false);
    }

    private void OnNewGamePressed()
    {
        CreateNewLeague();
        StartGameSession();
        ShowWelcome();
    }

    private void OnLoadGamePressed()
    {
        _loadDialog.PopupCentered();
    }

    private void OnSimulateMatchPressed()
    {
        ClearOutput();
        if (_clubs.Count < 2)
        {
            AddOutput("Khong du CLB de mo phong tran dau.");
            return;
        }

        ShowMyClubScreen(false);
        var results = SimulateRound();
        var builder = new StringBuilder();
        builder.AppendLine($"Vong {_currentRound}:");
        foreach (var result in results)
        {
            builder.AppendLine($"{result.Home.Name} {result.HomeGoals} - {result.AwayGoals} {result.Away.Name}");
        }

        AddOutput(builder.ToString().TrimEnd());
    }

    private void OnLeagueTablePressed()
    {
        ClearOutput();
        ShowMyClubScreen(false);
        var builder = new StringBuilder();
        builder.AppendLine($"Bang xep hang (Vong {_currentRound}):");
        builder.AppendLine(FormatStandingsHeader());
        builder.AppendLine(FormatStandingsDivider());
        var standings = GetSortedStandings();
        for (var index = 0; index < standings.Count; index++)
        {
            var entry = standings[index];
            var club = _clubs[entry.Index];
            var goalDiff = entry.Standing.GoalsFor - entry.Standing.GoalsAgainst;
            builder.AppendLine(FormatStandingsRow(
                index + 1,
                club.Name,
                entry.Standing.Played,
                entry.Standing.Wins,
                entry.Standing.Draws,
                entry.Standing.Losses,
                goalDiff,
                entry.Standing.Points));
        }

        AddOutput(builder.ToString().TrimEnd());
    }

    private void OnMyClubPressed()
    {
        ClearOutput();
        ShowMyClubScreen(!_myClubPanel.Visible);
    }

    private void OnMyClubInfoPressed()
    {
        AddOutput($"CLB cua ban: {_myClub.Name} | Uy tin {_myClub.Reputation} | Ngan sach {FormatCurrency(_myClub.Budget)}");
        if (!string.IsNullOrWhiteSpace(_myClub.Formation))
        {
            AddOutput($"So do: {_myClub.Formation}");
        }

        if (_myClub.Lineup.Count > 0)
        {
            AddOutput("Doi hinh mac dinh:\n" + string.Join("\n", _myClub.Lineup));
        }

        if (_myClub.Squad.Count > 0)
        {
            var players = _myClub.Squad.Select(player => $"{player.Name} ({player.Position}, {player.Rating})");
            AddOutput("Danh sach cau thu:\n" + string.Join("\n", players));
        }
    }

    private void OnImproveReputationPressed()
    {
        if (_myClub.Budget < ReputationBoostCost)
        {
            AddOutput("Ngan sach khong du de tang uy tin.");
            return;
        }

        UpdateMyClub(_myClub with
        {
            Budget = _myClub.Budget - ReputationBoostCost,
            Reputation = _myClub.Reputation + 1
        });

        AddOutput($"Da chi {FormatCurrency(ReputationBoostCost)} de tang uy tin. Uy tin hien tai: {_myClub.Reputation}.");
    }

    private void OnSponsorPressed()
    {
        UpdateMyClub(_myClub with
        {
            Budget = _myClub.Budget + SponsorBonus
        });

        AddOutput($"Ky hop dong tai tro thanh cong! Ngan sach moi: {FormatCurrency(_myClub.Budget)}.");
    }

    private void OnRenameClubPressed()
    {
        var newName = _clubNameInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            AddOutput("Vui long nhap ten CLB moi.");
            return;
        }

        UpdateMyClub(_myClub with { Name = newName });
        AddOutput($"Da doi ten CLB thanh: {_myClub.Name}.");
    }

    private void OnSaveGamePressed()
    {
        _saveDialog.PopupCentered();
    }

    private void OnExitPressed()
    {
        GetTree().Quit();
    }

    private void AddOutput(string message)
    {
        if (!string.IsNullOrWhiteSpace(_output.Text))
        {
            _output.Text += "\n\n";
        }

        _output.Text += message;
        _output.ScrollToLine(_output.GetLineCount());
    }

    private void ClearOutput()
    {
        _output.Text = string.Empty;
    }

    private bool TryLoadGame(string savePath, out string message)
    {
        if (!File.Exists(savePath))
        {
            message = "Chua co file save. Hay tao game moi va luu game truoc.";
            return false;
        }

        try
        {
            var json = File.ReadAllText(savePath);
            var data = JsonSerializer.Deserialize<SaveGameData>(json, SaveOptions);
            if (data == null || data.Clubs.Count == 0)
            {
                message = "File save khong hop le.";
                return false;
            }

            var clubs = data.Clubs.Select(ToClub).ToList();
            _clubs = clubs;
            _league = new League(data.LeagueName, _clubs);
            _myClubIndex = 0;
            _myClub = _clubs[_myClubIndex];
            _standings = LoadStandings(data, clubs.Count);
            _currentRound = data.CurrentRound;
            _subtitle.Text = $"League: {_league.Name}";
            _output.Text = string.Empty;
            _clubNameInput.Text = _myClub.Name;
            message = "Da load game thanh cong.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Khong the load game: {ex.Message}";
            return false;
        }
    }

    private FileDialog BuildSaveDialog()
    {
        var dialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Access = FileDialog.AccessEnum.Filesystem,
            Title = "Chon thu muc luu game",
            CurrentFile = "savegame.json",
            UseNativeDialog = true
        };
        dialog.Filters = new[] { "*.json ; Save game" };
        dialog.FileSelected += OnSaveFileSelected;
        return dialog;
    }

    private FileDialog BuildLoadDialog()
    {
        var dialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Filesystem,
            Title = "Chon file load game",
            UseNativeDialog = true
        };
        dialog.Filters = new[] { "*.json ; Save game" };
        dialog.FileSelected += OnLoadFileSelected;
        return dialog;
    }

    private void OnSaveFileSelected(string path)
    {
        var data = new SaveGameData
        {
            LeagueName = _league.Name,
            Clubs = _clubs.Select(ToClubData).ToList(),
            CurrentRound = _currentRound,
            Standings = _standings.Select(ToStandingData).ToList()
        };

        var json = JsonSerializer.Serialize(data, SaveOptions);
        File.WriteAllText(path, json);
        AddOutput($"Da luu game vao: {path}");
    }

    private void OnLoadFileSelected(string path)
    {
        if (!TryLoadGame(path, out var message))
        {
            _startMenuStatus.Text = message;
            return;
        }

        StartGameSession();
        AddOutput(message);
    }

    private static ClubData ToClubData(Club club)
    {
        return new ClubData
        {
            Name = club.Name,
            Reputation = club.Reputation,
            Budget = club.Budget,
            Formation = club.Formation,
            Squad = club.Squad.ToList(),
            Lineup = club.Lineup.ToList()
        };
    }

    private static Club ToClub(ClubData data)
    {
        return new Club(
            data.Name,
            data.Reputation,
            data.Budget,
            data.Formation,
            data.Squad,
            data.Lineup);
    }

    private static string FormatCurrency(int amount)
    {
        return amount.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static string FormatStandingsHeader()
    {
        return $"{Pad("Hang", 4)} | {Pad("CLB", 20)} | {Pad("Tran", 4)} | {Pad("Thang", 5)} | {Pad("Hoa", 3)} | {Pad("Thua", 4)} | {Pad("HS", 3)} | {Pad("Diem", 4)}";
    }

    private static string FormatStandingsDivider()
    {
        return $"{new string('-', 4)}-+-{new string('-', 20)}-+-{new string('-', 4)}-+-{new string('-', 5)}-+-{new string('-', 3)}-+-{new string('-', 4)}-+-{new string('-', 3)}-+-{new string('-', 4)}";
    }

    private static string FormatStandingsRow(
        int rank,
        string clubName,
        int played,
        int wins,
        int draws,
        int losses,
        int goalDiff,
        int points)
    {
        return $"{Pad(rank.ToString(), 4)} | {Pad(clubName, 20)} | {Pad(played.ToString(), 4)} | {Pad(wins.ToString(), 5)} | {Pad(draws.ToString(), 3)} | {Pad(losses.ToString(), 4)} | {Pad(goalDiff.ToString(), 3)} | {Pad(points.ToString(), 4)}";
    }

    private static string Pad(string value, int width)
    {
        if (value.Length >= width)
        {
            return value[..width];
        }

        return value.PadRight(width);
    }

    private void UpdateMyClub(Club updatedClub)
    {
        _myClub = updatedClub;
        _clubs[_myClubIndex] = updatedClub;
        _clubNameInput.Text = updatedClub.Name;
    }

    private static List<ClubStanding> InitializeStandings(int count)
    {
        var standings = new List<ClubStanding>(count);
        for (var i = 0; i < count; i++)
        {
            standings.Add(new ClubStanding());
        }

        return standings;
    }

    private List<MatchResult> SimulateRound()
    {
        var matches = new List<MatchResult>();
        var shuffled = _clubs.OrderBy(_ => _random.Next()).ToList();
        for (var i = 0; i + 1 < shuffled.Count; i += 2)
        {
            var home = shuffled[i];
            var away = shuffled[i + 1];
            var result = _simulator.PlayMatch(home, away);
            matches.Add(result);
            UpdateStandings(result);
        }

        _currentRound += 1;
        return matches;
    }

    private void UpdateStandings(MatchResult result)
    {
        var homeIndex = _clubs.IndexOf(result.Home);
        var awayIndex = _clubs.IndexOf(result.Away);
        if (homeIndex < 0 || awayIndex < 0)
        {
            return;
        }

        var homeStanding = _standings[homeIndex];
        var awayStanding = _standings[awayIndex];

        homeStanding.Played += 1;
        awayStanding.Played += 1;
        homeStanding.GoalsFor += result.HomeGoals;
        homeStanding.GoalsAgainst += result.AwayGoals;
        awayStanding.GoalsFor += result.AwayGoals;
        awayStanding.GoalsAgainst += result.HomeGoals;

        if (result.HomeGoals > result.AwayGoals)
        {
            homeStanding.Wins += 1;
            awayStanding.Losses += 1;
        }
        else if (result.HomeGoals < result.AwayGoals)
        {
            awayStanding.Wins += 1;
            homeStanding.Losses += 1;
        }
        else
        {
            homeStanding.Draws += 1;
            awayStanding.Draws += 1;
        }
    }

    private List<(int Index, ClubStanding Standing)> GetSortedStandings()
    {
        return _standings
            .Select((standing, index) => (Index: index, Standing: standing))
            .OrderByDescending(entry => entry.Standing.Points)
            .ThenByDescending(entry => entry.Standing.GoalsFor - entry.Standing.GoalsAgainst)
            .ThenByDescending(entry => entry.Standing.GoalsFor)
            .ToList();
    }

    private void ShowMyClubScreen(bool showMyClub)
    {
        _myClubPanel.Visible = showMyClub;
        _outputPanel.Visible = true;
    }

    private List<ClubStanding> LoadStandings(SaveGameData data, int clubCount)
    {
        if (data.Standings.Count != clubCount)
        {
            return InitializeStandings(clubCount);
        }

        return data.Standings.Select(FromStandingData).ToList();
    }

    private static ClubStandingData ToStandingData(ClubStanding standing)
    {
        return new ClubStandingData
        {
            Played = standing.Played,
            Wins = standing.Wins,
            Draws = standing.Draws,
            Losses = standing.Losses,
            GoalsFor = standing.GoalsFor,
            GoalsAgainst = standing.GoalsAgainst
        };
    }

    private static ClubStanding FromStandingData(ClubStandingData data)
    {
        return new ClubStanding
        {
            Played = data.Played,
            Wins = data.Wins,
            Draws = data.Draws,
            Losses = data.Losses,
            GoalsFor = data.GoalsFor,
            GoalsAgainst = data.GoalsAgainst
        };
    }

    private sealed class ClubStanding
    {
        public int Played { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points => Wins * 3 + Draws;
    }
}
