Nota: El desarrollador definió la solución del proyecto y la arquitectura base del proyecto
/sdd-new VehicleCatalog-API-Persistence

## Contexto Actua como Arquitecto de software en .Net y APIRest, el proyecto API necesita terminar la definicion de las conexion a base de datos y terminar de implementar el patron de UnitOfWork utilizando EntityFramework

## Objetivo

Completar las definicion del proyecto de APIRest Ajustar la configuracion de EntityFramework y realizar la conexion a base de datos, terminar el patron UnitOfWork. Antes de realizar cambios revisa el estado actual del proyecto y continua con la arquitectura establecidad.

## Alcance

Configurar EF para una base de datos Local en sql server
Configurar la cadena de conexión vía appsettings.json / variables de entorno (por el momento no existe la base de datos).

## Requisitos funcionales

- La aplicación debe poder conectarse a la base de datos configurada al iniciar.
- Debe soportar operaciones CRUD básicas sobre la entidad de prueba.
- Las migraciones deben poder generarse y aplicarse con "dotnet ef migrations" / "dotnet ef database update".

## Limitaciones

Los entpoint del API Rest se van a especificar mas adelante
Para la base de datos se piensa utilizar una base de datos local por el momento

Nota: El desarrollador implemento el patrón de Meditor para la comunicación entre la capa de API y aplicación con las QUERY, con el fin de que fuese guia para IA la creacion de nuevos endpoints

/sdd-new ImplementacionEnpoints

## Contexto Actua como Arquitecto de software en .Net y APIRest. Se espera la implementacion de nuevos enpoints para el APIRest, tambien el desarrollador ha realizado algunos cambios en el proyecto:
* se agrego el enpoint GetById utilizando el patron mediator para llamar a las Query en API.Application
* Se ajusto la configuracion de las entitys para la migracion en EF, tambien se realizo la primera migracion con data para realizar pruebas con consultas
* Se ajusto el connectionString ahora ya existe una base de datos local a la cual conectarse

## Objetivo 

Se necesita la implementacion de los enpoints CarBrand Y CarModel para la APIRest, deberá seguir el patron definido y la arquitectura ya construida, para ello es necesario analizar los cambios nuevos, tambien es necesario agregar y configurar Swagger para cuando se ejecute el APIRest.

Endpoints a implementar:
| Método | Endpoint                    | Uso                               |
| ------ | --------------------------- | --------------------------------- |
| `GET`  | `/api/carBrands`           | Listado de marcas                 |
| `GET`  | `/api/carModels`           | Todos los modelos con marca y año |
| `GET`  | `/api/carModels?Brand=""` | Modelos filtrados por marca        |

Nota: en el Filtrado por marca debera ingresar un valor de texto con el nombre de la marca


## Requisitos funcionales

- La aplicación debe poder implementar y consultar los endpoints solicitados.
- Debe poder hacer validaciones de datos erroneos, datos inexistentes y/o fuera de rango.
- Las migraciones deben poder generarse y aplicarse con "dotnet ef migrations" / "dotnet ef database update".
- El proyecto actual no cuenta con Readme, crear uno con la informacion del proyecto

## Limitaciones

El proyecto de test aun no existe en la solución, no es necesario crearlo ya que sera propuesto para una proxima session

/sdd-new TestProyect

## Contexto Actua como Arquitecto de software en .Net y APIRest, revisa los endpoints implementados, ahora se procede a crear el proyecto de pruebas.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/carBrands` | Get all car brands |
| GET | `/api/carModels` | Get all car models with brand and year |
| GET | `/api/carModels?brand={name}` | Get models filtered by brand name (case-insensitive) |
| GET | `/carBrand?id={id}` | Get a single car brand by ID |

## Objetivo

Crear un proyecto de pruebas para el proyecto API, en la ruta test/testAPI, probar los endpoints implementados tanto como las pruebas exitosas y las pruebas de error. 

