# VehicleCatalog REST API

![CI](https://github.com/arnol111/VehicleCatalog/actions/workflows/ci.yml/badge.svg)

API REST construida con .NET 10 siguiendo los principios de Clean Architecture. Expone endpoints para consultar marcas y modelos de autos almacenados en una base de datos SQL Server.

---

## Descripción

VehicleCatalog es una API REST desarrollada en .NET 10 con ASP.NET Core que implementa Clean Architecture. Permite gestionar un catálogo de marcas y modelos de vehículos mediante endpoints HTTP, con separación estricta de capas y un mediador personalizado liviano (sin MediatR).

---

## Arquitectura

La solución está organizada en cuatro proyectos, cada uno con una responsabilidad delimitada:

| Proyecto | Rol |
|----------|-----|
| `API` | Host ASP.NET Core — controladores, pipeline de middleware, raíz de composición DI |
| `API.Application` | Casos de uso — queries, handlers, DTOs, interfaces de dominio (sin dependencias de framework) |
| `API.Domain` | Entidades del dominio, interfaces de repositorio, excepciones de dominio |
| `API.Infrastructure` | `DbContext` de EF Core, implementaciones de repositorios, registro DI (`AddInfrastructure`) |

El flujo de solicitudes es: `Controlador → IDispatcher → IRequestHandler → IUnitOfWork → Repositorio → DbContext`.

---

## Stack tecnológico

- **.NET 10** — framework objetivo
- **ASP.NET Core 10** — host web y pipeline HTTP
- **Entity Framework Core 10** — ORM y migraciones
- **Scalar / OpenAPI** — documentación interactiva de la API (solo en desarrollo)
- **Mediador personalizado** — patrón liviano `IDispatcher` / `IRequestHandler<TRequest, TResponse>` (sin MediatR)
- **xUnit** — framework de pruebas
- **NSubstitute** — mocks para pruebas unitarias
- **Testcontainers** — SQL Server en contenedor para pruebas de integración

---

## Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server o SQL Server LocalDB
- Docker Desktop (solo para pruebas de integración)

---

## Configuración y ejecución local

1. **Clonar el repositorio**

   ```bash
   git clone <repo-url>
   cd VehicleCatalog
   ```

2. **Configurar la cadena de conexión**

   Editar `src/API/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VehicleCatalog;Trusted_Connection=True;"
     }
   }
   ```

3. **Aplicar migraciones de base de datos**

   ```bash
   dotnet ef database update --project src/API.Infrastructure --startup-project src/API
   ```

4. **Ejecutar la API**

   ```bash
   dotnet run --project src/API
   ```

   La API inicia en `https://localhost:7xxx` (el puerto se imprime al arrancar).

---

## Endpoints de la API

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/carBrands` | Obtener todas las marcas de autos |
| GET | `/api/carModels` | Obtener todos los modelos con marca y año |
| GET | `/api/carModels?brand={nombre}` | Obtener modelos filtrados por nombre de marca (insensible a mayúsculas) |
| GET | `/carBrand?id={id}` | Obtener una marca de auto por ID |

### Formatos de respuesta

**`GET /api/carBrands`**
```json
[{ "id": 1, "name": "Toyota" }, ...]
```

**`GET /api/carModels`** y **`GET /api/carModels?brand=toyota`**
```json
[{ "id": 1, "name": "Corolla", "year": 2022, "brandName": "Toyota" }, ...]
```

**`GET /carBrand?id=1`**
```json
{ "idCarBrand": 1, "brand": "Toyota" }
```

### Códigos de estado

| Código | Significado |
|--------|-------------|
| 200 | Éxito |
| 400 | El parámetro `brand` es una cadena vacía |
| 404 | Nombre de marca no encontrado (consulta filtrada de modelos) |
| 500 | Error inesperado del servidor |

---

## Documentación interactiva (Scalar)

Cuando la API se ejecuta en modo **Development**, la interfaz interactiva de Scalar está disponible en:

```
https://localhost:<puerto>/scalar/v1
```

El spec OpenAPI en formato JSON se encuentra en:

```
https://localhost:<puerto>/openapi/v1.json
```

---

## Migraciones

**Aplicar migraciones existentes a la base de datos:**

```bash
dotnet ef database update --project src/API.Infrastructure --startup-project src/API
```

**Agregar una nueva migración tras modificar entidades o configuraciones:**

```bash
dotnet ef migrations add <NombreMigracion> --project src/API.Infrastructure --startup-project src/API
```

Revisar el archivo de migración generado antes de aplicar para confirmar que no hay cambios de esquema inesperados.

---

## Pruebas

El proyecto `test/testAPI` contiene la suite de pruebas automatizadas. Incluye pruebas unitarias para los handlers CQRS y pruebas de integración completas contra SQL Server real mediante Testcontainers.

### Clases de prueba

| Clase | Tipo | Cobertura |
|-------|------|-----------|
| `GetAllBrandsQueryHandlerTests` | Unitaria | Handler que retorna todas las marcas |
| `GetByIdQueryHandlerTests` | Unitaria | Handler que retorna una marca por ID; lanza `NotFoundException` si no existe |
| `GetAllModelsQueryHandlerTests` | Unitaria | Handler de modelos: sin filtro, filtro exacto, insensible a mayúsculas, marca inexistente |
| `CarBrandsControllerTests` | Integración | `GET /api/carBrands` con datos y con base de datos vacía |
| `CarModelsControllerTests` | Integración | `GET /api/carModels` con y sin filtro por marca, case-insensitive, marca inexistente (404) |
| `CarBrandByIdControllerTests` | Integración | `GET /carBrand?id=` con ID válido (200), inexistente (404) y no numérico (400) |

### Ejecutar pruebas unitarias (no requieren Docker)

```bash
dotnet test --filter "Category=Unit" --configuration Release
```

### Ejecutar pruebas de integración (requieren Docker Desktop en ejecución)

```bash
dotnet test --filter "Category=Integration"
```

> ⚠️ Las pruebas de integración levantan un contenedor SQL Server automáticamente mediante Testcontainers. Docker Desktop debe estar en ejecución antes de ejecutar este comando.

### Ejecutar todas las pruebas

```bash
dotnet test
```

---

## Flujo de trabajo con ramas

El repositorio tiene habilitada la regla **"Require a pull request before merging"** en `main`. Nunca se debe hacer push directo a `main`.

### Convención de nombres

- Nuevas funcionalidades: `feature/nombre-feature`
- Correcciones: `fix/nombre-fix`

### Proceso de trabajo

1. Crear una rama desde `main`:
   ```bash
   git checkout -b feature/nombre-feature
   ```
2. Implementar los cambios y hacer commits con mensajes descriptivos.
3. Abrir un Pull Request hacia `main`.
4. El CI se ejecuta automáticamente — el PR no puede mergearse hasta que el workflow pase.
5. Una vez aprobado y con CI en verde, hacer merge.

---

## Integración continua

El repositorio cuenta con un workflow de GitHub Actions (`.github/workflows/ci.yml`) que se activa automáticamente en cada `push` o `pull_request` dirigido a `main`.

El workflow ejecuta los siguientes pasos en un runner `ubuntu-latest`:

1. Checkout del código
2. Instalación de .NET 10 SDK
3. Restauración de dependencias (`dotnet restore`)
4. Compilación en modo Release (`dotnet build --configuration Release`)
5. Ejecución de pruebas unitarias (`dotnet test --filter "Category=Unit" --configuration Release`)

Las pruebas de integración **no se ejecutan en CI** porque requieren Docker. Se ejecutan localmente o en un entorno con Docker disponible.

Los resultados del workflow se pueden ver en la pestaña **Actions** del repositorio en GitHub.
