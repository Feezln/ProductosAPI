# ProductosAPI

Backend en **.NET 10** que administra productos con operaciones **CRUD** sobre **SQL Server**, usando **ADO.NET** (`Microsoft.Data.SqlClient`). Se prueba con **Swagger**.

## Requisitos

- .NET SDK 10
- SQL Server (la instancia local `localhost`)

## Cómo correrlo

1. Crear la base de datos con el script:
   ```bash
   sqlcmd -S localhost -E -C -i Database\database.sql
   ```
   (o abrir `Database/database.sql` en SQL Server Management Studio y ejecutarlo)
2. Si tu servidor no es `localhost`, cambia la cadena de conexión en `appsettings.json`.
3. Arrancar la API:
   ```bash
   dotnet run
   ```
4. Se abre Swagger en **http://localhost:5196**.

## Estructura del proyecto

| Carpeta / archivo | Qué hace |
|---|---|
| `Database/database.sql` | Crea la base `TiendaDB`, la tabla `Productos` y 3 productos de ejemplo |
| `appsettings.json` | Guarda la cadena de conexión |
| `Models/Producto.cs` | La entidad: Id, Nombre, Descripcion, Precio, Stock |
| `Data/ConexionBD.cs` | **Conexión**: lee la cadena y abre conexiones a SQL Server |
| `Repositories/IProductoRepository.cs` | La interfaz (contrato) con los 5 métodos |
| `Repositories/ProductoRepository.cs` | Implementa la interfaz con ADO.NET (consultas SQL) |
| `Endpoints/ProductosEndpoints.cs` | Las rutas HTTP que se ven en Swagger |
| `Program.cs` | Configuración: registra servicios, Swagger y rutas |

## Endpoints

| Punto | Método | Ruta | Método del repositorio | Respuesta |
|---|---|---|---|---|
| Consultas | GET | `/api/productos` | `ObtenerTodos()` | 200 con la lista |
| Buscar | GET | `/api/productos/{id}` | `ObtenerPorId(id)` | 200 o 404 |
| Insert | POST | `/api/productos` | `Insertar(producto)` | 201 con el producto y su Id |
| Actualizar | PUT | `/api/productos/{id}` | `Actualizar(producto)` | 204 o 404 |
| Eliminar | DELETE | `/api/productos/{id}` | `Eliminar(id)` | 204 o 404 |

Ejemplo de cuerpo para POST y PUT:
```json
{
  "nombre": "Audífonos",
  "descripcion": "Audífonos bluetooth",
  "precio": 599.90,
  "stock": 20
}
```

## ¿Cómo funciona mi proyecto?

Hice una API para administrar productos. Se pueden consultar, buscar, agregar, actualizar y eliminar, y todo se guarda en una base de datos de SQL Server. Para conectarme usé ADO.NET y para probarla usé Swagger.

### Flujo general

Cuando hago una petición desde Swagger, pasa lo siguiente:

1. La petición llega a **ProductosEndpoints**, donde están las rutas. Aquí también reviso que los datos estén bien, por ejemplo que el producto tenga nombre.
2. El endpoint llama a un método de **IProductoRepository**, que es la interfaz donde se define qué operaciones se pueden hacer.
3. **ProductoRepository** es la clase que implementa esa interfaz. Aquí se escribe el SQL.
4. El repositorio le pide una conexión a **ConexionBD**, que usa la cadena de conexión guardada en `appsettings.json`.
5. Se ejecuta el SQL en la base de datos y el resultado regresa por el mismo camino hasta Swagger.

### Métodos que usé

- **ObtenerTodos:** trae todos los productos. Uso un DataReader para leer fila por fila y guardar cada producto en una lista.
- **ObtenerPorId:** busca un producto por su id. Si no existe regresa null y la API responde 404.
- **Insertar:** agrega un producto nuevo y regresa el id que le dio la base de datos.
- **Actualizar:** cambia los datos de un producto. Regresa true si lo encontró.
- **Eliminar:** borra un producto por su id. Regresa true si lo borró.

### Otras cosas importantes

- Uso **parámetros SQL** (`@id`, `@nombre`) para evitar la inyección SQL.
- Uso **using** para que la conexión se cierre sola al terminar.
- Uso **try/catch** para que, si falla la base de datos, la API responda con un mensaje de error en lugar de tronar.
