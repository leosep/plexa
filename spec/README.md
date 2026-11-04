# Syncra — Manual de usuario completo

Documentación funcional de **Syncra** (`https://syncra.liobella.com`), elaborada recorriendo la aplicación real a través de la interfaz web, con la cuenta:

| Dato | Valor |
|---|---|
| Usuario | `lsepulveda@dwn.com.do` |
| Nombre | Leandro Sepulveda |
| Puesto | Full-Stack Developer |
| Rol | **Member** (Miembro) |
| Espacio de trabajo | **Grupo Dominican Watchman** (3 personas) |

> **Nota sobre el idioma:** la cuenta y el navegador usados tenían el idioma **EN** activo, por lo que la interfaz aparece en inglés. La app tiene selector **ES / EN** (login y menú de usuario). En este manual se citan las etiquetas tal como aparecen en pantalla y se explica su significado en español.

> **Alcance:** se documenta todo lo que un usuario puede ver y hacer desde la interfaz con el rol *Member*. Las áreas que la app reserva a administradores (por ejemplo, crear sprints o proyectos) se indican explícitamente cuando la interfaz las menciona pero no las muestra a este rol.

> **Importante:** esta documentación se generó **solo en modo lectura**. No se creó, editó, movió ni eliminó ningún dato de negocio (tareas, proyectos, eventos, documentos). Solo se abrieron formularios y se cancelaron, y se inició/cerró sesión para documentar el acceso.

---

## Índice

| # | Documento | Contenido |
|---|---|---|
| 01 | [Acceso y cuenta](01-acceso-y-cuenta.md) | Login, sesión recordada, recuperar contraseña, invitaciones, idioma |
| 02 | [Navegación e interfaz](02-navegacion-e-interfaz.md) | Estructura de pantalla, barra lateral, cabecera, breadcrumb, comportamiento responsive |
| 03 | [Dashboard](03-dashboard.md) | KPIs, distribución de estados, actividad del equipo, prioridades, cronómetro, carga del equipo |
| 04 | [Tablero de tareas](04-tablero-de-tareas.md) | Columnas, tarjetas, filtros, menús de movimiento, añadir tarea rápida |
| 05 | [Tareas: crear, editar y tiempo](05-tareas-crear-editar-y-tiempo.md) | Formulario "New task", detalle de tarea, subtareas, dependencias, checklist, timer |
| 06 | [Proyectos](06-proyectos.md) | Listado, filtros, búsqueda y las 7 pestañas del detalle de proyecto |
| 07 | [Equipo y capacidad](07-equipo-y-capacidad.md) | Team workload, señales de capacidad y estados |
| 08 | [Reportes](08-reportes.md) | Horas estimadas vs registradas, filtros, exportaciones, reporte por departamento |
| 09 | [Analítica](09-analitica.md) | Throughput, cycle time, rework, flujo acumulado, exactitud de estimaciones, velocidad |
| 10 | [Calendario](10-calendario.md) | Vistas semana / 2 semanas / mes, eventos, tipos, detalle de evento |
| 11 | [Mensajes](11-mensajes.md) | Canales de proyecto/equipo, estado vacío |
| 12 | [Configuración y perfil](12-configuracion-y-perfil.md) | Profile, Preferences, My mailbox |
| 13 | [Notificaciones y menú de usuario](13-notificaciones-y-menu.md) | Campana, actividad reciente, menú, cerrar sesión |
| 14 | [Roles, permisos y límites](14-roles-permisos-y-limites.md) | Qué puede hacer un Member, qué se reserva a administradores |
| 15 | [Recetas y flujos de trabajo](15-recetas-y-flujos.md) | Guías paso a paso para las tareas más comunes |
| 16 | [Mapa de rutas y glosario](16-mapa-de-rutas-y-glosario.md) | Rutas, parámetros, estados, etiquetas y glosario |

---

## Resumen de la aplicación en 10 líneas

Syncra es una herramienta de **gestión de proyectos y operaciones para estudios/agencias**. Organiza el trabajo en:

1. **Proyectos** (`PRY-01`, `PRY-02`…) con cliente, líder, fechas, presupuesto y salud (health).
2. **Tareas** con estado (To do / In progress / Review / Completed), prioridad, etiqueta, fecha límite, estimación en horas, story points y subtareas.
3. **Sprints** (iteraciones) con capacidad y puntos comprometidos.
4. **Tablero Kanban** por proyecto o global, con filtros por proyecto y persona.
5. **Seguimiento de tiempo** con cronómetro en vivo y registro manual (estimado vs. registrado).
6. **Reportes** de horas y por departamento, exportables a CSV/Excel/PDF.
7. **Analítica** de entrega (throughput, cycle time, rework, flujo acumulado, velocidad).
8. **Calendario** compartido con reuniones, revisiones, demos, deadlines y retros.
9. **Mensajería** por canales de proyecto/equipo.
10. **Equipo** con carga de trabajo, vencidos y señal de capacidad.

---

## Cómo usar este manual

- Cada documento explica **qué se ve**, **para qué sirve** y **cómo se usa paso a paso**.
- Las capturas están en [`docs/img`](img) y se referencian en cada sección.
- Los términos de la interfaz se muestran entre comillas: `"New task"`, `"Move to Review"`, etc.
- Al final, [Recetas y flujos](15-recetas-y-flujos.md) resume los recorridos completos (crear una tarea, moverla, registrar tiempo, exportar un reporte…).

---

## Estado de los datos observados (referencia)

Estos datos son los que existían en el espacio de trabajo al momento del estudio y sirven para interpretar las capturas:

- **Proyecto único:** `PRY-01 · Sistema de Gestión de Capacitaciones Internas`
  - Cliente: *Internal development* · Líder: Leandro Sepulveda
  - Fechas: 01 Oct → 30 Nov · 3/14 tareas completadas · Progreso 17% · Salud: **At risk**
- **Tablero:** 14 tareas totales · 10 activas · 4 vencidas · 3 completadas en el sprint (23%)
- **Equipo:** Gerson Cabreja (Administrator), Leandro Sepulveda (Full-Stack Developer), Yoderick Emmanuel Montero Mejia (Full-Stack Developer)
- **Tiempo:** 42 h estimadas, 1 min registrado (solo una tarea), 7 h cerradas en los últimos 7 días
- **Documento compartido:** `SGCI_Presentacion_Wireframes.pdf` (4.8 MB)
- **Evento de calendario:** `sprint-revision_proyectos`, 09 Oct 15:00–16:00
