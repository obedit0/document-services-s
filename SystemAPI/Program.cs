using Application;
using EventListener;
using SystemAPI.Handlers;

/* ********************************************************************************************************          
# * Copyright � 2026 Arify Labs - All rights reserved.   
# * 
# * Info                  : System API Template.
# *
# * By                    : Victor Jhampier Caxi Maquera
# * Email/Mobile/Phone    : victorjhampier@gmail.com | 968991*14
# *
# * Creation date         : 01/01/2026
# * 
# **********************************************************************************************************/

var builder = WebApplication.CreateBuilder(args);

// *** Custom *** - Add Health Checks
builder.Services.AddHealthChecks();

// *** Custom *** - Add Authorizers
//builder.Services.AddApplicationServices(builder.Configuration, builder.Environment.IsDevelopment());
//builder.Services.ConfigureJwtKeycloak(builder.Configuration, builder.Environment.IsDevelopment());
//builder.Services.ConfigureJwtAuthentication(builder.Configuration, builder.Environment.IsDevelopment());
//builder.Services.ConfigureJwtScopes(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.ConfigureArixAuthentication(builder.Configuration);

// *** Custom *** - Add Application Layer
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment.IsDevelopment());

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// *** Custom *** - Add Event Listeners
builder.Services.AddInMemoryEventListenerServices();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    await next.Invoke();    
    if (context.Response.StatusCode != 200 || context.Request.Path.StartsWithSegments("/health"))
    {
        logger.LogWarning("{Method} {StatusCode} {Path}", context.Request.Method, context.Response.StatusCode, context.Request.Path);
    }
});

// Middleware de auditoría Arify (opcional - descomenta para habilitar logging detallado)
// app.UseArifyAudit();

// *** Custom *** - Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// *** Custom *** - Add custom Arify Authentication
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
