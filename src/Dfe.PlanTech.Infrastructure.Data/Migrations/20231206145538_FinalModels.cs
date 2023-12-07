using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfe.PlanTech.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_NextQuestionId",
                schema: "Contentful",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_ParentQuestionId",
                schema: "Contentful",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Pages_Titles_TitleId",
                schema: "Contentful",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "OrganisationName",
                schema: "Contentful",
                table: "Pages");

            migrationBuilder.RenameColumn(
                name: "SectionTitle",
                schema: "Contentful",
                table: "Pages",
                newName: "SectionId");

            migrationBuilder.AddColumn<string>(
                name: "SectionId",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(30)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Buttons",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsStartButton = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buttons", x => x.Id);
                });

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
                name: "ButtonWithEntryReferences",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ButtonId = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    LinkToEntryId = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ButtonWithEntryReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ButtonWithEntryReferences_Buttons_ButtonId",
                        column: x => x.ButtonId,
                        principalSchema: "Contentful",
                        principalTable: "Buttons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ButtonWithEntryReferences_ContentComponents_LinkToEntryId",
                        column: x => x.LinkToEntryId,
                        principalSchema: "Contentful",
                        principalTable: "ContentComponents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ButtonWithLinks",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ButtonId = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    Href = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ButtonWithLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ButtonWithLinks_Buttons_ButtonId",
                        column: x => x.ButtonId,
                        principalSchema: "Contentful",
                        principalTable: "Buttons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterstitialPageId = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Contentful",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        onDelete: ReferentialAction.Restrict);
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
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecommendationPages_Sections_SectionDbEntityId",
                        column: x => x.SectionDbEntityId,
                        principalSchema: "Contentful",
                        principalTable: "Sections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SectionId",
                schema: "Contentful",
                table: "Questions",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ButtonWithEntryReferences_ButtonId",
                schema: "Contentful",
                table: "ButtonWithEntryReferences",
                column: "ButtonId");

            migrationBuilder.CreateIndex(
                name: "IX_ButtonWithEntryReferences_LinkToEntryId",
                schema: "Contentful",
                table: "ButtonWithEntryReferences",
                column: "LinkToEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ButtonWithLinks_ButtonId",
                schema: "Contentful",
                table: "ButtonWithLinks",
                column: "ButtonId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_HeaderId",
                schema: "Contentful",
                table: "Categories",
                column: "HeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationPages_PageId",
                schema: "Contentful",
                table: "RecommendationPages",
                column: "PageId",
                unique: true,
                filter: "[PageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationPages_SectionDbEntityId",
                schema: "Contentful",
                table: "RecommendationPages",
                column: "SectionDbEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_CategoryId",
                schema: "Contentful",
                table: "Sections",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_InterstitialPageId",
                schema: "Contentful",
                table: "Sections",
                column: "InterstitialPageId",
                unique: true,
                filter: "[InterstitialPageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Warnings_TextId",
                schema: "Contentful",
                table: "Warnings",
                column: "TextId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_NextQuestionId",
                schema: "Contentful",
                table: "Answers",
                column: "NextQuestionId",
                principalSchema: "Contentful",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_ParentQuestionId",
                schema: "Contentful",
                table: "Answers",
                column: "ParentQuestionId",
                principalSchema: "Contentful",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_Titles_TitleId",
                schema: "Contentful",
                table: "Pages",
                column: "TitleId",
                principalSchema: "Contentful",
                principalTable: "Titles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Sections_SectionId",
                schema: "Contentful",
                table: "Questions",
                column: "SectionId",
                principalSchema: "Contentful",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_NextQuestionId",
                schema: "Contentful",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_ParentQuestionId",
                schema: "Contentful",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Pages_Titles_TitleId",
                schema: "Contentful",
                table: "Pages");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Sections_SectionId",
                schema: "Contentful",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "ButtonWithEntryReferences",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "ButtonWithLinks",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "RecommendationPages",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Warnings",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Buttons",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Sections",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "Contentful");

            migrationBuilder.DropIndex(
                name: "IX_Questions_SectionId",
                schema: "Contentful",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SectionId",
                schema: "Contentful",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "SectionId",
                schema: "Contentful",
                table: "Pages",
                newName: "SectionTitle");

            migrationBuilder.AddColumn<string>(
                name: "OrganisationName",
                schema: "Contentful",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_NextQuestionId",
                schema: "Contentful",
                table: "Answers",
                column: "NextQuestionId",
                principalSchema: "Contentful",
                principalTable: "Questions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_ParentQuestionId",
                schema: "Contentful",
                table: "Answers",
                column: "ParentQuestionId",
                principalSchema: "Contentful",
                principalTable: "Questions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_Titles_TitleId",
                schema: "Contentful",
                table: "Pages",
                column: "TitleId",
                principalSchema: "Contentful",
                principalTable: "Titles",
                principalColumn: "Id");
        }
    }
}
