# 15 · Recetas y flujos de trabajo

Guías paso a paso de las tareas más frecuentes. Todas son ejecutables con el rol **Member** salvo que se indique lo contrario.

---

## Receta 1 · Empezar el día

1. Entra en `https://syncra.liobella.com`.
2. Revisa el **Dashboard**:
   - ¿Cuántas **Overdue** hay?
   - ¿Cómo va **Completed this sprint**?
   - ¿Hay **sprint activo**?
3. Abre la **campana** y revisa *Recent activity* (recuerda: al abrirla se marcan como leídas).
4. Ve a **Task board** y filtra por ti (`Everyone` → tu nombre).
5. Elige la primera tarea, ábrela y pulsa **Start timer**.

---

## Receta 2 · Crear una tarea y dejarla lista

1. **Task board → New task**.
2. **Title**: verbo + resultado ("Implementar API de cartelera").
3. **Project**: elige el proyecto (o *Loose task* si es suelta).
4. **Assignee**: tú o quien la hará.
5. **Priority**: `High` solo si bloquea a alguien o tiene fecha crítica.
6. **Tag**: `Backend`, `Frontend`, `QA`…
7. **Due date** y **Start** si aplica.
8. **Estimate (hours)**: tu mejor estimación.
9. **Story points** si el equipo planifica por puntos.
10. **Measure progress by**: `Hours` si vas a estimar subtareas; `Percentage` si son pasos equivalentes.
11. **Subtasks**: añade las partes si son más de 2–3.
12. **Depends on**: marca lo que debe terminar antes.
13. **Description**: contexto, enlaces y criterios de aceptación.
14. **Create task**.

---

## Receta 3 · Trabajar una tarea (flujo completo)

1. En el tablero, mueve la tarjeta a **In progress** (`Move…`).
2. Abre el detalle y pulsa **Start timer**.
3. Trabaja; marca **Checklist** y subtareas a medida que avanzas.
4. Al terminar, mueve la tarea a **Review**.
5. Detén el cronómetro.
6. Cuando se apruebe, muévela a **Completed**.

> Si la tarea no puede empezar, verás *"It cannot start yet; it waits for: …"* → resuelve la dependencia primero.

---

## Receta 4 · Dividir una tarea grande

1. Abre la tarea → **Edit**.
2. Verifica **Measure progress by = Hours** (recomendado).
3. Estima las horas totales.
4. Añade **subtareas** con horas y responsables.
5. Guarda.
6. Comprueba que *"Task hours not allocated to subtasks"* queda en `0h 00m`.
7. Trabaja cada subtarea moviéndola por separado.

---

## Receta 5 · Bloquear trabajo que no puede empezar

1. Edita la tarea dependiente.
2. En **Depends on**, marca la tarea o subtarea que debe completarse antes.
3. Guarda.
4. La tarea mostrará **Blocked** y el aviso de espera.
5. Cuando la dependencia se complete y apruebe, el bloqueo desaparece.

---

## Receta 6 · Registrar tiempo olvidado

1. Abre **Reports**.
2. Activa **"Time logged"** para ver qué tiene registro y qué no.
3. Para las tareas con `LOGGED 0h 00m`, abre cada tarea y usa el registro de tiempo (**Start timer** o edición del tiempo, según permisos).
4. Vuelve a Reports y confirma que la varianza se actualiza.

---

## Receta 7 · Preparar la daily

1. **Team workload**: revisa `Overdue` y la señal de cada persona.
2. **Task board**: identifica tarjetas `Blocked` y las que llevan mucho en *In progress*.
3. **Dashboard**: mira *Overdue* y *Task priority*.
4. Anota:
   - Qué hice ayer (tareas movidas/completadas).
   - Qué haré hoy (siguiente tarea del tablero).
   - Bloqueos (dependencias, accesos, decisiones).
5. Tras la daily, actualiza estados y fechas.

