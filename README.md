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

## Flujo dentro del proyecto

Cuando alguien hace una petición, por ejemplo desde Swagger, la información pasa por estas capas:

```
Swagger / Cliente
      │  petición HTTP (GET, POST, PUT, DELETE)
      ▼
Endpoints (ProductosEndpoints.cs)   ← recibe la petición y valida los datos
      │  llama al método de la interfaz
      ▼
IProductoRepository                 ← el contrato: qué se puede hacer
      │
      ▼
ProductoRepository                  ← arma el SQL con parámetros (ADO.NET)
      │  pide una conexión
      ▼
ConexionBD                          ← abre la conexión con la cadena de appsettings.json
      │
      ▼
SQL Server (TiendaDB → Productos)
```

La respuesta regresa por el mismo camino: SQL Server → repositorio (convierte las filas en objetos `Producto`) → endpoint (elige el código HTTP) → Swagger.

### Paso a paso

1. **Program.cs** registra los servicios al arrancar. `ConexionBD` se registra como *Singleton* (una sola para toda la app) y `ProductoRepository` como *Scoped* (uno por petición). Cuando un endpoint pide `IProductoRepository`, .NET le entrega un `ProductoRepository` automáticamente (inyección de dependencias).
2. **El endpoint** recibe la petición. Si trae un producto (POST o PUT), primero lo **valida**: nombre obligatorio, precio y stock no negativos. Si algo está mal, responde 400.
3. **El repositorio** pide una conexión a `ConexionBD`, crea un `SqlCommand` con la instrucción SQL y le agrega **parámetros** (`@id`, `@nombre`...) para evitar la inyección SQL.
4. **Se ejecuta el comando** según el caso:
   - `ExecuteReader()` para los SELECT. El `SqlDataReader` lee fila por fila y cada fila se convierte en un `Producto`.
   - `ExecuteScalar()` para el INSERT. Con `OUTPUT INSERTED.Id` regresa el Id nuevo.
   - `ExecuteNonQuery()` para UPDATE y DELETE. Regresa cuántas filas cambiaron; si es 0, el producto no existía.
5. **Se cierra la conexión** sola gracias a `using`.
6. **El endpoint** elige la respuesta: 200, 201, 204 o 404.
7. **Si falla la base de datos** (servidor apagado, base inexistente...), se lanza una `SqlException`. El método `Ejecutar` la atrapa con `try/catch` y responde 500 con un mensaje claro, en lugar de que la API truene.

### Métodos principales

- **`ObtenerTodos()`**: hace `SELECT` de toda la tabla y recorre el `DataReader` con `while (lector.Read())` para llenar una lista.
- **`ObtenerPorId(id)`**: `SELECT ... WHERE Id = @id`. Si `Read()` encuentra una fila, regresa el producto; si no, regresa `null`.
- **`Insertar(producto)`**: `INSERT ... OUTPUT INSERTED.Id`. Regresa el Id que generó SQL Server.
- **`Actualizar(producto)`**: `UPDATE ... WHERE Id = @id`. Regresa `true` si actualizó al menos una fila.
- **`Eliminar(id)`**: `DELETE ... WHERE Id = @id`. Regresa `true` si borró al menos una fila.
- **`LeerProducto(lector)`**: convierte la fila actual del `DataReader` en un objeto `Producto`. Revisa con `IsDBNull` si la descripción viene vacía.
- **`AgregarParametros(comando, producto)`**: agrega los parámetros comunes del INSERT y del UPDATE.
