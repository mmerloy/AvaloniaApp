using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.SQLight.Migrations
{
    /// <inheritdoc />
    public partial class AddDefectToAnImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PredictedFromImageId",
                table: "Defects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImageEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullPath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageEntities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Defects_PredictedFromImageId",
                table: "Defects",
                column: "PredictedFromImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Defects_ImageEntities_PredictedFromImageId",
                table: "Defects",
                column: "PredictedFromImageId",
                principalTable: "ImageEntities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defects_ImageEntities_PredictedFromImageId",
                table: "Defects");

            migrationBuilder.DropTable(
                name: "ImageEntities");

            migrationBuilder.DropIndex(
                name: "IX_Defects_PredictedFromImageId",
                table: "Defects");

            migrationBuilder.DropColumn(
                name: "PredictedFromImageId",
                table: "Defects");
        }
    }
}
