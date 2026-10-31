using ProductosAPI.Data;
using ProductosAPI.Endpoints;
using ProductosAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Swagger para probar la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Nuestros servicios
builder.Services.AddSingleton<ConexionBD>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //Swagger se muestra directo en http://localhost:5196
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductosAPI v1");
        options.RoutePrefix = string.Empty;
    });
}

//Endpoints
app.MapProductosEndpoints();

app.Run();
