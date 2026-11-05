# 03 · Dashboard

Route: `/dashboard` · **"Dashboard"** item in the menu.

It is the home screen after signing in. It summarizes the sprint status, priorities, team activity and your time for the day.

![Dashboard — upper part](img/dashboard.jpeg)

![Dashboard — lower part](img/dashboard-2.jpeg)

---

## 1. Header

- **Personalized greeting**: *"Good afternoon, Leandro Sepulveda"* (changes with the time of day: Good morning / afternoon / evening).
- **Context line**: notice about the sprint and overdue tasks. In the example:
  *"No active sprint. 4 tasks are past their deadline. Fri 30 Oct"*
- **"This sprint" button**: takes you to the board filtered by the current sprint (`/board`). It is used to move from the summary to the sprint work.
- **"New task" button**: opens the task creation form (see [05 · Tasks](05-tareas-crear-editar-y-tiempo.md)).

---

## 2. KPI cards (top row)

| Card | What it shows | Observed example |
|---|---|---|
| **Active tasks** | Open (not completed) tasks and how many of them are due today | `10` active · `0 due today` |
| **Completed this sprint** | Completed tasks / sprint total and % | `3 / 13` · `23% of sprint` |
| **Hours closed (7d)** | Hours logged and closed in the last 7 days, compared with the previous week | `7h` · `▲ +100% vs prev. week` |
| **Overdue** | Tasks past their due date, with a warning signal | `4` · `▼ Needs attention` |

**How to read them:** they are the pulse of the sprint. If *Overdue* goes up and *Completed* does not, the sprint is at risk.

---

## 3. Task status distribution

Card with the distribution of all backlog tasks by status.

- Header: **"13 tasks in the backlog"**.
- Four blocks with **%**, status name, mini bar and number of tasks:
  - **To do** — 77% · 10 tasks
  - **In progress** — 0% · 0 tasks
  - **Review** — 0% · 0 tasks
  - **Completed** — 23% · 3 tasks

Each block has an associated color (gray, light blue, peach, green) that is repeated throughout the rest of the app.

**Use:** to detect accumulation. Many tasks in *To do* and few in *In progress* usually means work that has not started yet.

---

## 4. Team activity (heat map)

- Header: **"20 actions in the last 4 weeks"**.
- Rows: `-3 wk`, `-2 wk`, `-1 wk`, `This wk`.
- Columns: `Mon` … `Sun`.
- Each cell is colored according to the number of actions (legend **Less ↔ More**).

**What it counts:** team actions (creating tasks, moving them, completing them, etc.). It is useful to see the rhythm and the busiest days, not to evaluate people.

---

## 5. Task priority

- Header: **"Open tasks by priority level"**.
- Center donut with the total: `10 open`.
- Legend: **High 10 · Medium 0 · Low 0**.

**Use:** to prioritize. If almost everything is *High*, priority stops discriminating and it is worth recalibrating.

---

## 6. Tracked today (timer)

Time tracking panel for the day.

- Status: **"Paused"** and **"No timer running"**.
- Large clock: `0:00:00`.
- Three metrics for the task in progress: **Estimate**, **Logged**, **Left** (empty if there is no task).
- Circular **play** button (▶).
- Guidance text: **"Pick a task on the board to start."**

### How it is used
1. Go to the **Task board** and open a task.
2. In the detail press **"Start timer"**.
3. The timer runs and the Dashboard shows the accumulated time for the day.
4. To stop it, go back to the task and press **Pause/Stop** (the panel shows *Paused* when it is not running).

> The panel does not start the timer by itself: you have to pick a task.

---

## 7. Team workload (summary)

Table with each person's status:

| Column | Meaning |
|---|---|
| **NAME** | Person (avatar + name + title) |
| **ACTIVE** | Active assigned tasks |
| **OVERDUE** | Overdue assigned tasks |
| **CAPACITY** | Bar + % of capacity used |
| **STATUS** | Signal: **On track**, *At risk*, *Overloaded*… |

**"See all"** link → `/team` (see [07 · Team](07-equipo-y-capacidad.md)).

Example data:

| Person | Title | Active | Overdue | Capacity | Status |
|---|---|---|---|---|---|
| Gerson Cabreja | Administrator | 0 | 0 | 0% | On track |
| Leandro Sepulveda | Full-Stack Developer | 10 | 4 | 0% | On track |
| Yoderick Emmanuel Montero Mejia | Full-Stack Developer | 0 | 0 | 0% | On track |

---

## 8. Recommended routine when you sign in

1. Read the **greeting/context**: is there an active sprint? how many are overdue?
2. Review **Overdue** and **Task priority** to decide the focus.
3. Look at **Tracked today** and start the timer on the task you are going to work on.
4. Use **"This sprint"** or **"New task"** to move to action.
5. At the end of the day, check **Hours closed (7d)** and the team workload.
