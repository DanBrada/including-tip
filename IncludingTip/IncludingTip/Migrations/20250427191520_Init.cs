using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IncludingTip.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    iso_country_code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    tip_policy = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.iso_country_code);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "places",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    latitutde = table.Column<double>(type: "double precision", nullable: false),
                    country_code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_places", x => x.id);
                    table.ForeignKey(
                        name: "fk_places_countries_country_code",
                        column: x => x.country_code,
                        principalTable: "countries",
                        principalColumn: "iso_country_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "country_edits",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    country_iso_country_code = table.Column<string>(type: "text", nullable: true),
                    country_code = table.Column<string>(type: "text", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_country_edits", x => x.id);
                    table.ForeignKey(
                        name: "fk_country_edits_countries_country_iso_country_code",
                        column: x => x.country_iso_country_code,
                        principalTable: "countries",
                        principalColumn: "iso_country_code");
                    table.ForeignKey(
                        name: "fk_country_edits_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tips",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    percent = table.Column<int>(type: "integer", nullable: false),
                    experience = table.Column<string>(type: "text", nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tips", x => x.id);
                    table.ForeignKey(
                        name: "fk_tips_places_place_id",
                        column: x => x.place_id,
                        principalTable: "places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tips_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_country_edits_author_id",
                table: "country_edits",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_country_edits_country_iso_country_code",
                table: "country_edits",
                column: "country_iso_country_code");

            migrationBuilder.CreateIndex(
                name: "ix_places_country_code",
                table: "places",
                column: "country_code");

            migrationBuilder.CreateIndex(
                name: "ix_tips_author_id",
                table: "tips",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_tips_place_id",
                table: "tips",
                column: "place_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "country_edits");

            migrationBuilder.DropTable(
                name: "tips");

            migrationBuilder.DropTable(
                name: "places");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "countries");
        }
    }
}
