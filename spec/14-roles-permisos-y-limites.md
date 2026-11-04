# 14 · Roles, permisos y límites

Qué puede hacer la cuenta documentada y qué queda reservado a administradores. Se basa en lo que la interfaz muestra y oculta para un usuario con rol **Member** que, además, es **líder** del proyecto existente.

---

## 1. Roles observados en el espacio

| Persona | Rol en el espacio | Puesto |
|---|---|---|
| **Gerson Cabreja** | **Administrator** | Administrator |
| **Leandro Sepulveda** | **Member** | Full-Stack Developer |
| **Yoderick Emmanuel Montero Mejia** | **Member** | Full-Stack Developer |

El rol se muestra en el chip de usuario de la cabecera (`Miembro` / `Member`) y en la tabla de *Team workload*.

---

## 2. Qué puede hacer un Member (comprobado en la interfaz)

| Acción | ¿Disponible? | Dónde |
|---|---|---|
| Ver Dashboard y métricas | ✅ | `/dashboard` |
| Ver y usar el tablero | ✅ | `/board` |
| Crear tareas | ✅ | **New task** / **+ Add task** |
| Editar tareas | ✅ | Detalle → **Edit** |
| Crear subtareas | ✅ | Detalle → **New subtask** / formulario |
| Definir dependencias | ✅ | Formulario |
| Mover tareas de estado | ✅ | **Move…** / **Move to…** |
| Iniciar/detener cronómetro | ✅ | Detalle → **Start timer** |
| Añadir pasos al checklist | ✅ | Detalle → **Checklist** |
| Ver proyectos y su detalle | ✅ | `/projects` |
| Editar equipo de un proyecto que lidera | ✅ | Proyecto → **Team** → **Edit team** |
| Subir documentos al proyecto | ✅ | Proyecto → **Documents** → **Attach documents** |
| Ver reportes y exportar | ✅ | `/reports` |
| Ver analítica | ✅ | `/analytics` |
| Ver calendario y crear eventos | ✅ | `/calendar` → **New event** |
| Ver mensajes | ✅ (vacío) | `/messages` |
| Editar su foto de perfil | ✅ | Settings → Profile |
| Cambiar preferencias de layout/idioma | ✅ | Settings → Preferences |
| Configurar su buzón de envío | ✅ | Settings → My mailbox |
| Cerrar sesión | ✅ | Menú de usuario |

---

## 3. Qué no aparece para un Member

| Acción | Evidencia en la interfaz |
|---|---|
| **Crear sprints** | La tarjeta *Active sprint* dice *"Create one in Settings → Sprints"*, pero **Settings no muestra "Sprints"**. |
| **Crear proyectos** | El listado dice *"create, edit, archive"*, pero **no hay botón de creación** para este rol. |
| **Cambiar nombre, puesto o capacidad** | *"Ask a workspace admin to change your name, title or capacity."* |
| **Crear canales de mensajes** | *"Managers can open a project or team channel from a project page."* |
| **Gestionar miembros/roles del espacio** | No hay sección de miembros en Settings. |
| **Facturación / Upgrade** | El botón **Upgrade now** no abre ningún flujo en esta cuenta. |
| **Eliminar tareas** | No hay acción de borrado en el detalle. |

> Estas ausencias son **por rol**, no por un fallo: la app las reserva a administradores/managers.

---

## 4. Permisos a nivel de proyecto

La pestaña **Team** lo explica:

> *"Only the team and people allowed to see every project can open this project and its tasks."*

Por tanto, el acceso a un proyecto depende de:

1. Ser **Project lead**.
2. Ser **Collaborator**.
3. Tener permiso global para **ver todos los proyectos** (administradores).

**Consecuencia práctica:** si no ves un proyecto o sus tareas, pide que te añadan en **Team → Edit team** o que te den permiso global.

El **Project lead** puede editar el equipo (departamento, lead y colaboradores). Otros miembros probablemente no; en la cuenta probada el usuario era lead, por lo que el formulario sí estaba disponible.

---

## 5. Permisos sobre documentos

En **Documents** cualquier miembro del proyecto ve:

- **Download** en todos los archivos.
- **Delete** en los archivos (con confirmación).

> El botón **Delete** aparece para el usuario documentado; úsalo con cuidado: elimina el archivo para todo el equipo. Antes de borrar, confirma que no es la única copia.

Límites:
- **25 MB por archivo**.
- Varios archivos a la vez.
- Extensiones permitidas: PDF, Office, OpenDocument, imágenes, texto, CSV, Markdown y ZIP.

---

## 6. Permisos sobre tareas

- **Asignación**: cualquier miembro puede asignar a cualquier persona del espacio.
- **Movimiento**: se permiten transiciones entre estados (con retrocesos).
- **Cambio de proyecto**: solo mientras la tarea **no haya empezado**; arrastra subtareas y **elimina dependencias**.
- **Modo de progreso**: se bloquea cuando ya hay subtareas (*"It cannot change: the task already has subtasks."*).
- **Borrado**: no expuesto.

---

## 7. Recomendaciones de gobierno

1. **Define un administrador claro** (en este espacio, Gerson Cabreja) para altas, sprints y proyectos.
2. **Asigna leads** por proyecto: pueden gestionar su equipo sin dar permisos globales.
3. **Revisa colaboradores** al incorporar o rotar personas.
4. **No compartas la cuenta**: cada persona debe tener su usuario para que la actividad y el tiempo sean trazables.
5. **Documenta el rol** de cada persona en su puesto/capacidad (lo edita el administrador).

---

## 8. Si necesitas un permiso que no tienes

1. Identifica la acción exacta (p. ej. "crear sprint", "ver proyecto X").
2. Comprueba si depende del **rol** o de la **pertenencia al proyecto**.
3. Escríbele al administrador con el contexto (proyecto, tarea, motivo).
4. Mientras tanto, usa las alternativas disponibles: crear la tarea en un proyecto existente, pedir que te asignen como colaborador, o registrar el trabajo en una tarea ya creada.
