/* ═══════════════════════════════════════════════════════════════
   Dita – Global Theme Switcher
   Manages the data-dita-theme attribute on <html> and persists
   the user's preference in localStorage. Also keeps the dashboard
   theme link in sync when on the LiveTranslation page.
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

    const dashboardEl = document.querySelector(".translation-service-dashboard");
    if (dashboardEl) {
      dashboardEl.dataset.translationTheme = theme;
    }

    /* Keep the dashboard inline theme-toggle label in sync */
    const dashLabel = document.getElementById("themeToggleLabel");
    if (dashLabel) {
      dashLabel.textContent = theme === DARK ? "Light" : "Dark";
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

  /* Also intercept the dashboard's own toggle so both stay in sync */
  const dashToggle = document.getElementById("themeToggleBtn");
  if (dashToggle) {
    /* Remove the inline handler (set in LiveTranslation.cshtml script)
       by cloning the button and replacing it.                    */
    const clone = dashToggle.cloneNode(true);
    dashToggle.parentNode.replaceChild(clone, dashToggle);
    clone.addEventListener("click", toggleTheme);
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