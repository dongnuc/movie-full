using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_StructureCode.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppRolePermissionMask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PermissionMask",
                table: "AspNetRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PermissionMask",
                table: "AspNetRoles");
        }
    }
}
