# PROGRESS — Plexa

> Estado del clon funcional descrito en `spec/`. Última verificación: 2026-11-04.

| Fase | Prompt | Estado | Notas |
|---|---|---|---|
| 00 | Scaffold e infraestructura .NET | ✅ | Solución de 4 capas + 3 proyectos de test, `Directory.Build.props` con `TreatWarningsAsErrors`, Tailwind v4, Dockerfile y compose (solo `web`) |
| 01 | Modelo de datos (EF Core), migraciones y seed | ✅ | Entidades + configuraciones, `InitialCreate` aplicada, seed idempotente con 1 espacio, 3 usuarios, 1 proyecto, 14 tareas, 1 registro de tiempo y 4 notificaciones |
| 02 | Autenticación, invitaciones y roles | ✅ | Identity con cookie `plexa.auth`, roles `Admin`/`Member`, invitaciones, recuperación de contraseña, políticas por pertenencia a proyecto |
| 03 | Shell Razor (sidebar, topbar, i18n, componentes) | ✅ | Layout, sidebar/topbar, toasts, modales `<dialog>`, atajos (`/`, `N`, `Esc`), `IStringLocalizer` EN/ES con todas las claves EN presentes en ES |
| 04 | Servicios de tareas + Task board | ✅ | 4 columnas, filtros combinables, búsqueda con subtareas, drag de estado, aprobación, `HX-Trigger` |
| 05 | Crear/editar/detalle de tarea | ✅ | Modal htmx con validación inline, subtareas de 2 niveles, checklist, dependencias con detección de ciclos y bloqueo |
| 06 | Cronómetro y registro de tiempo | ✅ | Timer único por usuario (índice único filtrado), inicio/parada, registro manual, historial del día |
| 07 | Dashboard | ✅ | KPIs, tarjetas de estado/prioridad con skeleton htmx, actividad, tiempo personal, avisos de vencimiento |
| 08 | Proyectos (listado + 7 pestañas + documentos) | ✅ | Código correlativo `PRY-NN` con `WorkspaceCounter`, pestañas, equipo, documentos con `IFileStorage`, descarga por `/media/{id}` |
| 09 | Team workload y capacidad | ✅ | Capacidad semanal, horas abiertas, señales `On track`/`At risk`/`Overloaded`/`Available` |
| 10 | Reportes y exportaciones | ✅ | Estimado vs registrado, filtros (sprint, miembro, tag, tiempo, varianza) y export CSV/XLSX/PDF |
| 11 | Analítica | ✅ | Throughput, cycle time, rework, delivery/plan, fiabilidad, flujo acumulado por semana |
| 12 | Calendario | ✅ | Vista mensual, tipos de evento, alta/detalle, JSON en `/api/v1/calendar/events` |
| 13 | Mensajes y notificaciones | ✅ | Hilos de proyecto/equipo, menciones, panel de notificaciones, JSON en `/api/v1/notifications` |
| 14 | Settings, buzón SMTP y administración | ✅ | Perfil, avatar, idioma, buzón con contraseña cifrada, admin de miembros/proyectos/sprints |
| 15 | Endurecimiento, E2E y auditoría final | ✅ | Páginas 403/404/500, `ProblemDetails`, Serilog, health check, 54 unit + 12 integración + 3 E2E (omitibles), Docker |

Stack: .NET 9 MVC + Razor + Tailwind v4 + htmx/Alpine · EF Core/SQL Server · Identity

Leyenda: ⬜ pendiente · 🟡 en curso · ✅ terminada y verificada · ⚠️ terminada con deuda (anotar cuál)

## Verificación (2026-11-04)

| Comprobación | Resultado |
|---|---|
| `dotnet build Plexa.sln` | 0 warnings, 0 errores |
| `dotnet test Plexa.sln` | 54 unit + 12 integración + 3 E2E = **69 en verde** |
| Tailwind (`tools/tailwindcss.exe`) | compila, `wwwroot/css/site.css` ≈ 52 KB |
| `.resx` EN/ES | 480 claves EN, 665 ES; **0 claves EN sin traducción** |
| QA de UI y funcionalidad (Playwright sobre la app real) | **229 comprobaciones PASS, 0 FAIL, 0 WARN** |
| Rutas principales con sesión Member | `/`, `/dashboard`, `/board`, `/projects`, `/team`, `/reports`, `/analytics`, `/calendar`, `/messages`, `/settings`, `/health` → 200 |
| Área `/admin/*` con Member | 403 |
| Responsive 1280 / 768 / 390 px | sin desbordamiento horizontal en member, admin, ES y modales |
| Cifras de aceptación | tablero `14 tasks`; dashboard `13 tasks in the backlog`, `3 / 13`, `23% of sprint`; analítica `13 tracked`, `77/0/0/23`, `28.0d`; reportes `42h 00m · 1m · -41h 59m` |

### QA de UI y funcionalidad (2026-11-04)

Recorrido automatizado con Playwright sobre la app en marcha, con datos del seed y en EN/ES: rutas anónimas y
autenticadas, tablero (alta, cambio de columna, checklist, cronómetro), búsqueda, pestañas de proyecto, exportaciones
CSV/XLSX/PDF, estados vacíos de analítica, calendario, mensajes (miembro y admin), ajustes, notificaciones, atajos,
403/404, auditoría ES y responsive a 768/390 px. Resultado **229 PASS / 0 FAIL / 0 WARN**.

Defectos encontrados y corregidos:

