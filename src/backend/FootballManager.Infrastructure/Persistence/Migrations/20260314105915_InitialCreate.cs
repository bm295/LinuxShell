using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leagues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    transfer_budget = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    league_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clubs_leagues_league_id",
                        column: x => x.league_id,
                        principalTable: "leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    last_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    position = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    squad_number = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    club_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_players_clubs_club_id",
                        column: x => x.club_id,
                        principalTable: "clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clubs_league_id_name",
                table: "clubs",
                columns: new[] { "league_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_club_id_squad_number",
                table: "players",
                columns: new[] { "club_id", "squad_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "players");

            migrationBuilder.DropTable(
                name: "clubs");

            migrationBuilder.DropTable(
                name: "leagues");
        }
    }
}
