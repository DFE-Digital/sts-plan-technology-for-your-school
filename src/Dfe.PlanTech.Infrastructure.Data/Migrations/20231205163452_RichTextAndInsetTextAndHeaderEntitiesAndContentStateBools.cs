using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfe.PlanTech.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RichTextAndInsetTextAndHeaderEntitiesAndContentStateBools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Archived",
                schema: "Contentful",
                table: "ContentComponents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                schema: "Contentful",
                table: "ContentComponents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Published",
                schema: "Contentful",
                table: "ContentComponents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Headers",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tag = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Headers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsetTexts",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsetTexts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RichTextDataDbEntity",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RichTextDataDbEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RichTextDataDbEntity_ContentComponents_Id",
                        column: x => x.Id,
                        principalSchema: "Contentful",
                        principalTable: "ContentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RichTextContents",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NodeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataId = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    RichTextContentDbEntityId = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RichTextContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RichTextContents_ContentComponents_Id",
                        column: x => x.Id,
                        principalSchema: "Contentful",
                        principalTable: "ContentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RichTextContents_RichTextContents_RichTextContentDbEntityId",
                        column: x => x.RichTextContentDbEntityId,
                        principalSchema: "Contentful",
                        principalTable: "RichTextContents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RichTextContents_RichTextDataDbEntity_DataId",
                        column: x => x.DataId,
                        principalSchema: "Contentful",
                        principalTable: "RichTextDataDbEntity",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RichTextMarkDbEntity",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RichTextContentDbEntityId = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RichTextMarkDbEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RichTextMarkDbEntity_ContentComponents_Id",
                        column: x => x.Id,
                        principalSchema: "Contentful",
                        principalTable: "ContentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RichTextMarkDbEntity_RichTextContents_RichTextContentDbEntityId",
                        column: x => x.RichTextContentDbEntityId,
                        principalSchema: "Contentful",
                        principalTable: "RichTextContents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RichTextContents_DataId",
                schema: "Contentful",
                table: "RichTextContents",
                column: "DataId");

            migrationBuilder.CreateIndex(
                name: "IX_RichTextContents_RichTextContentDbEntityId",
                schema: "Contentful",
                table: "RichTextContents",
                column: "RichTextContentDbEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_RichTextMarkDbEntity_RichTextContentDbEntityId",
                schema: "Contentful",
                table: "RichTextMarkDbEntity",
                column: "RichTextContentDbEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Headers",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "InsetTexts",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "RichTextMarkDbEntity",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "RichTextContents",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "RichTextDataDbEntity",
                schema: "Contentful");

            migrationBuilder.DropColumn(
                name: "Archived",
                schema: "Contentful",
                table: "ContentComponents");

            migrationBuilder.DropColumn(
                name: "Deleted",
                schema: "Contentful",
                table: "ContentComponents");

            migrationBuilder.DropColumn(
                name: "Published",
                schema: "Contentful",
                table: "ContentComponents");
        }
    }
}
