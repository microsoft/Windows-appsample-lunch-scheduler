//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//
//  Microsoft License for use of Images
//
//  Microsoft grants you a worldwide, non-exclusive, non-transferrable, revocable, 
//  royalty-free license to use the Microsoft photographs or images contained in this
//  Microsoft sample project, Lunch Scheduler, (“Images”) solely for your purposes
//  of internal using or testing the sample application.You may not copy, modify,
//  reproduce, distribute, publicly display, offer for sale,
//  sell, market, or promote the Microsoft Images.
//  ---------------------------------------------------------------------------------
using LunchScheduler.Models;
using LunchScheduler.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace LunchScheduler.Repository.Migrations
{
    [DbContext(typeof(LunchContext))]
    [Migration("20171005171403_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("LunchScheduler.Models.Friendship", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("FriendId");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("FriendId");

                    b.HasIndex("UserId");

                    b.ToTable("Friends");
                });

            modelBuilder.Entity("LunchScheduler.Models.Group", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<Guid>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("LunchScheduler.Models.GroupMembership", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("GroupId");

                    b.Property<Guid>("MemberId");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("MemberId");

                    b.ToTable("GroupMemberships");
                });

            modelBuilder.Entity("LunchScheduler.Models.Invitation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("LunchId");

                    b.Property<int>("Response");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("LunchId");

                    b.HasIndex("UserId");

                    b.ToTable("Invitations");
                });

            modelBuilder.Entity("LunchScheduler.Models.Lunch", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<Guid>("HostId");

                    b.Property<Guid>("LocationId");

                    b.Property<string>("Notes");

                    b.Property<int>("State");

                    b.HasKey("Id");

                    b.HasIndex("HostId");

                    b.HasIndex("LocationId");

                    b.ToTable("Lunches");
                });

            modelBuilder.Entity("LunchScheduler.Models.Restaurant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("Category");

                    b.Property<double>("Distance");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<string>("Name");

                    b.Property<string>("PhotoUrl");

                    b.Property<string>("Price");

                    b.Property<double>("Rating");

                    b.Property<string>("YelpId");

                    b.HasKey("Id");

                    b.ToTable("Restaurants");
                });

            modelBuilder.Entity("LunchScheduler.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthenticationProviderId");

                    b.Property<int>("AuthenticationProviderKind");

                    b.Property<string>("AuthorizationToken");

                    b.Property<DateTime>("AuthorizationTokenExpiration");

                    b.Property<string>("Name");

                    b.Property<string>("PhotoUrl");

                    b.Property<Guid?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("LunchScheduler.Models.Friendship", b =>
                {
                    b.HasOne("LunchScheduler.Models.User", "Friend")
                        .WithMany()
                        .HasForeignKey("FriendId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("LunchScheduler.Models.User", "User")
                        .WithMany("Friendships")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("LunchScheduler.Models.Group", b =>
                {
                    b.HasOne("LunchScheduler.Models.User", "Owner")
                        .WithMany("Groups")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LunchScheduler.Models.GroupMembership", b =>
                {
                    b.HasOne("LunchScheduler.Models.Group", "Group")
                        .WithMany("Members")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("LunchScheduler.Models.User", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LunchScheduler.Models.Invitation", b =>
                {
                    b.HasOne("LunchScheduler.Models.Lunch", "Lunch")
                        .WithMany("Invitations")
                        .HasForeignKey("LunchId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("LunchScheduler.Models.User", "User")
                        .WithMany("Invitations")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LunchScheduler.Models.Lunch", b =>
                {
                    b.HasOne("LunchScheduler.Models.User", "Host")
                        .WithMany("Lunches")
                        .HasForeignKey("HostId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("LunchScheduler.Models.Restaurant", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LunchScheduler.Models.User", b =>
                {
                    b.HasOne("LunchScheduler.Models.User")
                        .WithMany("Friends")
                        .HasForeignKey("UserId");
                });
#pragma warning restore 612, 618
        }
    }
}
