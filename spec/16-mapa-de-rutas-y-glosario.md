# 16 · Route map and glossary

Quick reference of the routes, parameters, statuses, labels and terms used in Syncra.

---

## 1. Application routes

| Route | Screen | Access |
|---|---|---|
| `/login` | Sign in | Public |
| `/forgot-password` | Password recovery | Public |
| `/invitations` | Accept invitation | Public |
| `/dashboard` | Dashboard | Authenticated |
| `/board` | Global board | Authenticated |
| `/board?task=<id>` | Board with the task detail open | Authenticated |
| `/board?q=<text>` | Board filtered by text search | Authenticated |
| `/projects` | Project listing | Authenticated |
| `/projects/<projectId>` | Project detail (Overview by default) | Project member or global permission |
| `/projects/<projectId>?tab=Overview` | Project summary | idem |
| `/projects/<projectId>?tab=Dashboard` | Project metrics | idem |
| `/projects/<projectId>?tab=Board` | Project board | idem |
| `/projects/<projectId>?tab=Timeline` | Timeline / phases | idem |
| `/projects/<projectId>?tab=Calendar` | Project events | idem |
| `/projects/<projectId>?tab=Team` | Project team | idem |
| `/projects/<projectId>?tab=Documents` | Project documents | idem |
| `/team` | Team workload | Authenticated |
| `/reports` | Reports | Authenticated |
| `/analytics` | Analytics | Authenticated |
| `/calendar` | Calendar | Authenticated |
| `/messages` | Messages | Authenticated |
| `/settings` | Settings (Profile by default) | Authenticated |
| `/settings?section=profile` | Profile | Authenticated |
| `/settings?section=preferences` | Preferences | Authenticated |
| `/settings?section=mailbox` | Sending mailbox | Authenticated |

### Parameters
- `tab`: tab of the project detail.
- `section`: section of Settings.
- `task`: id of the task open on the board.

---

## 2. Task statuses

| Status | Meaning | Color |
|---|---|---|
| **To do** | Pending start | Gray |
| **In progress** | In development | Sky blue |
| **Review** | Under review/validation | Peach |
| **Completed** | Finished | Green |

### Transitions observed
- To do → In progress, Review
- In progress → To do, Review
- Review → To do, In progress (and Completed with approval)
- Completed → reopening

---

## 3. Priorities

| Value | Recommended use |
|---|---|
| **High** | Blocks other people, critical date or a client waiting |
| **Medium** | Normal sprint work (default value) |
| **Low** | Improvements, non-urgent technical debt |

---

## 4. Labels (tags)

`Backend`, `Database`, `Design`, `Frontend`, `Research`, `QA`, `Performance` — in addition to `None`.

They are used to:
- Filter on the board and in reports.
- Group estimate accuracy in Analytics (*"Logged vs estimated hours per tag"*).

---

## 5. Project statuses

| Status | Meaning |
|---|---|
| **Planning** | Definition/planning |
| **In progress** | In execution |
| **At risk** | At risk (dates, load, blockers) |
| **On hold** | Paused |
| **Completed** | Finished |
| **Archived** | Archived |

### Health
- **On track**: according to the plan.
- **At risk**: a deviation that requires attention.

---

## 6. Capacity statuses (team)

| Signal | Meaning |
|---|---|
| **On track** | Load within what is expected |
| **At risk** | Close to the limit or with overdue items |
| **Overloaded** | Above capacity |
| **Available** | With room for more work |

---

## 7. Progress modes of a task

| Mode | Calculation |
|---|---|
| **Hours** | Each subtask weighs according to its estimated hours |
| **Percentage** | Each subtask weighs the same |

It is chosen when creating the task (`Measure progress by`) and **cannot be changed** if subtasks already exist.

---

## 8. Calendar event types

| Kind | Use |
|---|---|
| **Meeting** | Team/client meetings |
| **Review** | Sprint or deliverable reviews |
| **Demo** | Demonstrations |
| **Deadline** | Deadlines |
| **Retro** | Retrospectives |
| **Other** | Any other block |

Additional fields: **Place / tool**, **Project**, **Members**, **Departments**.

---

## 9. Metrics glossary

| Term | Definition |
|---|---|
| **Active tasks** | Open (not completed) tasks |
| **Overdue** | Tasks with a past due date that are not completed |
| **Completed this sprint** | Tasks completed within the sprint / sprint total |
| **Hours closed (7d)** | Hours logged and closed in 7 days, with a weekly comparison |
| **Task status distribution** | Distribution of the backlog by status |
| **Team activity** | Actions per day/week over the last 4 weeks |
| **Task priority** | Distribution of open tasks by priority |
| **Capacity** | % of capacity used relative to the available capacity |
| **Tracked today** | Time logged today with the timer |
| **Task time** | Estimated hours of the task |
| **Logged** | Time actually logged |
| **Variance** | Logged − Task time (positive = overconsumption) |
| **Tasks closed** | Tasks closed in the period |
| **Cycle time** | Average time from start to close |
| **Rework** | % of work reopened/redone |
| **Throughput** | Tasks completed per week |
| **Cumulative flow** | Cumulative evolution of tasks by status |
| **Estimate reliability** | % of estimates within ±20% |
| **Delivery / plan** | Compliance with the plan (target 1.00) |
| **Sprint velocity** | Points delivered vs. committed |
| **Load per member** | Active load per person |
| **Estimate accuracy** | Comparison of logged vs. estimated by tag |
| **WIP** | Work in progress (cards in *In progress*) |

