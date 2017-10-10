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
using Microsoft.EntityFrameworkCore;

namespace LunchScheduler.Repository
{
    /// <summary>
    /// The database context for Entity Framework Core. 
    /// </summary>
    public class LunchContext : DbContext
    {
        /// <summary>
        /// Creates a new Lunch Context with the given options. Pass the connection
        /// string string as part of UseSqlServer() or UseSqlite().
        /// </summary>
        public LunchContext(DbContextOptions<LunchContext> options) : base(options)
        {
            Database.Migrate(); 
        }

        /// <summary>
        /// Gets the <see cref="Lunch"/> table.
        /// </summary>
        public DbSet<Lunch> Lunches { get; set; }

        /// <summary>
        /// Gets the <see cref="Invitation"/> table. 
        /// </summary>
        public DbSet<Invitation> Invitations { get; set; }

        /// <summary>
        /// Gets the <see cref="User"/> table. 
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets the <see cref="Friendship"/> table.
        /// </summary>
        public DbSet<Friendship> Friends { get; set; }

        /// <summary>
        /// Gets the <see cref="Group"/> table. 
        /// </summary>
        public DbSet<Group> Groups { get; set; }

        /// <summary>
        /// Gets the <see cref="GroupMembership"/> table. 
        /// </summary>
        public DbSet<GroupMembership> GroupMemberships { get; set; }

        /// <summary>
        /// Gets the <see cref="Restaurant"/> table. Contains only 
        /// locations that are already part of a lunch, not all locations
        /// possibly discoverable with the Yelp API. 
        /// </summary>
        public DbSet<Restaurant> Restaurants { get; set; }

        /// <summary>
        /// Configures Entity Framework relationships when the model is created. 
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasMany(x => x.Friendships)
                .WithOne(x => x.User).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Group>().HasMany(x => x.Members)
                .WithOne(x => x.Group).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Lunch>().HasMany(x => x.Invitations)
                .WithOne(x => x.Lunch).OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
