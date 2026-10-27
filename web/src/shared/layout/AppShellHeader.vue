<script setup lang="ts">
// The one header of every page, as the APEX applications drew it (App 105, finova-portfolio-bilingual.js): a dark teal
// band. First row: the CAPITRA logo, the module and the page title; Home, today's date and the signed-in user with the
// employee no. and department. Second row: the module name, the Menu (every page of every module), the search (pages,
// and portfolios by number, developer or project), the EN / Arabic switch and the inbox bell.
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter, type RouteLocationRaw } from 'vue-router'
import capitraLogo from '@/assets/capitra-mark.svg'
import { getUser, type SessionUser } from '@/shared/auth/session'
import { http } from '@/shared/api/http'
import { logout as endSession, refreshCurrentUser } from '@/modules/sec/api/auth.api'
import { workflowService } from '@/modules/wfe/services/workflowService'
import { appMenu, type MenuLink } from './appMenu'
import { currentLang, switchLanguage } from './language'

type Module = { key: string; title: string; titleAr: string; home: string; prefixes: string[]; group?: string; sub: string; subAr: string }

// The modules in the header, in the order of the business.
const modules: Module[] = [
  { key: 'mtg', title: 'Portfolio Acquisition', titleAr: 'استحواذ المحافظ', home: '/mortgage', prefixes: ['/mortgage/', '/mortgage'], group: 'Portfolio Acquisition', sub: 'Mortgage finance platform', subAr: 'منصة التمويل العقاري' },
  { key: 'mff', title: 'Mortgage Finance', titleAr: 'التمويل العقاري', home: '/mortgage-finance', prefixes: ['/mortgage-finance'], group: 'Mortgage Finance', sub: 'Fund-supported mortgage finance', subAr: 'التمويل العقاري المدعوم' },
  { key: 'gl', title: 'Accounting', titleAr: 'المحاسبة', home: '/gl', prefixes: ['/gl'], group: 'Accounting', sub: 'General ledger', subAr: 'دفتر الأستاذ العام' },
  { key: 'chq', title: 'Cheques', titleAr: 'الشيكات', home: '/cheque-wallet', prefixes: ['/cheque-wallet'], group: 'Cheques Wallet', sub: 'Client cheques wallet', subAr: 'محفظة شيكات العملاء' },
  { key: 'wfe', title: 'Workflow', titleAr: 'سير العمل', home: '/workflow/inbox', prefixes: ['/workflowengine', '/workflow'], sub: 'Approvals and tasks', subAr: 'الموافقات والمهام' },
  { key: 'adm', title: 'Administration', titleAr: 'الإدارة', home: '/admin', prefixes: ['/admin'], sub: 'Users, roles and setup', subAr: 'المستخدمون والأدوار والإعدادات' }
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
// the title of the open page: the menu entry that opens it, else the route's function name
const pageTitle = computed(() => {
  for (const g of appMenu) for (const l of g.links) if (isHere(l)) return l.label
  const fn = route.meta?.functionName
  return typeof fn === 'string' ? fn : ''
})
const today = new Intl.DateTimeFormat(ar ? 'ar-EG' : 'en-GB', { timeZone: 'Africa/Cairo', day: 'numeric', month: 'short', year: 'numeric' }).format(new Date())
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
    <div class="h1">
      <RouterLink :to="{ name: 'home' }" class="h1__brand" aria-label="CAPITRA Home">
        <span class="h1__logo"><img :src="capitraLogo" alt="" /></span>
        <b class="h1__word">CAPITRA</b>
      </RouterLink>
      <span class="h1__div"></span>
      <span class="h1__mod">{{ current ? (ar ? current.titleAr : current.title) : t('Finance', 'التمويل') }}</span>
      <span v-if="pageTitle" class="h1__page">{{ pageTitle }}</span>

      <div class="h1__tools">
        <RouterLink :to="{ name: 'home' }" class="h1__home" :title="t('Home', 'الرئيسية')"><i class="pi pi-home"></i></RouterLink>
        <span class="pill">{{ today }}</span>
        <div class="h1__user">
          <button type="button" class="pill pill--user" @click.stop="userOpen = !userOpen">
            <span class="av">{{ initials }}</span><b>{{ displayName }}</b><i class="pi pi-chevron-down"></i>
          </button>
          <div v-if="userOpen" class="pop pop--user" @click.stop>
            <div class="pop__me"><span class="av av--lg">{{ initials }}</span><div><b>{{ displayName }}</b><small>{{ user?.userName }}</small></div></div>
            <dl>
              <dt>{{ t('Employee no.', 'رقم الموظف') }}</dt><dd>{{ employeeNo }}</dd>
              <dt>{{ t('Department', 'الإدارة') }}</dt><dd>{{ department }}</dd>
              <dt>{{ t('Roles', 'الأدوار') }}</dt><dd>{{ roles.join(', ') || '-' }}</dd>
            </dl>
            <a @click="go('/workflow/inbox')"><i class="pi pi-inbox"></i>{{ t('My inbox', 'صندوق المهام') }}</a>
            <a @click="go('/admin')"><i class="pi pi-cog"></i>{{ t('Administration', 'الإدارة') }}</a>
            <a class="out" @click="logout"><i class="pi pi-sign-out"></i>{{ t('Log out', 'تسجيل الخروج') }}</a>
          </div>
        </div>
        <span class="pill pill--sm" :title="t('Employee no.', 'رقم الموظف')">{{ t('Employee', 'الموظف') }} <b>{{ employeeNo }}</b></span>
        <span class="pill pill--sm" :title="t('Department', 'الإدارة')">{{ t('Department', 'الإدارة') }} <b>{{ department }}</b></span>
      </div>
    </div>

    <div class="h2">
      <RouterLink :to="current?.home ?? { name: 'home' }" class="h2__app">
        <span class="h2__logo"><img :src="capitraLogo" alt="" /></span>
        <span><b>{{ current ? (ar ? current.titleAr : current.title) : 'Capitra Finance' }}</b><small>{{ current ? (ar ? current.subAr : current.sub) : t('Digital finance platform', 'منصة التمويل الرقمية') }}</small></span>
      </RouterLink>
      <button type="button" class="h2__menu" :class="{ on: menuOpen }" aria-haspopup="true" :aria-expanded="menuOpen" @click.stop="menuOpen = !menuOpen; userOpen = false">
        <i :class="menuOpen ? 'pi pi-times' : 'pi pi-th-large'"></i><span>{{ t('Menu', 'القائمة') }}</span>
      </button>

      <div class="h2__search" role="search" @click.stop="searchOpen = true">
        <i class="pi pi-search"></i>
        <input v-model="q" type="search" :placeholder="t('Find a page, portfolio, developer or project', 'ابحث عن صفحة أو محفظة أو مطور أو مشروع')"
               @focus="searchOpen = true" @keydown.enter="hits[0] && go(hits[0].to)" />
        <div v-if="searchOpen && q.trim().length > 1" class="hits">
          <a v-for="h in hits" :key="String(h.label) + h.sub" @click.prevent="go(h.to)">
            <i class="pi" :class="String(h.to).includes('/mortgage/') ? 'pi-briefcase' : 'pi-file'"></i>
            <span><b>{{ h.label }}</b><small>{{ h.sub }}</small></span>
          </a>
          <div v-if="!hits.length" class="hits__none">{{ t('No results', 'لا توجد نتائج') }}</div>
        </div>
      </div>

      <div class="h2__tools">
        <div class="lang" role="group" aria-label="Language">
          <button type="button" :class="{ on: !ar }" @click="ar && switchLanguage('EN')">EN</button>
          <button type="button" :class="{ on: ar }" lang="ar" @click="!ar && switchLanguage('AR')">ع</button>
        </div>
        <RouterLink to="/workflow/inbox" class="bell" :title="t('My inbox', 'صندوق المهام')">
          <i class="pi pi-bell"></i><b v-if="inboxCount">{{ inboxCount > 99 ? '99+' : inboxCount }}</b>
        </RouterLink>
      </div>
    </div>

    <div v-if="menuOpen" class="mega" @click.stop>
      <div class="mega__h">
        <b>{{ t('All pages', 'كل الصفحات') }}</b>
        <label><i class="pi pi-search"></i><input v-model="filter" type="search" :placeholder="t('Filter pages', 'ابحث في القائمة')" /></label>
      </div>
      <div class="mega__b">
        <section v-for="g in groups" :key="g.title" :class="{ ws: g.workspace }">
          <h4><span class="mega__ic"><i :class="g.icon"></i></span>{{ g.title }}</h4>
          <a v-for="l in g.links" :key="l.label" :class="{ here: isHere(l) }" @click="go(l.to)"><i v-if="g.workspace && l.icon" :class="l.icon"></i>{{ l.label }}</a>
        </section>
      </div>
    </div>
    <div v-if="menuOpen" class="mega__scrim" @click="menuOpen = false"></div>
  </header>
</template>


<style scoped>
.shell { position: sticky; top: 0; z-index: 100; font-family: Inter, 'Segoe UI', Arial, sans-serif; color: #fff; }
.shell a { text-decoration: none; }
.h1 {
  height: 64px; display: flex; align-items: center; gap: 14px; padding: 0 24px;
  background: radial-gradient(120% 220% at 0% 0%, #0B4A52 0%, #063840 55%, #042C33 100%);
}
.h1__brand { display: flex; align-items: center; gap: 12px; color: #fff; }
.h1__logo { width: 44px; height: 44px; border-radius: 12px; display: grid; place-items: center; background: rgba(255, 255, 255, .08); border: 1px solid rgba(255, 255, 255, .14); }
.h1__logo img { width: 30px; height: 30px; display: block; }
.h1__word { font-size: 19px; font-weight: 800; letter-spacing: .12em; }
.h1__div { width: 1px; height: 22px; background: rgba(255, 255, 255, .25); }
.h1__mod { font-size: 12.5px; font-weight: 800; letter-spacing: .1em; text-transform: uppercase; white-space: nowrap; }
.h1__page { font-size: 12.5px; font-weight: 600; color: rgba(255, 255, 255, .78); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; min-width: 0; }
.h1__tools { margin-inline-start: auto; display: flex; align-items: center; gap: 10px; }
.h1__home {
  width: 40px; height: 40px; border-radius: 11px; display: grid; place-items: center; color: #fff; font-size: 16px;
  background: rgba(255, 255, 255, .1); border: 1px solid rgba(255, 255, 255, .2); transition: background .15s;
}
.h1__home:hover { background: rgba(255, 255, 255, .18); }
.pill {
  display: inline-flex; align-items: center; gap: 8px; height: 38px; padding: 0 14px; border-radius: 99px; white-space: nowrap;
  background: rgba(255, 255, 255, .06); border: 1px solid rgba(255, 255, 255, .2); color: #fff; font: 600 12.5px Inter, Arial, sans-serif;
}
.pill--sm { font-size: 10.5px; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: rgba(255, 255, 255, .8); }
.pill--sm b { font-weight: 700; color: #fff; letter-spacing: 0; text-transform: none; font-size: 12px; }
.pill--user { cursor: pointer; padding-inline-start: 5px; }
.pill--user .pi { font-size: 10px; opacity: .8; }
.pill--user:hover { background: rgba(255, 255, 255, .12); }
.av { width: 28px; height: 28px; border-radius: 50%; display: grid; place-items: center; background: #0E7C7B; color: #fff; font-size: 11px; font-weight: 700; flex: none; }
.av--lg { width: 44px; height: 44px; font-size: 15px; }
.h1__user { position: relative; }
.h2 {
  height: 60px; display: flex; align-items: center; gap: 16px; padding: 0 24px;
  background: linear-gradient(90deg, #0A4148 0%, #0B4A51 60%, #0A4148 100%); border-bottom: 2px solid #E87722;
  box-shadow: 0 10px 24px -16px rgba(1, 40, 45, .8);
}
.h2__app { display: flex; align-items: center; gap: 12px; color: #fff; min-width: 0; }
.h2__logo { width: 32px; height: 32px; border-radius: 9px; display: grid; place-items: center; background: rgba(255, 255, 255, .08); border: 1px solid rgba(255, 255, 255, .14); flex: none; }
.h2__logo img { width: 22px; height: 22px; }
.h2__app b { display: block; font-size: 15px; font-weight: 700; white-space: nowrap; }
.h2__app small { display: block; font-size: 12px; color: rgba(255, 255, 255, .7); white-space: nowrap; }
.h2__menu {
  display: inline-flex; align-items: center; gap: 9px; height: 40px; padding: 0 16px; border-radius: 11px; cursor: pointer;
  background: rgba(255, 255, 255, .08); border: 1px solid rgba(255, 255, 255, .22); color: #fff; font: 700 14px Inter, Arial, sans-serif; transition: background .15s;
}
.h2__menu:hover, .h2__menu.on { background: rgba(255, 255, 255, .16); }
.h2__search {
  position: relative; flex: 0 1 520px; margin: 0 auto; height: 40px; display: flex; align-items: center; gap: 10px; padding: 0 14px; border-radius: 11px;
  background: rgba(255, 255, 255, .08); border: 1px solid rgba(255, 255, 255, .22); color: rgba(255, 255, 255, .75);
}
.h2__search:focus-within { background: rgba(255, 255, 255, .14); border-color: rgba(255, 255, 255, .45); }
.h2__search input { flex: 1; min-width: 0; border: 0; outline: 0; background: transparent; color: #fff; font: 500 14px Inter, Arial, sans-serif; }
.h2__search input::placeholder { color: rgba(255, 255, 255, .7); }
.hits {
  position: absolute; top: 46px; inset-inline: 0; background: #fff; border: 1px solid #DFE8EC; border-radius: 14px; padding: 6px;
  box-shadow: 0 18px 40px -16px rgba(11, 37, 51, .45); color: #0B2533;
}
.hits a { display: flex; gap: 10px; align-items: center; padding: 8px 10px; border-radius: 10px; cursor: pointer; color: #0B2533; }
.hits a:hover { background: #F4F8F9; }
.hits a > i { width: 28px; height: 28px; border-radius: 8px; display: grid; place-items: center; background: #E6F3F4; color: #00636D; font-size: 12px; }
.hits b { display: block; font-size: 13px; font-weight: 600; }
.hits small { display: block; font-size: 12px; color: #5F7B8A; }
.hits__none { padding: 10px 12px; color: #5F7B8A; font-size: 13px; }
.h2__tools { display: flex; align-items: center; gap: 14px; }
.lang { display: flex; gap: 2px; padding: 3px; border-radius: 11px; background: rgba(255, 255, 255, .08); border: 1px solid rgba(255, 255, 255, .22); }
.lang button { min-width: 38px; height: 30px; border: 0; border-radius: 8px; background: transparent; color: #fff; font: 700 13px Inter, Arial, sans-serif; cursor: pointer; }
.lang button.on { background: #fff; color: #0A4148; }
.bell { position: relative; color: #fff; font-size: 18px; padding: 6px; }
.bell b { position: absolute; top: -2px; inset-inline-end: -4px; min-width: 17px; height: 17px; padding: 0 4px; border-radius: 9px; display: grid; place-items: center; background: #E87722; color: #fff; font-size: 10px; font-weight: 700; }
.pop { position: absolute; background: #fff; border: 1px solid #DFE8EC; border-radius: 14px; box-shadow: 0 18px 40px -16px rgba(11, 37, 51, .45); z-index: 10; color: #0B2533; }
.pop a { display: flex; align-items: center; gap: 10px; padding: 9px 16px; cursor: pointer; color: #0B2533; font-size: 13px; }
.pop a:hover { background: #F4F8F9; }
.pop--user { top: 46px; inset-inline-end: 0; width: 300px; padding: 14px 0 6px; }
.pop__me { display: flex; gap: 12px; align-items: center; padding: 0 16px 12px; }
.pop__me b { display: block; font-size: 14px; }
.pop__me small { color: #5F7B8A; font-size: 12px; }
.pop dl { display: grid; grid-template-columns: auto 1fr; gap: 6px 12px; margin: 0 0 6px; padding: 10px 16px; border-block: 1px solid #DFE8EC; font-size: 12.5px; }
.pop dt { color: #5F7B8A; }
.pop dd { margin: 0; }
.pop a.out { color: #B42318; }
.mega {
  position: absolute; top: 132px; inset-inline: 24px; z-index: 2; background: #fff; border-radius: 18px; color: #0B2533;
  box-shadow: 0 30px 60px -20px rgba(1, 40, 45, .55); max-height: calc(100vh - 160px); overflow: auto;
}
.mega__scrim { position: fixed; inset: 132px 0 0; background: rgba(4, 30, 36, .35); z-index: 1; }
.mega__h { display: flex; align-items: center; gap: 20px; padding: 16px 24px; border-bottom: 1px solid #DFE8EC; }
.mega__h b { font-size: 15px; font-weight: 700; }
.mega__h label { flex: 0 1 380px; height: 36px; display: flex; align-items: center; gap: 8px; padding: 0 12px; border: 1px solid #DFE8EC; border-radius: 10px; background: #F4F8F9; color: #5F7B8A; }
.mega__h input { flex: 1; border: 0; outline: 0; background: transparent; font: 500 13px Inter, Arial, sans-serif; color: #0B2533; }
.mega__b { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 18px 28px; padding: 20px 24px 24px; }
.mega__b h4 { display: flex; align-items: center; gap: 10px; margin: 0 0 8px; font-size: 11.5px; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: #5F7B8A; }
.mega__ic { width: 28px; height: 28px; border-radius: 8px; display: grid; place-items: center; background: #E6F3F4; color: #00636D; font-size: 12px; }
.mega__b a { display: flex; align-items: center; gap: 8px; padding: 6px 0; color: #0B2533; font-size: 13.5px; cursor: pointer; }
.mega__b a:hover { color: #00636D; }
.mega__b a.here { color: #00636D; font-weight: 700; }
.mega__b a i { color: #00636D; font-size: 12px; }
.mega__b .ws { padding: 14px 16px; border-radius: 14px; background: #F4F8F9; }
@media (max-width: 1400px) { .pill--sm { display: none; } }
@media (max-width: 1100px) { .h1__page, .h2__app small { display: none; } .pill--user b { display: none; } }
@media (max-width: 760px) { .h2__app span:last-child, .h1__div, .h1__mod { display: none; } .h1, .h2 { padding: 0 12px; } }
</style>
