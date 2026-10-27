<script setup lang="ts">
// The one header of every page, in the Salesforce Lightning layout. First row: the logo, the global search (pages,
// and portfolios by number, developer or project), the EN / Arabic switch, Setup, the inbox bell and the signed-in
// user (with employee no. and department). Second row: the App Launcher (every page of every module), the app name
// and one tab per module; a module tab's arrow opens that module's pages.
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter, type RouteLocationRaw } from 'vue-router'
import capitraLogo from '@/assets/capitra-mark.svg'
import { getUser, type SessionUser } from '@/shared/auth/session'
import { http } from '@/shared/api/http'
import { logout as endSession, refreshCurrentUser } from '@/modules/sec/api/auth.api'
import { workflowService } from '@/modules/wfe/services/workflowService'
import { appMenu, type MenuLink } from './appMenu'
import { currentLang, switchLanguage } from './language'

type Module = { key: string; title: string; titleAr: string; home: string; prefixes: string[]; group?: string }

// The modules in the header, in the order of the business.
const modules: Module[] = [
  { key: 'mtg', title: 'Portfolio Acquisition', titleAr: 'استحواذ المحافظ', home: '/mortgage', prefixes: ['/mortgage/', '/mortgage'], group: 'Portfolio Acquisition' },
  { key: 'mff', title: 'Mortgage Finance', titleAr: 'التمويل العقاري', home: '/mortgage-finance', prefixes: ['/mortgage-finance'], group: 'Mortgage Finance' },
  { key: 'gl', title: 'Accounting', titleAr: 'المحاسبة', home: '/gl', prefixes: ['/gl'], group: 'Accounting' },
  { key: 'chq', title: 'Cheques', titleAr: 'الشيكات', home: '/cheque-wallet', prefixes: ['/cheque-wallet'], group: 'Cheques Wallet' },
  { key: 'wfe', title: 'Workflow', titleAr: 'سير العمل', home: '/workflow/inbox', prefixes: ['/workflowengine', '/workflow'] },
  { key: 'adm', title: 'Administration', titleAr: 'الإدارة', home: '/admin', prefixes: ['/admin'] }
]

const route = useRoute()
const router = useRouter()
const ar = currentLang() === 'AR'
const t = (en: string, arText: string) => (ar ? arText : en)

const user = ref<SessionUser | null>(getUser())
const displayName = computed(() => user.value?.fullName?.trim() || user.value?.userName?.trim() || 'CAPITRA User')
const initials = computed(() =>
  displayName.value.split(/\s+/).filter(Boolean).slice(0, 2).map((w) => w[0]!.toUpperCase()).join('')
)
const employeeNo = computed(() => user.value?.employeeNo?.trim() || '-')
const department = computed(() => user.value?.department?.trim() || '-')
const roles = computed(() => user.value?.roles ?? [])

// '/mortgage' must not claim '/mortgage-finance'.
function moduleOfPath(path: string) {
  return modules.find((m) =>
    m.prefixes.some((p) => (p.endsWith('/') ? path.startsWith(p) : path === p || path.startsWith(p + '/')))
  )
}
const current = computed(() => moduleOfPath(route.path))
// the pages a module tab's arrow opens
const workspace = appMenu.find((g) => g.workspace)?.links ?? []
function pagesOf(m: Module): MenuLink[] {
  if (m.group) return appMenu.find((g) => g.title === m.group)?.links ?? []
  const names = m.key === 'wfe' ? ['My Inbox', 'Workflow Engine'] : ['Administration', 'Users', 'Roles', 'Reference Data']
  return workspace.filter((l) => names.includes(l.label))
}
const dropFor = ref('')
function isHere(l: MenuLink) {
  try {
    return router.resolve(l.to).path === route.path
  } catch {
    return false
  }
}

// ---------- Menu panel ----------
const menuOpen = ref(false)
const filter = ref('')
const groups = computed(() => {
  const q = filter.value.trim().toLowerCase()
  if (!q) return appMenu
  return appMenu
    .map((g) => ({ ...g, links: g.title.toLowerCase().includes(q) ? g.links : g.links.filter((l) => l.label.toLowerCase().includes(q)) }))
    .filter((g) => g.links.length)
})

