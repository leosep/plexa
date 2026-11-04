# 16 · Mapa de rutas y glosario

Referencia rápida de rutas, parámetros, estados, etiquetas y términos usados en Syncra.

---

## 1. Rutas de la aplicación

| Ruta | Pantalla | Acceso |
|---|---|---|
| `/login` | Inicio de sesión | Público |
| `/forgot-password` | Recuperar contraseña | Público |
| `/invitations` | Aceptar invitación | Público |
| `/dashboard` | Dashboard | Autenticado |
| `/board` | Tablero global | Autenticado |
| `/board?task=<id>` | Tablero con detalle de tarea abierto | Autenticado |
| `/board?q=<texto>` | Tablero filtrado por búsqueda de texto | Autenticado |
| `/projects` | Listado de proyectos | Autenticado |
| `/projects/<projectId>` | Detalle de proyecto (Overview por defecto) | Miembro del proyecto o permiso global |
| `/projects/<projectId>?tab=Overview` | Resumen del proyecto | idem |
| `/projects/<projectId>?tab=Dashboard` | Métricas del proyecto | idem |
| `/projects/<projectId>?tab=Board` | Tablero del proyecto | idem |
| `/projects/<projectId>?tab=Timeline` | Línea de tiempo / fases | idem |
| `/projects/<projectId>?tab=Calendar` | Eventos del proyecto | idem |
| `/projects/<projectId>?tab=Team` | Equipo del proyecto | idem |
| `/projects/<projectId>?tab=Documents` | Documentos del proyecto | idem |
| `/team` | Team workload | Autenticado |
| `/reports` | Reportes | Autenticado |
| `/analytics` | Analítica | Autenticado |
| `/calendar` | Calendario | Autenticado |
| `/messages` | Mensajes | Autenticado |
| `/settings` | Configuración (Profile por defecto) | Autenticado |
| `/settings?section=profile` | Perfil | Autenticado |
| `/settings?section=preferences` | Preferencias | Autenticado |
| `/settings?section=mailbox` | Buzón de envío | Autenticado |

### Parámetros
- `tab`: pestaña del detalle de proyecto.
- `section`: sección de Settings.
- `task`: id de tarea abierta en el tablero.

---

## 2. Estados de tarea

| Estado | Significado | Color |
|---|---|---|
| **To do** | Pendiente de empezar | Gris |
| **In progress** | En desarrollo | Celeste |
| **Review** | En revisión/validación | Durazno |
| **Completed** | Terminada | Verde |

### Transiciones vistas
- To do → In progress, Review
- In progress → To do, Review
- Review → To do, In progress (y Completed con aprobación)
- Completed → reapertura

---

## 3. Prioridades

| Valor | Uso recomendado |
|---|---|
| **High** | Bloquea a otras personas, fecha crítica o cliente esperando |
| **Medium** | Trabajo normal del sprint (valor por defecto) |
| **Low** | Mejoras, deuda técnica no urgente |

---

## 4. Etiquetas (tags)

`Backend`, `Base de datos`, `Diseño`, `Frontend`, `Investigación`, `QA`, `Rendimiento` — además de `None`.

Sirven para:
- Filtrar en el tablero y en reportes.
- Agrupar la exactitud de estimaciones en Analytics (*"Logged vs estimated hours per tag"*).

---

## 5. Estados de proyecto

| Estado | Significado |
|---|---|
| **Planning** | Definición/planificación |
| **In progress** | En ejecución |
| **At risk** | Con riesgo (fechas, carga, bloqueos) |
| **On hold** | Pausado |
| **Completed** | Terminado |
| **Archived** | Archivado |

### Salud (health)
- **On track**: según el plan.
- **At risk**: desviación que requiere atención.

---

## 6. Estados de capacidad (equipo)

| Señal | Significado |
|---|---|
| **On track** | Carga dentro de lo esperado |
| **At risk** | Cerca del límite o con vencidos |
| **Overloaded** | Por encima de la capacidad |
| **Available** | Con margen para más trabajo |

