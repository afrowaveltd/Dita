/* ═══════════════════════════════════════════════════════════════
   Dita – Global Theme Switcher
   Manages the data-dita-theme attribute on <html> and persists
   the user's preference in localStorage.
   ═══════════════════════════════════════════════════════════════ */

(function () {
  "use strict";

  const STORAGE_KEY = "dita-theme";
  const DARK = "dark";
  const LIGHT = "light";

  const html = document.documentElement;
  const toggleBtn = document.getElementById("globalThemeToggle");
  const toggleIcon = document.getElementById("themeToggleIcon");

  /* ── Helpers ───────────────────────────────────────────────── */

  function getStored() {
    try { return localStorage.getItem(STORAGE_KEY); } catch { return null; }
  }

  function setStored(theme) {
    try { localStorage.setItem(STORAGE_KEY, theme); } catch { /* no-op */ }
  }

  function applyTheme(theme) {
    html.setAttribute("data-dita-theme", theme);
    if (toggleIcon) {
      toggleIcon.textContent = theme === DARK ? "☀" : "☾";
    }

    setStored(theme);
  }

  function currentTheme() {
    return html.getAttribute("data-dita-theme") || DARK;
  }

  function toggleTheme() {
    applyTheme(currentTheme() === DARK ? LIGHT : DARK);
  }

  /* ── Initialise ────────────────────────────────────────────── */

  /* Restore saved preference or fall back to dark */
  const saved = getStored();
  applyTheme(saved === LIGHT ? LIGHT : DARK);

  /* Wire up the global toggle */
  if (toggleBtn) {
    toggleBtn.addEventListener("click", toggleTheme);
  }

  /* ── Mobile nav toggle ─────────────────────────────────────── */

  const navToggle = document.getElementById("navToggle");
  const navLinks = document.getElementById("navLinks");
  if (navToggle && navLinks) {
    navToggle.addEventListener("click", function () {
      const open = navLinks.classList.toggle("show");
      navToggle.setAttribute("aria-expanded", open);
      navToggle.textContent = open ? "✕" : "☰";
    });
  }
})();
