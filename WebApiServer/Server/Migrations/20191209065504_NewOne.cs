using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class NewOne : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blobs_Answers_DataId",
                table: "Blobs");

            migrationBuilder.DropIndex(
                name: "IX_Blobs_DataId",
                table: "Blobs");

            migrationBuilder.DropColumn(
                name: "DataId",
                table: "Blobs");

            migrationBuilder.AddColumn<int>(
                name: "BlobId",
                table: "Answers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Answers_BlobId",
                table: "Answers",
                column: "BlobId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Blobs_BlobId",
                table: "Answers",
                column: "BlobId",
                principalTable: "Blobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Blobs_BlobId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_BlobId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "BlobId",
                table: "Answers");

            migrationBuilder.AddColumn<int>(
                name: "DataId",
                table: "Blobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_DataId",
                table: "Blobs",
                column: "DataId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Blobs_Answers_DataId",
                table: "Blobs",
                column: "DataId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
