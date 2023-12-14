using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.SQLight.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToMethodConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "ConfigType",
                table: "MethodConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigType",
                table: "MethodConfigurations");
        }
    }
}
