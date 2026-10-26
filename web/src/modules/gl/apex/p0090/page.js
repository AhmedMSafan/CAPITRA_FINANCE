(function () {
  var root = document.getElementById('glHelp'); if (!root) return;
  /* the Balances tab comes in its own region (region size limit): move it in before Quick rules */
  var p7 = document.getElementById('ghP7'), p6 = document.getElementById('ghP6');
  if (p7 && p6 && p7.parentNode !== p6.parentNode) { var r7 = document.getElementById('glHelpR7'); p6.parentNode.insertBefore(p7, p6); if (r7) r7.style.display = 'none'; }
  var tabs = [].slice.call(root.querySelectorAll('.gh-tab')), dots = root.querySelectorAll('.gh-dots i'), cur = 0;
  var prev = document.getElementById('ghPrev'), next = document.getElementById('ghNext');
  function closeHelp() {
    window.close();                       /* works when Help opened this window */
    setTimeout(function () { if (!window.closed) history.back(); }, 200);
  }
  var cl = document.getElementById('ghClose'); if (cl) cl.addEventListener('click', closeHelp);
  function show(i, focus) {
    cur = Math.max(0, Math.min(tabs.length - 1, i));
    tabs.forEach(function (t, k) {
      var on = k === cur;
      t.setAttribute('aria-selected', on ? 'true' : 'false'); t.tabIndex = on ? 0 : -1;
      document.getElementById(t.getAttribute('aria-controls')).hidden = !on;
      if (dots[k]) dots[k].className = on ? 'on' : '';
    });
    prev.disabled = cur === 0;
    next.innerHTML = cur === tabs.length - 1 ? 'Close &#10005;' : 'Next &#8594;';
    if (focus) tabs[cur].focus();
    tabs[cur].scrollIntoView({ block: 'nearest', inline: 'nearest' });
    if (!focus) window.scrollTo(0, 0);
  }
  tabs.forEach(function (t, k) {
    t.addEventListener('click', function () { show(k); });
    t.addEventListener('keydown', function (e) {
      if (e.key === 'ArrowRight') { show(cur + 1, true); e.preventDefault(); }
      else if (e.key === 'ArrowLeft') { show(cur - 1, true); e.preventDefault(); }
    });
  });
  prev.addEventListener('click', function () { show(cur - 1); });
  next.addEventListener('click', function () {
    if (cur === tabs.length - 1) closeHelp();
    else show(cur + 1);
  });
  show(0);
})();