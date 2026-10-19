using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMentorProfileId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MentorPlans_MentorProfiles_MentorId",
                table: "MentorPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_MentorProfiles_MentorId",
                table: "Bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MentorProfiles",
                table: "MentorProfiles");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "MentorProfiles",
                type: "integer",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_MentorProfiles_AccountId",
                table: "MentorProfiles",
                column: "AccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MentorProfiles",
                table: "MentorProfiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MentorPlans_MentorProfiles_MentorId",
                table: "MentorPlans",
                column: "MentorId",
                principalTable: "MentorProfiles",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_MentorProfiles_MentorId",
                table: "Bookings",
                column: "MentorId",
                principalTable: "MentorProfiles",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MentorPlans_MentorProfiles_MentorId",
                table: "MentorPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_MentorProfiles_MentorId",
                table: "Bookings");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_MentorProfiles_AccountId",
                table: "MentorProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MentorProfiles",
                table: "MentorProfiles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MentorProfiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MentorProfiles",
                table: "MentorProfiles",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_MentorPlans_MentorProfiles_MentorId",
                table: "MentorPlans",
                column: "MentorId",
                principalTable: "MentorProfiles",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_MentorProfiles_MentorId",
                table: "Bookings",
                column: "MentorId",
                principalTable: "MentorProfiles",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
