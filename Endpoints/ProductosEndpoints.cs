using Microsoft.Data.SqlClient;
using ProductosAPI.Models;
using ProductosAPI.Repositories;

namespace ProductosAPI.Endpoints;

// Rutas HTTP: reciben la petición, llaman al repositorio y regresan la respuesta
public static class ProductosEndpoints
{
    public static void MapProductosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/productos").WithTags("Productos");

        // GET /api/productos -> Consultas
        group.MapGet("/", (IProductoRepository repo) =>
            Ejecutar(() => Results.Ok(repo.ObtenerTodos())));

        // GET /api/productos/{id} -> Buscar
        group.MapGet("/{id:int}", (IProductoRepository repo, int id) => Ejecutar(() =>
        {
            var producto = repo.ObtenerPorId(id);
            return producto is null
                ? Results.NotFound(new { mensaje = "Producto no encontrado" })
                : Results.Ok(producto);
        }));

        // POST /api/productos -> Insert
        group.MapPost("/", (IProductoRepository repo, Producto producto) => Ejecutar(() =>
        {
            var error = Validar(producto);
            if (error is not null) return Results.BadRequest(new { mensaje = error });

            producto.Id = repo.Insertar(producto);
            return Results.Created($"/api/productos/{producto.Id}", producto); //201
        }));

        // PUT /api/productos/{id} -> Actualizar
        group.MapPut("/{id:int}", (IProductoRepository repo, int id, Producto producto) => Ejecutar(() =>
        {
            var error = Validar(producto);
            if (error is not null) return Results.BadRequest(new { mensaje = error });

            producto.Id = id;
            return repo.Actualizar(producto)
                ? Results.NoContent() //204
                : Results.NotFound(new { mensaje = "Producto no encontrado" }); //404
        }));

        // DELETE /api/productos/{id} -> Eliminar
        group.MapDelete("/{id:int}", (IProductoRepository repo, int id) => Ejecutar(() =>
            repo.Eliminar(id)
                ? Results.NoContent() //204
                : Results.NotFound(new { mensaje = "Producto no encontrado" }))); //404
    }

    // Excepciones: si falla la base de datos, regresamos un error 500 con un mensaje claro
    private static IResult Ejecutar(Func<IResult> accion)
    {
        try
        {
            return accion();
        }
        catch (SqlException ex)
        {
            return Results.Problem(title: "Error de base de datos", detail: ex.Message, statusCode: 500);
        }
    }

    // Revisa que los datos del producto sean válidos antes de guardarlos
    private static string? Validar(Producto producto)
    {
        if (string.IsNullOrWhiteSpace(producto.Nombre)) return "El nombre es obligatorio";
        if (producto.Precio < 0) return "El precio no puede ser negativo";
        if (producto.Stock < 0) return "El stock no puede ser negativo";
        return null;
    }
}
