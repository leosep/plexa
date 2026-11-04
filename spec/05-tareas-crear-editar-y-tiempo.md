# 05 · Tareas: crear, editar y registrar tiempo

Este documento cubre el ciclo de vida completo de una tarea: crearla, verla, editarla, desglosarla en subtareas, gestionar dependencias, usar el checklist y registrar tiempo.

---

## 1. Crear una tarea

Desde **Task board → "New task"**, **Dashboard → "New task"** o **"+ Add task"** en una columna.

![Nueva tarea — parte superior](img/nueva-tarea-1.jpeg)

![Nueva tarea — parte inferior](img/nueva-tarea-2.jpeg)

### Campos del formulario

| Campo | Tipo | Obligatorio | Descripción / opciones |
|---|---|---|---|
| **Title** | texto | Sí | Qué hay que hacer. Placeholder: *"What needs to be done?"* |
| **Project** | select | Sí | Proyecto destino o **"Loose task (no project)"** (tarea suelta) |
| **Assignee** | select | No | `Unassigned` o cualquier miembro del espacio |
| **Priority** | select | No | `High`, `Medium` (por defecto), `Low` |
| **Tag** | select | No | `None`, `Backend`, `Base de datos`, `Diseño`, `Frontend`, `Investigación`, `QA`, `Rendimiento` |
| **Due date** | selector de día | No | Fecha límite (`Select a day` / `No date`) |
| **Start** | selector de día | No | Fecha de inicio (`Select a day` / `No date`) |
| **Estimate (hours)** | número | No | Estimación en horas (por defecto `4`) |
| **Story points** | número | No | Puntos de historia (por defecto `3`) |
| **Add to backlog** | casilla | — | Marcada por defecto; la tarea entra al backlog |
| **Measure progress by** | select | No | `Hours` (por horas) o `Percentage` (por porcentaje) |
| **Subtasks** | lista dinámica | No | Botón **"Add subtask"**; se crean junto con la tarea |
| **Depends on** | checkboxes | No | Lista de tareas/subtareas existentes de las que depende |
| **Description** | texto largo | No | *"Context, links, acceptance criteria"* |

> **Valores por defecto del formulario:** Priority `Medium`, Estimate `4` horas, Story points `3`, **Add to backlog** marcado y progreso `Hours`. Al abrirlo desde **"+ Add task"** de una columna, el **proyecto** llega precargado.

### Ayudas contextuales del formulario
- Bajo **Start**: *"With start, due date and assignee the task is checked against the assignee's free hours in that range."* → si indicas inicio, fin y responsable, Syncra valida la **disponibilidad** de esa persona en el rango.
- Bajo **Measure progress by → Hours**: *"Each subtask takes its hours from the task and weighs by them."* → el avance se pondera por las horas de cada subtarea.
- Bajo **Subtasks**: *"Add this task's subtasks; they are created with it."*
- Bajo **Depends on**: *"The task cannot start until these are completed and approved."*

### Cómo crear una tarea (paso a paso)
1. Pulsa **"New task"**.
2. Escribe el **título** (obligatorio).
3. Elige **proyecto** (o *Loose task*).
4. Asigna **responsable** (opcional, recomendable).
5. Ajusta **prioridad** y **etiqueta**.
6. Define **fechas** de inicio y límite si aplica.
7. Añade **estimación** y **story points**.
8. Deja **"Add to backlog"** marcado si aún no entra al sprint.
9. Elige cómo medir el progreso (**Hours** o **Percentage**).
10. Añade **subtareas** y marca **dependencias** si existen.
11. Escribe la **descripción** con contexto y criterios de aceptación.
12. Pulsa **"Create task"**. Si no quieres guardar, **"Cancel"**.

> El botón de cerrar (X) o **Cancel** descarta el formulario. No se guarda nada.

### Errores y validaciones
- Sin título, el formulario no crea la tarea.
- Si `Start` > `Due date`, la app lo señala.
- Si el responsable no tiene horas libres en el rango, se advierte (capacidad).

---

## 2. Detalle de una tarea

Se abre al pulsar el título en el tablero (URL `…/board?task=<id>`).

![Detalle de tarea](img/tarea-detalle.jpeg)

### Cabecera
- Chips: etiqueta (`Backend`), prioridad (`High`), proyecto (`PRY-01`).
- Título grande: **Backend núcleo [2]**.
- Subtítulo: **"Owner Leandro Sepulveda · Subtasks 0 / 0"**.
- **X** para cerrar.

### Aviso de dependencias
Si la tarea no puede empezar:
> *"It cannot start yet; it waits for: Backend núcleo [1] · Review"*

### Métricas
| Bloque | Significado |
|---|---|
| **ESTIMATE** | Horas estimadas (`0h 00m`) |
| **LOGGED** | Tiempo registrado (`0h 00m`) |
| **DEADLINE** | Fecha límite (`16 Oct`) |
| **Priority** | `High` |
| **Points** | Story points (`3`) |
| **Steps** | Checklist completado/total (`0 / 0`) |

### Acciones
- **"Edit"**: abre el formulario de edición (ver abajo).
- **"Subtasks"**: lista de subtareas con su % global y modo **"By hours"**.

Cada subtarea muestra:
- Título y descripción.
- Estado (`To do`), horas y % de peso (`4h 00m · 22%`).
- Select de **responsable**.
- Select **Move…** (`Move to In progress`, `Move to Review`…).

