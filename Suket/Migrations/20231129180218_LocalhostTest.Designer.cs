﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Suket.Data;

#nullable disable

namespace Suket.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20231129180218_LocalhostTest")]
    partial class LocalhostTest
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Suket.Models.Adoption", b =>
                {
                    b.Property<int>("AdoptionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("AdoptionId"));

                    b.Property<int>("PostId")
                        .HasColumnType("integer");

                    b.Property<string>("UserAccountId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("AdoptionId");

                    b.HasIndex("PostId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("Adoption");
                });

            modelBuilder.Entity("Suket.Models.Confirm", b =>
                {
                    b.Property<int>("ConfirmId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ConfirmId"));

                    b.Property<int>("PostId")
                        .HasColumnType("integer");

                    b.Property<string>("UserAccountId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ConfirmId");

                    b.HasIndex("PostId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("Confirm");
                });

            modelBuilder.Entity("Suket.Models.Contact", b =>
                {
                    b.Property<int>("ContactId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ContactId"));

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("UserAccountId")
                        .HasColumnType("text");

                    b.HasKey("ContactId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("Contact");
                });

            modelBuilder.Entity("Suket.Models.PaymentRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("PaymentIntentId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PostId")
                        .HasColumnType("integer");

                    b.Property<bool>("Refunded")
                        .HasColumnType("boolean");

                    b.Property<string>("UserAccountId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("PaymentRecord");
                });

            modelBuilder.Entity("Suket.Models.Post", b =>
                {
                    b.Property<int>("PostId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PostId"));

                    b.Property<int>("Certification")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Genre")
                        .HasColumnType("integer");

                    b.Property<string>("Item")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("PeopleCount")
                        .HasColumnType("integer");

                    b.Property<string>("Place")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Prefecture")
                        .HasColumnType("integer");

                    b.Property<int>("Reward")
                        .HasColumnType("integer");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("Time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserAccountId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("PostId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("Post");
                });

            modelBuilder.Entity("Suket.Models.Reply", b =>
                {
                    b.Property<int>("ReplyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ReplyId"));

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int>("PostId")
                        .HasColumnType("integer");

                    b.Property<string>("UserAccountId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ReplyId");

                    b.HasIndex("PostId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("Reply");
                });

            modelBuilder.Entity("Suket.Models.Review", b =>
                {
                    b.Property<int>("ReviewId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ReviewId"));

                    b.Property<int>("MannerLevel")
                        .HasColumnType("integer");

                    b.Property<int>("PostId")
                        .HasColumnType("integer");

                    b.Property<string>("ReviewedId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ReviewerId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("SkillLevel")
                        .HasColumnType("integer");

                    b.HasKey("ReviewId");

                    b.HasIndex("PostId");

                    b.HasIndex("ReviewedId");

                    b.HasIndex("ReviewerId");

                    b.ToTable("Review");
                });

            modelBuilder.Entity("Suket.Models.RollCall", b =>
                {
                    b.Property<int>("RollCallId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RollCallId"));

                    b.Property<int>("PostId")
                        .HasColumnType("integer");

                    b.Property<string>("UserAccountId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("RollCallId");

                    b.HasIndex("PostId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("RollCall");
                });

            modelBuilder.Entity("Suket.Models.Subscription", b =>
                {
                    b.Property<int>("SubscriptionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SubscriptionId"));

                    b.Property<int>("PostId")
                        .HasColumnType("integer");

                    b.Property<string>("UserAccountId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("SubscriptionId");

                    b.HasIndex("PostId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("Subscription");
                });

            modelBuilder.Entity("Suket.Models.UserAccount", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<DateOnly>("Birthday")
                        .HasColumnType("date");

                    b.Property<bool>("ChargesEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<bool>("DetailsSubmitted")
                        .HasColumnType("boolean");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NickName")
                        .HasColumnType("text");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("Profile")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("ProfilePictureUrl")
                        .HasColumnType("text");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<int>("SelectSex")
                        .HasColumnType("integer");

                    b.Property<string>("StripeAccountId")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Suket.Models.UserAccount", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Suket.Models.UserAccount", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Suket.Models.UserAccount", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Suket.Models.UserAccount", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Suket.Models.Adoption", b =>
                {
                    b.HasOne("Suket.Models.Post", "Post")
                        .WithMany("Adoptions")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Suket.Models.UserAccount", "UserAccount")
                        .WithMany("Adoptions")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("Suket.Models.Confirm", b =>
                {
                    b.HasOne("Suket.Models.Post", "Post")
                        .WithMany("Confirms")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Suket.Models.UserAccount", "UserAccount")
                        .WithMany("Confirms")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("Suket.Models.Contact", b =>
                {
                    b.HasOne("Suket.Models.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("Suket.Models.PaymentRecord", b =>
                {
                    b.HasOne("Suket.Models.Post", "Post")
                        .WithMany("PaymentRecords")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Suket.Models.UserAccount", "UserAccount")
                        .WithMany("PaymentRecords")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("Suket.Models.Post", b =>
                {
                    b.HasOne("Suket.Models.UserAccount", "UserAccount")
                        .WithMany("Posts")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("Suket.Models.Reply", b =>
                {
                    b.HasOne("Suket.Models.Post", "Post")
                        .WithMany("Replys")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Suket.Models.UserAccount", "UserAccount")
                        .WithMany("Replys")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("Suket.Models.Review", b =>
                {
                    b.HasOne("Suket.Models.Post", "Post")
                        .WithMany("Reviews")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Suket.Models.UserAccount", "Reviewed")
                        .WithMany("ReviewsReceived")
                        .HasForeignKey("ReviewedId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Suket.Models.UserAccount", "Reviewer")
                        .WithMany("ReviewsWritten")
                        .HasForeignKey("ReviewerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("Reviewed");

                    b.Navigation("Reviewer");
                });

            modelBuilder.Entity("Suket.Models.RollCall", b =>
                {
                    b.HasOne("Suket.Models.Post", "Post")
                        .WithMany("RollCalls")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Suket.Models.UserAccount", "UserAccount")
                        .WithMany("RollCalls")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("Suket.Models.Subscription", b =>
                {
                    b.HasOne("Suket.Models.Post", "Post")
                        .WithMany("Subscriptions")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Suket.Models.UserAccount", "UserAccount")
                        .WithMany("Subscriptions")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("Suket.Models.Post", b =>
                {
                    b.Navigation("Adoptions");

                    b.Navigation("Confirms");

                    b.Navigation("PaymentRecords");

                    b.Navigation("Replys");

                    b.Navigation("Reviews");

                    b.Navigation("RollCalls");

                    b.Navigation("Subscriptions");
                });

            modelBuilder.Entity("Suket.Models.UserAccount", b =>
                {
                    b.Navigation("Adoptions");

                    b.Navigation("Confirms");

                    b.Navigation("PaymentRecords");

                    b.Navigation("Posts");

                    b.Navigation("Replys");

                    b.Navigation("ReviewsReceived");

                    b.Navigation("ReviewsWritten");

                    b.Navigation("RollCalls");

                    b.Navigation("Subscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
