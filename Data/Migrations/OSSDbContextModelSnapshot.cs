﻿// <auto-generated />
using Aiursoft.OSS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace Aiursoft.OSS.Data.Migrations
{
    [DbContext(typeof(OSSDbContext))]
    partial class OSSDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Aiursoft.Pylon.Models.OSS.Bucket", b =>
                {
                    b.Property<int>("BucketId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BelongingAppId");

                    b.Property<string>("BucketName");

                    b.Property<bool>("OpenToRead");

                    b.Property<bool>("OpenToUpload");

                    b.HasKey("BucketId");

                    b.HasIndex("BelongingAppId");

                    b.ToTable("Bucket");
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.OSS.OSSApp", b =>
                {
                    b.Property<string>("AppId")
                        .ValueGeneratedOnAdd();

                    b.HasKey("AppId");

                    b.ToTable("Apps");
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.OSS.OSSFile", b =>
                {
                    b.Property<int>("FileKey")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BucketId");

                    b.Property<int>("DownloadTimes");

                    b.Property<string>("FileExtension");

                    b.Property<string>("RealFileName");

                    b.HasKey("FileKey");

                    b.HasIndex("BucketId");

                    b.ToTable("OSSFile");
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.OSS.Bucket", b =>
                {
                    b.HasOne("Aiursoft.Pylon.Models.OSS.OSSApp", "BelongingApp")
                        .WithMany("MyBuckets")
                        .HasForeignKey("BelongingAppId");
                });

            modelBuilder.Entity("Aiursoft.Pylon.Models.OSS.OSSFile", b =>
                {
                    b.HasOne("Aiursoft.Pylon.Models.OSS.Bucket", "BelongingBucket")
                        .WithMany("Files")
                        .HasForeignKey("BucketId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