## Pruebas 
Acontinuacion se presenta el conjunto de pruebas propuestas por el desarrollador:
- getAllBrands_ShouldReturnList: Verifica que el método retorne la lista completa de marcas cuando existen registros.
- getBrandById_WithValidId_ShouldReturnBrand: Comprueba que se devuelva la marca correcta al proporcionar un ID existente.
- getBrandById_WithInvalidId_ShouldThrowNotFoundException: Valida que se lance una excepción de recurso no encontrado cuando el ID no existe en el sistema.
- getAllModels_ShouldReturnModelsWithDetails: Asegura que se retornen todos los modelos incluyendo la información de la marca asociada y el año.
- getModelsByBrand_WithExactName_ShouldFilterCorrectly: Comprueba que la consulta devuelva solo los modelos de la marca especificada.
- getModelsByBrand_CaseInsensitive_ShouldReturnFilteredModels: Verifica que el filtrado ignore mayúsculas y minúsculas (por ejemplo, buscar "toyota", "TOYOTA" o "Toyota" devuelva el mismo resultado).
- getModelsByBrand_NonExistentBrand_ShouldReturnEmptyList: Valida que retorne una lista vacía si la marca consultada no tiene modelos registrados o no existe.
- GET /api/carBrandsListar marcas existentes200 OKRetorna un arreglo JSON con la estructura correcta de todas las marcas.
- GET /api/carBrandsBase de datos vacía200 OKRetorna un arreglo JSON vacío [] sin romper la ejecución.
- GET /api/carModelsListar modelos con relaciones200 OKRetorna los modelos incluyendo campos anidados de marca y año.
- GET /api/carModels?brand=ToyotaFiltrado por marca exacta200 OKRetorna únicamente los modelos asociados a Toyota.
- GET /api/carModels?brand=tOyOtAFiltrado case-insensitive200 OKRetorna los modelos de Toyota sin importar variaciones de mayúsculas/minúsculas.
- GET /api/carModels?brand=MarcaInexistenteFiltrado por marca no encontrada200 OK o 404 Not FoundRetorna una lista vacía [] (según diseño REST estándar para colecciones filtradas).
- GET /carBrand?id=1Consultar marca por ID existente200 OKRetorna el objeto JSON con los detalles de la marca solicitada.
- GET /carBrand?id=9999Consultar marca por ID inexistente404 Not FoundRetorna un mensaje de error claro indicando que el recurso no fue encontrado.
- GET /carBrand?id=abcConsultar marca con ID de formato inválido400 Bad RequestValida la protección contra tipos de datos incorrectos en parámetros de consulta.

## Requisitos funcionales 

- Creacion del proyecto de pruebas en la ruta test/testAPI
- El proyecto de pruebas debera hacer pruebas unitarias y de implementacion con base de datos
- Instalar en el proyecto de pruebas Testcontainers para las pruebas de base de datos
- Usar Testcontainers para las pruebas de base de datos
- Actualiza el Readme con lo que se agregue al final y su contenido en español

## Limitaciones

Las pruebas se van a limitar al proyecto API, sugerir las pruebas unitarias y de implementacion que se consideran deberia hacer para otros proyectos


/sdd-new GithubWorkflowCreacion

## Contexto Actua como Arquitecto de software en .Net y APIRest y github administrador, se ha agregado un nuevo ruleset "Require a pull request before merging". Partir de ahora cada new feature tiene que crearse una rama nueva a partir de main (actualizada)

## Crear un nuevo archivo .yaml para Github Workflow para integracion continua en github y el archivo README debe estar actualizado y en español 

## Requisitos funcionales 

- Crear una nueva rama a partir de main
- Crear un archivo .yaml que realice push, pull request,  jobs: Obtener el código, Instalar .NET 10, Restaurar dependencias, Compilar, Ejecutar tests
- el archivo README tiene que estar en español


/sdd-new Sitio Web ASP.NET Core MVC — Catálogo de Modelos de Vehículos

## Contexto

Existe una API REST (OpenAPI 3.1.1) que expone información de marcas y modelos de vehículos, corriendo en `https://localhost:7294/`. Sus endpoints son:

| Método | Ruta | Query params | Respuesta |
|---|---|---|---|
| GET | `/api/carBrands` | — | `CarBrandResponse[]` → `{ id, name }` |
| GET | `/api/carModels` | `brand` (string, opcional) | `CarModelResponse[]` → `{ id, name, year, brandName }` |
| GET | `/CarBrand` | `id` (int, opcional) | `CarBrandDTO` → `{ idCarBrand, brand }` |

El filtro de `/api/carModels` se hace por **nombre de marca** (string), no por id.

## Objetivo

Crear un sitio web **ASP.NET Core MVC (.NET 8)** que consuma esta API y muestre:

