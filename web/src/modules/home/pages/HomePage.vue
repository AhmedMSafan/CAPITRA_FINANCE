<template>
  <!-- App 100 (FINOVA launcher) page 1 "Home", region "Applications": the same markup and CSS (the page's inline CSS),
       with each card opening this app's module instead of an APEX application. -->
  <div class="fm">
    <section class="fm-hero">
      <div class="fm-hero-l">
        <span class="fm-eyebrow"><i></i>CAPITRA &middot; Digital Finance Platform</span>
        <h1><span>{{ greeting }}</span>, <em>{{ displayName }}</em></h1>
        <p>Choose an application to continue. You are signed in once for all CAPITRA applications.</p>
        <div class="fm-cta">
          <router-link class="fm-btn fm-btn--inbox" to="/workflow/inbox">
            <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M22 12h-6l-2 3h-4l-2-3H2" /><path d="M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z" /></svg>
            <span>Open my inbox</span><em>{{ tasks === null ? 0 : tasks }}</em>
          </router-link>
        </div>
      </div>
      <div class="fm-kpis">
        <div><b>{{ live }}</b><span>Live applications</span></div>
        <router-link class="fm-kpi-link" to="/workflow/inbox"><b>{{ tasks === null ? '–' : tasks }}</b><span>Workflow tasks for you</span></router-link>
        <div><b>{{ today }}</b><span>Today</span></div>
      </div>
    </section>

    <div class="fm-sec"><h2>Business applications</h2></div>
    <div class="fm-grid fm-grid--1">
      <component :is="app.to ? 'router-link' : 'div'" v-for="app in tier(1)" :key="app.name" :to="app.to" class="fm-card fm-card--t1"
                 :class="{ 'is-off': !app.to }" :aria-disabled="app.to ? undefined : 'true'" :style="{ '--c': app.color, '--cl': app.tint }">
        <div class="fm-top"><span class="fm-ico" v-html="icon(app.icon)"></span><span class="fm-pill" :class="app.to ? 'fm-pill--live' : 'fm-pill--soon'">{{ app.to ? 'Live' : 'Coming soon' }}</span></div>
        <div class="fm-body">
          <small>{{ app.short }}</small><h3>{{ app.name }}</h3><p>{{ app.descr }}</p>
          <ul v-if="app.feats.length" class="fm-feats"><li v-for="f in app.feats" :key="f">{{ f }}</li></ul>
        </div>
        <div class="fm-foot">
          <span v-if="app.to" class="fm-go">Open {{ app.short }}<span v-html="icon(ARROW, 16)"></span></span>
          <span v-else class="fm-muted">In preparation &middot; available soon</span>
        </div>
      </component>
    </div>

    <div class="fm-sec"><h2>Shared services</h2></div>
    <div class="fm-grid fm-grid--2">
      <router-link v-for="app in tier(2)" :key="app.name" :to="app.to!" class="fm-card fm-card--t2" :style="{ '--c': app.color, '--cl': app.tint }">
        <div class="fm-top"><span class="fm-ico" v-html="icon(app.icon)"></span><span class="fm-pill fm-pill--live">Live</span></div>
        <div class="fm-body"><small>{{ app.short }}</small><h3>{{ app.name }}</h3><p>{{ app.descr }}</p></div>
        <div v-if="app.inbox && tasks !== null" class="fm-note"><b>{{ tasks }}</b> task{{ tasks === 1 ? '' : 's' }} waiting in your inbox</div>
        <div class="fm-foot"><span class="fm-go">Open {{ app.short }}<span v-html="icon(ARROW, 16)"></span></span></div>
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { getUser } from '@/shared/auth/session'
import { workflowService } from '@/modules/wfe/services/workflowService'

type App = { tier: 1 | 2; name: string; short: string; descr: string; to: string | null; color: string; tint: string; icon: string; feats: string[]; inbox?: boolean }

const user = getUser()
const displayName = user?.fullName?.trim() || user?.userName?.trim() || 'CAPITRA User'
const hour = new Date().getHours()
const greeting = hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening'
const today = new Date().toLocaleDateString(undefined, { day: 'numeric', month: 'short' })

const ARROW = '<path d="M5 12h14M13 5l7 7-7 7"/>'
// the card icons of the APEX page (its svg() function)
const icon = (paths: string, size = 24) =>
  `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${paths}</svg>`

// the APEX page's card list (reg calls), each pointed at this app's module; Personal Mortgage is not built here yet
const apps: App[] = [
  { tier: 1, name: 'Portfolio Acquisition', short: 'Portfolio', descr: 'Evaluate, screen and acquire real-estate financing portfolios through one controlled, fully audited workflow.',
    to: '/mortgage', color: '#E87722', tint: '#FFF1E6', icon: '<path d="M3 21h18"/><path d="M5 21V7l7-4 7 4v14"/><path d="M9 21v-6h6v6"/><path d="M9 10h.01M15 10h.01"/>',
    feats: ['Due diligence & AML screening', 'Committee approval', 'Acquisition tracking'] },
  { tier: 1, name: 'Subsidized Mortgage Finance', short: 'Subsidized', descr: 'Government-subsidised home finance: eligibility, subsidy calculation and disbursement under programme rules.',
    to: '/mortgage-finance', color: '#14734F', tint: '#E7F6EF',
    icon: '<path d="M3 11l9-7 9 7"/><path d="M5 10v10h14V10"/><path d="M10 15.5c0-1 .9-1.5 2-1.5s2 .5 2 1.5-1 1.3-2 1.5-2 .5-2 1.5.9 1.5 2 1.5 2-.5 2-1.5"/><path d="M12 13v1M12 19v1"/>',
    feats: ['Eligibility checks', 'Subsidy calculation', 'Programme reporting'] },
  { tier: 1, name: 'Personal Mortgage', short: 'Mortgage', descr: 'Standard mortgage finance: calculation, offer, origination, credit, booking, servicing and collections.',
    to: null, color: '#5B5BD6', tint: '#EEEEFC', icon: '<path d="M3 11l9-7 9 7"/><path d="M5 10v10h14V10"/><path d="M9 17l2-2 2 1.5 3-3.5"/>',
    feats: ['Calculator & customer offers', 'Origination, credit & booking', 'Servicing & collections'] },
  { tier: 2, name: 'General Ledger', short: 'GL', descr: 'Segment-based chart of accounts, balanced journals and controlled period close.', to: '/gl', color: '#0E7490', tint: '#E0F2F7',
    icon: '<path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5z"/><path d="M4 19.5A2.5 2.5 0 0 0 6.5 22H20v-5"/><path d="M9 7h7M9 11h5"/>', feats: [] },
  { tier: 2, name: 'Workflow Engine', short: 'Workflow', descr: 'Design approval workflows and follow every task, SLA and decision.', to: '/workflowengine', color: '#00636D', tint: '#E6F3F4',
    icon: '<rect x="3" y="3" width="7" height="6" rx="1.5"/><rect x="14" y="15" width="7" height="6" rx="1.5"/><path d="M6.5 9v3a3 3 0 0 0 3 3h4.5"/><circle cx="18" cy="6" r="3"/>', feats: [], inbox: true },
  { tier: 2, name: 'Cheques Wallet', short: 'Wallet', descr: 'Client cheque batches from upload to custody, deposit and collection.', to: '/cheque-wallet', color: '#B4570E', tint: '#FFF1E6',
    icon: '<path d="M20 7H5a2 2 0 0 1 0-4h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15V7H5"/><path d="M16 14h.01"/>', feats: [] },
  { tier: 2, name: 'Admin Setup', short: 'Admin', descr: 'Users, roles, modules and platform-wide settings in one place.', to: '/admin', color: '#475569', tint: '#EEF2F6',
    icon: '<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.8-.3 1.7 1.7 0 0 0-1 1.5V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-1.1-1.5 1.7 1.7 0 0 0-1.8.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.8 1.7 1.7 0 0 0-1.5-1H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.5-1.1 1.7 1.7 0 0 0-.3-1.8l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.8.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.8-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.8V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1z"/>', feats: [] }
]
const tier = (n: 1 | 2) => apps.filter((a) => a.tier === n)
const live = computed(() => apps.filter((a) => a.to).length)

const tasks = ref<number | null>(null)
onMounted(async () => {
  try {
    tasks.value = (await workflowService.getMyInbox()).length
  } catch {
    tasks.value = null
  }
})
</script>

<style>
/* App 100 page 1 inline CSS */
:root{--fm-ink:#0B2533;--fm-ink2:#29495A;--fm-muted:#5F7B8A;--fm-line:#DFE8EC;--fm-soft:#F4F8F9;--fm-teal:#00636D;--fm-teal-d:#014B55;--fm-orange:#E87722;--fm-green:#14734F}
.fm{padding:4px 0 24px;font-family:Inter,'Segoe UI',Arial,sans-serif;color:var(--fm-ink);-webkit-font-smoothing:antialiased}
.fm *,.fm *::before,.fm *::after{box-sizing:border-box}
.fm a{text-decoration:none}
/* hero */
.fm-hero{position:relative;overflow:hidden;display:grid;grid-template-columns:minmax(0,1.4fr) minmax(0,1fr);gap:28px;align-items:center;border-radius:22px;color:#fff;padding:32px 34px;
  background:radial-gradient(120% 140% at 0% 0%,#07505A 0%,#033239 55%,#022429 100%);box-shadow:0 24px 50px -24px rgba(1,50,57,.55)}
.fm-hero::before{content:"";position:absolute;inset:0;background-image:radial-gradient(rgba(127,224,201,.16) 1.1px,transparent 1.2px);background-size:22px 22px;
  -webkit-mask-image:linear-gradient(90deg,transparent 0%,#000 60%);mask-image:linear-gradient(90deg,transparent 0%,#000 60%)}
.fm-hero::after{content:"";position:absolute;width:440px;height:440px;border-radius:50%;right:-120px;top:-230px;background:#E87722;opacity:.22;filter:blur(70px)}
.fm-hero>*{position:relative;z-index:1}
.fm-eyebrow{display:inline-flex;align-items:center;gap:8px;padding:5px 12px 5px 9px;border-radius:99px;background:rgba(255,255,255,.08);border:1px solid rgba(255,255,255,.15);
  font-size:11.5px;font-weight:600;letter-spacing:.12em;text-transform:uppercase;color:rgba(255,255,255,.85)}
.fm-eyebrow i{width:7px;height:7px;border-radius:50%;background:#2BD4A8;box-shadow:0 0 10px #2BD4A8}
.fm-hero h1{margin:14px 0 6px;font-size:clamp(26px,2.6vw,36px);line-height:1.15;font-weight:800;letter-spacing:-.02em;color:#fff}
.fm-hero h1 em{font-style:normal;color:#FFB25E}
.fm-hero p{margin:0;max-width:620px;font-size:14.5px;line-height:1.6;color:rgba(255,255,255,.75)}
.fm-kpis{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:10px}
.fm-kpis div{padding:14px 16px;border-radius:14px;background:rgba(255,255,255,.07);border:1px solid rgba(255,255,255,.12);-webkit-backdrop-filter:blur(8px);backdrop-filter:blur(8px)}
.fm-kpis b{display:block;font-size:24px;font-weight:800;letter-spacing:-.02em;line-height:1.15;color:#fff;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.fm-kpis span{display:block;margin-top:3px;font-size:12px;color:rgba(255,255,255,.66)}
/* sections */
.fm-sec{display:flex;align-items:center;gap:12px;margin:32px 2px 14px}
.fm-sec h2{margin:0;font-size:12px;font-weight:700;letter-spacing:.12em;text-transform:uppercase;color:var(--fm-muted)}
.fm-sec::after{content:"";flex:1;height:1px;background:var(--fm-line)}
.fm-grid{display:grid;gap:16px}
.fm-grid--1{grid-template-columns:repeat(3,minmax(0,1fr))}
.fm-grid--2{grid-template-columns:repeat(4,minmax(0,1fr))}
/* cards */
.fm-card{position:relative;display:flex;flex-direction:column;gap:16px;padding:22px;border-radius:18px;background:#fff;border:1px solid var(--fm-line);color:var(--fm-ink)!important;overflow:hidden;
  box-shadow:0 1px 2px rgba(11,37,51,.04);transition:transform .18s,box-shadow .18s,border-color .18s}
.fm-card::before{content:"";position:absolute;inset:0 0 auto 0;height:4px;background:var(--c)}
a.fm-card:hover{transform:translateY(-4px);border-color:color-mix(in srgb,var(--c) 45%,#DFE8EC);box-shadow:0 22px 40px -22px rgba(11,37,51,.35)}
a.fm-card:focus-visible{outline:3px solid rgba(232,119,34,.45);outline-offset:3px}
.fm-card--t1{min-height:300px;padding:26px}
.fm-card--t1::after{content:"";position:absolute;width:220px;height:220px;border-radius:50%;right:-80px;bottom:-90px;background:var(--cl);z-index:0}
.fm-card>*{position:relative;z-index:1}
.fm-top{display:flex;align-items:center;justify-content:space-between;gap:10px}
.fm-ico{width:48px;height:48px;border-radius:14px;display:grid;place-items:center;color:var(--c);background:var(--cl)}
.fm-card--t1 .fm-ico{width:56px;height:56px;border-radius:16px;color:#fff;background:var(--c);box-shadow:0 12px 24px -12px var(--c)}
.fm-card--t1 .fm-ico svg{width:28px;height:28px}
.fm-body small{display:block;font-size:11.5px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--c)}
.fm-body h3{margin:4px 0 0;font-size:17px;font-weight:700;letter-spacing:-.01em;line-height:1.3;color:var(--fm-ink)}
.fm-card--t1 .fm-body h3{font-size:21px}
.fm-body p{margin:6px 0 0;font-size:13.5px;line-height:1.55;color:var(--fm-muted)}
.fm-feats{list-style:none;margin:14px 0 0;padding:0;display:flex;flex-direction:column;gap:7px}
.fm-feats li{position:relative;padding-inline-start:22px;font-size:13px;color:var(--fm-ink2)}
.fm-feats li::before{content:"";position:absolute;inset-inline-start:2px;top:4px;width:11px;height:6px;border-inline-start:2px solid var(--c);border-bottom:2px solid var(--c);transform:rotate(-45deg)}
html[dir=rtl] .fm-feats li::before{transform:rotate(45deg)}
.fm-note{display:inline-flex;align-items:center;gap:6px;align-self:flex-start;padding:6px 10px;border-radius:9px;background:var(--cl);font-size:12.5px;color:var(--fm-ink2)}
.fm-note b{color:var(--c);font-size:14px}
.fm-foot{margin-top:auto;padding-top:14px;border-top:1px dashed var(--fm-line);display:flex;align-items:center;justify-content:space-between;gap:10px}
.fm-go{display:inline-flex;align-items:center;gap:8px;font-size:13.5px;font-weight:700;color:var(--c)}
.fm-go svg{transition:transform .18s}
a.fm-card:hover .fm-go svg{transform:translateX(3px)}
html[dir=rtl] .fm-go svg{transform:scaleX(-1)}
.fm-muted{font-size:12.5px;color:var(--fm-muted)}
.fm-pill{display:inline-flex;align-items:center;gap:6px;height:24px;padding:0 10px;border-radius:99px;font-size:11.5px;font-weight:700;white-space:nowrap}
.fm-pill::before{content:"";width:7px;height:7px;border-radius:50%;background:currentColor}
.fm-pill--live{background:#E7F6EF;color:#14734F}
.fm-pill--live::before{box-shadow:0 0 0 0 rgba(20,115,79,.5);animation:fmPulse 2s infinite}
.fm-pill--soon{background:#FFF4E5;color:#9A4F08}
.fm-pill--lock{background:#EEF2F4;color:#56707B}
@keyframes fmPulse{0%{box-shadow:0 0 0 0 rgba(20,115,79,.45)}70%{box-shadow:0 0 0 7px rgba(20,115,79,0)}100%{box-shadow:0 0 0 0 rgba(20,115,79,0)}}
.fm-card.is-off{background:#FBFCFD}
.fm-card.is-off .fm-ico,.fm-card.is-off .fm-body,.fm-card.is-off::before{filter:grayscale(.55);opacity:.8}
.fm-card.is-off.fm-card--t1 .fm-ico{opacity:.75}
.fm-empty{margin-top:18px;padding:18px;border-radius:14px;border:1px dashed var(--fm-line);background:#fff;color:var(--fm-muted);font-size:13.5px}
.fm-kpis .fm-kpi-link{display:block;padding:14px 16px;border-radius:14px;background:rgba(255,255,255,.07);border:1px solid rgba(255,255,255,.12);-webkit-backdrop-filter:blur(8px);backdrop-filter:blur(8px);color:inherit!important;text-decoration:none!important;transition:background .15s,border-color .15s}.fm-kpis .fm-kpi-link:hover{background:rgba(232,119,34,.22);border-color:#E87722}.fm-cta{display:flex;flex-wrap:wrap;gap:10px;margin-top:18px}.fm-btn{display:inline-flex;align-items:center;gap:9px;height:40px;padding:0 16px;border-radius:11px;font-weight:600;font-size:14px;text-decoration:none!important;transition:background .15s,border-color .15s}.fm-btn--inbox{position:relative;background:#E87722;border:1px solid #E87722;color:#fff!important;box-shadow:0 6px 18px -8px rgba(232,119,34,.9)}.fm-btn--inbox:hover{background:#F08A3A;border-color:#F08A3A}.fm-btn em{position:absolute;top:-8px;right:-8px;font-style:normal;min-width:22px;height:22px;padding:0 6px;box-sizing:border-box;border-radius:11px;background:#DC2626;border:2px solid #fff;color:#fff;display:inline-flex;align-items:center;justify-content:center;font-size:11.5px;font-weight:700}
@media(max-width:1200px){.fm-grid--2{grid-template-columns:repeat(2,minmax(0,1fr))}}
@media(max-width:980px){.fm-hero{grid-template-columns:1fr}.fm-grid--1{grid-template-columns:1fr}.fm-card--t1{min-height:0}}
@media(max-width:640px){.fm-hero{padding:22px 18px}.fm-kpis{grid-template-columns:repeat(2,minmax(0,1fr))}.fm-grid--2{grid-template-columns:1fr}.fm{padding:0}}
@media(prefers-reduced-motion:reduce){.fm *{transition:none!important;animation:none!important}}
</style>
