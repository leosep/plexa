// Plexa · comportamiento de cliente (htmx + Alpine + Chart.js)
(function () {
  "use strict";

  const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* ---------------------------------- Toasts --------------------------------- */
  function toastContainer() {
    let el = document.getElementById("toasts");
    if (!el) {
      el = document.createElement("div");
      el.id = "toasts";
      el.className = "fixed bottom-4 right-4 z-[80] flex w-full max-w-sm flex-col gap-2";
      el.setAttribute("aria-live", "polite");
      document.body.appendChild(el);
    }
    return el;
  }

  function showToast(type, message, undoUrl) {
    if (!message) return;
    const tones = {
      success: "border-brand-500 bg-white text-ink-900",
      error: "border-danger-500 bg-white text-ink-900",
      info: "border-cyan-500 bg-white text-ink-900"
    };
    const icons = { success: "✓", error: "!", info: "i" };
    const el = document.createElement("div");
    el.className = `flex items-start gap-3 rounded-xl border-l-4 p-3 shadow-card ${tones[type] || tones.info}`;
    el.setAttribute("role", "status");
    el.innerHTML = `
      <span class="mt-0.5 grid size-5 shrink-0 place-items-center rounded-full bg-ink-100 text-xs font-semibold">${icons[type] || "i"}</span>
      <p class="flex-1 text-sm">${escapeHtml(message)}</p>
      ${undoUrl ? `<a href="${undoUrl}" class="text-sm font-medium text-brand-700 underline">Undo</a>` : ""}
      <button type="button" class="text-ink-400 hover:text-ink-900" aria-label="Dismiss">×</button>`;
    el.querySelector("button").addEventListener("click", () => el.remove());
    toastContainer().appendChild(el);
    window.setTimeout(() => {
      el.style.transition = "opacity .2s ease";
      el.style.opacity = "0";
      window.setTimeout(() => el.remove(), 220);
    }, 4200);
  }

  function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
  }

  window.plexaToast = showToast;

  document.body.addEventListener("toast", (event) => {
    const detail = event.detail || {};
    showToast(detail.type || "info", detail.message, detail.undoUrl);
  });

  document.body.addEventListener("htmx:afterRequest", (event) => {
    const header = event.detail.xhr.getResponseHeader("HX-Trigger");
    if (!header) return;
    try {
      const data = JSON.parse(header);
      if (data.toast) showToast(data.toast.type, data.toast.message, data.toast.undoUrl);
    } catch (_) { /* ignorado */ }
  });

  /* ---------------------------------- Modales -------------------------------- */
  function closeModal() {
    const root = document.getElementById("modal-root");
    if (!root) return;
    const dialog = root.querySelector("dialog");
    if (dialog) dialog.close();
    root.innerHTML = "";
    document.body.style.overflow = "";
    if (window.location.search.includes("task=")) {
      const url = new URL(window.location.href);
      url.searchParams.delete("task");
      history.pushState({}, "", url.toString());
    }
  }

  window.plexaCloseModal = closeModal;

  document.body.addEventListener("htmx:afterSwap", (event) => {
    const target = event.detail.target;
    if (target && target.id === "modal-root") {
      const dialog = target.querySelector("dialog");
      if (dialog && !dialog.open) {
        dialog.showModal();
        document.body.style.overflow = "hidden";
        const focusable = dialog.querySelector("[autofocus], input, select, textarea, button");
        if (focusable) window.setTimeout(() => focusable.focus(), 40);
      }
    }
  });

  document.body.addEventListener("closeModal", closeModal);

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      const root = document.getElementById("modal-root");
      if (root && root.querySelector("dialog[open]")) closeModal();
    }
  });

  document.body.addEventListener("click", (event) => {
    const dialog = event.target.closest("dialog");
    if (dialog && event.target === dialog) dialog.close();
  });

  /* ------------------------------- Atajos teclado ---------------------------- */
  const isTyping = (el) =>
    el && (el.tagName === "INPUT" || el.tagName === "TEXTAREA" || el.tagName === "SELECT" || el.isContentEditable);

  document.addEventListener("keydown", (event) => {
    if (isTyping(document.activeElement) || event.metaKey || event.ctrlKey || event.altKey) {
      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        document.getElementById("global-search")?.focus();
      }
      return;
    }

    if (event.key === "/") {
      event.preventDefault();
      document.getElementById("global-search")?.focus();
    } else if (event.key.toLowerCase() === "n" && !event.shiftKey) {
      const trigger = document.querySelector("[data-shortcut-new-task]");
      if (trigger) {
        event.preventDefault();
        trigger.click();
      }
    } else if (event.key === "?") {
      event.preventDefault();
      showToast("info", "Shortcuts: / search · N new task · Esc close · ? help");
    }
  });

  /* ---------------------------------- Gráficos ------------------------------- */
  const palette = {
    ink: "#0F2A3D",
    ink400: "#8696A5",
    grid: "#EEF3F6",
    brand: "#0AA58A",
    cyan: "#00AEC0",
    accent: "#F28C28",
    danger: "#D9503F",
    todo: "#C9CED6",
    progress: "#7FD6E0",
    review: "#F6B98A",
    done: "#8FD9C5"
  };

  window.plexaCharts = { palette };

  window.plexaChart = function (canvas, config) {
    if (!canvas || typeof Chart === "undefined") return null;
    return new Chart(canvas.getContext("2d"), Object.assign({
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: reduceMotion ? false : { duration: 500 },
        plugins: { legend: { labels: { color: palette.ink, boxWidth: 10, usePointStyle: true } } },
        scales: {
          x: { grid: { color: palette.grid }, ticks: { color: palette.ink400 } },
          y: { grid: { color: palette.grid }, ticks: { color: palette.ink400 }, beginAtZero: true }
        }
      }
    }, config));
  };

  /* ---------------------------------- Timer ---------------------------------- */
  window.plexaTimer = function (initial) {
    return {
      taskId: initial.taskId,
      running: initial.running,
      seconds: initial.seconds || 0,
      busy: false,
      timer: null,
      init() {
        if (this.running) this.startTicking();
      },
      startTicking() {
        this.stopTicking();
        this.timer = window.setInterval(() => { this.seconds += 1; }, 1000);
      },
      stopTicking() {
        if (this.timer) window.clearInterval(this.timer);
        this.timer = null;
      },
      async toggle() {
        if (this.busy) return;
        this.busy = true;
        try {
          const endpoint = this.running
            ? `/api/v1/time/timer/stop`
            : `/api/v1/time/timer/start?taskId=${this.taskId}`;
          const response = await fetch(endpoint, {
            method: "POST",
            headers: { "X-CSRF-TOKEN": document.querySelector('meta[name="csrf-token"]')?.content || "" }
          });
          if (!response.ok) throw new Error("timer");
          this.running = !this.running;
          if (this.running) {
            this.seconds = 0;
            this.startTicking();
            showToast("success", "Timer started");
          } else {
            this.stopTicking();
            showToast("success", "Timer stopped");
          }
          document.body.dispatchEvent(new CustomEvent("task:changed"));
        } catch (_) {
          showToast("error", "The timer could not be updated.");
        } finally {
          this.busy = false;
        }
      },
      label() {
        const h = Math.floor(this.seconds / 3600);
        const m = Math.floor((this.seconds % 3600) / 60);
        const s = this.seconds % 60;
        return h > 0
          ? `${h}:${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`
          : `${m}:${String(s).padStart(2, "0")}`;
      }
    };
  };

  /* --------------------------- Refresco de tablero --------------------------- */
  document.body.addEventListener("task:changed", () => {
    const board = document.getElementById("board-results");
    if (board && window.htmx) {
      htmx.ajax("GET", window.location.pathname + window.location.search, { target: "#board-results", swap: "outerHTML" });
    }
  });

  document.body.addEventListener("notifications:changed", () => {
    const badge = document.getElementById("notification-badge");
    if (badge) badge.remove();
  });

  /* ------------------------------ Sondeo suave ------------------------------- */
  function poll() {
    fetch("/notifications/unread", { headers: { "X-Requested-With": "fetch" } })
      .then((r) => (r.ok ? r.json() : null))
      .then((data) => {
        if (!data) return;
        const badge = document.getElementById("notification-badge");
        if (badge) {
          badge.textContent = data.notifications > 9 ? "9+" : String(data.notifications);
          badge.hidden = data.notifications === 0;
        }
        const chat = document.getElementById("messages-badge");
        if (chat) {
          chat.textContent = data.messages > 9 ? "9+" : String(data.messages);
          chat.hidden = data.messages === 0;
        }
      })
      .catch(() => {});
  }

  if (document.getElementById("notification-badge")) {
    window.setInterval(poll, 60000);
  }
})();
