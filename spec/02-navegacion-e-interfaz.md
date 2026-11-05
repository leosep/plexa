# 02 · Navigation and interface

How the screen is organized inside the application and where each thing is.

![App overview](img/dashboard.jpeg)

---

## 1. Screen structure

The app uses a three-zone layout:

```
┌───────────────────────────────────────────────────────────────┐
│ SIDEBAR (left)       │  TOPBAR (header)                       │
│                      ├────────────────────────────────────────┤
│  Workspace           │  Breadcrumb · messages · bell ·        │
│  Main menu           │  user                                  │
│  Active sprint       ├────────────────────────────────────────┤
│  General             │                                        │
│  Upgrade to Pro      │        PAGE CONTENT                    │
│                      │                                        │
└───────────────────────────────────────────────────────────────┘
```

---

## 2. Sidebar

### 2.1 Workspace selector (top)
Shows:
- Square workspace logo (or initials).
- Name: **Grupo Dominican Watchman**.
- Subtitle: **Workspace · 3 people**.
- An arrow (⌄) indicating that it is a selector.

If you belong to more than one workspace, this control lets you switch between them; the active workspace determines all the data you see (projects, team, reports…).

### 2.2 **MENU** section
| Element | Route | What it is for |
|---|---|---|
| **Dashboard** | `/dashboard` | General summary: KPIs, activity, priorities, timer and team workload |
| **Task board** | `/board` | Global Kanban board with all tasks |
| **Project detail** | `/projects` | Project listing (opening one shows its detail) |
| **Team workload** | `/team` | Workload and capacity of each person |
| **Reports** | `/reports` | Estimated vs logged hours and report by department |
| **Analytics** | `/analytics` | Delivery and quality metrics |
| **Calendar** | `/calendar` | Shared event calendar |
| **Messages** | `/messages` | Project and team channels |

The active item is highlighted with a light blue background and a turquoise side bar.

### 2.3 **ACTIVE SPRINT** card
Informational block below the menu.
- If there is an active sprint, it shows its name, dates and progress.
- If there is none, it shows: **"No active sprint. Create one in Settings → Sprints."**

In the documented workspace it appears **with no active sprint**. Sprints are created in *Settings → Sprints*, a section that is not available to the **Member** role (see [14 · Roles and limits](14-roles-permisos-y-limites.md)).

### 2.4 **GENERAL** section
| Element | Action |
|---|---|
| **Settings** | Opens settings (Profile, Preferences, My mailbox) |
| **Help & support** | Opens your mail client with `mailto:support@syncra.app` |
| **Log out** | Ends the session |

### 2.5 **Upgrade to Pro** card
Promotional block:
- Title **"Upgrade to Pro"**.
- Text: *"Unlimited projects and automated reports"*.
- Circular button with an up arrow (`aria-label="Upgrade now"`).

> In the documented account, pressing the button **does not open any screen or modal** (there is no payment flow exposed for this role). It is documented as a commercial notice.

---

## 3. Header (topbar)

From left to right:

1. **Menu button** (`aria-label="Open menu"`) — visible on narrow screens; collapses/expands the sidebar.
2. **Breadcrumb**: `Grupo Dominican Watchman / <Current page>`. Examples: `Grupo Dominican Watchman / Task board`, `… / Settings`.
3. **Global search** (`Search tasks, people, docs…`): submitting it goes to `/board?q=<text>`. On narrow windows it is hidden for space reasons.
4. **Messages icon** (chat bubble) — direct access to `/messages`.
5. **Notifications bell** — shows a red counter with the unread ones (in the screenshot, **4**).
6. **User chip**: avatar with initials, name and role (`Leandro Sepulveda` / `Miembro`); pressing it opens the user menu.

### User menu
![User menu](img/menu-usuario.jpeg)

| Option | Action |
|---|---|
| **My profile** | Goes to *Settings → Profile* |
| **Settings** | Goes to *Settings* |
| **Log out** | Logs out |
| **Language** | **ES / EN** toggle |

---

## 4. Page content

Every page follows the same pattern:

1. **Large title** (e.g. *"Task board"*).
2. **Subtitle** with context and summary metrics (e.g. *"All projects · Backlog · 14 tasks"*).
3. **Actions** on the right (filters, buttons such as **"New task"**, **"New event"**, **"Export CSV"**…).
4. **Cards / tables / charts** with the data.

---

## 5. Responsive behavior

- At narrow widths, the sidebar is hidden and the **"Open menu"** button appears in the header to open it.
- Board columns and tables compress; some columns end up off screen and are reached by scrolling the page or the card horizontally.
- Modals adapt to the available width.

---

## 6. Common interaction patterns

| Pattern | Where it appears | How it is used |
|---|---|---|
| **Filter chips** | Projects, Reports | Press to filter; the active chip is highlighted and shows a counter |
| **Chip-style selects** | Board (`All projects`, `Everyone`), Reports | Native dropdown with options |
| **Modals** | New task, New event, task detail, event detail | Closed with the X, with **Cancel** or by clicking outside |
| **Inline panels** | *Edit team*, task *Edit* | The form replaces the content; **Cancel** discards changes |
| **Toasts** | When saving actions | Brief confirmation message |
| **Empty states** | Messages, Timeline, Milestones | Explain what is missing and how to continue |
| **Loading states** | The whole app | "Loading…" / "Restoring session" / spinners |

---

## 7. Navigation shortcuts

- The header **Breadcrumb**: it is not always clickable; it serves as context.
- In the project detail, the **"← Projects"** link goes back to the listing.
- In the project detail, the tabs change the URL: `?tab=Overview|Dashboard|Board|Timeline|Calendar|Team|Documents`.
- On the board, opening a task adds `?task=<id>` to the URL (the direct link to a task can be shared).
- In settings, the section is reflected in `?section=profile|preferences|mailbox`.
