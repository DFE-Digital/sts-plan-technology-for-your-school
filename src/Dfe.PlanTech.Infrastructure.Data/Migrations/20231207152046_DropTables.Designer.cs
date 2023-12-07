﻿// <auto-generated />
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Dfe.PlanTech.Infrastructure.Data.Migrations
{
    [DbContext(typeof(CmsDbContext))]
    [Migration("20231207152046_DropTables")]
    partial class DropTables
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Contentful")
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<bool>("Archived")
                        .HasColumnType("bit");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<bool>("Published")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("ContentComponents", "Contentful");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.PageContentDbEntity", b =>
                {
                    b.Property<string>("ContentComponentId")
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("PageId")
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("ContentComponentId", "PageId");

                    b.HasIndex("PageId");

                    b.ToTable("PageContents", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.Buttons.ButtonDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<bool>("IsStartButton")
                        .HasColumnType("bit");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("Buttons", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.Buttons.ButtonWithEntryReferenceDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("ButtonId")
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("LinkToEntryId")
                        .HasColumnType("nvarchar(30)");

                    b.HasIndex("ButtonId");

                    b.HasIndex("LinkToEntryId");

                    b.ToTable("ButtonWithEntryReferences", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.Buttons.ButtonWithLinkDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("ButtonId")
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Href")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasIndex("ButtonId");

                    b.ToTable("ButtonWithLinks", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.HeaderDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<int>("Size")
                        .HasColumnType("int");

                    b.Property<int>("Tag")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("Headers", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.InsetTextDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("InsetTexts", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.NavigationLinkDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("DisplayText")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Href")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("OpenInNewTab")
                        .HasColumnType("bit");

                    b.ToTable("NavigationLink", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.PageDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<bool>("DisplayBackButton")
                        .HasColumnType("bit");

                    b.Property<bool>("DisplayHomeButton")
                        .HasColumnType("bit");

                    b.Property<bool>("DisplayOrganisationName")
                        .HasColumnType("bit");

                    b.Property<bool>("DisplayTopicTitle")
                        .HasColumnType("bit");

                    b.Property<string>("InternalName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("RequiresAuthorisation")
                        .HasColumnType("bit");

                    b.Property<string>("SectionId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TitleId")
                        .HasColumnType("nvarchar(30)");

                    b.HasIndex("TitleId");

                    b.ToTable("Pages", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.TitleDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("Titles", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.AnswerDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("Maturity")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NextQuestionId")
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("ParentQuestionId")
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasIndex("NextQuestionId");

                    b.HasIndex("ParentQuestionId");

                    b.ToTable("Answers", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.CategoryDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("HeaderId")
                        .HasColumnType("nvarchar(30)");

                    b.HasIndex("HeaderId");

                    b.ToTable("Categories", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.QuestionDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("HelpText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SectionId")
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasIndex("SectionId");

                    b.ToTable("Questions", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.RecommendationPageDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InternalName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Maturity")
                        .HasColumnType("int");

                    b.Property<string>("PageId")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("SectionDbEntityId")
                        .HasColumnType("nvarchar(30)");

                    b.HasIndex("PageId")
                        .IsUnique()
                        .HasFilter("[PageId] IS NOT NULL");

                    b.HasIndex("SectionDbEntityId");

                    b.ToTable("RecommendationPages", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.SectionDbEntity", b =>
                {
                    b.HasBaseType("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity");

                    b.Property<string>("CategoryId")
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("InterstitialPageId")
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasIndex("CategoryId");

                    b.HasIndex("InterstitialPageId")
                        .IsUnique()
                        .HasFilter("[InterstitialPageId] IS NOT NULL");

                    b.ToTable("Sections", "Contentful");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.PageContentDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", "ContentComponent")
                        .WithMany()
                        .HasForeignKey("ContentComponentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.PageDbEntity", "Page")
                        .WithMany()
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ContentComponent");

                    b.Navigation("Page");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.Buttons.ButtonWithEntryReferenceDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.Buttons.ButtonDbEntity", "Button")
                        .WithMany()
                        .HasForeignKey("ButtonId");

                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", "LinkToEntry")
                        .WithMany()
                        .HasForeignKey("LinkToEntryId");

                    b.Navigation("Button");

                    b.Navigation("LinkToEntry");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.Buttons.ButtonWithLinkDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.Buttons.ButtonDbEntity", "Button")
                        .WithMany()
                        .HasForeignKey("ButtonId");

                    b.Navigation("Button");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.PageDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", null)
                        .WithOne()
                        .HasForeignKey("Dfe.PlanTech.Domain.Content.Models.PageDbEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.TitleDbEntity", "Title")
                        .WithMany("Pages")
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Title");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.TitleDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", null)
                        .WithOne()
                        .HasForeignKey("Dfe.PlanTech.Domain.Content.Models.TitleDbEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.AnswerDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", null)
                        .WithOne()
                        .HasForeignKey("Dfe.PlanTech.Domain.Questionnaire.Models.AnswerDbEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dfe.PlanTech.Domain.Questionnaire.Models.QuestionDbEntity", "NextQuestion")
                        .WithMany("PreviousAnswers")
                        .HasForeignKey("NextQuestionId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Dfe.PlanTech.Domain.Questionnaire.Models.QuestionDbEntity", "ParentQuestion")
                        .WithMany("Answers")
                        .HasForeignKey("ParentQuestionId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("NextQuestion");

                    b.Navigation("ParentQuestion");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.CategoryDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.HeaderDbEntity", "Header")
                        .WithMany()
                        .HasForeignKey("HeaderId");

                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", null)
                        .WithOne()
                        .HasForeignKey("Dfe.PlanTech.Domain.Questionnaire.Models.CategoryDbEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Header");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.QuestionDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", null)
                        .WithOne()
                        .HasForeignKey("Dfe.PlanTech.Domain.Questionnaire.Models.QuestionDbEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dfe.PlanTech.Domain.Questionnaire.Models.SectionDbEntity", "Section")
                        .WithMany("Questions")
                        .HasForeignKey("SectionId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Section");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.RecommendationPageDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", null)
                        .WithOne()
                        .HasForeignKey("Dfe.PlanTech.Domain.Questionnaire.Models.RecommendationPageDbEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.PageDbEntity", "Page")
                        .WithOne("RecommendationPage")
                        .HasForeignKey("Dfe.PlanTech.Domain.Questionnaire.Models.RecommendationPageDbEntity", "PageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Dfe.PlanTech.Domain.Questionnaire.Models.SectionDbEntity", null)
                        .WithMany("Recommendations")
                        .HasForeignKey("SectionDbEntityId");

                    b.Navigation("Page");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.SectionDbEntity", b =>
                {
                    b.HasOne("Dfe.PlanTech.Domain.Questionnaire.Models.CategoryDbEntity", "Category")
                        .WithMany("Sections")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.ContentComponentDbEntity", null)
                        .WithOne()
                        .HasForeignKey("Dfe.PlanTech.Domain.Questionnaire.Models.SectionDbEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dfe.PlanTech.Domain.Content.Models.PageDbEntity", "InterstitialPage")
                        .WithOne("Section")
                        .HasForeignKey("Dfe.PlanTech.Domain.Questionnaire.Models.SectionDbEntity", "InterstitialPageId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Category");

                    b.Navigation("InterstitialPage");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.PageDbEntity", b =>
                {
                    b.Navigation("RecommendationPage");

                    b.Navigation("Section");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Content.Models.TitleDbEntity", b =>
                {
                    b.Navigation("Pages");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.CategoryDbEntity", b =>
                {
                    b.Navigation("Sections");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.QuestionDbEntity", b =>
                {
                    b.Navigation("Answers");

                    b.Navigation("PreviousAnswers");
                });

            modelBuilder.Entity("Dfe.PlanTech.Domain.Questionnaire.Models.SectionDbEntity", b =>
                {
                    b.Navigation("Questions");

                    b.Navigation("Recommendations");
                });
#pragma warning restore 612, 618
        }
    }
}