// ---------- search: pages, and on Portfolio Acquisition pages also portfolios ----------
type Hit = { label: string; sub: string; to: RouteLocationRaw }
const q = ref('')
const searchOpen = ref(false)
const portfolioHits = ref<Hit[]>([])
const pageHits = computed<Hit[]>(() => {
  const s = q.value.trim().toLowerCase()
  if (s.length < 2) return []
  const out: Hit[] = []
  for (const g of appMenu) for (const l of g.links) if (l.label.toLowerCase().includes(s)) out.push({ label: l.label, sub: g.title, to: l.to })
  return out.slice(0, 8)
})
let timer = 0
let seq = 0
watch(q, (text) => {
  window.clearTimeout(timer)
  portfolioHits.value = []
  if (text.trim().length < 2) return
  timer = window.setTimeout(async () => {
    const my = ++seq
    try {
      const d = await http.post<{ rows?: { no: string; developer: string; project: string; status: string; url: string }[] }>(
        '/api/apex/105/0/FNV_FIND_PORTFOLIO', { x01: text.trim() })
      if (my === seq)
        portfolioHits.value = (d.rows ?? []).slice(0, 6).map((r) => ({
          label: `${t('Portfolio', 'محفظة')} ${r.no} · ${r.status}`, sub: `${r.developer} · ${r.project}`, to: r.url
        }))
    } catch {
      /* the page results still show */
    }
  }, 250)
})
const hits = computed(() => [...portfolioHits.value, ...pageHits.value])

// ---------- inbox bell ----------
const inboxCount = ref(0)
async function loadInbox() {
  try {
    inboxCount.value = (await workflowService.getMyInbox()).length
  } catch {
    inboxCount.value = 0
  }
}

const userOpen = ref(false)
function closeAll() {
  dropFor.value = ''
  menuOpen.value = false
  userOpen.value = false
  searchOpen.value = false
}
function go(to: RouteLocationRaw) {
  closeAll()
  q.value = ''
  filter.value = ''
  void router.push(to)
}
async function logout() {
  endSession()
  closeAll()
  await router.push({ name: 'login' })
}
function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape') closeAll()
}
function onDocClick() {
  dropFor.value = ''
  userOpen.value = false
  searchOpen.value = false
}
watch(() => route.fullPath, closeAll)
onMounted(async () => {
  window.addEventListener('keydown', onKey)
  document.addEventListener('click', onDocClick)
  void loadInbox()
  try {
    user.value = await refreshCurrentUser()
  } catch {
    /* keep the user from the login token */
  }
})
onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKey)
  document.removeEventListener('click', onDocClick)
})
</script>