---

## 10. Interface glossary

| Element | Meaning |
|---|---|
| **Breadcrumb** | Context path in the header (`Workspace / Page`) |
| **Chip** | Compact clickable or informational label (status, priority, tag) |
| **KPI card** | Card with a large number and its context |
| **Modal** | Overlay window (New task, New event, detail) |
| **Panel inline** | Form that replaces the content (Edit team, Edit task) |
| **Toast** | Brief notice after an action |
| **Empty state** | Message when there is no data ("No threads yet") |
| **Skeleton/Loading** | Loading states ("Loading…", "Restoring session") |
| **Loose task** | Task without a project |
| **Backlog** | Set of pending tasks, not yet in a sprint |
| **Sprint** | Iteration with dates, capacity and committed points |
| **Story points** | Relative unit of effort/complexity |
| **Blocked** | Task that is waiting for a dependency |
| **Depends on** | Dependencies of a task |
| **Owner** | Person responsible for the task |
| **Lead** | Project lead |
| **Collaborators** | Project members besides the lead |
| **Department** | Area of the organization (e.g. Software Development) |

---

## 11. Behavior observations (verified in the UI)

1. **Persistent session**: with *Keep me signed in* the session is restored when you come back; *"Restoring session"* is shown.
2. **Deep links**: project tabs, Settings sections and open tasks use query params, so they can be shared.
3. **Notifications are marked as read when the panel is opened** (the counter disappears and *"You're all caught up"* appears).
4. **The "Upgrade now" button** does not open a flow in the Member account (there is no payment screen exposed).
5. **The "Help & support" link** is a `mailto:support@syncra.app`: it opens the mail client.
6. **Only one timer** active at a time; the Dashboard shows the *Paused* status when it is not running.
7. **"Add to backlog"** is checked by default in New task.
8. **New task defaults**: Priority `Medium`, Estimate `4`, Story points `3`, progress `Hours`.
9. **Board cards show the project** with its code (`PRY-01`) and the truncated name.
10. **Documents** accept up to **25 MB** per file and several at the same time.
11. **The global calendar** may show 0 events in the current week and events when you navigate to other weeks.
12. **The app is an SPA** (React): navigation does not reload the whole page; loading states are brief.
13. **Global search**: the header has a search box that sends to `/board?q=<text>`; the results include subtasks with the path `↳ Parent task` and a **clear** link to clear the search.
14. **"+ Add task"** on a column opens **New task** with the project preloaded (it does not set the status automatically).
15. **Language and layout** are saved **in the browser**, not in the account.
16. **Project activity** records creations, moves and completions with author and date.

---

## 12. Observed endpoints (technical reference)

During navigation, the SPA consumes a REST API under `/api/v1`. Examples seen in the network tab:

| Endpoint | Use |
|---|---|
| `GET /api/v1/calendar/events?from=YYYY-MM-DD&to=YYYY-MM-DD` | Events in the visible range |
| `GET /api/v1/time/timer` | Timer status (204 if there is none) |
| `GET /api/v1/notifications?limit=10` | Recent notifications |
| `GET /api/v1/threads` | Message threads |
| `GET /api/v1/media/<id>` | Images/avatars and files |

> This list is illustrative only; using the documentation does not require knowing the API.

---

## 13. Coverage of this documentation

| Area | Documented | Evidence |
|---|---|---|
| Login / logout / session | ✅ | Screens and flow |
| Password recovery | ✅ | Screen |
| Invitations | ✅ | Screen |
| Dashboard | ✅ | Screenshots and metrics |
| Board | ✅ | Screenshot, cards, filters |
| Create/edit task | ✅ | Complete forms |
| Subtasks/dependencies/checklist | ✅ | Task detail |
| Timer | ✅ | Dashboard and detail |
| Projects (7 tabs) | ✅ | Screenshots of each tab |
| Documents | ✅ | List, limits and actions |
| Team/capacity | ✅ | Screenshot |
| Reports and exports | ✅ | Screenshot and tables |
| Analytics | ✅ | Screenshots |
| Calendar and events | ✅ | Screenshots and form |
| Messages | ✅ (empty state) | Screenshot |
| Settings (3 sections) | ✅ | Screenshots |
| Notifications and menu | ✅ | Screenshots |
| Roles and limits | ✅ | Evidence in the UI |
| Admin (sprints, project creation, members, billing) | ⚠️ Referenced, not accessible with the Member role | Mentions in the UI |
