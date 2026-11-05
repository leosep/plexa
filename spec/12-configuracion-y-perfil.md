# 12 · Settings and profile

Route: `/settings` · **Settings** side menu.

Settings are divided into sections that open in the left submenu and are reflected in the URL (`?section=profile|preferences|mailbox`).

Sections available to the **Member** role:

1. **Profile**
2. **Preferences**
3. **My mailbox**

> The workspace also shows references to *Settings → Sprints* (to create sprints), but that section **does not appear** for the Member role; it is managed by an administrator.

---

## 1. Profile

Route: `/settings?section=profile`

![Settings · Profile](img/ajustes-perfil.jpeg)

### What it contains
- **Profile picture**
  - Formats: **PNG, JPEG or GIF**.
  - Maximum size: **2 MB**.
  - It is cropped to a **square**.
  - **"Choose image"** button to upload/change the photo.
- **Name**: Leandro Sepulveda.
- **Email**: `lsepulveda@dwn.com.do`.
- **Title and role**: `Full-Stack Developer · Member`.
- Note: *"Ask a workspace admin to change your name, title or capacity."*

### Changing the photo
1. Click **"Choose image"**.
2. Select a PNG/JPEG/GIF file of up to 2 MB.
3. Adjust the square crop if the app requests it.
4. Save. The new image appears in the header, cards and tables.

### What you cannot change yourself
- **Name**, **title** and **capacity**: these are modified by a workspace administrator.
- **Email**: tied to the account; managed by an administrator.

---

## 2. Preferences

Route: `/settings?section=preferences`

![Settings · Preferences](img/ajustes-preferencias.jpeg)

### Layout (configurable panels)
Two checkboxes:

| Option | Effect |
|---|---|
| **"Show the insights panel on project pages"** | Shows/hides the insights panel on project pages |
| **"Show the upgrade block in the sidebar"** | Shows/hides the **Upgrade to Pro** card in the sidebar |

> **"Saved in this browser only."** → these preferences are local to the browser; they do not affect other users or other devices.

### Language
- **"Interface language for this browser"**.
- **ES** / **EN** buttons.
- Changes the interface texts in this browser.

### How to use them
1. Go to **Settings → Preferences**.
2. Check/uncheck the boxes according to your ideal workspace.
3. Choose **ES** or **EN**.
4. Changes are applied instantly and saved in the browser.

---

## 3. My mailbox

Route: `/settings?section=mailbox`

![Settings · My mailbox](img/ajustes-buzon.jpeg)

It is used so that **the reports you send from the app go out from your own mailbox**.

### Screen text
> *"The mail you send from Syncra leaves from this address, through your organisation's server. The password is stored encrypted and never shown."*

### Fields
| Field | Description |
|---|---|
| **Email** | Address from which the messages will be sent |
| **Password** | Password or **app password** for the mailbox |
| **Save mailbox** | Saves the configuration |

### Important note
> *"If your provider uses two-step verification (Gmail, Outlook), use an app password."*

If your provider uses two-step verification, **generate an app password** and paste it here; do not use your normal password.

### How to configure it
1. Open **Settings → My mailbox**.
2. Enter the **email** from which you want to send.
3. Enter the **app password**.
4. Click **"Save mailbox"**.
5. Send a test report to confirm.

### Security
- The password is stored **encrypted** and **never shown**.
- If you change your email password, update it here.
- If sending fails, check: the organisation's SMTP server, the app password, and that the mailbox allows sending.

---

## 4. Administration sections (not visible to Member)

The app mentions these capabilities in different places; they belong to administrators:

| Section | Where it is mentioned | What it is for |
|---|---|---|
| **Sprints** | *Active sprint* card: *"Create one in Settings → Sprints"* | Create/close sprints, capacity, point commitment |
| **Members / team** | *"Ask a workspace admin to change your name, title or capacity"* | Onboarding, roles, titles, capacity |
| **Projects** | *"Manage the portfolio: create, edit, archive…"* | Creating/editing/archiving projects |
| **Billing / Pro** | *Upgrade to Pro* card | Plan and billing |

If you need any of these actions, contact an administrator (in this workspace, **Gerson Cabreja**).

---

## 5. Quick summary

| I want to… | I go to… |
|---|---|
| Change my photo | Settings → Profile → Choose image |
| Hide the upgrade block | Settings → Preferences |
| Hide insights in projects | Settings → Preferences |
| Change language | User menu or Settings → Preferences → ES/EN |
| Send reports from my email | Settings → My mailbox |
| Change my name/title | Ask an administrator |