<template>
  <header class="shell" :dir="ar ? 'rtl' : 'ltr'">
    <div class="gh">
      <RouterLink :to="{ name: 'home' }" class="gh__logo" aria-label="CAPITRA Home"><img :src="capitraLogo" alt="CAPITRA" /></RouterLink>

      <div class="gh__search" @click.stop="searchOpen = true">
        <span class="gh__scope">{{ t('All', 'الكل') }}</span>
        <i class="pi pi-search"></i>
        <input v-model="q" type="search" :placeholder="t('Search portfolios, developers, pages and more...', 'ابحث في المحافظ والمطورين والصفحات...')"
               @focus="searchOpen = true" @keydown.enter="hits[0] && go(hits[0].to)" />
        <div v-if="searchOpen && q.trim().length > 1" class="gh__hits">
          <a v-for="h in hits" :key="String(h.label) + h.sub" @click.prevent="go(h.to)">
            <i class="pi" :class="String(h.to).includes('/mortgage/') ? 'pi-briefcase' : 'pi-file'"></i>
            <span><b>{{ h.label }}</b><small>{{ h.sub }}</small></span>
          </a>
          <div v-if="!hits.length" class="gh__none">{{ t('No results', 'لا توجد نتائج') }}</div>
        </div>
      </div>

      <div class="gh__tools">
        <div class="gh__lang" role="group" aria-label="Language">
          <button type="button" :class="{ on: !ar }" @click="ar && switchLanguage('EN')">EN</button>
          <button type="button" :class="{ on: ar }" lang="ar" @click="!ar && switchLanguage('AR')">عربي</button>
        </div>
        <RouterLink to="/admin" class="gh__icon" :title="t('Setup', 'الإعدادات')"><i class="pi pi-cog"></i></RouterLink>
        <RouterLink to="/workflow/inbox" class="gh__icon" :title="t('My inbox', 'صندوق المهام')">
          <i class="pi pi-bell"></i><b v-if="inboxCount">{{ inboxCount > 99 ? '99+' : inboxCount }}</b>
        </RouterLink>
        <div class="gh__user">
          <button type="button" @click.stop="userOpen = !userOpen; dropFor = ''">
            <span class="gh__av">{{ initials }}</span>
            <span class="gh__who"><b>{{ displayName }}</b><small>{{ t('Emp.', 'موظف') }} {{ employeeNo }} · {{ department }}</small></span>
          </button>
          <div v-if="userOpen" class="pop pop--user" @click.stop>
            <div class="pop__me"><span class="gh__av gh__av--lg">{{ initials }}</span><div><b>{{ displayName }}</b><small>{{ user?.userName }}</small></div></div>
            <dl>
              <dt>{{ t('Employee no.', 'رقم الموظف') }}</dt><dd>{{ employeeNo }}</dd>
              <dt>{{ t('Department', 'الإدارة') }}</dt><dd>{{ department }}</dd>
              <dt>{{ t('Roles', 'الأدوار') }}</dt><dd>{{ roles.join(', ') || '-' }}</dd>
            </dl>
            <a @click="go('/workflow/inbox')"><i class="pi pi-inbox"></i>{{ t('My inbox', 'صندوق المهام') }}</a>
            <a @click="go('/admin')"><i class="pi pi-cog"></i>{{ t('Setup', 'الإعدادات') }}</a>
            <a class="out" @click="logout"><i class="pi pi-sign-out"></i>{{ t('Log out', 'تسجيل الخروج') }}</a>
          </div>
        </div>
      </div>
    </div>

    <nav class="nb" aria-label="Modules">
      <button type="button" class="nb__waffle" :title="t('App Launcher', 'التطبيقات')" @click.stop="menuOpen = !menuOpen; dropFor = ''">
        <span class="nb__dots"><i v-for="n in 9" :key="n"></i></span>
        <span class="nb__app">Capitra Finance</span>
      </button>
      <RouterLink :to="{ name: 'home' }" class="nb__tab" :class="{ on: route.name === 'home' }">{{ t('Home', 'الرئيسية') }}</RouterLink>
      <div v-for="m in modules" :key="m.key" class="nb__tab" :class="{ on: current?.key === m.key, open: dropFor === m.key }">
        <RouterLink :to="m.home">{{ ar ? m.titleAr : m.title }}</RouterLink>
        <button type="button" class="nb__arrow" :aria-label="m.title" @click.stop="dropFor = dropFor === m.key ? '' : m.key; userOpen = false">
          <i class="pi pi-chevron-down"></i>
        </button>
        <div v-if="dropFor === m.key" class="pop pop--drop" @click.stop>
          <a v-for="l in pagesOf(m)" :key="l.label" :class="{ here: isHere(l) }" @click="go(l.to)">{{ l.label }}</a>
        </div>
      </div>
    </nav>

    <div v-if="menuOpen" class="launch__scrim" @click="menuOpen = false"></div>
    <div v-if="menuOpen" class="launch" @click.stop>
      <div class="launch__h">
        <b>{{ t('App Launcher', 'التطبيقات') }}</b>
        <label><i class="pi pi-search"></i><input v-model="filter" type="search" :placeholder="t('Search apps and pages...', 'ابحث في التطبيقات والصفحات...')" /></label>
        <button type="button" class="launch__x" aria-label="Close" @click="menuOpen = false"><i class="pi pi-times"></i></button>
      </div>
      <div class="launch__b">
        <section v-for="g in groups" :key="g.title">
          <h4><span class="tile" :class="'tile--' + (appMenu.indexOf(appMenu.find((x) => x.title === g.title)!) % 6)"><i :class="g.icon"></i></span>{{ g.title }}</h4>
          <a v-for="l in g.links" :key="l.label" @click="go(l.to)">{{ l.label }}</a>
        </section>
      </div>
    </div>
  </header>
