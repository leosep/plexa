# Plexa

Plexa es una aplicación web de **gestión de proyectos y operaciones para estudios y agencias**: proyectos, tablero Kanban,
sprints, cronómetro, reportes, analítica, calendario, mensajes y carga del equipo. Está construida como una aplicación
**ASP.NET Core MVC con Razor renderizado en servidor** y una interfaz con acabado profesional (sin SPA, sin jQuery, sin Bootstrap).

La especificación funcional que describe la aplicación de referencia está en [`spec/`](spec/README.md) (16 documentos + capturas).
Plexa la reimplementa con marca y datos propios.

---

## Funcionalidad

| Área | Ruta | Qué incluye |
|---|---|---|
| Acceso | `/login`, `/forgot-password`, `/reset-password`, `/invitations` | Identity con cookie, invitaciones y roles `Admin`/`Member` |
| Dashboard | `/dashboard` | KPIs del sprint, distribución por estado y prioridad, actividad, tiempo del día (tarjetas htmx con skeleton) |
| Tablero | `/board` | 4 columnas, filtros por proyecto/persona/texto, búsqueda que incluye subtareas, arrastre de estado, aprobación |
| Tareas | `/tasks/new`, `/tasks/{id}`, `/tasks/{id}/edit` | Formulario en modal, subtareas (2 niveles), checklist, dependencias con detección de ciclos, bloqueo |
| Tiempo | `/api/v1/time/timer`, `/board/timer/*` | Cronómetro de un solo timer activo por usuario, registro manual, historial |
| Proyectos | `/projects`, `/projects/{id}?tab=` | Listado y detalle con pestañas (resumen, tablero, equipo, sprints, hitos, documentos, actividad) |
| Equipo | `/team` | Carga por persona, capacidad semanal, señales `On track` / `At risk` / `Overloaded` |
| Reportes | `/reports`, `/reports/export` | Tiempo estimado vs registrado, filtros y exportación CSV / Excel / PDF |
| Analítica | `/analytics` | Throughput, cycle time, rework, fiabilidad de estimaciones, flujo acumulado |
| Calendario | `/calendar`, `/api/v1/calendar/events` | Vista mensual, eventos por tipo, alta y detalle |
| Mensajes | `/messages`, `/notifications/*` | Hilos de proyecto y de equipo, menciones, notificaciones con panel |
| Configuración | `/settings?section=` | Perfil, avatar, preferencias de idioma, buzón SMTP con contraseña cifrada |
| Administración | `/admin`, `/admin/members`, `/admin/projects`, `/admin/sprints` | Solo `Admin`: alta de miembros, proyectos, sprints y archivado |

Idioma: inglés por defecto y español, con `IStringLocalizer` + `.resx` y cookie `plexa.culture`.

---

## Stack

| Capa | Tecnología |
|---|---|
| Plataforma | .NET 9 (`net9.0`), C# con `Nullable` e `ImplicitUsings` |
| Web | ASP.NET Core MVC + Razor Views (server-rendered) |
| Estilos | Tailwind CSS v4 (CLI, tokens en `@theme`), tipografía Jost autohospedada |
| Interactividad | htmx 2 + Alpine.js 3 (mejora progresiva) |
| Gráficos / iconos | Chart.js 4, iconos SVG tipo Lucide |
| Datos | EF Core 9 + SQL Server, migraciones, ids `Guid`, `rowversion` |
| Auth | ASP.NET Core Identity (cookie), roles y políticas por pertenencia a proyecto |
| Validación | FluentValidation en servidor, errores inline vía htmx |
| Exportación | CsvHelper, ClosedXML, QuestPDF |
| Correo | MailKit (SMTP del buzón del usuario, contraseña con `IDataProtector`) |
| Logs | Serilog (consola + archivo rotado) |
| Tests | xUnit + FluentAssertions, `WebApplicationFactory`, Playwright .NET |

> **Desviación documentada:** el objetivo original era .NET 10 LTS; el entorno disponible solo tiene el SDK 9.0.102,
> así que la solución compila en `net9.0`. Cambiar `TargetFramework` a `net10.0` es suficiente cuando el SDK esté instalado.

---

## Requisitos

- .NET SDK 9 (o superior) y Git.
- SQL Server accesible (probado contra SQL Server 2012, nivel de compatibilidad 110).
- Para recompilar CSS: Node 20+ (`npm install`) **o** el CLI autónomo de Tailwind en `tools/tailwindcss.exe`.
- Opcional para E2E: navegadores de Playwright (`pwsh bin/Debug/net9.0/playwright.ps1 install chromium`).

---

## Puesta en marcha

1. **Configura los secretos** (nunca se versionan):

   ```bash
   cp .env.example .env
   # edita .env con tu cadena de conexión real
   ```

   La cadena se lee de `ConnectionStrings__Default`. También puedes usar `dotnet user-secrets`:

   ```bash
   dotnet user-secrets set "ConnectionStrings:Default" "Server=...;Database=Plexa;..." --project src/Plexa.Web
   ```

