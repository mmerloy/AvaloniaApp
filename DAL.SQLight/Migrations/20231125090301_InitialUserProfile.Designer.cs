﻿// <auto-generated />
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DAL.SQLight.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20231125090301_InitialUserProfile")]
    partial class InitialUserProfile
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.14");

            modelBuilder.Entity("Domain.MethodConfigurations.MethodConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Inaccuracy")
                        .HasColumnType("REAL");

                    b.Property<string>("method_config_type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("MethodConfigurations");

                    b.HasDiscriminator<string>("method_config_type").HasValue("MethodConfiguration");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Domain.UserProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("MethodConfigurationId")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("SearchObjectType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("MethodConfigurationId");

                    b.ToTable("UsersProfiles");
                });

            modelBuilder.Entity("Domain.MethodConfigurations.Implementation.InterpolationMethodConfiguration", b =>
                {
                    b.HasBaseType("Domain.MethodConfigurations.MethodConfiguration");

                    b.Property<byte>("InterpolationCount")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("interpolation");
                });

            modelBuilder.Entity("Domain.MethodConfigurations.Implementation.RecursionMethodConfiguration", b =>
                {
                    b.HasBaseType("Domain.MethodConfigurations.MethodConfiguration");

                    b.Property<double>("NotAllCoverage")
                        .HasColumnType("REAL");

                    b.HasDiscriminator().HasValue("recursion");
                });

            modelBuilder.Entity("Domain.MethodConfigurations.Implementation.WeightCoefficientsMethodConfiguration", b =>
                {
                    b.HasBaseType("Domain.MethodConfigurations.MethodConfiguration");

                    b.Property<bool>("Brightness")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Color")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ContrastRatio")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("weighted");
                });

            modelBuilder.Entity("Domain.UserProfile", b =>
                {
                    b.HasOne("Domain.MethodConfigurations.MethodConfiguration", "MethodConfiguration")
                        .WithMany()
                        .HasForeignKey("MethodConfigurationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MethodConfiguration");
                });
#pragma warning restore 612, 618
        }
    }
}