---

## Receta 8 · Revisar un proyecto con el cliente

1. **Projects** → abre el proyecto.
2. **Overview**: salud, fechas, 3/14 tareas y actividad.
3. **Dashboard**: tareas/puntos entregados y vencidos.
4. **Timeline**: fases (si están definidas).
5. **Documents**: muestra entregables.
6. **Reports**: filtra por el proyecto/sprint y exporta **PDF** si necesitas un informe.

---

## Receta 9 · Exportar horas para facturación

1. **Reports**.
2. Selecciona **Member** (o *All members*), **Sprint** y **Tag**.
3. Activa **"Time logged"**.
4. Pulsa **Export Excel** (o CSV/PDF).
5. Verifica el archivo: tarea, responsable, tag, tiempo estimado, registrado y varianza.

---

## Receta 10 · Crear una reunión o deadline

1. **Calendar → New event**.
2. Título claro ("Revisión sprint 4 con cliente").
3. Fecha y horas.
4. **Kind**: `Meeting`, `Review`, `Demo`, `Deadline`, `Retro` u `Other`.
5. **Place / tool**: sala o videollamada.
6. **Project**: vincúlalo.
7. **Members** / **Departments**: invita.
8. **Add event**.
9. Comprueba que aparece en el día y en la pestaña **Calendar** del proyecto.

---

## Receta 11 · Publicar un entregable

1. Abre el proyecto → **Documents**.
2. **Attach documents**.
3. Selecciona el archivo (< 25 MB, formato permitido).
4. Verifica que aparece con tamaño, autor y fecha.
5. Avisa por **Messages** o en la tarea correspondiente.
6. Para retirarlo, usa **Delete** (confirmar antes).

---

## Receta 12 · Preparar el cierre de sprint (con administrador)

1. Verifica que no queden tareas `Blocked` sin motivo.
2. Mueve lo terminado a **Completed** y lo no terminado al siguiente sprint (lo hace un administrador).
3. Revisa **Analytics**: throughput, cycle time, rework, velocity.
4. Exporta **Reports** del sprint.
5. Documenta acuerdos y próximos pasos.
6. El administrador **cierra el sprint** en *Settings → Sprints*.

---

## Receta 13 · Incorporar a alguien a un proyecto

1. Abre el proyecto → **Team**.
2. **Edit team**.
3. Selecciona **Department**, **Project lead** y **Collaborators**.
4. **Save team**.
5. La persona verá el proyecto y sus tareas.
6. Si no lo ves tú, pide al administrador que te añada o que revise permisos.

---

## Receta 14 · Ajustar tu entorno

1. **Settings → Preferences**.
2. Desmarca **"Show the upgrade block in the sidebar"** si quieres más espacio.
3. Desmarca **"Show the insights panel on project pages"** si prefieres una vista limpia.
4. Elige **ES/EN**.
5. **Settings → Profile → Choose image** para tu foto.

---

## Receta 15 · Recuperar acceso

1. En el login, **"Forgot your password?"**.
2. Escribe tu correo y **Send instructions**.
3. Abre el enlace del correo y define la contraseña.
4. Vuelve a **Sign in**.
5. Si no llega el correo: revisa spam, confirma el dominio y contacta a `support@syncra.app`.

---

## Mapa rápido "quiero → hago"

| Quiero… | Ruta / acción |
|---|---|
| Ver el estado general | Dashboard |
| Ver/mover tareas | Task board |
| Crear trabajo | New task |
| Ver un proyecto | Projects → nombre |
| Saber quién puede más | Team workload |
| Comparar estimado vs real | Reports |
| Ver tendencias | Analytics |
| Agendar algo | Calendar → New event |
| Hablar con el equipo | Messages |
| Cambiar mi foto/idioma | Settings |
| Salir | Menú de usuario → Log out |
| Ayuda | Sidebar → Help & support (`support@syncra.app`) |