- Un listado de **todos los modelos de vehículos**, con su **marca** y **año**.
- Un **filtro por marca** (dropdown poblado desde `/api/carBrands`) que recarga el listado usando `/api/carModels?brand=...`.

No requiere autenticación, base de datos propia ni CRUD: es un sitio de solo lectura que consume la API existente.

## Stack técnico

- ASP.NET Core MVC, .NET 10.
- `IHttpClientFactory` con **HttpClient tipado** para consumir la API (sin librerías externas de terceros).
- Bootstrap 5 (el que trae la plantilla por defecto de MVC).
- Sin autenticación / sin base de datos.

## Estructura de carpetas esperada

```
/Controllers
    VehiculosController.cs
/Models
    CarBrandViewModel.cs
    CarModelViewModel.cs
/Services
    ICarCatalogService.cs
    CarCatalogService.cs
/Views
    /Vehiculos
        Index.cshtml
    /Shared
        _Layout.cshtml
        Error.cshtml
appsettings.json
Program.cs
```

## Modelos (ViewModels)

```csharp
public class CarBrandViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CarModelViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public string BrandName { get; set; } = string.Empty;
}

public class VehiculosIndexViewModel
{
    public List<CarModelViewModel> Modelos { get; set; } = new();
    public List<CarBrandViewModel> Marcas { get; set; } = new();
    public string? MarcaSeleccionada { get; set; }
}
```

## Capa de servicio (consumo de la API)

```csharp
public interface ICarCatalogService
{
    Task<List<CarBrandViewModel>> GetBrandsAsync();
    Task<List<CarModelViewModel>> GetModelsAsync(string? brand = null);
}
```

- `CarCatalogService` implementa la interfaz usando `HttpClient` inyectado.
- `GetModelsAsync` arma la URL como `api/carModels` o `api/carModels?brand={brand}` cuando `brand` no es nulo/vacío.
- Deserializar con `System.Text.Json`, usando `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`.
- Registrar el HttpClient tipado en `Program.cs`:

```csharp
builder.Services.AddHttpClient<ICarCatalogService, CarCatalogService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
});
```

- En `appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7294/"
  }
}
```

## Definición de páginas del MVC

### Página única: Listado de modelos con filtro (página principal)

- **Ruta:** `/` (o `/Vehiculos`, redirigiendo la raíz a esta acción).
- **Controlador:** `VehiculosController`
- **Acción:** `Index(string? marca)` — método `[HttpGet]`.
- **Vista:** `Views/Vehiculos/Index.cshtml`

**Lógica de la acción:**
1. Llama a `GetBrandsAsync()` para poblar el dropdown de marcas.
2. Llama a `GetModelsAsync(marca)` para obtener el listado (filtrado o completo si `marca` es null/"Todas").
3. Arma un `VehiculosIndexViewModel` con ambos resultados + la marca seleccionada.
4. Maneja errores de conexión con la API (try/catch), mostrando un mensaje amigable si falla.

**Contenido de la vista:**
- Formulario `GET` con un `<select asp-for="MarcaSeleccionada">` con opción "Todas las marcas" + una `<option>` por cada marca; al cambiar, se envía automáticamente (o botón "Filtrar").
- Tabla Bootstrap con columnas: **Modelo**, **Marca**, **Año**.
- Mensaje ("No se encontraron modelos para esta marca") si la lista viene vacía.
- Mantener la marca seleccionada en el dropdown tras recargar (usar el valor de `MarcaSeleccionada`).
- Título de página: "Catálogo de Vehículos".

## Manejo de errores

- Si la llamada HTTP falla (API caída, timeout), capturar la excepción y mostrar una vista con mensaje de error, sin romper la página.
- Validar que `brand` viaje correctamente codificado en la URL (usar `Uri.EscapeDataString`).

## Criterios de aceptación

- Al entrar al sitio, se ve el listado completo de modelos con marca y año.
- Al seleccionar una marca en el filtro, la tabla se actualiza mostrando solo los modelos de esa marca.
- Al volver a "Todas las marcas", se muestra el listado completo nuevamente.
- El diseño es responsive y usa los componentes Bootstrap ya incluidos en la plantilla MVC estándar.


/sdd-new Aplicación .NET MAUI — Catálogo de Modelos de Vehículos

## Contexto