2. **Carga las variables en la sesión** (PowerShell):

   ```powershell
   Get-Content .env | ForEach-Object {
     if ($_ -match '^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*)$') {
       $k = $Matches[1]; $v = $Matches[2].Trim().Trim('"')
       [Environment]::SetEnvironmentVariable($k, $v, 'Process')
     }
   }
   ```

   En bash: `set -a; source .env; set +a`.

3. **Aplica migraciones y datos de ejemplo**:

   ```bash
   dotnet run --project src/Plexa.Web -- --seed
   ```

   En `Development` la aplicación también migra y siembra al arrancar. Si el login no puede crear la base,
   créala vacía con anticipación (`CREATE DATABASE Plexa`).

4. **Arranca**:

   ```bash
   dotnet run --project src/Plexa.Web
   ```

   Abre `http://localhost:5087` (el puerto del perfil `http` de `launchSettings.json`) e inicia sesión con una cuenta del seed:

   | Rol | Correo | Contraseña |
   |---|---|---|
   | Admin | `gerson.cabreja@dwn.com.do` | `Plexa123$` |
   | Member | `lsepulveda@dwn.com.do` | `Plexa123$` |
   | Member | `ymontero@dwn.com.do` | `Plexa123$` |

   > Cambia estas credenciales antes de cualquier despliegue real.

5. **CSS** (solo si tocas clases de Tailwind):

   ```bash
   npm install
   npm run build:css          # o: npm run build:css:standalone
   ```

   `wwwroot/css/site.css` se versiona ya compilado para que un clon funcione sin Node.

---

## Pruebas

```bash
dotnet test Plexa.sln
```

| Proyecto | Qué cubre |
|---|---|
| `tests/Plexa.UnitTests` | Reglas de dominio: progreso, bloqueo por dependencias, ciclos, capacidad, formato de duración, transiciones |
| `tests/Plexa.IntegrationTests` | Rutas y cifras de aceptación de la spec sobre una base temporal `Plexa_Test_<guid>` (se crea y se elimina por prueba). Requiere `Tests__ConnectionString`; sin ella se omiten |
| `tests/Plexa.E2E` | Smoke tests con Playwright (login, tablero, modal). Requieren `E2E__BaseUrl` apuntando a una instancia en marcha; sin ella se omiten |

Las pruebas de integración **nunca** tocan la base de desarrollo `Plexa`.

---

## Docker

La base de datos es externa; el compose solo levanta el servicio web.

```bash
docker compose up --build
```

Expone `http://localhost:8080` y monta volúmenes para `storage/` (archivos y claves de Data Protection) y `logs/`.
`Database__MigrateOnStartup=true` aplica migraciones al arrancar; el seed queda desactivado.

---

## Estructura

```
Plexa.sln
src/Plexa.Domain/          Entidades, enums y reglas puras (progreso, bloqueo, capacidad)
src/Plexa.Application/     Contratos de servicios, DTOs y abstracciones (IClock, IMailer, IFileStorage)
src/Plexa.Infrastructure/  PlexaDbContext, configuraciones EF, migraciones, Identity, servicios, seed, exports, mail
src/Plexa.Web/             Controladores, Vistas Razor, ViewModels, ViewComponents, recursos .resx, wwwroot
Styles/app.css             Entrada de Tailwind (tokens @theme)
tests/                     UnitTests · IntegrationTests · E2E
spec/                      Manual funcional de referencia (fuente de verdad)
```

### Convenciones clave

- Controladores delgados; la lógica vive en `Domain`/`Application` y cada vista usa su ViewModel (nunca entidades).
- Patrón htmx: la acción devuelve la página completa o, si la petición es htmx, un `PartialView`; los eventos al cliente viajan en `HX-Trigger`.
- Errores JSON en RFC 7807 (`ProblemDetails`); páginas 403/404/500 con diseño propio; PRG y antiforgery en todo POST.
- Autorización siempre en servidor (políticas + pertenencia a proyecto); la UI solo oculta controles.
- Fechas en UTC en base de datos, `TimeProvider` inyectado, `async/await` con `CancellationToken`.
- Estilos solo con utilidades Tailwind y los componentes del sistema visual; CSS propio mínimo.
- Textos siempre por `.resx` (inglés por defecto, español en `SharedResource.es.resx`).

---

## Decisiones y supuestos

Están registrados en [`PROGRESS.md`](PROGRESS.md), junto con el estado de cada fase. Los más relevantes:

- `net9.0` en lugar de `net10.0` por disponibilidad del SDK.
- SQL Server en nivel de compatibilidad 110 (2012): sin `OPENJSON`, `STRING_AGG`, `TRIM` ni `CONCAT_WS`.
- `InBacklog` distingue las tareas del backlog: el tablero muestra los 14 nodos (padres + subtareas) y el dashboard/analítica
  los 13 elementos rastreados, igual que las cifras de la spec.
- Las pantallas de administración (sprints, alta de miembros/proyectos, archivado) no aparecen en la spec y se definieron de forma razonable.
- Soporte: [support@plexa.app](mailto:support@plexa.app).

## Licencia

Uso interno / demostración. El código no incluye activos, logo ni datos reales de la aplicación original.
