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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LunchScheduler.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LunchScheduler.Repository
{
    /// <summary>
    /// Contains methods for interacting with a mock backend service. 
    /// The demo repository can retrieve and store data just like production, 
    /// but also contains helpers for generating fake data and mock responses. 
    /// </summary>
    public class LunchDemoRepository : ILunchRepository
    {
        private readonly Random _random = new Random();
        private readonly DateTime _maxDate = new DateTime(2038, 1, 1);
        private readonly string _usersResource = "LunchScheduler.Repository.Data.Users.json";
        private readonly string _restaurantsResource = "LunchScheduler.Repository.Data.Restaurants.json";
        private readonly DbContextOptions<LunchContext> _dbOptions;

        /// <summary>
        /// Creates a new demo repository backed by an Sqlite database 
        /// with the given connection string. Input should be something like: 
        /// "Data Source='LunchScheduler.db". 
        /// </summary>
        public LunchDemoRepository(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LunchContext>()
                .UseSqlite(connectionString)
                .EnableSensitiveDataLogging();
            _dbOptions = optionsBuilder.Options;
        }

        public User User { get; set; }

        public async Task AddFriendAsync(Friendship friend)
        {
            var db = new LunchContext(_dbOptions);
            await db.Friends.AddAsync(friend);
            await db.SaveChangesAsync();
        }

        public async Task CancelLunchAsync(Lunch lunch)
        {
            var db = new LunchContext(_dbOptions);
            db.Lunches.Remove(lunch);
            await db.SaveChangesAsync();
        }

        public async Task CreateGroupAsync(Group group)
        {
            var db = new LunchContext(_dbOptions);
            await db.Groups.AddAsync(group);
            await db.SaveChangesAsync();
        }

        public async Task CreateLunchAsync(Lunch lunch)
        {
            var db = new LunchContext(_dbOptions);

            // Demo users won't respond to the lunch on their own, 
            // so fake some responses so the app appears interactive. 

            foreach (var invite in lunch.Invitations)
            {
                invite.Response = _random.Next(0, 2) == 0 ? InviteResponseKind.Accepted :
                    InviteResponseKind.Declined;
            }
            await db.SaveChangesAsync();
        }

        public async Task DeleteGroupAsync(Group group)
        {
            var db = new LunchContext(_dbOptions);
            var entity = db.Groups.FindAsync(group.Id);
            db.Groups.Remove(group);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetFriendsAsync()
        {
            var db = new LunchContext(_dbOptions);
            var friends = await db.Friends
                .Where(x => x.UserId == User.Id)
                .Include(x => x.Friend)
                .AsNoTracking()
                .ToListAsync();
            return friends.Select(x => x.Friend);
        }

        public async Task<IEnumerable<Invitation>> GetInvitationsAsync()
        {
            var db = new LunchContext(_dbOptions);
            var invites = await db.Invitations
                .Where(x => x.UserId == User.Id && x.Response == InviteResponseKind.None)
                .Include(x => x.User)
                .Include(x => x.Lunch)
                .Include(x => x.Lunch.Host)
                .Include(x => x.Lunch.Location)
                .Include(x => x.Lunch.Invitations)
                .ThenInclude(x => x.User)
                .AsNoTracking()
                .ToListAsync();
            return invites;
        }

        public async Task<IEnumerable<Lunch>> GetLunchesAsync()
        {
            var db = new LunchContext(_dbOptions);

            var userHostedLunches = await db.Lunches
                .Include(x => x.Location)
                .Include(x => x.Host)
                .Include(x => x.Invitations)
                .ThenInclude(x => x.User)
                .Where(x => x.HostId == User.Id)
                .AsNoTracking()
                .ToListAsync();

            var userAcceptedLunches = await db.Invitations
                .Where(x => x.UserId == User.Id && x.Response == InviteResponseKind.Accepted)
                .Select(x => x.Lunch)
                .Include(x => x.Host)
                .Include(x => x.Location)
                .Include(x => x.Invitations)
                .ThenInclude(x => x.User)
                .AsNoTracking()
                .ToListAsync();

            return userHostedLunches.Concat(userAcceptedLunches).ToList();
        }

        public async Task<IEnumerable<Restaurant>> GetRestaurantsAsync(string address)
        {
            // In demo mode, you can't search, so we use Microsoft's Redmond campus
            // as a default location. 

            return await GetRestaurantsAsync(Constants.DemoLatitude, Constants.DemoLongitude);
        }

        public async Task<IEnumerable<Restaurant>> GetRestaurantsAsync(double lat, double lng)
        {
            double GetCoordinate(int range, double midpoint) =>
                (_random.Next(range) - (range / 2)) * 0.00001 + midpoint;

            var locations = await GetDemoRestaurantsAsync();

            foreach (var location in locations)
            {
                location.Latitude = GetCoordinate(3000, lat);
                location.Longitude = GetCoordinate(5000, lng);
            }

            return locations;
        }

        private async Task<IEnumerable<Restaurant>> GetDemoRestaurantsAsync()
        {
            var restaurants = await ReadEmbeddedResourcesAsync<List<Restaurant>>(_restaurantsResource);
            return restaurants;
        }

        public async Task<IEnumerable<User>> GetSuggestedFriendsAsync()
        {
            var db = new LunchContext(_dbOptions);

            var friends = await db.Friends
                .Where(x => x.UserId == User.Id)
                .Select(x => x.Id)
                .ToListAsync();

            var matches = await db.Invitations
                .Where(x => x.UserId == User.Id)
                .OrderByDescending(x => x.Lunch.Date)
                .Take(10)
                .SelectMany(x => x.Lunch.Invitations
                    .Select(y => y.User))
                .Where(x => !friends.Contains(x.Id))
                .AsNoTracking()
                .ToListAsync();

            return matches;
        }

        public async Task JoinGroupAsync(GroupMembership membership)
        {
            var db = new LunchContext(_dbOptions);
            await db.GroupMemberships.AddAsync(membership);
            await db.SaveChangesAsync();
        }

        public async Task LeaveGroupAsync(GroupMembership membership)
        {
            var db = new LunchContext(_dbOptions);
            var entity = await db.GroupMemberships.FindAsync(membership.Id);
            db.GroupMemberships.Remove(entity);
            await db.SaveChangesAsync();
        }

        public async Task LogoutAsync()
        {
            var db = new LunchContext(_dbOptions);
            var entity = await db.Users.FindAsync(User.Id);
            if (entity != null && entity.Id == User.Id)
            {
                entity.AuthorizationToken = null;
                entity.AuthorizationTokenExpiration = DateTime.MinValue;
                await db.SaveChangesAsync();
            }
            User = null;
        }

        public async Task RemoveFriendAsync(Friendship friend)
        {
            var db = new LunchContext(_dbOptions);
            var entity = await db.Friends.FindAsync(friend.Id);
            db.Friends.Remove(entity);
            await db.SaveChangesAsync();
        }

        public async Task RespondToInvitationAsync(Invitation invite)
        {
            var db = new LunchContext(_dbOptions);
            var entity = await db.Invitations.FindAsync(invite.Id);
            entity.Response = invite.Response;
            await db.SaveChangesAsync();
        }

        public async Task<User> LoginAsyc(AuthenticationProviderKind provider, string token)
        {
            var db = new LunchContext(_dbOptions);

            // First, check if our demo user exists already. If so, log them back in. 

            var user = await db.Users.FirstOrDefaultAsync(x => x.AuthenticationProviderKind == provider &&
                   x.AuthorizationToken == token);
            if (user != null)
            {
                User = user;
                return user;
            }

            // Otherwise, we need to make a new demo user. 

            // Load a preset list of demo users, except give each a unique Id
            // and authorization token so the login experience remains authentic. 

            var users = (await ReadEmbeddedResourcesAsync<List<User>>(_usersResource))
                .Select(x => new User
                {
                    AuthenticationProviderId = Guid.NewGuid().ToString(),
                    AuthenticationProviderKind = x.AuthenticationProviderKind,
                    AuthorizationTokenExpiration = x.AuthorizationTokenExpiration,
                    AuthorizationToken = Guid.NewGuid().ToString(),
                    Name = x.Name,
                    PhotoUrl = x.PhotoUrl
                }).ToList();

            // If our user logged in with a provider, use that information and fake all the rest. 
            // Otherwise, create a fake profile for them too. 

            if (provider != AuthenticationProviderKind.Demo)
            {
                user = new User
                {
                    AuthenticationProviderKind = provider,
                    AuthorizationToken = token,
                    AuthorizationTokenExpiration = new DateTime(2038, 1, 1)
                };
            }
            else
            {
                user = RandomAndRemove(users);
            }
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // When we create a demo user, also generate them some random friends.  

            for (int i = 0; i < users.Count; i++)
            {
                user.Friends.Add(users[i]);
            }
            await db.SaveChangesAsync();

            // Lastly, set up some invitations and lunches for them, so there's content when 
            // they first launch the app. 

            var hosts = user.Friends.ToList();
            var locations = (await GetRestaurantsAsync("")).ToList();
            for (int i = 0; i < 4; i++)
            {
                var lunch = new Lunch
                {
                    Date = AddDaysAtNoonToUtc(i + 1),
                    Host = RandomAndRemove(hosts),
                    Location = RandomAndRemove(locations)
                };

                // Invite the user to lunch, plus a bunch of their friends as well
                // so the list of accepts/declines has something to show.

                lunch.Invitations.Add(new Invitation(lunch, user));
                foreach (var friend in user.Friends.OrderBy(x => _random.Next())
                    .Take(_random.Next(1, user.Friends.Count / 2)))
                {
                    lunch.Invitations.Add(new Invitation
                    {
                        Lunch = lunch,
                        User = friend,
                        Response = _random.Next(0, 2) == 0 ? InviteResponseKind.Accepted : InviteResponseKind.Declined
                    });
                }
                db.Lunches.Add(lunch);
                await db.SaveChangesAsync();
            }


            var userLunch = new Lunch
            {
                Host = user,
                Location = RandomAndRemove(locations),
                Date = AddDaysAtNoonToUtc(1),
                Notes = "My first lunch!"
            };
            userLunch.Invitations = new ObservableCollection<Invitation>(user.Friends
                .OrderBy(x => _random.Next())
                .Take(_random.Next(1, user.Friends.Count))
                .Select(x => new Invitation(userLunch, x)));
            db.Lunches.Add(userLunch);

            await db.SaveChangesAsync();

            var dbTest = new LunchContext(_dbOptions);
            var lunches = dbTest.Lunches.Include(x => x.Invitations).ToList();

            db.Entry(user).State = EntityState.Detached;
            User = user;
            return user;
        }

        /// <summary>
        /// Finds a random element from a collection, then removes and 
        /// returns it. Used when generating fake lunches and invitations 
        /// so all lunches aren't "randomly" from the same person. 
        /// </summary>
        private T RandomAndRemove<T>(IList<T> source)
        {
            int i = _random.Next(source.Count);
            T element = source[i];
            source.RemoveAt(i);
            return element;
        }

        /// <summary>
        /// Returns noon (in UTC format) the given number of days 
        /// in the future. Used when generating random lunches so 
        /// they're different days in the future, but all at lunchtime. 
        /// </summary>
        public DateTime AddDaysAtNoonToUtc(int i)
        {
            var now = DateTime.Now;
            var target = new DateTime(now.Year, now.Month, now.Day)
                .AddDays(i).AddHours(12);
            return target.ToUniversalTime();
        }

        private async Task<T> ReadEmbeddedResourcesAsync<T>(string resource)
        {
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resource))
            {
                using (var reader = new StreamReader(stream))
                {
                    string json = await reader.ReadToEndAsync();
                    T data = JsonConvert.DeserializeObject<T>(json);
                    return data;
                }
            }
        }
    }
}
