using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfe.PlanTech.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IdChangesAndPageAndTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentfulId",
                schema: "Contentful",
                table: "Questions")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                schema: "Contentful",
                table: "Questions")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                schema: "Contentful",
                table: "Questions")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "ContentfulId",
                schema: "Contentful",
                table: "Answers")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                schema: "Contentful",
                table: "Answers")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                schema: "Contentful",
                table: "Answers")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterTable(
                name: "Questions",
                schema: "Contentful")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterTable(
                name: "Answers",
                schema: "Contentful")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Param",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "HelpText",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true)
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                schema: "Contentful",
                table: "Answers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "ParentQuestionId",
                schema: "Contentful",
                table: "Answers",
                type: "nvarchar(30)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true)
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "NextQuestionId",
                schema: "Contentful",
                table: "Answers",
                type: "nvarchar(30)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true)
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Maturity",
                schema: "Contentful",
                table: "Answers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                schema: "Contentful",
                table: "Answers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

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
                    BeforeTitleContentId = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    BeforeTitleContentPagesId = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    PageId = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    ContentComponentId = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    ContentId = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    ContentPagesId = table.Column<string>(type: "nvarchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageContentDbEntity", x => new { x.BeforeTitleContentId, x.BeforeTitleContentPagesId });
                    table.ForeignKey(
                        name: "FK_PageContentDbEntity_ContentComponentDbEntity_BeforeTitleContentId",
                        column: x => x.BeforeTitleContentId,
                        principalTable: "ContentComponentDbEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PageContentDbEntity_ContentComponentDbEntity_ContentComponentId",
                        column: x => x.ContentComponentId,
                        principalTable: "ContentComponentDbEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageContentDbEntity_ContentComponentDbEntity_ContentId",
                        column: x => x.ContentId,
                        principalTable: "ContentComponentDbEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PageContentDbEntity_Pages_BeforeTitleContentPagesId",
                        column: x => x.BeforeTitleContentPagesId,
                        principalSchema: "Contentful",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageContentDbEntity_Pages_ContentPagesId",
                        column: x => x.ContentPagesId,
                        principalSchema: "Contentful",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageContentDbEntity_Pages_PageId",
                        column: x => x.PageId,
                        principalSchema: "Contentful",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageContentDbEntity_BeforeTitleContentPagesId",
                table: "PageContentDbEntity",
                column: "BeforeTitleContentPagesId");

            migrationBuilder.CreateIndex(
                name: "IX_PageContentDbEntity_ContentComponentId",
                table: "PageContentDbEntity",
                column: "ContentComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_PageContentDbEntity_ContentId",
                table: "PageContentDbEntity",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_PageContentDbEntity_ContentPagesId",
                table: "PageContentDbEntity",
                column: "ContentPagesId");

            migrationBuilder.CreateIndex(
                name: "IX_PageContentDbEntity_PageId",
                table: "PageContentDbEntity",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_TitleId",
                schema: "Contentful",
                table: "Pages",
                column: "TitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Answers",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_ContentComponentDbEntity_Id",
                schema: "Contentful",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "PageContentDbEntity");

            migrationBuilder.DropTable(
                name: "Pages",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "Titles",
                schema: "Contentful");

            migrationBuilder.DropTable(
                name: "ContentComponentDbEntity");

            migrationBuilder.AlterTable(
                name: "Questions",
                schema: "Contentful")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterTable(
                name: "Answers",
                schema: "Contentful")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Param",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "HelpText",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "Contentful",
                table: "Questions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30)
                .Annotation("SqlServer:Identity", "1, 1")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<string>(
                name: "ContentfulId",
                schema: "Contentful",
                table: "Questions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodEnd",
                schema: "Contentful",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStart",
                schema: "Contentful",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "QuestionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                schema: "Contentful",
                table: "Answers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<long>(
                name: "ParentQuestionId",
                schema: "Contentful",
                table: "Answers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldNullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<long>(
                name: "NextQuestionId",
                schema: "Contentful",
                table: "Answers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldNullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<string>(
                name: "Maturity",
                schema: "Contentful",
                table: "Answers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "Contentful",
                table: "Answers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30)
                .Annotation("SqlServer:Identity", "1, 1")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<string>(
                name: "ContentfulId",
                schema: "Contentful",
                table: "Answers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodEnd",
                schema: "Contentful",
                table: "Answers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStart",
                schema: "Contentful",
                table: "Answers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AnswersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "Contentful")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");
        }
    }
}
