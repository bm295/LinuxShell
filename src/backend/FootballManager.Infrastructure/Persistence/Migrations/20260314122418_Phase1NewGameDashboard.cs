using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase1NewGameDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_template",
                table: "leagues",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "seasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    current_round = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    league_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_seasons_leagues_league_id",
                        column: x => x.league_id,
                        principalTable: "leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fixtures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    home_club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    away_club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_number = table.Column<int>(type: "integer", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_played = table.Column<bool>(type: "boolean", nullable: false),
                    home_goals = table.Column<int>(type: "integer", nullable: true),
                    away_goals = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fixtures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fixtures_clubs_away_club_id",
                        column: x => x.away_club_id,
                        principalTable: "clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fixtures_clubs_home_club_id",
                        column: x => x.home_club_id,
                        principalTable: "clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fixtures_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_saves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    selected_club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_saves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_game_saves_clubs_selected_club_id",
                        column: x => x.selected_club_id,
                        principalTable: "clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_saves_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fixtures_away_club_id",
                table: "fixtures",
                column: "away_club_id");

            migrationBuilder.CreateIndex(
                name: "IX_fixtures_home_club_id",
                table: "fixtures",
                column: "home_club_id");

            migrationBuilder.CreateIndex(
                name: "IX_fixtures_season_id_round_number_home_club_id_away_club_id",
                table: "fixtures",
                columns: new[] { "season_id", "round_number", "home_club_id", "away_club_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_saves_season_id",
                table: "game_saves",
                column: "season_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_saves_selected_club_id",
                table: "game_saves",
                column: "selected_club_id");

            migrationBuilder.CreateIndex(
                name: "IX_seasons_league_id_name",
                table: "seasons",
                columns: new[] { "league_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fixtures");

            migrationBuilder.DropTable(
                name: "game_saves");

            migrationBuilder.DropTable(
                name: "seasons");

            migrationBuilder.DropColumn(
                name: "is_template",
                table: "leagues");
        }
    }
}
