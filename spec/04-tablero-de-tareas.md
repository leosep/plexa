# 04 · Task board

Route: `/board` · **"Task board"** item in the menu.

It is the Kanban where work is seen and moved. It shows all the tasks in the workspace (or of a project/person) grouped by status.

![Task board](img/board.jpeg)

---

## 1. Header and filters

- Title: **"Task board"**.
- Dynamic subtitle: **"All projects · Backlog · 14 tasks"** (active filter · scope · total tasks).
- **Project filter** (select `All projects`):
  - **All projects** → all tasks.
  - **Loose tasks** → only tasks without a project.
  - **PRY-01 · Sistema de Gestión de Capacitaciones Internas** → only that project.
- **Person filter** (select `Everyone`):
  - **Everyone**, **Gerson Cabreja**, **Leandro Sepulveda**, **Yoderick Emmanuel Montero Mejia**.
- **"New task" button**: opens the creation form (see [05](05-tareas-crear-editar-y-tiempo.md)).

Filters combine: for example, project `PRY-01` + person `Leandro Sepulveda` shows only Leandro's tasks in that project.

### Text search

The header includes a global search box (*placeholder* `"Search tasks, people, docs…"`) that, when submitted, takes you to:

```
/board?q=<text>
```

The board then shows the subtitle with the applied filter and a **"clear"** link:

> *"All projects · Backlog · 2 tasks · matching “API” clear"*

The search **includes subtasks**: the cards found show the `↳ Parent task` path above the title.

![Search on the board](img/board-busqueda.jpeg)

- Click **"clear"** to remove the search and return to the full board.
- The URL with `?q=` can be saved/shared.
- In narrow windows the header search field is hidden, but the `q` filter still works through the URL.

---

## 2. Columns

Four fixed columns, in this order:

| Column | Color/icon | Counter | Meaning |
|---|---|---|---|
| **To do** | Gray, clipboard | no. of cards | Yet to start |
| **In progress** | Light blue, play | no. | In development |
| **Review** | Peach, eye | no. | Under review/validation |
| **Completed** | Green, check | no. | Finished |

Each column ends with a dotted **"+ Add task"** button that creates a task directly in that status.

> The column header counter counts **cards** (parent tasks), not subtasks.

---

## 3. Anatomy of a card

Example: *Backend núcleo [2]*

```
┌──────────────────────────────────────────────┐
│ ▣ PRY-01 · Sistema de Gestión de Capacita…   │  ← project (code + name)
│ Backend núcleo [2]                    (LS)   │  ← title + assignee avatar
│ 🗓 16 Oct   [High]   [Backend]   [🚫 Blocked]│  ← date, priority, tag, blocked
│ ▓▓▓░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  0%        │  ← progress bar
│                                              │
│ Subtasks: Open                            ⊛  │  ← subtask status + action
│  ○ Capacitaciones y sesiones          (LS)   │
│  ○ API de cartelera                   (LS)   │
│  ○ Inscripción y cancelación          (LS)   │
│  ○ Asistencia en sitio                (LS)   │
│                                              │
│ Assigned team                    ⊙ Assign    │
│ (LS)                                         │
│ ──────────────────────────────────────────── │
│ 🗒 0/4                                   (▶) │  ← subtask progress + time + timer
└──────────────────────────────────────────────┘
```

Elements:

| Element | Detail |
|---|---|
| **Project code** | `PRY-01` + truncated name; identifies which project it belongs to |
| **Title** | Task name; clicking it opens the detail |
| **Avatar** | Assignee (initials and color) |
| **Date** | Due date with calendar icon; in red if overdue |
| **Priority** | Chip `High` (red), `Medium` (amber), `Low` (gray) |
| **Tag** | `Backend`, `Frontend`, `QA`… |
| **Blocked** | Dark chip with 🚫 icon when the task has uncompleted dependencies |
| **Progress** | Bar + % (by hours or by percentage, depending on the task) |
| **Subtasks** | List of subtasks with their status (`Open`/`Completed`), assignee and ⊛ action to manage them |
| **Assigned team** | Assigned people + **Assign** link |
| **Footer** | `completed/total` of subtasks on the left and logged time (`1m`) + timer button (▶) on the right |
| **Move…** | Select to change status (see below) |

### Colors and signals
- Card with a colored border/accent = column status.
- `Blocked` means it **cannot start** until its dependencies are completed and approved.
- The percentage is calculated according to the task's progress mode (by hours or by subtask percentage).

---

## 4. Moving a task

Each card has a **"Move…"** select with the allowed transitions:

- From **To do**: `Move to In progress`, `Move to Review`.
- From **In progress**: `Move to To do`, `Move to Review`.
- From **Review**: `Move to To do`, `Move to In progress` (and to Completed if it is approved).
- From **Completed**: reopening options.

When you choose an option, the card moves to another column and the change is recorded in the project activity.

> The transitions shown depend on the current status and on permissions. In the screenshot, To do offers *In progress* and *Review*; Review offers *To do* and *In progress*.

---

## 5. Adding quick tasks

- **"+ Add task"** at the bottom of a column: opens the **New task** form with the **project already pre-loaded** (the one from the board context). The status is set afterwards by moving the card.
- **"New task"** in the header: opens the full form, where you choose project, assignee, dates, subtasks, dependencies, etc.

---

## 6. Opening a task's detail

Click the card **title**. A modal opens and the URL becomes `/board?task=<id>`.

The detail includes: badges, owner, dependencies, estimate/logged/deadline, priority, points, subtasks, checklist, timer and status movement. Everything is explained in [05 · Tasks](05-tareas-crear-editar-y-tiempo.md).

---

## 7. Good practices on the board

1. **One card = one deliverable unit.** If it has many subtasks, that is normal; if it has many distinct responsibilities, split it.
2. **Keep WIP low**: few cards in *In progress*.
3. **Move the card when you start**, not when you finish; that way the Dashboard and the reports reflect reality.
4. **Use `Blocked`/dependencies** so that nobody starts work they cannot finish.
5. **Check the filter** before concluding that "a task is missing": it may be hidden by project or person.