---

## 7. Modos de progreso de una tarea

| Modo | Cálculo |
|---|---|
| **Hours** | Cada subtarea pesa según sus horas estimadas |
| **Percentage** | Cada subtarea pesa igual |

Se elige al crear (`Measure progress by`) y **no se puede cambiar** si ya existen subtareas.

---

## 8. Tipos de evento de calendario

| Kind | Uso |
|---|---|
| **Meeting** | Reuniones de equipo/cliente |
| **Review** | Revisiones de sprint o entregables |
| **Demo** | Demostraciones |
| **Deadline** | Fechas límite |
| **Retro** | Retrospectivas |
| **Other** | Cualquier otro bloqueo |

Campos adicionales: **Place / tool**, **Project**, **Members**, **Departments**.

---

## 9. Glosario de métricas

| Término | Definición |
|---|---|
| **Active tasks** | Tareas abiertas (no completadas) |
| **Overdue** | Tareas con fecha límite pasada y sin completar |
| **Completed this sprint** | Tareas completadas dentro del sprint / total del sprint |
| **Hours closed (7d)** | Horas registradas y cerradas en 7 días, con comparación semanal |
| **Task status distribution** | Reparto del backlog por estado |
| **Team activity** | Acciones por día/semana en las últimas 4 semanas |
| **Task priority** | Distribución de tareas abiertas por prioridad |
| **Capacity** | % de capacidad usada respecto a la disponible |
| **Tracked today** | Tiempo registrado hoy en el cronómetro |
| **Task time** | Horas estimadas de la tarea |
| **Logged** | Tiempo realmente registrado |
| **Variance** | Logged − Task time (positivo = sobreconsumo) |
| **Tasks closed** | Tareas cerradas en el periodo |
| **Cycle time** | Tiempo medio desde el inicio hasta el cierre |
| **Rework** | % de trabajo reabierto/rehecho |
| **Throughput** | Tareas completadas por semana |
| **Cumulative flow** | Evolución acumulada de tareas por estado |
| **Estimate reliability** | % de estimaciones dentro de ±20% |
| **Delivery / plan** | Cumplimiento del plan (target 1.00) |
| **Sprint velocity** | Puntos entregados vs. comprometidos |
| **Load per member** | Carga activa por persona |
| **Estimate accuracy** | Comparación registrado vs. estimado por tag |
| **WIP** | Trabajo en curso (tarjetas en *In progress*) |

---

## 10. Glosario de interfaz

| Elemento | Significado |
|---|---|
| **Breadcrumb** | Ruta de contexto en la cabecera (`Espacio / Página`) |
| **Chip** | Etiqueta compacta clicable o informativa (estado, prioridad, tag) |
| **KPI card** | Tarjeta con un número grande y su contexto |
| **Modal** | Ventana superpuesta (New task, New event, detalle) |
| **Panel inline** | Formulario que sustituye el contenido (Edit team, Edit task) |
| **Toast** | Aviso breve tras una acción |
| **Empty state** | Mensaje cuando no hay datos ("No threads yet") |
| **Skeleton/Loading** | Estados de carga ("Loading…", "Restoring session") |
| **Loose task** | Tarea sin proyecto |
| **Backlog** | Conjunto de tareas pendientes, aún sin sprint |
| **Sprint** | Iteración con fechas, capacidad y puntos comprometidos |
| **Story points** | Unidad relativa de esfuerzo/complejidad |
| **Blocked** | Tarea que espera una dependencia |
| **Depends on** | Dependencias de una tarea |
| **Owner** | Responsable de la tarea |
| **Lead** | Líder del proyecto |
| **Collaborators** | Miembros del proyecto además del lead |
| **Department** | Área de la organización (p. ej. Software Development) |

---

## 11. Observaciones de comportamiento (verificadas en el UI)

