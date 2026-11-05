# 15 · Recipes and workflows

Step-by-step guides for the most frequent tasks. All of them can be carried out with the **Member** role unless stated otherwise.

---

## Recipe 1 · Starting the day

1. Go to `https://syncra.liobella.com`.
2. Review the **Dashboard**:
   - How many **Overdue** items are there?
   - How is **Completed this sprint** doing?
   - Is there an **active sprint**?
3. Open the **bell** and review *Recent activity* (remember: opening it marks them as read).
4. Go to **Task board** and filter by yourself (`Everyone` → your name).
5. Pick the first task, open it and click **Start timer**.

---

## Recipe 2 · Creating a task and leaving it ready

1. **Task board → New task**.
2. **Title**: verb + result ("Implementar API de cartelera").
3. **Project**: choose the project (or *Loose task* if it is a standalone task).
4. **Assignee**: you or whoever will do it.
5. **Priority**: `High` only if it blocks someone or has a critical date.
6. **Tag**: `Backend`, `Frontend`, `QA`…
7. **Due date** and **Start** if applicable.
8. **Estimate (hours)**: your best estimate.
9. **Story points** if the team plans by points.
10. **Measure progress by**: `Hours` if you are going to estimate subtasks; `Percentage` if they are equivalent steps.
11. **Subtasks**: add the parts if there are more than 2–3.
12. **Depends on**: mark what must finish first.
13. **Description**: context, links and acceptance criteria.
14. **Create task**.

---

## Recipe 3 · Working a task (full flow)

1. On the board, move the card to **In progress** (`Move…`).
2. Open the detail and click **Start timer**.
3. Work; tick off **Checklist** items and subtasks as you make progress.
4. When you finish, move the task to **Review**.
5. Stop the timer.
6. Once it is approved, move it to **Completed**.

> If the task cannot start, you will see *"It cannot start yet; it waits for: …"* → resolve the dependency first.

---

## Recipe 4 · Splitting a large task

1. Open the task → **Edit**.
2. Verify **Measure progress by = Hours** (recommended).
3. Estimate the total hours.
4. Add **subtasks** with hours and owners.
5. Save.
6. Check that *"Task hours not allocated to subtasks"* is left at `0h 00m`.
7. Work on each subtask by moving it separately.

---

## Recipe 5 · Blocking work that cannot start

1. Edit the dependent task.
2. Under **Depends on**, mark the task or subtask that must be completed first.
3. Save.
4. The task will show **Blocked** and the waiting notice.
5. When the dependency is completed and approved, the block disappears.

---

## Recipe 6 · Logging forgotten time

1. Open **Reports**.
2. Enable **"Time logged"** to see what has a record and what does not.
3. For tasks with `LOGGED 0h 00m`, open each task and use time logging (**Start timer** or time editing, depending on permissions).
4. Go back to Reports and confirm that the variance updates.

---

## Recipe 7 · Preparing the daily

1. **Team workload**: review `Overdue` and each person's signal.
2. **Task board**: identify `Blocked` cards and those that have been in *In progress* for a long time.
3. **Dashboard**: look at *Overdue* and *Task priority*.
4. Write down:
   - What I did yesterday (tasks moved/completed).
   - What I will do today (next task on the board).
   - Blockers (dependencies, access, decisions).
5. After the daily, update statuses and dates.

---

## Recipe 8 · Reviewing a project with the client

1. **Projects** → open the project.
2. **Overview**: health, dates, 3/14 tasks and activity.
3. **Dashboard**: tasks/points delivered and overdue.
4. **Timeline**: phases (if they are defined).
5. **Documents**: show the deliverables.
6. **Reports**: filter by the project/sprint and export **PDF** if you need a report.

---

## Recipe 9 · Exporting hours for billing

1. **Reports**.
2. Select **Member** (or *All members*), **Sprint** and **Tag**.
3. Enable **"Time logged"**.
4. Click **Export Excel** (or CSV/PDF).
5. Verify the file: task, owner, tag, estimated time, logged time and variance.

---

## Recipe 10 · Creating a meeting or deadline

1. **Calendar → New event**.
2. Clear title ("Revisión sprint 4 con cliente").
3. Date and times.
4. **Kind**: `Meeting`, `Review`, `Demo`, `Deadline`, `Retro` or `Other`.
5. **Place / tool**: room or video call.
6. **Project**: link it.
7. **Members** / **Departments**: invite.
8. **Add event**.
9. Check that it appears on the day and in the project's **Calendar** tab.

---

## Recipe 11 · Publishing a deliverable

1. Open the project → **Documents**.
2. **Attach documents**.
3. Select the file (< 25 MB, allowed format).
4. Verify that it appears with size, author and date.
5. Notify via **Messages** or on the corresponding task.
6. To remove it, use **Delete** (confirm first).

---

## Recipe 12 · Preparing the sprint close (with an administrator)

1. Verify that no `Blocked` tasks are left without a reason.
2. Move what is finished to **Completed** and what is not finished to the next sprint (an administrator does this).
3. Review **Analytics**: throughput, cycle time, rework, velocity.
4. Export the sprint **Reports**.
5. Document agreements and next steps.
6. The administrator **closes the sprint** in *Settings → Sprints*.

---

## Recipe 13 · Adding someone to a project

1. Open the project → **Team**.
2. **Edit team**.
3. Select **Department**, **Project lead** and **Collaborators**.
4. **Save team**.
5. The person will see the project and its tasks.
6. If you cannot see it yourself, ask the administrator to add you or to review permissions.

---

## Recipe 14 · Adjusting your environment

1. **Settings → Preferences**.
2. Uncheck **"Show the upgrade block in the sidebar"** if you want more space.
3. Uncheck **"Show the insights panel on project pages"** if you prefer a clean view.
4. Choose **ES/EN**.
5. **Settings → Profile → Choose image** for your photo.

---

## Recipe 15 · Recovering access

1. On the login screen, **"Forgot your password?"**.
2. Enter your email and click **Send instructions**.
3. Open the link in the email and set the password.
4. Go back to **Sign in**.
5. If the email does not arrive: check spam, confirm the domain and contact `support@syncra.app`.

---

## Quick map "I want → I do"

| I want… | Route / action |
|---|---|
| See the overall status | Dashboard |
| View/move tasks | Task board |
| Create work | New task |
| View a project | Projects → name |
| Know who can take on more | Team workload |
| Compare estimated vs actual | Reports |
| See trends | Analytics |
| Schedule something | Calendar → New event |
| Talk to the team | Messages |
| Change my photo/language | Settings |
| Sign out | User menu → Log out |
| Help | Sidebar → Help & support (`support@syncra.app`) |