- **"Task hours not allocated to subtasks: 0h 00m"** → horas de la tarea padre que no están repartidas en subtareas.
- **"New subtask"**: crea una subtarea.
- **"Checklist"**: pasos rápidos; si está vacío: *"No steps yet. Break the task down below."*
- **"Start timer"**: inicia el cronómetro de la tarea.
- **"Move to…"**: cambia el estado de la tarea.

---

## 3. Editar una tarea

Botón **"Edit"** dentro del detalle.

### Campos editables
| Campo | Notas |
|---|---|
| **Title** | Texto |
| **Description** | Texto |
| **Project** | *"It can move to another project until it is started; its subtasks go with it and its dependencies are removed."* → solo antes de iniciar; arrastra subtareas y pierde dependencias |
| **Priority** | `Low`, `Medium`, `High` |
| **Tag** | Igual que en creación |
| **Assignee** | Miembro o `Unassigned` |
| **Start** | Fecha |
| **Due date** | Fecha |
| **Estimate (hours)** | Número |
| **Story points** | Número |
| **Depends on** | Checkboxes (misma ayuda que en creación) |
| **Measure progress by** | `Hours` / `Percentage`; **bloqueado** si la tarea ya tiene subtareas: *"It cannot change: the task already has subtasks."* |

### Guardar o descartar
- **"Save"** aplica los cambios.
- **"Cancel"** descarta y vuelve al detalle.
- Los cambios de estado/proyecto quedan registrados en la **actividad del proyecto**.

---

## 4. Subtareas

Las subtareas son tareas hijas que permiten dividir el trabajo.

### Crear
- Desde el formulario **New task** con **"Add subtask"** (se crean junto a la tarea).
- Desde el detalle con **"New subtask"**.

### Comportamiento
- Tienen su propio estado, responsable, horas y fecha.
- El **% de la tarea padre** se calcula según el modo:
  - **Hours**: cada subtarea pesa según sus horas.
  - **Percentage**: cada subtarea pesa igual.
- La lista del tablero muestra `Subtasks: Open/Completed` y cuántas van (`0/4`).
- Se pueden mover de estado individualmente sin mover la tarea padre.

### Buenas prácticas
- Estima horas por subtarea para que el % sea significativo.
- No mezcles subtareas de distintos responsables sin asignarlas: aparecen como *Unassigned*.
- Si una subtarea es en realidad un entregable grande, conviértela en tarea independiente.

---

## 5. Dependencias ("Depends on")

- Se definen en **New task** y en **Edit**.
- Puedes depender de una **tarea** o de una **subtarea** concreta (se listan como `Tarea › Subtarea`).
- Mientras la dependencia no esté **completada y aprobada**, la tarea muestra el aviso **"It cannot start yet; it waits for: …"** y el chip **Blocked** en el tablero.
- Si mueves una tarea de proyecto, **sus dependencias se eliminan**.

**Uso típico:** impedir que QA empiece antes de que Backend termine, o que el frontend consuma un endpoint que aún no existe.

---

## 6. Checklist (Steps)

- Bloque **"Checklist"** en el detalle.
- Añade pasos con el campo *"Add a step and press Enter"*.
- Cada paso se marca como hecho; el contador **Steps** sube.
- Es independiente de las subtareas: úsalo para pasos pequeños (p. ej. "actualizar documentación", "avisar a soporte").

---

## 7. Registrar tiempo (cronómetro)

### Iniciar
1. Abre la tarea.
2. Pulsa **"Start timer"**.
3. El botón cambia a estado de pausa y el tiempo corre.
4. El **Dashboard → Tracked today** refleja el tiempo del día.

### Detener / pausar
- Vuelve a la tarea y pulsa el control de pausa/detener.
- El tiempo queda **Logged** en la tarea y alimenta *Hours closed (7d)* y los reportes.

### Reglas prácticas
- Un solo cronómetro activo a la vez (el panel del Dashboard muestra la tarea en curso).
- Si olvidaste iniciarlo, el tiempo se puede ajustar desde la edición del registro (según permisos).
- El tiempo registrado se compara con **Estimate** en *Reports* (varianza).

---

## 8. Mover una tarea de estado

Tres formas:
1. Select **"Move…"** en la tarjeta del tablero.
2. Select **"Move to…"** en el detalle.
3. Select **"Move…"** por subtarea.

Estados: **To do → In progress → Review → Completed** (con retrocesos permitidos).

> En muchos equipos, *Review* es el estado donde otra persona valida antes de *Completed*; la app registra quién movió y cuándo en la actividad del proyecto.

---

## 9. Eliminar / archivar

La eliminación de tareas no está expuesta como acción principal en el detalle para el rol **Member**. Si necesitas borrar o archivar, hazlo con un administrador (ver [14 · Roles](14-roles-permisos-y-limites.md)).

---

## 10. Resumen de atajos mentales

| Quiero… | Voy a… |
|---|---|
| Crear trabajo nuevo | **New task** |
| Ver todo el detalle | Clic en el **título** de la tarjeta |
| Cambiar fechas/horas/puntos | **Edit** |
| Dividir el trabajo | **Add subtask** / **New subtask** |
| Evitar empezar antes de tiempo | **Depends on** |
| Medir mi tiempo | **Start timer** |
| Cambiar de columna | **Move…** |
| Ver por qué está bloqueada | Aviso *"It cannot start yet…"* |
