using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase2SquadManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "attack",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "defense",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "fitness",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "morale",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "passing",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "formations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    defenders = table.Column<int>(type: "integer", nullable: false),
                    midfielders = table.Column<int>(type: "integer", nullable: false),
                    forwards = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lineups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_save_id = table.Column<Guid>(type: "uuid", nullable: false),
                    formation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    starter_player_ids = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lineups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lineups_formations_formation_id",
                        column: x => x.formation_id,
                        principalTable: "formations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lineups_game_saves_game_save_id",
                        column: x => x.game_save_id,
                        principalTable: "game_saves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_formations_name",
                table: "formations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lineups_formation_id",
                table: "lineups",
                column: "formation_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineups_game_save_id",
                table: "lineups",
                column: "game_save_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lineups");

            migrationBuilder.DropTable(
                name: "formations");

            migrationBuilder.DropColumn(
                name: "attack",
                table: "players");

            migrationBuilder.DropColumn(
                name: "defense",
                table: "players");

            migrationBuilder.DropColumn(
                name: "fitness",
                table: "players");

            migrationBuilder.DropColumn(
                name: "morale",
                table: "players");

            migrationBuilder.DropColumn(
                name: "passing",
                table: "players");
        }
    }
}
