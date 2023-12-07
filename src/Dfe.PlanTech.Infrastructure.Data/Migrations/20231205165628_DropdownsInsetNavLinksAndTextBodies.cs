using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfe.PlanTech.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropdownsInsetNavLinksAndTextBodies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComponentDropDowns",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentId = table.Column<string>(type: "nvarchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentDropDowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentDropDowns_RichTextContents_ContentId",
                        column: x => x.ContentId,
                        principalSchema: "Contentful",
                        principalTable: "RichTextContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NavigationLink",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DisplayText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Href = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenInNewTab = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationLink", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TextBodies",
                schema: "Contentful",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RichTextId = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextBodies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextBodies_ContentComponents_Id",
                        column: x => x.Id,
                        principalSchema: "Contentful",
                        principalTable: "ContentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TextBodies_RichTextContents_RichTextId",
                        column: x => x.RichTextId,
                        principalSchema: "Contentful",
                        principalTable: "RichTextContents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDropDowns_ContentId",
                schema: "Contentful",
                table: "ComponentDropDowns",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_TextBodies_RichTextId",
                schema: "Contentful",
                table: "TextBodies",
                column: "RichTextId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentDropDowns",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "NavigationLink",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "TextBodies",
                schema: "Contentful");
        }
    }
}
