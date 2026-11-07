/* FINOVA single sign-on: "FINOVA Home" button in the application header (all FINOVA apps) */
(function () {
  "use strict";
  var MAIN = "FINOVA";                                   /* alias of the FINOVA main application */
  function env() { return (window.apex && apex.env) || {}; }
  function base() { var p = location.pathname, i = p.indexOf("/r/"); return i >= 0 ? p.slice(0, i + 1) : p.replace(/[^\/]*$/, ""); }
  function visible(el) { return !!(el && (el.offsetWidth || el.offsetHeight || el.getClientRects().length)); }
  function add() {
    var bar = document.querySelector(".t-Header-navBar");
    if (!bar || document.getElementById("fnvHomeBtn")) return;
    var user = env().APP_USER || "";
    if (!user || /^nobody$/i.test(user)) return;
    var links = bar.querySelectorAll("a[href*='" + MAIN + ":1:']"), i;
    for (i = 0; i < links.length; i++) { if (visible(links[i])) return; }   /* already shown by the nav bar */
    var a = document.createElement("a");
    a.id = "fnvHomeBtn"; a.className = "fnv-home-btn";
    a.href = base() + "f?p=" + MAIN + ":1:" + (env().APP_SESSION || "") + "::NO:::";
    a.title = "Back to the FINOVA application launcher";
    a.innerHTML = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'
      + '<rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></svg>'
      + '<span>FINOVA Home</span>';
    bar.insertBefore(a, bar.firstChild);
    if (!document.getElementById("fnvHomeCss")) {
      var st = document.createElement("style"); st.id = "fnvHomeCss";
      st.textContent = ".t-Header-navBar{display:flex;align-items:center;gap:8px}"
        + ".fnv-home-btn{display:inline-flex;align-items:center;gap:7px;height:34px;padding:0 12px;margin-inline-end:4px;border-radius:10px;"
        + "background:#014B55;border:1px solid rgba(255,255,255,.28);color:#fff!important;box-shadow:0 2px 8px -4px rgba(0,0,0,.4);font:600 13px/1 Inter,'Segoe UI',Arial,sans-serif;"
        + "text-decoration:none!important;white-space:nowrap;transition:background .15s,border-color .15s}"
        + ".fnv-home-btn:hover{background:#E87722;border-color:#E87722}"
        + ".fnv-home-btn:focus-visible{outline:2px solid #F59A3C;outline-offset:2px}"
        + "@media(max-width:640px){.fnv-home-btn span{display:none}.fnv-home-btn{padding:0 9px}}";
      document.head.appendChild(st);
    }
  }
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", function () { add(); setTimeout(add, 400); });
  else { add(); setTimeout(add, 400); }
})();
