using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfe.PlanTech.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UndoLastChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_PageContents_ContentComponentDbEntity_ContentComponentId",
                schema: "Contentful",
                table: "PageContents");

            migrationBuilder.DropForeignKey(
                name: "FK_Pages_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Pages");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Titles_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Titles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentComponentDbEntity",
                table: "ContentComponentDbEntity");

            migrationBuilder.RenameTable(
                name: "ContentComponentDbEntity",
                newName: "ContentComponents",
                newSchema: "Contentful");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentComponents",
                schema: "Contentful",
                table: "ContentComponents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_ContentComponents_Id",
                schema: "Contentful",
                table: "Answers",
                column: "Id",
                principalSchema: "Contentful",
                principalTable: "ContentComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageContents_ContentComponents_ContentComponentId",
                schema: "Contentful",
                table: "PageContents",
                column: "ContentComponentId",
                principalSchema: "Contentful",
                principalTable: "ContentComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_ContentComponents_Id",
                schema: "Contentful",
                table: "Pages",
                column: "Id",
                principalSchema: "Contentful",
                principalTable: "ContentComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_ContentComponents_Id",
                schema: "Contentful",
                table: "Questions",
                column: "Id",
                principalSchema: "Contentful",
                principalTable: "ContentComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Titles_ContentComponents_Id",
                schema: "Contentful",
                table: "Titles",
                column: "Id",
                principalSchema: "Contentful",
                principalTable: "ContentComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_ContentComponents_Id",
                schema: "Contentful",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_PageContents_ContentComponents_ContentComponentId",
                schema: "Contentful",
                table: "PageContents");

            migrationBuilder.DropForeignKey(
                name: "FK_Pages_ContentComponents_Id",
                schema: "Contentful",
                table: "Pages");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_ContentComponents_Id",
                schema: "Contentful",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Titles_ContentComponents_Id",
                schema: "Contentful",
                table: "Titles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentComponents",
                schema: "Contentful",
                table: "ContentComponents");

            migrationBuilder.RenameTable(
                name: "ContentComponents",
                schema: "Contentful",
                newName: "ContentComponentDbEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentComponentDbEntity",
                table: "ContentComponentDbEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Answers",
                column: "Id",
                principalTable: "ContentComponentDbEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageContents_ContentComponentDbEntity_ContentComponentId",
                schema: "Contentful",
                table: "PageContents",
                column: "ContentComponentId",
                principalTable: "ContentComponentDbEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Pages",
                column: "Id",
                principalTable: "ContentComponentDbEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Questions",
                column: "Id",
                principalTable: "ContentComponentDbEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Titles_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Titles",
                column: "Id",
                principalTable: "ContentComponentDbEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