Existe una API REST (OpenAPI 3.1.1) que expone información de marcas y modelos de vehículos, corriendo en `https://localhost:7294/`. Sus endpoints son:

| Método | Ruta | Query params | Respuesta |
|---|---|---|---|
| GET | `/api/carBrands` | — | `CarBrandResponse[]` → `{ id, name }` |
| GET | `/api/carModels` | `brand` (string, opcional) | `CarModelResponse[]` → `{ id, name, year, brandName }` |
| GET | `/CarBrand` | `id` (int, opcional) | `CarBrandDTO` → `{ idCarBrand, brand }` |

El filtro de `/api/carModels` se hace por **nombre de marca** (string), no por id.

## Objetivo

Crear una aplicación **.NET MAUI (.NET 10)** simple, de una sola pantalla, que consuma esta API y muestre:

- Un listado de **todos los modelos de vehículos**, con su **marca** y **año**.
- Un **filtro por marca** (Picker poblado desde `/api/carBrands`) que recarga el listado usando `/api/carModels?brand=...`.

**Mantener la app simple:** una sola página, sin DI containers complejos, sin librerías de MVVM de terceros (CommunityToolkit.Mvvm, Prism, etc.), sin navegación entre pantallas. Basta con `HttpClient` + `ObservableCollection` + binding directo desde el code-behind.

## Stack técnico

- .NET MAUI, .NET 8.
- `HttpClient` simple (una sola instancia, sin `IHttpClientFactory` ni DI complejo — se puede instanciar directo en el servicio).
- XAML estándar de MAUI (Picker + CollectionView), sin estilos ni temas custom.
- Sin autenticación, sin base de datos local, sin navegación (una sola `ContentPage`).

## Estructura de carpetas esperada

```
/Models
    CarBrand.cs
    CarModel.cs
/Services
    CarCatalogService.cs
MainPage.xaml
MainPage.xaml.cs
MauiProgram.cs
```

## Modelos

```csharp
public class CarBrand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CarModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public string BrandName { get; set; } = string.Empty;
}
```

## Definición de páginas

### Página única: MainPage (listado + filtro)

**XAML (`MainPage.xaml`):**
- `Picker` en la parte superior, con:
  - Ítem fijo "Todas las marcas".
  - Un ítem por cada marca obtenida de `GetBrandsAsync()`.
- `ActivityIndicator` (visible solo mientras carga).
- `CollectionView` debajo, con `ItemsSource` enlazado a un `ObservableCollection<CarModel>`, mostrando por cada item: **Nombre del modelo**, **Marca**, **Año** (un `Grid` o `VerticalStackLayout` simple por celda).
- `Label` con mensaje "No se encontraron modelos" visible solo si la colección está vacía.

**Code-behind (`MainPage.xaml.cs`):**
- Propiedad `ObservableCollection<CarModel> Modelos` seteada como `BindingContext` (o directamente como `ItemsSource`).
- `OnAppearing()`: carga marcas (llena el Picker) y carga todos los modelos (marca = null) al abrir la app.
- `Picker.SelectedIndexChanged`: vuelve a llamar `GetModelsAsync(marcaSeleccionada)` y refresca la `ObservableCollection` (limpiar y volver a agregar los ítems).
- Mostrar/ocultar el `ActivityIndicator` mientras se espera la respuesta HTTP.
- Envolver las llamadas en `try/catch` y mostrar un `DisplayAlert` simple si la API no responde.

## Configuración y notas de plataforma (importante)

- **Android emulator:** no puede resolver `localhost` como el equipo host. Usar `http://10.0.2.2:PUERTO/` en vez de `https://localhost:7294/` cuando se ejecuta en el emulador de Android.
- **iOS Simulator / Windows:** `https://localhost:7294/` funciona sin cambios.
- Si la API usa certificado de desarrollo HTTPS autofirmado, Android puede rechazar la conexión: para mantenerlo simple en desarrollo, considerar exponer la API también por HTTP y usarla desde el emulador, o configurar el certificado de confianza.
- Sugerencia simple: definir la URL base como constante en `CarCatalogService` y cambiarla manualmente según la plataforma de prueba (evitar lógica condicional compleja de `DeviceInfo.Platform` si se quiere mantener mínimo).

## Manejo de errores

- Si falla la llamada HTTP (API caída, timeout, DNS), capturar la excepción y mostrar un `DisplayAlert("Error", "No se pudo conectar con la API", "OK")`.
- No romper la UI si la lista de marcas o modelos viene vacía.

