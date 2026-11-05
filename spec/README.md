# Syncra — Complete user manual

Functional documentation of **Syncra** (`https://syncra.liobella.com`), compiled by walking through the real application via the web interface, with the account:

| Item | Value |
|---|---|
| User | `lsepulveda@dwn.com.do` |
| Name | Leandro Sepulveda |
| Title | Full-Stack Developer |
| Role | **Member** (Miembro) |
| Workspace | **Grupo Dominican Watchman** (3 people) |

> **Note on language:** the account and browser used had **EN** active, so the interface appears in English. The app has an **ES / EN** selector (login and user menu). In this manual, labels are quoted as they appear on screen and their meaning is explained.

> **Scope:** everything a user can see and do from the interface with the *Member* role is documented. The areas the app reserves for administrators (for example, creating sprints or projects) are explicitly noted when the interface mentions them but does not show them to this role.

> **Important:** this documentation was produced **in read-only mode only**. No business data (tasks, projects, events, documents) was created, edited, moved or deleted. Only forms were opened and cancelled, and sessions were started/ended to document access.

---

## Table of contents

| # | Document | Contents |
|---|---|---|
| 01 | [Access and account](01-acceso-y-cuenta.md) | Login, remembered session, password recovery, invitations, language |
| 02 | [Navigation and interface](02-navegacion-e-interfaz.md) | Screen structure, sidebar, header, breadcrumb, responsive behavior |
| 03 | [Dashboard](03-dashboard.md) | KPIs, status distribution, team activity, priorities, timer, team workload |
| 04 | [Task board](04-tablero-de-tareas.md) | Columns, cards, filters, move menus, quick add task |
| 05 | [Tasks: create, edit and time](05-tareas-crear-editar-y-tiempo.md) | "New task" form, task detail, subtasks, dependencies, checklist, timer |
| 06 | [Projects](06-proyectos.md) | Listing, filters, search and the 7 tabs of the project detail |
| 07 | [Team and capacity](07-equipo-y-capacidad.md) | Team workload, capacity signals and statuses |
| 08 | [Reports](08-reportes.md) | Estimated vs logged hours, filters, exports, report by department |
| 09 | [Analytics](09-analitica.md) | Throughput, cycle time, rework, cumulative flow, estimate accuracy, velocity |
| 10 | [Calendar](10-calendario.md) | Week / 2 weeks / month views, events, types, event detail |
| 11 | [Messages](11-mensajes.md) | Project/team channels, empty state |
| 12 | [Settings and profile](12-configuracion-y-perfil.md) | Profile, Preferences, My mailbox |
| 13 | [Notifications and user menu](13-notificaciones-y-menu.md) | Bell, recent activity, menu, log out |
| 14 | [Roles, permissions and limits](14-roles-permisos-y-limites.md) | What a Member can do, what is reserved for administrators |
| 15 | [Recipes and workflows](15-recetas-y-flujos.md) | Step-by-step guides for the most common tasks |
| 16 | [Route map and glossary](16-mapa-de-rutas-y-glosario.md) | Routes, parameters, statuses, labels and glossary |

---

## The application in 10 lines

Syncra is a **project and operations management tool for studios/agencies**. It organizes work into:

1. **Projects** (`PRY-01`, `PRY-02`…) with client, lead, dates, budget and health.
2. **Tasks** with status (To do / In progress / Review / Completed), priority, label, due date, estimate in hours, story points and subtasks.
3. **Sprints** (iterations) with capacity and committed points.
4. **Kanban board** by project or global, with filters by project and person.
5. **Time tracking** with a live timer and manual entry (estimated vs. logged).
6. **Reports** of hours and by department, exportable to CSV/Excel/PDF.
7. **Delivery analytics** (throughput, cycle time, rework, cumulative flow, velocity).
8. **Shared calendar** with meetings, reviews, demos, deadlines and retros.
9. **Messaging** through project/team channels.
10. **Team** with workload, overdue items and capacity signal.

---

## How to use this manual

- Each document explains **what you see**, **what it is for** and **how to use it step by step**.
- Screenshots are in [`docs/img`](img) and are referenced in each section.
- Interface terms are shown in quotes: `"New task"`, `"Move to Review"`, etc.
- At the end, [Recipes and workflows](15-recetas-y-flujos.md) summarizes the complete flows (create a task, move it, log time, export a report…).

---

## State of the observed data (reference)

These are the data that existed in the workspace at the time of the study, and they serve to interpret the screenshots:

- **Single project:** `PRY-01 · Sistema de Gestión de Capacitaciones Internas`
  - Client: *Internal development* · Lead: Leandro Sepulveda
  - Dates: 01 Oct → 30 Nov · 3/14 tasks completed · Progress 17% · Health: **At risk**
- **Board:** 14 total tasks · 10 active · 4 overdue · 3 completed in the sprint (23%)
- **Team:** Gerson Cabreja (Administrator), Leandro Sepulveda (Full-Stack Developer), Yoderick Emmanuel Montero Mejia (Full-Stack Developer)
- **Time:** 42 h estimated, 1 min logged (only one task), 7 h closed in the last 7 days
- **Shared document:** `SGCI_Presentacion_Wireframes.pdf` (4.8 MB)
- **Calendar event:** `sprint-revision_proyectos`, 09 Oct 15:00–16:00
