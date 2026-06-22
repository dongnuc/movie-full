using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_StructureCode.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieBannerPosterIsHomePage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageBanner",
                table: "Movie",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePoster",
                table: "Movie",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHomePage",
                table: "Movie",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBanner",
                table: "Movie");

            migrationBuilder.DropColumn(
                name: "ImagePoster",
                table: "Movie");

            migrationBuilder.DropColumn(
                name: "IsHomePage",
                table: "Movie");
        }
    }
}
