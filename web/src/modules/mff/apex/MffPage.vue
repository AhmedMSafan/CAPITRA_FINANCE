<script setup lang="ts">
// A page of App 110 (Mortgage Finance), rendered from its definition generated from the APEX export, in the app's
// Universal Theme shell: the header with the app logo and the Navigation Bar list (user, EN, AR), and the side
// Navigation Menu (a tree, collapsed by default and hidden when collapsed: js-defaultCollapsed, js-navCollapsed--hidden).
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ApexWizardPage from '@/modules/apex/ApexWizardPage.vue'
import { appPageUrl } from '@/modules/apex/wizard'
import { getUser } from '@/shared/auth/session'
import { navMenu, type NavItem } from './navMenu'
import { getLang, setLanguage, useMffUi } from './mffUi'
import { wizardPages } from './wizard/wizardPages'

const props = defineProps<{ page: number }>()
const route = useRoute()
const router = useRouter()
const opened = ref(0)
const user = getUser()?.userName ?? ''
// the app's CSS File URLs (#APP_FILES#finova-v14.css) and the global page's styles
const appCss = ['/apex/110/finova-v14.css', '/apex/110/mff-ui.css']

onMounted(() => useMffUi(router, () => props.page))

// the side navigation: open state kept for the session, as the Universal Theme keeps it
const NAV_KEY = 'MFF_NAV_EXPANDED'
const navOpen = ref(sessionStorage.getItem(NAV_KEY) === 'Y')
function toggleNav() {
  navOpen.value = !navOpen.value
  sessionStorage.setItem(NAV_KEY, navOpen.value ? 'Y' : 'N')
}

function isCurrent(n: NavItem): boolean {
  return (n.current ?? (n.page != null ? [n.page] : [])).includes(props.page)
}
function hasCurrent(n: NavItem): boolean {
  return isCurrent(n) || (n.children ?? []).some(hasCurrent)
}
const expanded = ref(new Set(navMenu.filter(hasCurrent).map((n) => n.label)))
function toggle(n: NavItem) {
  const s = new Set(expanded.value)
  if (s.has(n.label)) s.delete(n.label)
  else s.add(n.label)
  expanded.value = s
}
function href(n: NavItem) {
  return n.page == null ? '#' : appPageUrl(110, n.page)
}
function go(event: MouseEvent, n: NavItem) {
  if (n.page == null || event.ctrlKey || event.metaKey || event.shiftKey) return
  event.preventDefault()
  void router.push(href(n))
}
const lang = ref(getLang())
function language(l: 'EN' | 'AR') {
  setLanguage(l)
  lang.value = l
}
const home = computed(() => appPageUrl(110, 1))
</script>

