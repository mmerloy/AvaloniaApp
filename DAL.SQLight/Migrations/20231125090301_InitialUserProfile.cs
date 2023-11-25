using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.SQLight.Migrations
{
    /// <inheritdoc />
    public partial class InitialUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MethodConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Inaccuracy = table.Column<double>(type: "REAL", nullable: false),
                    method_config_type = table.Column<string>(type: "TEXT", nullable: false),
                    InterpolationCount = table.Column<byte>(type: "INTEGER", nullable: true),
                    NotAllCoverage = table.Column<double>(type: "REAL", nullable: true),
                    Color = table.Column<bool>(type: "INTEGER", nullable: true),
                    ContrastRatio = table.Column<bool>(type: "INTEGER", nullable: true),
                    Brightness = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MethodConfigurationId = table.Column<int>(type: "INTEGER", nullable: false),
                    SearchObjectType = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersProfiles_MethodConfigurations_MethodConfigurationId",
                        column: x => x.MethodConfigurationId,
                        principalTable: "MethodConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersProfiles_MethodConfigurationId",
                table: "UsersProfiles",
                column: "MethodConfigurationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsersProfiles");

            migrationBuilder.DropTable(
                name: "MethodConfigurations");
        }
    }
}
