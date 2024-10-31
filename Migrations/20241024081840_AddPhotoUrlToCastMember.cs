using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabProject.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoUrlToCastMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "CastMember",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "CastMember");
        }
    }

}
