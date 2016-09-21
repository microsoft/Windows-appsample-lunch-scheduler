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
//  ---------------------------------------------------------------------------------

using LunchScheduler.Models;
using Microsoft.Azure.Mobile.Server.Tables;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace LunchScheduler.Site
{
    /// <summary>
    /// The Entity Framework database context. 
    /// </summary>
    public class LunchSchedulerContext : DbContext 
    {
        /// <summary>
        /// The name of the connection string in Web.config.
        /// </summary>
        private const string ConnectionStringName = "Name=MS_TableConnectionString";

        /// <summary>
        /// The Users table.
        /// </summary>
        public DbSet<User> Users => this.Set<User>();

        /// <summary>
        /// The lunches table.
        /// </summary>
        public DbSet<Lunch> Lunches => this.Set<Lunch>(); 

        /// <summary>
        /// Creates a new lunchtime context.
        /// </summary>
        public LunchSchedulerContext() : base(ConnectionStringName)
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<LunchSchedulerContext>());
            Database.CommandTimeout = 180; 
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (x, y) => y.Single().ColumnType.ToString()));

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Lunch>().ToTable("Lunches"); 
        }
    }
}