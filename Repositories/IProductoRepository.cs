using ProductosAPI.Models;

namespace ProductosAPI.Repositories;

// Contrato: indica qué operaciones debe tener el repositorio
public interface IProductoRepository
{
    List<Producto> ObtenerTodos();
    Producto? ObtenerPorId(int id);
    int Insertar(Producto producto);
    bool Actualizar(Producto producto);
    bool Eliminar(int id);
}
