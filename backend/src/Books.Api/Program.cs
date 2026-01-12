using Books.Api.Clients;
using Books.Api.Repositories;
using Books.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<OpenLibraryClient>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("VistaTiBooksFavorites/1.0 (felipe@local)");
});

builder.Services.AddScoped<BooksService>();
builder.Services.AddScoped<FavoritesRepository>();
builder.Services.AddScoped<FavoritesService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
