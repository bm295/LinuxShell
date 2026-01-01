using System.Globalization;
using System.Text;
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
    private Label _subtitle = null!;
    private LineEdit _clubNameInput = null!;
    private Club _myClub = null!;

    public override void _Ready()
    {
        _simulator = new MatchSimulator();
        _output = GetNode<RichTextLabel>("Root/Card/Layout/OutputPanel/Output");
        _subtitle = GetNode<Label>("Root/Card/Layout/Subtitle");
        _clubNameInput = GetNode<LineEdit>("Root/Card/Layout/MyClubPanel/MyClubContent/RenameRow/ClubNameInput");

        GetNode<Button>("Root/Card/Layout/MenuPanel/Menu/SimulateMatchButton").Pressed += OnSimulateMatchPressed;
        GetNode<Button>("Root/Card/Layout/MenuPanel/Menu/LeagueTableButton").Pressed += OnLeagueTablePressed;
        GetNode<Button>("Root/Card/Layout/MenuPanel/Menu/ResetSeasonButton").Pressed += OnResetSeasonPressed;

        GetNode<Button>("Root/Card/Layout/MyClubPanel/MyClubContent/MyClubActions/MyClubInfoButton").Pressed += OnMyClubInfoPressed;
        GetNode<Button>("Root/Card/Layout/MyClubPanel/MyClubContent/MyClubActions/ImproveReputationButton").Pressed += OnImproveReputationPressed;
        GetNode<Button>("Root/Card/Layout/MyClubPanel/MyClubContent/MyClubActions/SponsorButton").Pressed += OnSponsorPressed;
        GetNode<Button>("Root/Card/Layout/MyClubPanel/MyClubContent/RenameRow/RenameClubButton").Pressed += OnRenameClubPressed;

        GetNode<Button>("Root/Card/Layout/ExitButton").Pressed += OnExitPressed;

        CreateNewLeague();
        ShowWelcome();
    }

    private void CreateNewLeague()
    {
        _league = League.Sample();
        _myClub = _league.Clubs[0];
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

    private void OnSimulateMatchPressed()
    {
        if (_league.Clubs.Count < 2)
        {
            AddOutput("Khong du CLB de mo phong tran dau.");
            return;
        }

        var result = _simulator.PlayMatch(_league.Clubs[0], _league.Clubs[1]);
        AddOutput($"Ket qua: {result.Home.Name} {result.HomeGoals} - {result.AwayGoals} {result.Away.Name}");
    }

    private void OnLeagueTablePressed()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Danh sach CLB:");
        for (var index = 0; index < _league.Clubs.Count; index++)
        {
            var club = _league.Clubs[index];
            builder.AppendLine($"{index + 1}. {club.Name} (Uy tin {club.Reputation}, Ngan sach {FormatCurrency(club.Budget)})");
        }

        AddOutput(builder.ToString().TrimEnd());
    }

    private void OnMyClubInfoPressed()
    {
        AddOutput($"CLB cua ban: {_myClub.Name} | Uy tin {_myClub.Reputation} | Ngan sach {FormatCurrency(_myClub.Budget)}");
    }

    private void OnImproveReputationPressed()
    {
        if (_myClub.Budget < ReputationBoostCost)
        {
            AddOutput("Ngan sach khong du de tang uy tin.");
            return;
        }

        _myClub = _myClub with
        {
            Budget = _myClub.Budget - ReputationBoostCost,
            Reputation = _myClub.Reputation + 1
        };

        AddOutput($"Da chi {FormatCurrency(ReputationBoostCost)} de tang uy tin. Uy tin hien tai: {_myClub.Reputation}.");
    }

    private void OnSponsorPressed()
    {
        _myClub = _myClub with
        {
            Budget = _myClub.Budget + SponsorBonus
        };

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

        _myClub = _myClub with { Name = newName };
        AddOutput($"Da doi ten CLB thanh: {_myClub.Name}.");
    }

    private void OnResetSeasonPressed()
    {
        CreateNewLeague();
        AddOutput("Da tao mua giai moi. Hay tiep tuc quan ly!");
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

    private static string FormatCurrency(int amount)
    {
        return amount.ToString("N0", CultureInfo.InvariantCulture);
    }
}
