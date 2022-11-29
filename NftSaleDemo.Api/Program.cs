using CardanoSharp.Koios.Client;
using Microsoft.EntityFrameworkCore;
using NftSaleDemo.Api.Data;
using NftSaleDemo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddKoios("https://preprod.koios.rest/api/v0");
builder.Services.AddTransient<ITransactionService, TransactionService>();
builder.Services.AddTransient<INftRepository, NftRepository>();
builder.Services.AddTransient<INftSaleRepository, NftSaleRepository>();
builder.Services.AddSingleton<IPolicyManager, PolicyManager>();

string dbConnectionString = builder.Configuration["ConnectionStrings:Default"];
builder.Services.AddDbContext<NftSaleDemoContext>(o =>
    o.UseNpgsql(dbConnectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();


var app = builder.Build();
using var scope = app.Services.CreateScope();
{
    var dbContext = scope.ServiceProvider.GetService<NftSaleDemoContext>();
    if (dbContext == null) throw new Exception("Unable to get db context");
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();