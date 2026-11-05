# 14 · Roles, permissions and limits

What the documented account can do and what stays reserved for administrators. It is based on what the interface shows and hides for a user with the **Member** role who is also the **lead** of the existing project.

---

## 1. Roles observed in the workspace

| Person | Role in the workspace | Position |
|---|---|---|
| **Gerson Cabreja** | **Administrator** | Administrator |
| **Leandro Sepulveda** | **Member** | Full-Stack Developer |
| **Yoderick Emmanuel Montero Mejia** | **Member** | Full-Stack Developer |

The role is shown in the header user chip (`Miembro` / `Member`) and in the *Team workload* table.

---

## 2. What a Member can do (verified in the interface)

| Action | Available? | Where |
|---|---|---|
| View the Dashboard and metrics | ✅ | `/dashboard` |
| View and use the board | ✅ | `/board` |
| Create tasks | ✅ | **New task** / **+ Add task** |
| Edit tasks | ✅ | Detail → **Edit** |
| Create subtasks | ✅ | Detail → **New subtask** / form |
| Define dependencies | ✅ | Form |
| Move tasks between states | ✅ | **Move…** / **Move to…** |
| Start/stop the timer | ✅ | Detail → **Start timer** |
| Add steps to the checklist | ✅ | Detail → **Checklist** |
| View projects and their detail | ✅ | `/projects` |
| Edit the team of a project they lead | ✅ | Project → **Team** → **Edit team** |
| Upload documents to the project | ✅ | Project → **Documents** → **Attach documents** |
| View reports and export | ✅ | `/reports` |
| View analytics | ✅ | `/analytics` |
| View the calendar and create events | ✅ | `/calendar` → **New event** |
| View messages | ✅ (empty) | `/messages` |
| Edit their profile photo | ✅ | Settings → Profile |
| Change layout/language preferences | ✅ | Settings → Preferences |
| Configure their sending mailbox | ✅ | Settings → My mailbox |
| Log out | ✅ | User menu |

---

## 3. What does not appear for a Member

| Action | Evidence in the interface |
|---|---|
| **Create sprints** | The *Active sprint* card says *"Create one in Settings → Sprints"*, but **Settings does not show "Sprints"**. |
| **Create projects** | The listing says *"create, edit, archive"*, but **there is no creation button** for this role. |
| **Change name, position or capacity** | *"Ask a workspace admin to change your name, title or capacity."* |
| **Create message channels** | *"Managers can open a project or team channel from a project page."* |
| **Manage workspace members/roles** | There is no members section in Settings. |
| **Billing / Upgrade** | The **Upgrade now** button does not open any flow in this account. |
| **Delete tasks** | There is no delete action in the detail. |

> These absences are **due to the role**, not a bug: the app reserves them for administrators/managers.

---

## 4. Project-level permissions

The **Team** tab explains it:

> *"Only the team and people allowed to see every project can open this project and its tasks."*

Therefore, access to a project depends on:

1. Being a **Project lead**.
2. Being a **Collaborator**.
3. Having global permission to **see all projects** (administrators).

**Practical consequence:** if you do not see a project or its tasks, ask to be added in **Team → Edit team** or to be given global permission.

The **Project lead** can edit the team (department, lead and collaborators). Other members probably cannot; in the tested account the user was the lead, so the form was indeed available.

---

## 5. Permissions on documents

In **Documents** any member of the project sees:

- **Download** on all files.
- **Delete** on the files (with confirmation).

> The **Delete** button appears for the documented user; use it carefully: it deletes the file for the whole team. Before deleting, confirm that it is not the only copy.

Limits:
- **25 MB per file**.
- Several files at the same time.
- Allowed extensions: PDF, Office, OpenDocument, images, text, CSV, Markdown and ZIP.

---

## 6. Permissions on tasks

- **Assignment**: any member can assign to any person in the workspace.
- **Movement**: transitions between states are allowed (including going back).
- **Project change**: only while the task **has not started**; it drags subtasks along and **removes dependencies**.
- **Progress mode**: it is locked once there are subtasks (*"It cannot change: the task already has subtasks."*).
- **Deletion**: not exposed.

---

## 7. Governance recommendations

1. **Define a clear administrator** (in this workspace, Gerson Cabreja) for new members, sprints and projects.
2. **Assign leads** per project: they can manage their team without granting global permissions.
3. **Review collaborators** when onboarding or rotating people.
4. **Do not share the account**: each person should have their own user so that activity and time are traceable.
5. **Document each person's role** in their position/capacity (the administrator edits it).

---

## 8. If you need a permission you do not have

1. Identify the exact action (e.g. "create sprint", "view project X").
2. Check whether it depends on the **role** or on **membership of the project**.
3. Write to the administrator with the context (project, task, reason).
4. In the meantime, use the available alternatives: create the task in an existing project, ask to be assigned as a collaborator, or log the work on an already-created task.
