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
using System.Text;
using LunchScheduler.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using LunchScheduler.Repository;
using Microsoft.EntityFrameworkCore;

namespace LunchScheduler.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LunchContext>(x => x.UseSqlServer(Constants.SqlAzureConnectionString));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x =>
            {
                x.Audience = Constants.JwtAudience; 
                x.RequireHttpsMetadata = false;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Constants.JwtSecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = Constants.JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = Constants.JwtAudience,
                    ClockSkew = TimeSpan.Zero
                }; 
            });
            services.AddAuthorization();
            services.AddMvc().AddJsonOptions(x =>
            {
                x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; 
                x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            }); 
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