## Criterios de aceptación

- Al abrir la app, se ve el listado completo de modelos con marca y año.
- Al seleccionar una marca en el Picker, la lista se actualiza mostrando solo los modelos de esa marca.
- Al volver a "Todas las marcas", se muestra el listado completo nuevamente.
- La app corre en al menos un emulador/simulador (Android o Windows) sin necesidad de configuración adicional más allá de la URL base.
**Nota: Es necesario el desarrollo de esta aplicacion sea realizada en una rama nueva y no trabajar sobre main** 


/sdd-new DocumentacionFinal

## Objetivo

Documentar el README.md con las instrucciones de como se deberia ejecutar los proyectos en la solucion

## Requisitos funcionales

El README.md debe tener una nueva seccion de como se deberia ejecutar la applicacion y debera contener las siguientes pasos

1. Configuración de la base de datos
2. Configuración de las cadenas de conexión
3. Ejecución del proyecto API
4. Ejecución del proyecto VehicleCatalog.Web
5. Ejecución del proyecto VehicleCatalog.Maui

### Migraciones y datos iniciales

En el README.md debe contener: Documentar cómo crear y aplicar las migraciones de Entity Framework.

Incluir:

- Prerrequisitos para ejecutar las migraciones.
- Comandos para crear migraciones, si corresponde.
- Comandos para aplicar las migraciones.
- Configuración de datos iniciales (seeds).
- Orden recomendado de ejecución.
- Proyecto donde se encuentra el DbContext.
- Proyecto utilizado como startup project.

Los comandos deben ser compatibles con la estructura real de la solución.

## Requisitos de calidad

- El README.md debe estar escrito en Markdown.
- Utilizar un lenguaje técnico claro y profesional.
- Incluir comandos completos y listos para copiar.

## Criterios de aceptación

- [ ] El README.md permite configurar el proyecto localmente.
- [ ] Se documentan los prerrequisitos.
- [ ] Se documenta la configuración de SQL Server.
- [ ] Se documentan las cadenas de conexión.
- [ ] Se documentan las migraciones y los seeds.
- [ ] Se documenta la ejecución de la API mediante HTTP y HTTPS.
- [ ] Se documentan los puertos de la API.
- [ ] Se documenta la ejecución de VehicleCatalog.Web.
- [ ] Se documentan los puertos de VehicleCatalog.Web.
- [ ] Se documenta la ejecución de VehicleCatalog.Maui.
- [ ] Se documenta la configuración de conectividad de MAUI.
- [ ] Todos los comandos documentados son coherentes con el repositorio.

/sdd-new EvidenciaProcesoIA

## Objetivo

Se necesita documentar el proceso iterativo de desarrollo del proyecto
VehicleCatalog utilizando Spec-Driven Development (SDD)
y gentle-ai.
Nota: Realizar los cambios en una rama nueva en base a main (actualizada)

## Requisitos

Crear un archivo AI-LOG.md en la raíz del repositorio.

En el archivo se debe documentar las iteraciones relevantes
del desarrollo, incluyendo:

1. Especificación inicial.
2. Prompt utilizado con la herramienta de IA.
3. Objetivo del prompt.
4. Resultado o propuesta generada por la IA.
5. Implementación realizada.
6. Validaciones ejecutadas.
7. Problemas identificados.
8. Ajustes realizados.
9. Decisiones técnicas tomadas por el desarrollador.
10. Commit relacionado, cuando exista.

## Reglas

- Analizar las especificaciones SDD existentes.
- Revisar el historial de Git cuando esté disponible.
- No inventar prompts, resultados ni commits.
- Diferenciar las sugerencias de la IA
  de las decisiones del desarrollador.
- Registrar únicamente las iteraciones que puedan
  respaldarse con evidencia.
- Mantener el archivo en formato Markdown.
- Utilizar un lenguaje técnico claro y profesional.

## Criterios de aceptación

- [ ] Existe un archivo AI-LOG.md.
- [ ] Se documentan las iteraciones relevantes.
- [ ] Se incluyen los prompts clave.
- [ ] Se relacionan las especificaciones con la implementación.
- [ ] Se documentan validaciones y ajustes.
- [ ] Se incluyen commits cuando sea posible.
- [ ] No se inventa evidencia del proceso.