</template>

<style scoped>
.shell { position: sticky; top: 0; z-index: 100; font-family: var(--ds-font); }
.gh { height: 50px; background: #fff; display: flex; align-items: center; gap: 16px; padding: 0 16px; border-bottom: 1px solid var(--ds-line); }
.gh__logo img { width: 34px; height: 34px; display: block; }
.gh__search { position: relative; flex: 0 1 600px; margin: 0 auto; height: 32px; display: flex; align-items: center; gap: 8px; padding: 0 10px; border: 1px solid #c9c9c9; border-radius: 4px; background: #fff; color: #747474; }
.gh__search:focus-within { border-color: var(--ds-brand); box-shadow: 0 0 3px var(--ds-brand); }
.gh__scope { padding-inline-end: 10px; border-inline-end: 1px solid #c9c9c9; color: var(--ds-ink); font-size: 13px; }
.gh__search input { flex: 1; border: 0; outline: 0; font: 13px var(--ds-font); color: var(--ds-ink); background: transparent; }
.gh__hits { position: absolute; top: 36px; inset-inline: 0; background: #fff; border: 1px solid var(--ds-line); border-radius: 4px; box-shadow: 0 2px 8px rgba(0,0,0,.16); padding: 4px 0; }
.gh__hits a { display: flex; gap: 10px; align-items: center; padding: 8px 12px; cursor: pointer; color: var(--ds-ink); }
.gh__hits a:hover { background: var(--ds-soft); }
.gh__hits a > i { width: 24px; height: 24px; border-radius: 4px; display: grid; place-items: center; background: #f2a33a; color: #fff; font-size: 12px; }
.gh__hits b { display: block; font-size: 13px; font-weight: 400; }
.gh__hits small { display: block; font-size: 12px; color: var(--ds-muted); }
.gh__none { padding: 10px 12px; color: var(--ds-muted); }
.gh__tools { display: flex; align-items: center; gap: 14px; }
.gh__lang { display: flex; border: 1px solid #c9c9c9; border-radius: 4px; overflow: hidden; }
.gh__lang button { border: 0; background: #fff; padding: 5px 10px; font: 12px var(--ds-font); color: var(--ds-ink); cursor: pointer; }
.gh__lang button.on { background: var(--ds-brand); color: #fff; }
.gh__icon { position: relative; color: #747474; font-size: 16px; text-decoration: none; padding: 4px; }
.gh__icon:hover { color: var(--ds-brand); }
.gh__icon b { position: absolute; top: -5px; inset-inline-end: -6px; background: #c23934; color: #fff; font-size: 10px; border-radius: 8px; padding: 0 4px; line-height: 15px; }
.gh__user { position: relative; }
.gh__user > button { display: flex; align-items: center; gap: 8px; border: 0; background: transparent; cursor: pointer; text-align: start; font-family: var(--ds-font); }
.gh__av { width: 32px; height: 32px; border-radius: 50%; background: var(--ds-brand); color: #fff; display: grid; place-items: center; font-size: 12px; font-weight: 700; flex: none; }
.gh__av--lg { width: 44px; height: 44px; font-size: 15px; }
.gh__who b { display: block; font-size: 13px; color: var(--ds-ink); font-weight: 600; }
.gh__who small { display: block; font-size: 12px; color: var(--ds-muted); }
.nb { height: 40px; background: #fff; display: flex; align-items: stretch; padding: 0 12px; box-shadow: 0 2px 3px rgba(0,0,0,.10); position: relative; }
.nb__waffle { display: flex; align-items: center; gap: 12px; border: 0; background: transparent; cursor: pointer; padding: 0 16px 0 4px; }
.nb__dots { display: grid; grid-template-columns: repeat(3, 4px); gap: 3px; }
.nb__dots i { width: 4px; height: 4px; border-radius: 50%; background: #706e6b; }
.nb__waffle:hover .nb__dots i { background: var(--ds-brand); }
.nb__app { font-size: 18px; color: var(--ds-ink); white-space: nowrap; font-family: var(--ds-font); }
.nb__tab { position: relative; display: flex; align-items: center; border-bottom: 3px solid transparent; color: var(--ds-ink); text-decoration: none; padding: 0 12px; font-size: 13px; white-space: nowrap; }
.nb > a.nb__tab:hover, div.nb__tab:hover { background: var(--ds-soft); }
.nb__tab a { color: inherit; text-decoration: none; height: 100%; display: flex; align-items: center; }
.nb__tab.on { border-bottom-color: var(--ds-brand); font-weight: 600; }
.nb__arrow { border: 0; background: transparent; cursor: pointer; color: #706e6b; font-size: 9px; padding: 4px 0 4px 6px; margin-inline-start: 2px; }
.nb__arrow:hover { color: var(--ds-brand); }
.pop { position: absolute; background: #fff; border: 1px solid var(--ds-line); border-radius: 4px; box-shadow: 0 2px 8px rgba(0,0,0,.16); z-index: 10; }
.pop a { display: flex; align-items: center; gap: 10px; padding: 8px 14px; cursor: pointer; color: var(--ds-ink); font-size: 13px; font-weight: 400; }
.pop a:hover { background: var(--ds-soft); }
.pop--drop { top: 40px; inset-inline-start: 0; min-width: 240px; padding: 4px 0; }
.pop--drop a.here { font-weight: 600; box-shadow: inset 3px 0 var(--ds-brand); }
.pop--user { top: 42px; inset-inline-end: 0; width: 290px; padding: 14px 0 6px; }
.pop__me { display: flex; gap: 12px; align-items: center; padding: 0 14px 12px; }
.pop__me b { display: block; font-size: 14px; }
.pop__me small { color: var(--ds-muted); font-size: 12px; }
.pop dl { display: grid; grid-template-columns: auto 1fr; gap: 6px 12px; margin: 0 0 6px; padding: 10px 14px; border-block: 1px solid var(--ds-line); font-size: 12.5px; }
.pop dt { color: var(--ds-muted); }
.pop dd { margin: 0; }
.pop a.out { color: #c23934; }
.launch__scrim { position: fixed; inset: 0; background: rgba(8, 7, 7, .45); z-index: 1; }
.launch { position: fixed; top: 60px; inset-inline: 5%; z-index: 2; background: #fff; border-radius: 4px; box-shadow: 0 4px 16px rgba(0,0,0,.3); max-height: calc(100vh - 100px); overflow: auto; }
.launch__h { display: flex; align-items: center; gap: 20px; padding: 14px 20px; border-bottom: 1px solid var(--ds-line); }
.launch__h b { font-size: 20px; font-weight: 400; color: var(--ds-ink); }
.launch__h label { flex: 0 1 420px; margin: 0 auto; height: 32px; display: flex; align-items: center; gap: 8px; border: 1px solid #c9c9c9; border-radius: 4px; padding: 0 10px; color: #747474; }
.launch__h input { flex: 1; border: 0; outline: 0; font: 13px var(--ds-font); }
.launch__x { border: 1px solid #c9c9c9; background: #fff; border-radius: 4px; width: 32px; height: 32px; cursor: pointer; color: #706e6b; }
.launch__b { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px 24px; padding: 20px; }
.launch__b h4 { display: flex; align-items: center; gap: 10px; margin: 0 0 8px; font-size: 14px; font-weight: 700; color: var(--ds-ink); }
.launch__b a { display: block; padding: 6px 0 6px 42px; color: var(--ds-link); font-size: 13px; cursor: pointer; }
.launch__b a:hover { text-decoration: underline; }
.tile { width: 32px; height: 32px; border-radius: 4px; display: grid; place-items: center; color: #fff; font-size: 15px; }
.tile--0 { background: #f2a33a; } .tile--1 { background: #5867e8; } .tile--2 { background: #0b5e5a; }
.tile--3 { background: #06a59a; } .tile--4 { background: #7f8de1; } .tile--5 { background: #706e6b; }
@media (max-width: 1350px) { .gh__who small { display: none; } .nb__tab { padding: 0 9px; } }
@media (max-width: 1150px) { .gh__who { display: none; } .nb__app { display: none; } }
</style>
