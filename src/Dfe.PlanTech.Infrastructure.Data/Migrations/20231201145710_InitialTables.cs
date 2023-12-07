using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfe.PlanTech.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Contentful");

            migrationBuilder.CreateTable(
                name: "ContentComponentDbEntity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentComponentDbEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HelpText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Param = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_ContentComponentDbEntity_Id",
                        column: x => x.Id,
                        principalTable: "ContentComponentDbEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Titles",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ContentfulId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Titles_ContentComponentDbEntity_Id",
                        column: x => x.Id,
                        principalTable: "ContentComponentDbEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextQuestionId = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    Maturity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentQuestionId = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_ContentComponentDbEntity_Id",
                        column: x => x.Id,
                        principalTable: "ContentComponentDbEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_NextQuestionId",
                        column: x => x.NextQuestionId,
                        principalSchema: "Contentful",
                        principalTable: "Questions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Answers_Questions_ParentQuestionId",
                        column: x => x.ParentQuestionId,
                        principalSchema: "Contentful",
                        principalTable: "Questions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InternalName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayBackButton = table.Column<bool>(type: "bit", nullable: false),
                    DisplayHomeButton = table.Column<bool>(type: "bit", nullable: false),
                    DisplayTopicTitle = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrganisationName = table.Column<bool>(type: "bit", nullable: false),
                    RequiresAuthorisation = table.Column<bool>(type: "bit", nullable: false),
                    SectionTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TitleId = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    OrganisationName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pages_ContentComponentDbEntity_Id",
                        column: x => x.Id,
                        principalTable: "ContentComponentDbEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pages_Titles_TitleId",
                        column: x => x.TitleId,
                        principalSchema: "Contentful",
                        principalTable: "Titles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PageContentDbEntity",
                columns: table => new
                {
                    ContentComponentId = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    PageId = table.Column<string>(type: "nvarchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageContentDbEntity", x => new { x.ContentComponentId, x.PageId });
                    table.ForeignKey(
                        name: "FK_PageContentDbEntity_ContentComponentDbEntity_ContentComponentId",
                        column: x => x.ContentComponentId,
                        principalTable: "ContentComponentDbEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageContentDbEntity_Pages_PageId",
                        column: x => x.PageId,
                        principalSchema: "Contentful",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_NextQuestionId",
                schema: "Contentful",
                table: "Answers",
                column: "NextQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_ParentQuestionId",
                schema: "Contentful",
                table: "Answers",
                column: "ParentQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PageContentDbEntity_PageId",
                table: "PageContentDbEntity",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_TitleId",
                schema: "Contentful",
                table: "Pages",
                column: "TitleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "PageContentDbEntity");

            migrationBuilder.DropTable(
                name: "Questions",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Pages",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Titles",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "ContentComponentDbEntity");
        }
    }
}
