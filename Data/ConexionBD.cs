using Microsoft.Data.SqlClient;

namespace ProductosAPI.Data;

// Conexión: lee la cadena de conexión de appsettings.json y crea conexiones a SQL Server
public class ConexionBD
{
    private readonly string _cadena;

    public ConexionBD(IConfiguration configuracion)
    {
        _cadena = configuracion.GetConnectionString("TiendaDB")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'TiendaDB' en appsettings.json");
    }

    // Cada llamada regresa una conexión nueva y ya abierta
    public SqlConnection ObtenerConexion()
    {
        var conexion = new SqlConnection(_cadena);
        conexion.Open();
        return conexion;
    }
}
