using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfe.PlanTech.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Buttons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ButtonWithEntryReferences",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "ButtonWithLinks",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Buttons",
                schema: "Contentful");
        }
    }
}
