using Microsoft.Data.SqlClient;
using ProductosAPI.Data;
using ProductosAPI.Models;

namespace ProductosAPI.Repositories;

// Implementación: aquí se habla con SQL Server usando ADO.NET
public class ProductoRepository : IProductoRepository
{
    private readonly ConexionBD _conexionBD;

    public ProductoRepository(ConexionBD conexionBD)
    {
        _conexionBD = conexionBD;
    }

    // Consultas: SELECT de todos los productos
    public List<Producto> ObtenerTodos()
    {
        var productos = new List<Producto>();

        using SqlConnection conexion = _conexionBD.ObtenerConexion();
        using SqlCommand comando = new SqlCommand(
            "SELECT Id, Nombre, Descripcion, Precio, Stock FROM Productos ORDER BY Id", conexion);

        using SqlDataReader lector = comando.ExecuteReader();
        while (lector.Read())
        {
            productos.Add(LeerProducto(lector));
        }

        return productos;
    }

    // Buscar: SELECT de un producto por su Id
    public Producto? ObtenerPorId(int id)
    {
        using SqlConnection conexion = _conexionBD.ObtenerConexion();
        using SqlCommand comando = new SqlCommand(
            "SELECT Id, Nombre, Descripcion, Precio, Stock FROM Productos WHERE Id = @id", conexion);
        comando.Parameters.AddWithValue("@id", id);

        using SqlDataReader lector = comando.ExecuteReader();
        return lector.Read() ? LeerProducto(lector) : null;
    }

    // Insert: agrega un producto y regresa el Id que le asignó SQL Server
    public int Insertar(Producto producto)
    {
        using SqlConnection conexion = _conexionBD.ObtenerConexion();
        using SqlCommand comando = new SqlCommand(
            @"INSERT INTO Productos (Nombre, Descripcion, Precio, Stock)
              OUTPUT INSERTED.Id
              VALUES (@nombre, @descripcion, @precio, @stock)", conexion);
        AgregarParametros(comando, producto);

        return (int)comando.ExecuteScalar();
    }

    // Actualizar: UPDATE del producto; regresa true si encontró el Id
    public bool Actualizar(Producto producto)
    {
        using SqlConnection conexion = _conexionBD.ObtenerConexion();
        using SqlCommand comando = new SqlCommand(
            @"UPDATE Productos
              SET Nombre = @nombre, Descripcion = @descripcion, Precio = @precio, Stock = @stock
              WHERE Id = @id", conexion);
        AgregarParametros(comando, producto);
        comando.Parameters.AddWithValue("@id", producto.Id);

        return comando.ExecuteNonQuery() > 0;
    }

    // Eliminar: DELETE por Id; regresa true si borró algo
    public bool Eliminar(int id)
    {
        using SqlConnection conexion = _conexionBD.ObtenerConexion();
        using SqlCommand comando = new SqlCommand("DELETE FROM Productos WHERE Id = @id", conexion);
        comando.Parameters.AddWithValue("@id", id);

        return comando.ExecuteNonQuery() > 0;
    }

    // Convierte la fila actual del DataReader en un objeto Producto
    private static Producto LeerProducto(SqlDataReader lector)
    {
        return new Producto
        {
            Id = lector.GetInt32(0),
            Nombre = lector.GetString(1),
            Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2),
            Precio = lector.GetDecimal(3),
            Stock = lector.GetInt32(4)
        };
    }

    // Parámetros SQL: evitan la inyección SQL
    private static void AgregarParametros(SqlCommand comando, Producto producto)
    {
        comando.Parameters.AddWithValue("@nombre", producto.Nombre);
        comando.Parameters.AddWithValue("@descripcion", (object?)producto.Descripcion ?? DBNull.Value);
        comando.Parameters.AddWithValue("@precio", producto.Precio);
        comando.Parameters.AddWithValue("@stock", producto.Stock);
    }
}
