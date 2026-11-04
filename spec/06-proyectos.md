# 06 · Proyectos

Ruta de listado: `/projects` · Elemento **"Project detail"** del menú.
Ruta de detalle: `/projects/<id>?tab=Overview|Dashboard|Board|Timeline|Calendar|Team|Documents`

---

## 1. Listado de proyectos

![Listado de proyectos](img/proyectos.jpeg)

### Cabecera
- Título **"Projects"**.
- Subtítulo: *"Manage the portfolio: create, edit, archive and open each project."*

### Tarjetas KPI
| KPI | Ejemplo | Significado |
|---|---|---|
| **Active projects** | `1` | Proyectos en curso |
| **At risk** | `1` | Proyectos con salud en riesgo |
| **Budget in progress** | `US$0` | Presupuesto de proyectos en curso |
| **Average progress** | `17%` | Progreso promedio |

### Filtros por estado (chips con contador)
`All 1` · `In progress 0` · `At risk 1` · `Planning 0` · `On hold 0` · `Completed 0` · `Archived 0`

- **All**: todos.
- **In progress**: en ejecución.
- **At risk**: con problemas (fechas, carga, dependencias…).
- **Planning**: aún en planificación.
- **On hold**: pausados.
- **Completed**: terminados.
- **Archived**: archivados (fuera de la vista operativa).

### Búsqueda
Campo *"Search a project or c…"* (nombre o cliente). Filtra la tabla al escribir.

### Tabla
| Columna | Contenido |
|---|---|
| **PROJECT** | Icono/iniciales, nombre y código (`PRY-01`) |
| **CLIENT** | Cliente (`Internal development`) |
| **LEAD** | Líder con avatar |
| **STATUS** | Chip de estado (`At risk`, `In progress`…) |
| **PROGRESS** | Barra + % |
| **BUDGET** | Presupuesto (`—` si no definido) |
| **DELIVERY** | Fecha de entrega (`30 Nov`) |
| **ACTIONS** | Abrir detalle |

Pie: **"1 of 1 projects"** y ayuda **"Click a name to open its detail"**.

### Abrir un proyecto
Pulsa el **nombre** del proyecto (o su fila) → se abre el detalle con 7 pestañas.

> En el rol **Member** no aparece un botón "New project". La creación/edición/archivo se hace con permisos de administrador (el subtítulo de la página lo menciona).

---

## 2. Detalle de proyecto — cabecera común

Todas las pestañas comparten una cabecera:

![Detalle · Overview](img/proyecto-overview.jpeg)

- Enlace **"← Projects"** para volver.
- Pestañas: **Overview · Dashboard · Board · Timeline · Calendar · Team · Documents**.
- Tarjeta de identidad:
  - Iniciales/cuadrado `SD`.
  - Nombre: **Sistema de Gestión de Capacitaciones Internas**.
  - Chip de salud: **At risk**.
  - Línea meta: `INTERNAL DEVELOPMENT · PRY-01 · 01 Oct → 30 Nov · 3/14 tasks`.
  - **LEAD**: Leandro Sepulveda.
  - **PROGRESS**: 17%.
  - **HEALTH**: At risk.

Las pestañas cambian la URL (`?tab=…`) y se pueden enlazar directamente.

---

## 3. Pestaña Overview

Resumen ejecutivo y operativo.

### Project timeline
- Rango `01 Oct → 30 Nov`.
- Si no hay fases: **"No phases defined yet."** (las fases se definen con permisos de gestión).

### Tasks and actions
Mini-tablero embebido con las 4 columnas y sus tarjetas (To do 1, In progress 0, Review 1, Completed 1). Permite ver el estado sin salir del proyecto.

### Project activity
Cronología de acciones, de la más reciente a la más antigua:
- *Task "Backend núcleo [1]" moved to In review* — Gerson Cabreja · 28 Oct
- *Task "Análisis y diseño" marked done* — Gerson Cabreja · 28 Oct
- *Task "Wireframes y validación con RRHH" marked done* — Gerson Cabreja · 28 Oct
- *Task "Modelo de datos, reglas y contrato de API" marked done* — Gerson Cabreja · 28 Oct
- *Task "Análisis y diseño" moved to In review* — Leandro Sepulveda · 27 Oct
- *Task "CRUD de Usuarios y Sucursales" created* — Gerson Cabreja · 06 Oct
- … (creaciones y movimientos desde el 01 Oct)

**Uso:** auditar quién hizo qué y cuándo; detectar trabajo sin movimiento.

---

## 4. Pestaña Dashboard

![Detalle · Dashboard](img/proyecto-dashboard.jpeg)

Métricas del proyecto:

| Métrica | Ejemplo | Lectura |
|---|---|---|
| **Tasks completed** | `3 / 14` · `21% of the project` | Avance por tareas |
| **Points delivered** | `3 / 9` | Avance por story points |
| **Estimated effort** | `42h 00m` · `11 open tasks` | Esfuerzo estimado pendiente |
| **Overdue** | `5` · `Review today` | Tareas vencidas y por revisar hoy |

### Milestones
Sección para hitos del proyecto. En el ejemplo aparece vacía.