<template>
  <component :is="'link'" v-for="href in appCss" :key="href" rel="stylesheet" :href="href" />
  <div class="mff-app t-PageBody t-PageBody--leftNav" :class="{ 'js-navExpanded': navOpen }">
    <header class="t-Header mff-header">
      <button type="button" class="mff-navbtn" :aria-expanded="navOpen" aria-label="Main Navigation" title="Main Navigation" @click="toggleNav">
        <span class="fa fa-bars" aria-hidden="true"></span>
      </button>
      <a class="mff-logo" :href="home" @click.prevent="router.push(home)">
        <div style="height: 58px; display: flex; align-items: center; background: #014b55; justify-content: flex-start; gap: 3px">
          <img src="/apex/110/bar4.png" alt="FINOVA" style="height: 100%; width: auto; object-fit: contain; display: block" />
          <span style="color: #ffffff; font-family: Inter, Arial, sans-serif; font-size: 1rem; font-weight: 500; line-height: 1">Mortgage Finance</span>
        </div>
      </a>
      <ul class="mff-navbar">
        <li><button type="button" :class="{ on: lang === 'EN' }" @click="language('EN')"><span class="fa fa-language" aria-hidden="true"></span> EN</button></li>
        <li><button type="button" :class="{ on: lang === 'AR' }" @click="language('AR')"><span class="fa fa-language" aria-hidden="true"></span> AR</button></li>
        <li class="mff-user"><span class="fa fa-user" aria-hidden="true"></span> {{ user }}</li>
      </ul>
    </header>
    <div class="mff-body">
      <nav v-show="navOpen" class="t-Body-nav mff-nav" aria-label="Main Navigation">
        <ul class="mff-tree">
          <li v-for="n in navMenu" :key="n.label" :class="{ 'is-current': isCurrent(n), 'is-expanded': expanded.has(n.label) }">
            <div class="mff-node">
              <button v-if="n.children?.length" type="button" class="mff-tog" :aria-label="expanded.has(n.label) ? 'Collapse' : 'Expand'" @click="toggle(n)">
                <span class="fa" :class="expanded.has(n.label) ? 'fa-caret-down' : 'fa-caret-right'" aria-hidden="true"></span>
              </button>
              <span v-else class="mff-tog"></span>
              <a :href="href(n)" @click="go($event, n)"><span class="fa" :class="n.icon" aria-hidden="true"></span><span class="a-TreeView-label">{{ n.label }}</span></a>
            </div>
            <ul v-if="n.children?.length && expanded.has(n.label)">
              <li v-for="c in n.children" :key="c.label" :class="{ 'is-current': isCurrent(c) }">
                <div class="mff-node">
                  <span class="mff-tog"></span>
                  <a :href="href(c)" @click="go($event, c)"><span class="fa" :class="c.icon" aria-hidden="true"></span><span class="a-TreeView-label">{{ c.label }}</span></a>
                </div>
              </li>
            </ul>
          </li>
        </ul>
      </nav>
      <main class="t-Body-main mff-main">
        <ApexWizardPage :key="`${route.fullPath}#${opened}`" :app="110" :pages="wizardPages" :page="page" @reload="opened++" />
      </main>
    </div>
  </div>
</template>

<style scoped>
.mff-app { min-height: calc(100vh - 64px); background: #f4f7f8; }
.mff-header { display: flex; align-items: center; height: 58px; background: #014b55; color: #fff; padding: 0; position: sticky; top: 0; z-index: 20; }
.mff-navbtn { width: 48px; height: 58px; border: 0; background: transparent; color: #fff; font-size: 18px; cursor: pointer; }
.mff-navbtn:hover { background: rgba(255, 255, 255, 0.1); }
.mff-logo { display: block; height: 58px; text-decoration: none; }
.mff-navbar { display: flex; align-items: center; gap: 4px; margin: 0 12px 0 auto; padding: 0; list-style: none; }
.mff-navbar button { border: 0; background: transparent; color: #fff; padding: 8px 10px; border-radius: 6px; cursor: pointer; font-size: 13px; }
.mff-navbar button.on, .mff-navbar button:hover { background: rgba(255, 255, 255, 0.14); }
.mff-user { padding: 8px 10px; font-size: 13px; }
.mff-body { display: flex; align-items: stretch; }
.mff-nav { width: 260px; flex: none; background: #fff; border-inline-end: 1px solid #dfe9ec; min-height: calc(100vh - 122px); padding: 8px 0; }
.mff-tree, .mff-tree ul { list-style: none; margin: 0; padding: 0; }
.mff-tree ul { padding-inline-start: 18px; }
.mff-node { display: flex; align-items: center; }
.mff-tog { width: 22px; flex: none; border: 0; background: transparent; color: #4b6b77; cursor: pointer; }
.mff-node a { display: flex; align-items: center; gap: 8px; flex: 1; padding: 8px 10px 8px 2px; color: #17384a; text-decoration: none; font-size: 13px; border-radius: 6px; }
.mff-node a .fa { width: 16px; text-align: center; color: #01696f; }
.mff-node a:hover { background: #eef5f6; }
.is-current > .mff-node a { background: #e3f0f1; font-weight: 700; color: #014b55; }
.mff-main { flex: 1; min-width: 0; padding: 0 0 24px; }
</style>