1. **Sesión persistente**: con *Keep me signed in* la sesión se restaura al volver; se muestra *"Restoring session"*.
2. **Deep links**: las pestañas de proyecto, secciones de Settings y tareas abiertas usan query params, por lo que se pueden compartir.
3. **Las notificaciones se marcan como leídas al abrir el panel** (el contador desaparece y aparece *"You're all caught up"*).
4. **El botón "Upgrade now"** no abre flujo en la cuenta Member (no hay pantalla de pago expuesta).
5. **El enlace "Help & support"** es un `mailto:support@syncra.app`: abre el cliente de correo.
6. **Un solo cronómetro** activo a la vez; el Dashboard muestra el estado *Paused* cuando no corre.
7. **"Add to backlog"** viene marcado por defecto en New task.
8. **Valores por defecto de New task**: Priority `Medium`, Estimate `4`, Story points `3`, progreso `Hours`.
9. **Las tarjetas del tablero muestran el proyecto** con su código (`PRY-01`) y el nombre truncado.
10. **Los documentos** aceptan hasta **25 MB** por archivo y varios a la vez.
11. **El calendario global** puede mostrar 0 eventos en la semana actual y sí eventos al navegar a otras semanas.
12. **La app es una SPA** (React): la navegación no recarga la página completa; los estados de carga son breves.
13. **Búsqueda global**: la cabecera tiene un buscador que envía a `/board?q=<texto>`; los resultados incluyen subtareas con la ruta `↳ Tarea padre` y un enlace **clear** para limpiar.
14. **"+ Add task"** de una columna abre **New task** con el proyecto precargado (no fija el estado automáticamente).
15. **El idioma y el layout** se guardan **en el navegador**, no en la cuenta.
16. **La actividad del proyecto** registra creaciones, movimientos y finalizaciones con autor y fecha.

---

## 12. Endpoints observados (referencia técnica)

Durante la navegación, la SPA consume una API REST bajo `/api/v1`. Ejemplos vistos en la pestaña de red:

| Endpoint | Uso |
|---|---|
| `GET /api/v1/calendar/events?from=YYYY-MM-DD&to=YYYY-MM-DD` | Eventos del rango visible |
| `GET /api/v1/time/timer` | Estado del cronómetro (204 si no hay) |
| `GET /api/v1/notifications?limit=10` | Notificaciones recientes |
| `GET /api/v1/threads` | Hilos de mensajes |
| `GET /api/v1/media/<id>` | Imágenes/avatares y archivos |

> Esta lista es solo ilustrativa; la documentación de uso no requiere conocer la API.

---

## 13. Cobertura de esta documentación

| Área | Documentada | Evidencia |
|---|---|---|
| Login / logout / sesión | ✅ | Pantallas y flujo |
| Recuperar contraseña | ✅ | Pantalla |
| Invitaciones | ✅ | Pantalla |
| Dashboard | ✅ | Capturas y métricas |
| Tablero | ✅ | Captura, tarjetas, filtros |
| Crear/editar tarea | ✅ | Formularios completos |
| Subtareas/dependencias/checklist | ✅ | Detalle de tarea |
| Cronómetro | ✅ | Dashboard y detalle |
| Proyectos (7 pestañas) | ✅ | Capturas de cada pestaña |
| Documentos | ✅ | Lista, límites y acciones |
| Equipo/capacidad | ✅ | Captura |
| Reportes y exports | ✅ | Captura y tablas |
| Analítica | ✅ | Capturas |
| Calendario y eventos | ✅ | Capturas y formulario |
| Mensajes | ✅ (estado vacío) | Captura |
| Settings (3 secciones) | ✅ | Capturas |
| Notificaciones y menú | ✅ | Capturas |
| Roles y límites | ✅ | Evidencia en UI |
| Admin (sprints, alta de proyectos, miembros, billing) | ⚠️ Referenciado, no accesible con el rol Member | Menciones en la UI |
