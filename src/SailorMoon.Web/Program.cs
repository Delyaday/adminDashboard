using DocumentSql.Indexes;

using Foundation.Data.Migrations;

using SailorMoon.Web;
using SailorMoon.Web.Records;
using SailorMoon.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFoundation();

builder.Services.AddSingleton<IIndexProvider, ClientRecordIndexProvider>();
builder.Services.AddSingleton<IIndexProvider, UserDescriptionRecordIndexProvider>();
builder.Services.AddSingleton<IIndexProvider, VisitRecordIndexProvider>();
builder.Services.AddSingleton<IDataMigration, Migrations>();
builder.Services.AddScoped<IClientsService, ClientsService>();
builder.Services.AddScoped<IServicesService, ServicesService>();
builder.Services.AddScoped<IUserDescriptionService, UserDescriptionService>();
builder.Services.AddScoped<IVisitsService, VisitsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseFoundation();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapFallbackToFile("index.html");
});

app.Run();