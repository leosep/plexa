# 10 · Calendar

Route: `/calendar` · **"Calendar"** item in the menu.

Shared workspace calendar: meetings, reviews, demos, deadlines and retros, linkable to projects, people and departments.

![Calendar — week view](img/calendario.jpeg)

---

## 1. Header

- Title **"Calendar"**.
- Visible range and number of events: *"26 Oct – 1 Nov · 0 events"* (or *"1 event"*).
- View switcher: **Week** · **2 weeks** · **Month**.
- Navigation: **← Prev** · **Today** · **Next →**.
- **"New event"** button.

### Views
| View | Shows |
|---|---|
| **Week** | 7 columns (Mon–Sun) with the events of each day |
| **2 weeks** | 14 columns |
| **Month** | Monthly grid (e.g. *October 2026*) with the number of events per day |

In **Month**, each cell shows the day and a counter; clicking a day opens its detail/creation.

---

## 2. Day cells

Each column shows:
- Day number and abbreviation (`26 Mon`, `27 Tue`…).
- **"Today"** chip on the current day.
- Event counter (`0`, `1`…).
- The events as cards with title and time.
- Dotted **"+ Add event"** button at the bottom.

Clicking **"+ Add event"** on a day opens **New event** with the date preloaded.

---

## 3. Existing events

Real example from the workspace: in the week of **5–11 Oct** there is one event:

- **sprint-revision_proyectos**
- **09 Oct · 15:00 – 16:00**
- Kind: **Meeting**
- Project: **PRY-01 · Sistema de Gestión de Capacitaciones Internas**
- Place: None · Members: None · Departments: None

![Event detail](img/calendario-evento.jpeg)

### Viewing the detail
Click the event card → modal with:
| Field | Example |
|---|---|
| Title | sprint-revision_proyectos |
| Date and time | 09 Oct · 15:00 – 16:00 |
| **Kind** | Meeting |
| **Place** | None |
| **Project** | PRY-01 · … |
| **Members** | None |
| **Departments** | None |

> In the documented account the modal is **read-only** (it does not show edit/delete). Full management depends on permissions.

---

## 4. Creating an event

**"New event"** button (or **"+ Add event"** on a day).

### Form
| Field | Type | Options / values |
|---|---|---|
| **Title** | text | Placeholder `Client demo` |
| **Date** | day picker | By default, today (`30.10.2026` in the example) |
| **Starts** | time picker | By default `10:00` |
| **Ends** | time picker | By default `11:00` |
| **Kind** | select | `Meeting`, `Review`, `Demo`, `Deadline`, `Retro`, `Other` |
| **Place / tool** | select | `No place` or place/tool options |
| **Project** | select | `No project` or any project |
| **Members** | multi-select | `Add a member…` |
| **Departments** | multi-select | `Add a department…` |
| **Cancel / Add event** | buttons | Discard / create |

### Step by step
1. Click **"New event"** (or the day's **+ Add event**).
2. Type the **title**.
3. Confirm **date**, **start time** and **end**.
4. Choose the **kind** (Meeting, Review, Demo, Deadline, Retro, Other).
5. Indicate the **place or tool** (room, Zoom, Meet…).
6. Link the **project** if applicable.
7. Add invited **members** and/or **departments**.
8. Click **"Add event"**.

> **Tip:** use `Deadline` for delivery milestones and `Review`/`Demo` for ceremonies; that way the calendar communicates the plan as well as the meetings.

---

## 5. Time navigation

- **← Prev / Next →**: moves forward or backward depending on the view (week, 2 weeks or month).
- **Today**: returns to the current date.
- The range is always shown in the subtitle.
- In **Month**, the days of adjacent months are shown with a counter.

---

## 6. Integration with projects

- The project's **Calendar** tab lists only its events and offers **"Open calendar"**.
- When creating an event you can link it to a project, and it will appear in both places.
- Events with **Members** or **Departments** are shown to those people.

---

## 7. Use cases

| Need | Suggested setup |
|---|---|
| Daily / weekly | `Meeting`, manual recurrence, team members |
| Sprint review | `Review`, project, closing day |
| Client demo | `Demo`, project, place/tool |
| Delivery date | `Deadline`, project, no time or end of day |
| Retrospective | `Retro`, team/department |
| Calendar block | `Other` + descriptive title |

---

## 8. Notes

- The global calendar shows **0 events** in the current week of the example; the October event appears when navigating to that week.
- If an event does not appear, check the date range and the event's project/member filters.
- Events without members or departments are visible to anyone with access to the project/workspace.
