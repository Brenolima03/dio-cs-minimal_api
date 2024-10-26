using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Services;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enums;
using minimal_api.Domain.DTOs;
using minimal_api.Infra.Db;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace minimal_api;

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
    key = Configuration.GetSection("Jwt").ToString() ?? "";
  }
  private readonly string key;

  public IConfiguration Configuration { get; set; } = default!;

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddAuthentication(option => {
      option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(option => {
      option.TokenValidationParameters = new TokenValidationParameters{
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(key)
        ),
        ValidateIssuer = false,
        ValidateAudience = false
      };
    });
    services.AddAuthorization();
    services.AddScoped<IAdminService, AdminService>();
    services.AddScoped<IVehicleService, VehicleService>();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options => {
      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insert the JWT token here"
      });

      options.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
            {
              Type = ReferenceType.SecurityScheme,
              Id = "Bearer"
            }
          },
          Array.Empty<string>()
        }
      });
    });

    services.AddDbContext<Db>(options =>
    {
      options.UseMySql(
        Configuration.GetConnectionString("Mysql"),
        ServerVersion.AutoDetect(Configuration.GetConnectionString("Mysql"))
      );
    });
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints => {
      #region Home
      endpoints.MapGet("/", () => Results.Json(new Home()))
        .AllowAnonymous().WithTags("Home");
      #endregion

      #region Admin
      string GenerateJwtToken(Admin admin)
      {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(
          securityKey, SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>()
        {
          new("Email", admin.Email),
          new("Profile", admin.Profile),
          new(ClaimTypes.Role, admin.Profile)
        };

        var token = new JwtSecurityToken(
          claims: claims,
          expires: DateTime.Now.AddDays(1),
          signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
      }

      endpoints.MapPost("/admins/login", 
        ([FromBody] LoginDTO loginDTO, IAdminService adminService) => {
          var adm = adminService.Login(loginDTO);
          if (adm != null)
          {
            string token = GenerateJwtToken(adm);
            return Results.Ok(new LoggedInAdm
            {
              Email = adm.Email,
              Profile = adm.Profile,
              Token = token
            });
          }
          else return Results.Unauthorized();
        }
      ).AllowAnonymous().WithTags("Admin");

      endpoints.MapPost("/admins", 
        ([FromBody] AdminDTO adminDTO, IAdminService adminService) => {
          var validation = new ValidationErrors{
            Messages = []
          };
          if (string.IsNullOrEmpty(adminDTO.Email))
            validation.Messages.Add("Email cannot be empty!");
          if (string.IsNullOrEmpty(adminDTO.Password))
            validation.Messages.Add("Password cannot be empty!");
          if (validation.Messages.Count > 0)
            return Results.BadRequest(validation);

          var admin = new Admin{
            Email = adminDTO.Email,
            Password = adminDTO.Password,
            Profile = adminDTO.Profile.ToString() ?? Profile.Editor.ToString()
          };

          adminService.Insert(admin);
          return Results.Created();
        }
      ).RequireAuthorization(
        new AuthorizeAttribute { Roles = "Adm" }
      ).WithTags("Admin");

      endpoints.MapGet("/admins", (int? page, IAdminService adminService) => {
        var adms = new List<AdminModelView>();
        var admins = adminService.GetAll(page ?? 1);
        foreach (var adm in admins)
        {
          adms.Add(new AdminModelView{
            Id = adm.Id,
            Email = adm.Email,
            Profile = adm.Profile
          });
        }
        return Results.Ok(adms);
      }).RequireAuthorization(
        new AuthorizeAttribute { Roles = "Adm" }
      ).WithTags("Admin");

      endpoints.MapGet(
        "/admins/{id}", 
        ([FromRoute] int id, IAdminService adminService) => {
          var admin = adminService.GetById(id);
          if (admin == null) return Results.NotFound();
          return Results.Ok(admin);
        }
      ).RequireAuthorization(
        new AuthorizeAttribute { Roles = "Adm" }
      ).WithTags("Admin");

      #endregion

      #region Vehicle
      static ValidationErrors validateDTO(VehicleDTO vehicleDTO)
      {
        var validation = new ValidationErrors{
          Messages = []
        };

        if (string.IsNullOrEmpty(vehicleDTO.Model))
          validation.Messages.Add("Model cannot be null!");
        if (string.IsNullOrEmpty(vehicleDTO.Brand))
          validation.Messages.Add("Brand cannot be null!");
        if (vehicleDTO.Year < 1950)
          validation.Messages.Add("Vehicle too old!");
        return validation;
      }
      endpoints.MapPost(
        "/vehicles", 
        ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) => {
          var validation = validateDTO(vehicleDTO);
          if (validation.Messages.Count > 0)
            return Results.BadRequest(validation);

          var vehicle = new Vehicle {
            Model = vehicleDTO.Model,
            Brand = vehicleDTO.Brand,
            Year = vehicleDTO.Year
          };
          vehicleService.Insert(vehicle);
          return Results.Created($"/vehicles/{vehicle.Id}", vehicle);
        }
      ).RequireAuthorization(
        new AuthorizeAttribute { Roles = "Adm, Editor" }
      ).WithTags("Vehicle");

      endpoints.MapGet("/vehicles",
        (int? page, IVehicleService vehicleService) =>
        {
          var vehicles = vehicleService.GetAll(page ?? 1);
          return Results.Ok(vehicles);
        }
      ).RequireAuthorization(
        new AuthorizeAttribute { Roles = "Adm, Editor" }
      ).WithTags("Vehicle");

      endpoints.MapGet(
        "/vehicles/{id}", 
        ([FromRoute] int id, IVehicleService vehicleService) => {
          var vehicle = vehicleService.GetById(id);
          if (vehicle == null) return Results.NotFound();
          return Results.Ok(vehicle);
      }).RequireAuthorization(
        new AuthorizeAttribute { Roles = "Adm, Editor" }
      ).WithTags("Vehicle");

      endpoints.MapPut(
        "/vehicles/{id}", 
        (
          [FromRoute] int id,
          VehicleDTO vehicleDTO,
          IVehicleService vehicleService
        ) =>
        {
          var validation = validateDTO(vehicleDTO);
          if (validation.Messages.Count > 0)
            return Results.BadRequest(validation);

          var vehicle = vehicleService.GetById(id);
          if (vehicle == null) return Results.NotFound();
          
          vehicle.Model = vehicleDTO.Model;
          vehicle.Brand = vehicleDTO.Brand;
          vehicle.Year = vehicleDTO.Year;
          vehicleService.Update(vehicle);
          return Results.Ok(vehicle);
        }
      ).RequireAuthorization(
        new AuthorizeAttribute { Roles = "Adm" }
      ).WithTags("Vehicle");

      endpoints.MapDelete(
        "/vehicles/{id}", 
        ([FromRoute] int id, IVehicleService vehicleService) => {
          var vehicle = vehicleService.GetById(id);
          if (vehicle == null) return Results.NotFound();
          
          vehicleService.Delete(vehicle);
          return Results.NoContent();
        }
      ).RequireAuthorization(
        new AuthorizeAttribute { Roles = "Adm" }
      ).WithTags("Vehicle");
      #endregion
    });
  }
}
