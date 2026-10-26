<script setup lang="ts">
// The application header of App 105 as finova-portfolio-bilingual.js builds it (v15 to v47): the logo, the Menu
// panel made from the Navigation Menu list (the side tree itself is hidden by finova-v15.css), the portfolio search
// (FNV_FIND_PORTFOLIO), the EN / Arabic switch and the inbox bell with window.FINOVA_NAV's count (page 0 region
// "Navigation Badges"). The user menu of the APEX header (Home, My inbox, Sign out) is the CAPITRA header's.
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { http } from '@/shared/api/http'
import capitraMark from '@/assets/capitra-mark.svg'
import { appPageUrl } from '@/modules/apex/wizard'
import { getLang, setLanguage, tr } from './finovaBilingual'
import { navMenu, type NavItem } from './navMenu'

const props = defineProps<{ page: number }>()
const route = useRoute()
const router = useRouter()
const ar = getLang() === 'AR'

// window.FINOVA_NAV
const nav = ref<{ inbox?: number; exceptions?: number; inboxUrl?: string; homeUrl?: string }>({})
async function loadNav() {
  try {
    const res = await http.post<{ nav?: typeof nav.value }>('/api/apex/105/0/NAVIGATION_BADGES', {})
    nav.value = res.nav ?? {}
  } catch {
    nav.value = {}
  }
}
const inbox = computed(() => Number(nav.value.inbox) || 0)
const inboxUrl = computed(() => nav.value.inboxUrl || appPageUrl(105, 920))

function go(e: MouseEvent, href: string) {
  if (e.ctrlKey || e.metaKey || e.shiftKey || !href.startsWith('/')) return
  e.preventDefault()
  menuOpen.value = false
  closeResults()
  void router.push(href)
}

// ---------- Menu panel built from the navigation menu ----------
type Link = { label: string; href: string; current: boolean; q: string }
type Group = { label: string; href: string; icon: string; links: Link[] }
const href = (n: NavItem) => (n.page == null ? '' : appPageUrl(105, n.page))
const isCurrent = (n: NavItem) => (n.current ? n.current.includes(props.page) : n.page === props.page)
function link(n: NavItem): Link {
  const h = href(n)
  return { label: tr(n.label), href: h, current: isCurrent(n) || (!!h && route.path === h), q: tr(n.label).toLowerCase() + ' ' + n.label.toLowerCase() }
}
const groups = computed<Group[]>(() => {
  const out: Group[] = []
  const loose = navMenu.filter((n) => !n.children?.length && href(n))
  if (loose.length) out.push({ label: ar ? 'الرئيسية' : 'Main', href: '', icon: 'fa-star-o', links: loose.map(link) })
  for (const n of navMenu) {
    if (!n.children?.length) continue
    const flat: NavItem[] = []
    const f = (list: NavItem[]) => list.forEach((c) => (flat.push(c), c.children?.length && f(c.children)))
    f(n.children)
    out.push({ label: tr(n.label), href: href(n), icon: n.icon, links: flat.filter((c) => href(c)).map(link) })
  }
  return out
})
const menuOpen = ref(false)
const menuBtn = ref<HTMLButtonElement>()
const panel = ref<HTMLElement>()
const filter = ref('')
const shown = (l: Link) => !filter.value.trim() || l.q.indexOf(filter.value.trim().toLowerCase()) >= 0
watch(menuOpen, async (on) => {
  if (!on) return
  await nextTick()
  panel.value?.querySelector('input')?.focus()
})

// ---------- header search: jump to a portfolio by number, developer or project ----------
type Hit = { no: string; developer: string; project: string; status: string; url: string }
const search = ref<HTMLInputElement>()
const box = ref<HTMLElement>()
const q = ref('')
const hits = ref<Hit[] | null>(null)
const active = ref(-1)
let timer = 0
let seq = 0
function closeResults() {
  hits.value = null
  active.value = -1
}
watch(q, (text) => {
  window.clearTimeout(timer)
  if (text.trim().length < 2) return closeResults()
  timer = window.setTimeout(async () => {
    const my = ++seq
    try {
      const d = await http.post<{ rows?: Hit[] }>('/api/apex/105/0/FNV_FIND_PORTFOLIO', { x01: text.trim() })
      if (my === seq) {
        hits.value = d.rows ?? []
        active.value = -1
      }
    } catch {
      if (my === seq) closeResults()
    }
  }, 250)
})
function searchKey(e: KeyboardEvent) {
  if (e.key === 'Escape') return closeResults()
  const rows = hits.value
  if (!rows?.length) return
  if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
    e.preventDefault()
    active.value = (active.value + (e.key === 'ArrowDown' ? 1 : -1) + rows.length) % rows.length
  }
  if (e.key === 'Enter') {
    e.preventDefault()
    const url = rows[active.value < 0 ? 0 : active.value]!.url
    closeResults()
    void router.push(url)
  }
}

function onDocClick(e: MouseEvent) {
  const t = e.target as Node
  if (box.value && !box.value.contains(t)) closeResults()
  if (menuOpen.value && panel.value && !panel.value.contains(t) && t !== menuBtn.value && !menuBtn.value?.contains(t)) menuOpen.value = false
}
function onDocKey(e: KeyboardEvent) {
  if (e.key === 'Escape' && menuOpen.value) {
    menuOpen.value = false
    menuBtn.value?.focus()
    return
  }
  if (e.key !== '/' || e.ctrlKey || e.metaKey || e.altKey) return
  const t = e.target as HTMLElement | null
  const tag = (t?.tagName || '').toLowerCase()
  if (tag === 'input' || tag === 'textarea' || tag === 'select' || t?.isContentEditable) return
  e.preventDefault()
  search.value?.focus()
}
onMounted(() => {
  void loadNav()
  document.addEventListener('click', onDocClick)
  document.addEventListener('keydown', onDocKey)
})
onBeforeUnmount(() => {
  document.removeEventListener('click', onDocClick)
  document.removeEventListener('keydown', onDocKey)
})
watch(() => route.fullPath, () => void loadNav())

