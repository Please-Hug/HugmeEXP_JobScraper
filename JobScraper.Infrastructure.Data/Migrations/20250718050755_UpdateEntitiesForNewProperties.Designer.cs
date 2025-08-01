﻿// <auto-generated />
using JobScraper.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace JobScraper.Infrastructure.Data.Migrations
{
    [DbContext(typeof(JobScraperDbContext))]
    [Migration("20250718050755_UpdateEntitiesForNewProperties")]
    partial class UpdateEntitiesForNewProperties
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("JobDetailEntitySkillEntity", b =>
                {
                    b.Property<int>("JobDetailsId")
                        .HasColumnType("int");

                    b.Property<int>("RequiredSkillsId")
                        .HasColumnType("int");

                    b.HasKey("JobDetailsId", "RequiredSkillsId");

                    b.HasIndex("RequiredSkillsId");

                    b.ToTable("JobDetailEntitySkillEntity");
                });

            modelBuilder.Entity("JobScraper.Infrastructure.Data.Entities.JobDetailEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("varchar(10000)");

                    b.Property<int>("EducationLevel")
                        .HasColumnType("int");

                    b.Property<int>("JobListingId")
                        .HasColumnType("int");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<string>("Prefers")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<string>("Qualifications")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<long>("Salary")
                        .HasColumnType("bigint");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasColumnType("json");

                    b.HasKey("Id");

                    b.HasIndex("JobListingId")
                        .IsUnique();

                    b.ToTable("JobDetails");
                });

            modelBuilder.Entity("JobScraper.Infrastructure.Data.Entities.JobListingEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Company")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("Experience")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("varchar(1000)");

                    b.HasKey("Id");

                    b.ToTable("JobListings");
                });

            modelBuilder.Entity("JobScraper.Infrastructure.Data.Entities.SkillEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Skills");
                });

            modelBuilder.Entity("JobDetailEntitySkillEntity", b =>
                {
                    b.HasOne("JobScraper.Infrastructure.Data.Entities.JobDetailEntity", null)
                        .WithMany()
                        .HasForeignKey("JobDetailsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("JobScraper.Infrastructure.Data.Entities.SkillEntity", null)
                        .WithMany()
                        .HasForeignKey("RequiredSkillsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("JobScraper.Infrastructure.Data.Entities.JobDetailEntity", b =>
                {
                    b.HasOne("JobScraper.Infrastructure.Data.Entities.JobListingEntity", "JobListing")
                        .WithOne("JobDetail")
                        .HasForeignKey("JobScraper.Infrastructure.Data.Entities.JobDetailEntity", "JobListingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("JobListing");
                });

            modelBuilder.Entity("JobScraper.Infrastructure.Data.Entities.JobListingEntity", b =>
                {
                    b.Navigation("JobDetail");
                });
#pragma warning restore 612, 618
        }
    }
}
