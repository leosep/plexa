# 08 · Reportes (Reports)

Ruta: `/reports` · Elemento **"Reports"** del menú.

Compara el **tiempo estimado (task time)** con el **tiempo registrado (logged)** y ofrece un reporte por departamento. Los exports respetan los filtros activos.

![Reportes](img/reportes.jpeg)

---

## 1. Encabezado y acciones

- Título **"Reports"**.
- Subtítulo: *"Task time vs logged time (informative). Exports carry the filters you set here."*
- Botones:
  - **"Hours report"** (botón naranja, activo por defecto): vista de horas por tarea.
  - **"Export CSV"**.
  - **"Export Excel"**.
  - **"Export PDF"**.

---

## 2. Filtros

| Filtro | Opciones | Efecto |
|---|---|---|
| **Sprint** | `Active sprint`, otros sprints | Limita al sprint seleccionado |
| **Member** | `All members`, cada persona | Solo tareas de esa persona |
| **Tag** | `All tags`, `Backend`, … | Solo tareas con esa etiqueta |
| **Time logged** (botón) | on/off | Muestra solo tareas **con tiempo registrado** |
| **Variance > 0** (botón) | on/off | Muestra solo tareas donde lo registrado **supera** lo estimado (sobreconsumo) |

Los filtros se combinan y se reflejan en la tabla, en los totales y en los archivos exportados.

---

## 3. Tabla "Hours report"

| Columna | Significado |
|---|---|
| **TASK** | Nombre de la tarea |
| **OWNER** | Responsable |
| **TAG** | Etiqueta (`Backend` o `—`) |
| **TASK TIME** | Horas estimadas |
| **LOGGED** | Tiempo registrado |
| **VARIANCE** | Diferencia registrado − estimado (`+1m` en rojo si hay exceso, `—` si no aplica) |

### Datos observados (extracto)

| Task | Owner | Tag | Task time | Logged | Variance |
|---|---|---|---|---|---|
| API de cartelera | Leandro Sepulveda | Backend | 2h 00m | 0h 00m | — |
| Acceso de empleado | Leandro Sepulveda | — | 3h 00m | 0h 00m | — |
| Análisis y diseño | Leandro Sepulveda | — | 0h 00m | 1m | +1m |
| Asistencia en sitio | Leandro Sepulveda | Backend | 6h 00m | 0h 00m | — |
| Capacitaciones y sesiones | Leandro Sepulveda | — | 4h 00m | 0h 00m | — |
| Catálogos | Leandro Sepulveda | — | 4h 00m | 0h 00m | — |
| Empleados y carga masiva | Leandro Sepulveda | — | 3h 00m | 0h 00m | — |
| Inscripción y cancelación | Leandro Sepulveda | Backend | 6h 00m | 0h 00m | — |
| Modelo de datos, reglas y contrato de API | Leandro Sepulveda | — | 4h 00m | 0h 00m | — |
| Seguridad administrativa | Leandro Sepulveda | — | 4h 00m | 0h 00m | — |
| Setup y Clean Architecture | Leandro Sepulveda | — | 3h 00m | 0h 00m | — |
| Wireframes y validación con RRHH | Leandro Sepulveda | — | 3h 00m | 0h 00m | — |
| **Total** | | | **42h 00m** | **1m** | **-41h 59m** |

Pie de tabla: **"Showing 12 tasks · Backlog · 1 with time logged"**.

### Cómo leer la varianza
- **—**: no hay registro o no aplica comparación.
- **+X**: se registró **más** de lo estimado (sobreconsumo).
- **−X** (en el total): queda mucho tiempo estimado sin registrar.

> El total `-41h 59m` no es un error: indica que la mayoría de las tareas estimadas **aún no tienen tiempo registrado**.

---

## 4. Exportaciones

Tres formatos, mismos datos y filtros:

| Botón | Formato | Cuándo usarlo |
|---|---|---|
| **Export CSV** | CSV | Análisis en Excel/Sheets, importar a BI |
| **Export Excel** | XLSX | Compartir con formato y varias hojas |
| **Export PDF** | PDF | Enviar a cliente/dirección sin edición |

**Cómo exportar:**
1. Ajusta **Sprint / Member / Tag** y los toggles.
2. Pulsa el botón de formato.
3. Se descarga el archivo con **los filtros aplicados**.

> El export se hace desde el navegador; si el navegador bloquea descargas, revisa el aviso de descarga.

---

## 5. Reporte "By department"

Debajo de la tabla principal:

> *"Last 30 days. Only tasks whose completion was approved count, with their time."*

| Columna | Significado |
|---|---|
| **DEPARTMENT** | Departamento y líder |
| **ACTIVE PROJECTS** | Proyectos activos del departamento |
| **MEMBERS** | Personas |
| **COMPLETED TASKS** | Tareas completadas y aprobadas en 30 días |
| **AWAITING APPROVAL** | Completadas pendientes de aprobación |
| **HOURS** | Horas registradas en esas tareas |

Ejemplo observado:

| Department | Active projects | Members | Completed | Awaiting approval | Hours |
|---|---|---|---|---|---|
| Software Development (Lead: Gerson Cabreja) | 1 | 3 | 3 | 1 | 7.0 |

**Uso:** reporte de gestión por área, útil para dirección y facturación.

---

## 6. Flujos típicos

### Revisar desviaciones de un proyecto
1. Filtra por **Sprint** (o deja *Active sprint*).
2. Activa **"Variance > 0"** para ver solo sobreconsumos.
3. Ordena mentalmente por magnitud y revisa las tareas con `+`.
4. Exporta a PDF para la reunión.

### Reporte mensual de horas
1. Filtra por **Member** o deja *All members*.
2. Activa **"Time logged"**.
3. Exporta **Excel**.
4. Adjunta el archivo al informe.

### Ver trabajo sin registrar
1. Desactiva **"Time logged"**.
2. Busca filas con `LOGGED 0h 00m` y `TASK TIME` alto → trabajo estimado sin registro.
3. Recuerda al equipo usar **Start timer**.

---

## 7. Limitaciones y notas

- La pantalla lo dice: los datos son **informativos**; no sustituyen una herramienta de facturación.
- Solo cuentan para *By department* las tareas cuya **finalización fue aprobada**.
- Si no hay sprint activo, el filtro *Active sprint* puede devolver el backlog completo.
- Los porcentajes y varianzas dependen de que las estimaciones estén bien cargadas.