const placeholder = ar ? 'ابحث عن محفظة أو مطور أو مشروع' : 'Find a portfolio, developer or project'
</script>

<template>
  <div class="t-Header-branding">
    <div class="t-Header-logo">
      <a class="t-Header-logo-link" href="/mortgage" @click="go($event, '/mortgage')">
        <span class="fxh-brand">
          <img class="fxh-logo" :src="capitraMark" alt="CAPITRA" />
          <span class="fxh-div"></span>
          <span class="fxh-title">
            <b data-finova-en="Portfolio Acquisition" data-finova-ar="استحواذ المحافظ">Portfolio Acquisition</b>
            <small data-finova-en="Mortgage finance platform" data-finova-ar="منصة التمويل العقاري">Mortgage finance platform</small>
          </span>
        </span>
      </a>
    </div>
    <button
      ref="menuBtn"
      type="button"
      class="fxh-menubtn"
      aria-haspopup="true"
      :aria-expanded="menuOpen ? 'true' : 'false'"
      aria-controls="fxhMega"
      @click.stop="menuOpen = !menuOpen"
    >
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="3" width="7" height="7" rx="1.5" /><rect x="14" y="3" width="7" height="7" rx="1.5" /><rect x="3" y="14" width="7" height="7" rx="1.5" /><rect x="14" y="14" width="7" height="7" rx="1.5" /></svg>
      <span>{{ ar ? 'القائمة' : 'Menu' }}</span>
    </button>
    <div ref="box" class="fx-gsearch" role="search">
      <span class="fxh-sicon" aria-hidden="true"><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="7" /><path d="m20 20-3.5-3.5" /></svg></span>
      <input
        ref="search"
        v-model="q"
        type="search"
        autocomplete="off"
        aria-autocomplete="list"
        aria-controls="fxGResults"
        :placeholder="placeholder"
        :aria-label="placeholder"
        @keydown="searchKey"
      />
      <kbd aria-hidden="true">/</kbd>
      <div id="fxGResults" class="fx-gresults" role="listbox" :hidden="hits === null">
        <div v-if="hits && !hits.length" class="fx-gnone">{{ ar ? 'لا توجد نتائج' : 'No matching portfolio' }}</div>
        <a v-for="(r, i) in hits ?? []" :key="r.url" role="option" :href="r.url" :class="{ 'is-active': i === active }" @click="go($event, r.url)">
          <strong>{{ r.no }}</strong> &middot; {{ r.status }}<small>{{ r.developer }} &middot; {{ r.project }}</small>
        </a>
      </div>
    </div>
    <div class="t-Header-navBar">
      <div class="fxh-tools">
        <div class="fxh-lang" role="group" aria-label="Language">
          <button type="button" data-l="EN" :class="{ on: !ar }" :aria-pressed="!ar ? 'true' : undefined" @click="ar && setLanguage('EN')">EN</button>
          <button type="button" data-l="AR" class="ar" :class="{ on: ar }" :aria-pressed="ar ? 'true' : undefined" lang="ar" @click="!ar && setLanguage('AR')">&#1593;</button>
        </div>
        <a class="fxh-icon" :href="inboxUrl" :title="(ar ? 'صندوق المهام' : 'My inbox') + (inbox ? ' (' + inbox + ')' : '')" @click="go($event, inboxUrl)">
          <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9" /><path d="M10.3 21a1.94 1.94 0 0 0 3.4 0" /></svg>
          <span v-if="inbox > 0" class="fxh-badge">{{ inbox > 99 ? '99+' : inbox }}</span>
        </a>
      </div>
    </div>
  </div>
  <Teleport to="body">
    <div id="fxhMega" ref="panel" class="fxh-mega" :hidden="!menuOpen" role="dialog" :aria-label="ar ? 'القائمة' : 'Menu'">
      <div class="fxh-mega-h">
        <b>{{ ar ? 'كل الصفحات' : 'All pages' }}</b>
        <input v-model="filter" type="search" :placeholder="ar ? 'ابحث في القائمة' : 'Filter pages'" :aria-label="ar ? 'ابحث في القائمة' : 'Filter pages'" />
      </div>
      <div v-if="groups.length" class="fxh-mega-b">
        <div v-for="g in groups" :key="g.label" class="fxh-grp" :hidden="!!filter.trim() && !g.links.some(shown)">
          <a v-if="g.href" class="fxh-grp-t" :href="g.href" @click="go($event, g.href)">
            <span class="fxh-gi fa" :class="g.icon" aria-hidden="true"></span>{{ g.label }}
          </a>
          <div v-else class="fxh-grp-t">
            <span v-if="g.icon" class="fxh-gi fa" :class="g.icon" aria-hidden="true"></span><span v-else class="fxh-gi" aria-hidden="true">&#9679;</span>{{ g.label }}
          </div>
          <a v-for="l in g.links" :key="l.href" class="fxh-l" :class="{ 'is-current': l.current }" :href="l.href" :hidden="!shown(l)" @click="go($event, l.href)">{{ l.label }}</a>
        </div>
      </div>
      <div v-else class="fxh-mega-empty">{{ ar ? 'لا توجد صفحات' : 'No pages' }}</div>
    </div>
  </Teleport>
</template>