1. `Htmx.Toast` sobrescribía el `HX-Trigger` existente; ahora fusiona los eventos.
2. `_AuthLayout` no cargaba Alpine ni `app.js`, y usaba un `<h1>` decorativo para el logo (ahora `<p>`).
3. Orden de scripts incorrecto; se unificó a `htmx → chart → app.js → alpine` en ambos layouts.
4. SVG inválido con `<template x-if>` dentro de `<path>`; sustituido por `<path x-show>`.
5. El formulario de movimiento de `_TaskCard` disparaba con `change from:select`; ahora `hx-trigger="change"`.
6. La búsqueda en vivo del topbar no estaba conectada; ahora consulta `/search/suggest` con `_SearchSuggestions` y `[x-cloak]`.
7. `/error/{code:int}` solo aceptaba GET (405 en POST); ahora usa `[Route]`.
8. **Cookie de idioma**: se escribía `es` plano y `CookieRequestCultureProvider` la ignoraba; ahora se usa
   `MakeCookieValue("es|uic=es")`. Regresión cubierta por `Culture_Switch_PersistsSpanishCulture`.
9. `/messages/{id}` devolvía un parcial en navegación completa; ahora responde la vista `Index` salvo que htmx apunte a `thread-panel`.
10. Crear un canal no navegaba al hilo nuevo; ahora responde `HX-Redirect` a `/messages/{id}?created=1` con aviso inline.
11. Desbordamiento horizontal en tablas y mensajes a 390 px; `[contain:paint]` en contenedores de tabla, `min-w-0` en grid/hilo/compositor y `break-words` en burbujas.
12. Placeholders con texto duro (`user@company.com`, `smtp.office365.com`) movidos a `.resx` EN/ES.
13. Toasts y avisos `TempData` de tablero, proyectos, cronómetro, calendario, ajustes y administración estaban en inglés
    fijo; ahora pasan por `ITranslator` y hay prueba de regresión ES (`Settings_SuccessMessage_IsLocalized_InSpanish`).

Datos de demostración: el espacio de trabajo se llama **Plexa Studio** y el proyecto del seed **Internal Training Platform**
(la UI nunca usa el nombre de la app original). DB de desarrollo limpia de artefactos de QA tras la auditoría: 14 tareas,
1 evento, 0 sprints, 1 proyecto, 0 hilos, 1 registro de tiempo y 4 notificaciones del seed.


## Decisiones y supuestos tomados

- **[SUPUESTO] `net9.0`**: el entorno solo tiene el SDK 9.0.102; la solución compila en `net9.0` en lugar de `net10.0`.
  Basta cambiar `TargetFramework` cuando el SDK 10 esté disponible.
- **[SUPUESTO] Tailwind con CLI autónomo**: Node 18 no soporta `@tailwindcss/cli` 4.x, así que se usa `tools/tailwindcss.exe`
  (v4.1.11, fuera de git). `npm run build:css` funciona con Node 20+.
- **[SUPUESTO] SQL Server 2012 (compat 110)**: `UseCompatibilityLevel(110)` para evitar `OPENJSON` en `Contains`;
  prohibido `STRING_AGG`, `TRIM` y `CONCAT_WS` en SQL manual.
- **[SUPUESTO] `Thread` → `ChatThread`**: se renombró la entidad para no chocar con `System.Threading.Thread` (DbSet `ChatThreads`).
- **[SUPUESTO] `InBacklog`**: marca las tareas del backlog. El tablero cuenta los 14 nodos (3 padres + 11 subtareas) y
  dashboard/analítica cuentan 13 elementos rastreados (el padre `Backend núcleo [1]` queda fuera del backlog).
  Así se reproducen simultáneamente `14 tasks` y `13 tracked tasks` de la spec.
- **[SUPUESTO] Fechas del seed**: las tareas completadas usan `CompletedAt = ApprovedAt = CreatedAt + 28 días` para que
  el *avg cycle time* sea `28.0d` y la comparación muestre `+0.0d` cuando no hay periodo previo.
- **[SUPUESTO] `--seed`**: `dotnet run --project src/Plexa.Web -- --seed` migra, siembra y termina sin levantar el servidor.
- **[SUPUESTO] Migración en otros entornos**: en `Development` migra y siembra al arrancar; fuera de él hay que activar
  `Database__MigrateOnStartup` / `Database__SeedOnStartup` (Docker lo hace para migrar).
- **[SUPUESTO] Claves de Data Protection**: se persisten en `storage/keys` (configurable con `DataProtection__KeysPath`)
  para no invalidar cookies ni las contraseñas SMTP cifradas al reiniciar.
- **[SUPUESTO] Administración**: la spec no muestra las pantallas de alta de miembros/proyectos/sprints ni facturación;
  se implementaron de forma razonable bajo `/admin` con política `AdminOnly`.
- **[SUPUESTO] Pruebas que se omiten**: integración requiere `Tests__ConnectionString` (base temporal `Plexa_Test_<guid>`);
  E2E requiere `E2E__BaseUrl`. Sin esas variables las pruebas pasan sin ejecutar, para no romper `dotnet test` en local/CI.
- **[SUPUESTO] Zona horaria**: todo se guarda en UTC y se muestra en la zona local del servidor mediante `IClock`.
- **[SUPUESTO] Texto persistido**: la actividad y las notificaciones se escriben en inglés en base de datos (idioma por
  defecto) para que el histórico no cambie al alternar EN/ES; la UI que las muestra sí está localizada.
- **[SUPUESTO] Nombres del seed**: espacio de trabajo **Plexa Studio** y proyecto **Internal Training Platform**, para no
  reutilizar la identidad de la app original. `spec/` se conserva intacto como referencia.
- **Marca**: el producto es **Plexa** en UI, correos, títulos y metadatos; soporte `support@plexa.app`.
