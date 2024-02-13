﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Smug.Models.SmugDbContext;

#nullable disable

namespace Smug.Migrations
{
    [DbContext(typeof(SmugDbContext))]
    [Migration("20240212233019_AddBannedUntilFieldToRestrictedUrls")]
    partial class AddBannedUntilFieldToRestrictedUrls
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Smug.Models.IpAddressInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Ip")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("ShouldHideIfBanned")
                        .HasColumnType("boolean");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("StatusChangeDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("StatusChangeReason")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("IpAddresses");
                });

            modelBuilder.Entity("Smug.Models.IpToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("IpAddressInfoId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("IpId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TokenId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("TokenInfoId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("IpAddressInfoId");

                    b.HasIndex("TokenInfoId");

                    b.ToTable("IpToken");
                });

            modelBuilder.Entity("Smug.Models.RestrictedUrl", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("RestrictedDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("RestrictedUrls");
                });

            modelBuilder.Entity("Smug.Models.TokenInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("StatusChangeDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Tokens");
                });

            modelBuilder.Entity("Smug.Models.UserRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Headers")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("IpInfoId")
                        .HasColumnType("uuid");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("RequestDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("TokenInfoId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("IpInfoId");

                    b.HasIndex("TokenInfoId");

                    b.ToTable("UserRequests");
                });

            modelBuilder.Entity("Smug.Models.IpToken", b =>
                {
                    b.HasOne("Smug.Models.IpAddressInfo", null)
                        .WithMany("IpTokens")
                        .HasForeignKey("IpAddressInfoId");

                    b.HasOne("Smug.Models.TokenInfo", null)
                        .WithMany("IpTokens")
                        .HasForeignKey("TokenInfoId");
                });

            modelBuilder.Entity("Smug.Models.UserRequest", b =>
                {
                    b.HasOne("Smug.Models.IpAddressInfo", "IpInfo")
                        .WithMany("UserRequests")
                        .HasForeignKey("IpInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Smug.Models.TokenInfo", "TokenInfo")
                        .WithMany("UserRequests")
                        .HasForeignKey("TokenInfoId");

                    b.Navigation("IpInfo");

                    b.Navigation("TokenInfo");
                });

            modelBuilder.Entity("Smug.Models.IpAddressInfo", b =>
                {
                    b.Navigation("IpTokens");

                    b.Navigation("UserRequests");
                });

            modelBuilder.Entity("Smug.Models.TokenInfo", b =>
                {
                    b.Navigation("IpTokens");

                    b.Navigation("UserRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
