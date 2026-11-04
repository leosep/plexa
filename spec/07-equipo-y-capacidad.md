# 07 · Equipo y capacidad (Team workload)

Ruta: `/team` · Elemento **"Team workload"** del menú. También se llega con **"See all"** desde el Dashboard.

Muestra cuánto trabajo tiene cada persona, cuánto está vencido y su señal de capacidad.

![Team workload](img/equipo.jpeg)

---

## 1. Encabezado

- Título **"Team workload"**.
- Subtítulo: *"Capacity, pressure signal and availability across 3 people."*
- No tiene filtros ni acciones: es una vista de lectura.

---

## 2. Tarjeta por persona

Cada miembro aparece en una tarjeta:

```
┌──────────────────────────────────────────────┐
│ (GC)  Gerson Cabreja                         │
│       Administrator                          │
│                                              │
│   0          0            0%                 │
│  Active    Overdue      Capacity             │
│  ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  (barra)   │
│                                              │
│  ● On track                      Available   │
└──────────────────────────────────────────────┘
```

### Métricas
| Métrica | Significado |
|---|---|
| **Active** | Tareas activas asignadas |
| **Overdue** | Tareas vencidas asignadas (en rojo) |
| **Capacity** | % de capacidad consumida; barra de progreso |
| **Señal** | Chip de estado: **On track**, *At risk*, *Overloaded*… |
| **Disponibilidad** | Texto a la derecha: **Available** o el estado correspondiente |

### Datos observados

| Persona | Puesto | Active | Overdue | Capacity | Señal |
|---|---|---|---|---|---|
| Gerson Cabreja | Administrator | 0 | 0 | 0% | On track · Available |
| Leandro Sepulveda | Full-Stack Developer | 10 | 4 | 0% | On track · Available |
| Yoderick Emmanuel Montero Mejia | Full-Stack Developer | 0 | 0 | 0% | On track · Available |

> **Nota:** la capacidad aparece en 0% porque no hay un sprint activo con capacidad comprometida. Al crear un sprint y asignar capacidad, estos porcentajes y señales cobran sentido.

---

## 3. Cómo interpretar la capacidad

| Señal | Qué significa | Qué hacer |
|---|---|---|
| **On track** | Carga dentro de lo esperado | Mantener |
| **At risk** | Carga alta o vencidos acumulados | Revisar prioridades, reasignar |
| **Overloaded** | Por encima de la capacidad | Mover trabajo a otra persona o al siguiente sprint |
| **Available** | Con margen | Puede asumir trabajo nuevo |

La señal combina **tareas activas**, **vencidas** y **capacidad del sprint**. Es una alerta, no una evaluación de desempeño.

---

## 4. Uso práctico

### Antes de asignar una tarea
1. Abre **Team workload**.
2. Mira quién está **Available** y con capacidad baja.
3. Evita asignar a quien está **Overloaded** o con muchos **Overdue**.
4. Asigna desde la tarea (**Assignee**) o desde el formulario **New task**.

### En la reunión diaria
1. Revisa **Overdue** por persona (rojo).
2. Identifica bloqueos (tareas con `Blocked` en el tablero).
3. Reequilibra: mueve tareas o cambia el alcance del sprint.

### Al planificar el sprint
1. Comprueba la capacidad de cada persona.
2. Compromete solo el trabajo que cabe.
3. Tras crear el sprint (administrador), vuelve aquí para validar los porcentajes.

---

## 5. Relación con otras pantallas

| Pantalla | Relación |
|---|---|
| **Dashboard → Team workload** | Versión resumida en tabla con enlace **See all** |
| **Task board** | El filtro `Everyone` permite ver solo las tareas de una persona |
| **Reports** | Filtro por **Member** para ver horas y varianza de cada uno |
| **Analytics → Load per member** | Burbujas con el tamaño de la carga activa |
| **Settings → Profile** | Muestra tu puesto; la capacidad la ajusta un administrador |
