using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfe.PlanTech.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CategoriesSectionsRecommendationPagesAndWarningComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectionDbEntityId",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryDbEntityId",
                schema: "Contentful",
                table: "ContentComponents",
                type: "nvarchar(30)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    HeaderId = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_ContentComponents_Id",
                        column: x => x.Id,
                        principalSchema: "Contentful",
                        principalTable: "ContentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Categories_Headers_HeaderId",
                        column: x => x.HeaderId,
                        principalSchema: "Contentful",
                        principalTable: "Headers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Warnings",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TextId = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warnings_ContentComponents_Id",
                        column: x => x.Id,
                        principalSchema: "Contentful",
                        principalTable: "ContentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Warnings_TextBodies_TextId",
                        column: x => x.TextId,
                        principalSchema: "Contentful",
                        principalTable: "TextBodies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterstitialPageId = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    CategoryDbEntityId1 = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Categories_CategoryDbEntityId1",
                        column: x => x.CategoryDbEntityId1,
                        principalSchema: "Contentful",
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sections_ContentComponents_Id",
                        column: x => x.Id,
                        principalSchema: "Contentful",
                        principalTable: "ContentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sections_Pages_InterstitialPageId",
                        column: x => x.InterstitialPageId,
                        principalSchema: "Contentful",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationPages",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InternalName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Maturity = table.Column<int>(type: "int", nullable: false),
                    PageId = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    SectionDbEntityId = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationPages_ContentComponents_Id",
                        column: x => x.Id,
                        principalSchema: "Contentful",
                        principalTable: "ContentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendationPages_Pages_PageId",
                        column: x => x.PageId,
                        principalSchema: "Contentful",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendationPages_Sections_SectionDbEntityId",
                        column: x => x.SectionDbEntityId,
                        principalSchema: "Contentful",
                        principalTable: "Sections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SectionDbEntityId",
                schema: "Contentful",
                table: "Questions",
                column: "SectionDbEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentComponents_CategoryDbEntityId",
                schema: "Contentful",
                table: "ContentComponents",
                column: "CategoryDbEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_HeaderId",
                schema: "Contentful",
                table: "Categories",
                column: "HeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationPages_PageId",
                schema: "Contentful",
                table: "RecommendationPages",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationPages_SectionDbEntityId",
                schema: "Contentful",
                table: "RecommendationPages",
                column: "SectionDbEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_CategoryDbEntityId1",
                schema: "Contentful",
                table: "Sections",
                column: "CategoryDbEntityId1");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_InterstitialPageId",
                schema: "Contentful",
                table: "Sections",
                column: "InterstitialPageId");

            migrationBuilder.CreateIndex(
                name: "IX_Warnings_TextId",
                schema: "Contentful",
                table: "Warnings",
                column: "TextId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentComponents_Categories_CategoryDbEntityId",
                schema: "Contentful",
                table: "ContentComponents",
                column: "CategoryDbEntityId",
                principalSchema: "Contentful",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Sections_SectionDbEntityId",
                schema: "Contentful",
                table: "Questions",
                column: "SectionDbEntityId",
                principalSchema: "Contentful",
                principalTable: "Sections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentComponents_Categories_CategoryDbEntityId",
                schema: "Contentful",
                table: "ContentComponents");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Sections_SectionDbEntityId",
                schema: "Contentful",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "RecommendationPages",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Warnings",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Sections",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "Contentful");

            migrationBuilder.DropIndex(
                name: "IX_Questions_SectionDbEntityId",
                schema: "Contentful",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_ContentComponents_CategoryDbEntityId",
                schema: "Contentful",
                table: "ContentComponents");

            migrationBuilder.DropColumn(
                name: "SectionDbEntityId",
                schema: "Contentful",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CategoryDbEntityId",
                schema: "Contentful",
                table: "ContentComponents");
        }
    }
}
