using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using chargebook.BackgroundServices;
using chargebook.data;
using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using chargebook.data.simulation;
using chargebook.data.user;
using chargebook.models;
using Microsoft.AspNetCore.Authentication;
using chargebook.models.email;
using ChargeBook.services;
using ChargeBook.services.backgroundServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace chargebook {
    public class Startup {
        public IConfiguration configuration { get; }

        public Startup(IConfiguration configuration) {
            this.configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void configureServices(IServiceCollection services) {
            services.AddControllersWithViews();
            services.AddAuthentication("Default").AddCookie("Default", options => {
                options.LoginPath = "/user/login";
                options.AccessDeniedPath = "/user/forbidden";
                options.Cookie.Name = "AuthenticationCookie";
                //ensures, that user claims are up to date
                options.Events = new CookieAuthenticationEvents() {
                    OnValidatePrincipal = cookieValidatePrincipalContext => {
                        var userUtils = cookieValidatePrincipalContext.HttpContext.RequestServices.GetRequiredService<IUserUtils>();
                        var userEmail = userUtils.getEmail(cookieValidatePrincipalContext.Principal);
                        var lastChanged = userUtils.getLastChanged(cookieValidatePrincipalContext.Principal);
                        var manager = cookieValidatePrincipalContext.HttpContext.RequestServices.GetRequiredService<IUserManager>();
                        try {
                            if (!manager.isUpToDate(userEmail, lastChanged)) {
                                cookieValidatePrincipalContext.ShouldRenew = true;
                                cookieValidatePrincipalContext.ReplacePrincipal(new ClaimsPrincipal(manager.getIdentity(userEmail)));
                            }
                        }
                        catch (UserNotFoundException) {
                            cookieValidatePrincipalContext.RejectPrincipal();
                            return cookieValidatePrincipalContext.HttpContext.SignOutAsync();
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization(options => {
                options.AddPolicy("LoggedInRequired", policy => policy.RequireClaim("email"));
                options.AddPolicy("AdminRequired", policy => policy.RequireClaim("isAdmin", "true", "True"));
            });
            var saveFilePaths = configuration.GetSection("SaveFilesPaths").Get<SaveFilesPathsConfiguration>();
            services.AddSingleton<IUserManager>((x) => {
                var userManager = new PersistentUserManager(saveFilePaths.pathToUsersJsonFile, saveFilePaths.pathToPossiblePriorityRolesJsonFile, configuration);
                return userManager;
            });
            services.AddSingleton<IInfrastructureManager, PersistentInfrastructureManager>(x => {
                var possibleChargeStationTypes =
                    PersistentInfrastructureManager.readPossibleChargeStationTypes(saveFilePaths.pathToPossibleChargeStationTypesJsonFile);
                return new PersistentInfrastructureManager(possibleChargeStationTypes, saveFilePaths.pathToLocationsJsonFile);
            });
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddSingleton<PartialSimulationCache>(x => {
                var possibleChargeStationTypes =
                    PersistentInfrastructureManager.readPossibleChargeStationTypes(saveFilePaths.pathToPossibleChargeStationTypesJsonFile);
                return new PartialSimulationCache(possibleChargeStationTypes);
            });
            services.AddSingleton<SimulationCarsManager>(x => new SimulationCarsManager(saveFilePaths.pathToSimulationCarsJsonFile));
            services.AddSingleton<IBookingManager, PersistentBookingManager>(x => new PersistentBookingManager(saveFilePaths.pathToBookingsJsonFile));
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddSingleton<SimulationLogManager>(x => new SimulationLogManager(saveFilePaths.pathToSimulationLogsJsonFile));
            services.AddHostedService<BookingNotifierBackgroundService>();
            services.AddHostedService<BookingDistributorBackgroundService>();
            services.AddHostedService<ExpireBookingsBackgroundService>();
            services.AddSingleton<IUserUtils, UserUtils>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/home/error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions() {
                ServeUnknownFileTypes = true
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=home}/{action=index}/{id?}");
            });
        }
    }
}