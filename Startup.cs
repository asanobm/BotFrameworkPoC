// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v4.11.1

using BotFrameworkPoC.Bots;
using BotFrameworkPoC.Bots.StateManagementBot;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BotFrameworkPoC
{
    public class Startup
    {
        private readonly MemoryStorage _storage;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _storage = new MemoryStorage();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            
            // Create the user state passing in the storage layer.
            var userState = new UserState(_storage);
            services.AddSingleton(userState);
            
            // Create the Conversation state passing in the storage layer.
            var conversationState = new ConversationState(_storage);
            services.AddSingleton(conversationState);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            
            // services.AddTransient<IBot, Bots.EchoBot>();
            // services.AddTransient<IBot, SendTypingIndicatorBot>();
            // services.AddTransient<IBot, AttachmentBot>();
            services.AddTransient<IBot, StateManagementBot>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
