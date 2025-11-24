using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class SaatlerEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "CalismaBaslangic",
                table: "Antrenorler",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CalismaBitis",
                table: "Antrenorler",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalismaBaslangic",
                table: "Antrenorler");

            migrationBuilder.DropColumn(
                name: "CalismaBitis",
                table: "Antrenorler");
        }
    }
}