### Project activity
La misma cronología del Overview.

**Uso:** responder "¿cuánto falta?" con dos unidades (tareas y puntos) y ver si el esfuerzo pendiente es alto.

---

## 5. Pestaña Board

![Detalle · Board](img/proyecto-board.jpeg)

- Es el **mismo tablero Kanban** que `/board`, pero **filtrado a este proyecto**.
- Columnas To do / In progress / Review / Completed con sus tarjetas.
- Permite abrir tareas, moverlas, asignar y añadir tareas al proyecto.
- El encabezado del tablero global (`All projects · Backlog · 14 tasks`) cambia aquí al contexto del proyecto.

**Uso:** trabajar el día a día del proyecto sin ruido de otros proyectos.

---

## 6. Pestaña Timeline

![Detalle · Timeline](img/proyecto-timeline.jpeg)

- Título **"Project timeline"** con el rango de fechas.
- Muestra **fases** del proyecto en una línea temporal.
- Si no hay fases: **"No phases defined yet."**

**Uso:** comunicar el plan por etapas (descubrimiento, diseño, desarrollo, QA, entrega) y comparar con las fechas de las tareas.

---

## 7. Pestaña Calendar

![Detalle · Calendar](img/proyecto-calendar.jpeg)

- Lista **"Project events"** del proyecto.
- Enlace **"Open calendar"** → calendario global.
- Cada evento muestra fecha/hora, título y tipo (`Meeting`).

Ejemplo: `09 Oct · 15:00 — sprint-revision_proyectos — Meeting`.

**Uso:** ver reuniones y deadlines del proyecto sin filtrar el calendario completo.

---

## 8. Pestaña Team

![Detalle · Team](img/proyecto-team.jpeg)

### Vista de solo lectura
- **DEPARTMENT**: departamento y área (`Software Development · Area lead: Gerson Cabreja`).
- **PROJECT LEAD**: líder del proyecto con su puesto (`Leandro Sepulveda · Full-Stack Developer · Lead`).
- **COLLABORATORS**: colaboradores (o **"No collaborators yet."**).
- Nota: *"Only the team and people allowed to see every project can open this project and its tasks."*

### Editar equipo
Botón **"Edit team"** → formulario inline:

![Editar equipo](img/equipo-editar.jpeg)

| Campo | Opciones |
|---|---|
| **Department** | `Choose a department`, `Software Development`… |
| **Project lead** | `No lead` o cualquier miembro |
| **Collaborators** | Chips de miembros (selección múltiple) |
| **Cancel / Save team** | Descartar / guardar |

**Importante:** el acceso al proyecto depende de esta configuración. Si no eres del equipo (ni tienes permiso global), no verás el proyecto ni sus tareas.

---

## 9. Pestaña Documents

![Detalle · Documents](img/proyecto-documents.jpeg)

### Qué acepta
Texto de la pantalla:
> *"PDF, Office, OpenDocument, images, text, CSV, Markdown or ZIP · several at once, up to 25 MB each"*

Tipos permitidos (atributo del selector de archivos):
`.pdf, .doc, .docx, .xls, .xlsx, .ppt, .pptx, .odt, .ods, .png, .jpg, .jpeg, .gif, .webp, .txt, .md, .csv, .zip`

- **Varios archivos a la vez**.
- **Máximo 25 MB por archivo**.

### Subir
1. Pulsa **"Attach documents"**.
2. Selecciona uno o varios archivos.
3. Se suben y aparecen en la lista.

### Lista de documentos
Cada archivo muestra:
- Icono por tipo.
- Nombre (`SGCI_Presentacion_Wireframes.pdf`).
- Tamaño · autor · fecha (`4.8 MB · Gerson Cabreja · 14 Oct`).
- **Download**.
- **Delete** (con confirmación; requiere permisos).

**Uso:** centralizar entregables, contratos, wireframes y evidencias junto al proyecto.

---

## 10. Flujo recomendado de un proyecto

1. **Listado** → filtra por estado y busca el proyecto.
2. **Overview** → entiende salud, fechas, avance y actividad reciente.
3. **Dashboard** → revisa tareas/puntos y vencidos.
4. **Board** → trabaja las tareas del día.
5. **Timeline** → valida el plan por fases.
6. **Team** → confirma quién participa y ajusta colaboradores (si tienes permiso).
7. **Calendar** → revisa reuniones y deadlines.
8. **Documents** → guarda y comparte entregables.

---

## 11. Preguntas frecuentes

**¿Por qué no veo un proyecto?**
Porque no eres del equipo y no tienes permiso para ver todos los proyectos. Pide que te añadan en **Team → Edit team**.

**¿Por qué el progreso es 17% si hay 3 de 14 tareas?**
21% por tareas, pero el progreso del proyecto (17%) puede ponderar por horas o puntos.

**¿Quién puede crear proyectos?**
El subtítulo indica que se crean/editan/archivan desde esta página; en el rol Member no aparece la acción. Lo hace un administrador.

**¿Se pueden mover tareas entre proyectos?**
Sí, desde **Edit** de la tarea, siempre que no haya empezado. Sus subtareas se mueven con ella y **se pierden las dependencias**.
