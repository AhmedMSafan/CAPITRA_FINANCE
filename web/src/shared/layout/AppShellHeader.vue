<script setup lang="ts">
// The one header of every page, as the APEX launcher (App 100) draws it: one flat teal bar with the name and the Menu
// button (every page of every module) on one side, and the EN / Arabic switch, the orange Inbox button with its count
// and the signed-in user (employee no., department, roles, sign out) on the other.
import capitraLogo from '@/assets/capitra-mark.svg'
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter, type RouteLocationRaw } from 'vue-router'
import { getUser, hasPermission, type SessionUser } from '@/shared/auth/session'
import { logout as endSession, refreshCurrentUser } from '@/modules/sec/api/auth.api'
import { workflowService } from '@/modules/wfe/services/workflowService'
import { appMenu, type MenuLink } from './appMenu'
import { currentLang, switchLanguage, tr } from './language'

const route = useRoute()
const router = useRouter()
const ar = computed(() => currentLang() === 'AR')
const t = (en: string, arText: string) => (ar.value ? arText : en)

const user = ref<SessionUser | null>(getUser())
const displayName = computed(() => user.value?.fullName?.trim() || user.value?.userName?.trim() || 'CAPITRA User')
const employeeNo = computed(() => user.value?.employeeNo?.trim() || '-')
const department = computed(() => user.value?.department?.trim() || '-')
const roles = computed(() => user.value?.roles ?? [])

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
}
function go(to: RouteLocationRaw) {
  closeAll()
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

// the user menu icons of ums-header.js
const ICON = {
  home: '<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m3 10 9-7 9 7v10a1 1 0 0 1-1 1h-5v-7H9v7H4a1 1 0 0 1-1-1z"/></svg>',
  list: '<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"/></svg>',
  grid: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></svg>',
  out: '<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><path d="m16 17 5-5-5-5"/><path d="M21 12H9"/></svg>'
}
const initials = computed(() => displayName.value.split(/\s+/).filter(Boolean).slice(0, 2).map((w) => w[0]!.toUpperCase()).join('') || '?')
// Users and Role permissions open only for SEC.USER.ADMIN (the API checks it again)
const canAdminUsers = hasPermission('SEC.USER.ADMIN')
</script>

<template>
  <header class="t-Header shell" :dir="ar ? 'rtl' : 'ltr'">
    <div class="t-Header-branding">
      <RouterLink :to="{ name: 'home' }" class="fx-brand" aria-label="CAPITRA Home"><img :src="capitraLogo" alt="" class="fx-logo" />CAPITRA</RouterLink>
      <RouterLink :to="{ name: 'home' }" class="fx-btn fx-home"><span v-html="ICON.home"></span>{{ t('Home', 'الرئيسية') }}</RouterLink>
      <button type="button" class="fx-btn" aria-haspopup="true" :aria-expanded="menuOpen ? 'true' : 'false'" @click.stop="menuOpen = !menuOpen; userOpen = false">
        {{ t('Menu', 'القائمة') }}
      </button>
      <div class="fx-right">
        <div class="fxh-lang" role="group" aria-label="Language">
          <button type="button" :class="{ on: !ar }" @click="ar && switchLanguage('EN')">EN</button>
          <button type="button" class="ar" :class="{ on: ar }" lang="ar" @click="!ar && switchLanguage('AR')">&#1593;</button>
        </div>
        <RouterLink to="/workflow/inbox" class="fx-inbox">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M22 12h-6l-2 3h-4l-2-3H2" /><path d="M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z" /></svg>
          <span>{{ t('Inbox', 'صندوق المهام') }}</span><b class="fx-count">{{ inboxCount > 99 ? '99+' : inboxCount }}</b>
        </RouterLink>
        <div class="fxh-userwrap">
          <button type="button" class="fx-user" :aria-expanded="userOpen ? 'true' : 'false'" @click.stop="userOpen = !userOpen; menuOpen = false">{{ displayName }}</button>
          <div v-if="userOpen" class="fxh-menu" role="menu" @click.stop>
            <div class="fxh-mh"><span class="fxh-av" aria-hidden="true">{{ initials }}</span><div><b>{{ displayName }}</b><small>{{ (user?.userName || '').toLowerCase() }}</small></div></div>
            <div class="fxh-info">
              <span>{{ t('Employee no.', 'رقم الموظف') }}</span><b>{{ employeeNo }}</b>
              <span>{{ t('Department', 'الإدارة') }}</span><b>{{ department }}</b>
              <span>{{ t('Roles', 'الأدوار') }}</span><b>{{ roles.join(', ') || '-' }}</b>
            </div>
            <a role="menuitem" @click="go({ name: 'home' })"><span v-html="ICON.home"></span><span>{{ t('Home', 'الرئيسية') }}</span></a>
            <template v-if="canAdminUsers">
            <a role="menuitem" @click="go('/admin/users')"><span v-html="ICON.list"></span><span>{{ t('Users', 'المستخدمون') }}</span></a>
            <a role="menuitem" @click="go('/admin/roles')"><span v-html="ICON.grid"></span><span>{{ t('Role permissions', 'صلاحيات الأدوار') }}</span></a>
            </template>
            <hr />
            <a role="menuitem" class="out" @click="logout"><span v-html="ICON.out"></span><span>{{ t('Sign out', 'تسجيل الخروج') }}</span></a>
          </div>
        </div>
      </div>
    </div>

    <div v-if="menuOpen" class="fxh-mega" role="dialog" :aria-label="t('Menu', 'القائمة')" @click.stop>
      <div class="fxh-mega-h">
        <b>{{ t('All pages', 'كل الصفحات') }}</b>
        <input v-model="filter" type="search" :placeholder="t('Filter pages', 'ابحث في القائمة')" :aria-label="t('Filter pages', 'ابحث في القائمة')" />
      </div>
      <div class="fxh-mega-b">
        <div v-for="g in groups" :key="g.title" class="fxh-grp">
          <div class="fxh-grp-t"><span class="fxh-gi" aria-hidden="true"><i :class="g.icon"></i></span>{{ tr(g.title) }}</div>
          <a v-for="l in g.links" :key="l.label" class="fxh-l" :class="{ 'is-current': isHere(l) }" href="#" @click.prevent="go(l.to)">{{ tr(l.label) }}</a>
        </div>
      </div>
    </div>
  </header>
</template>

<style scoped>
/* finova-v15.css and ums-header.css of the APEX apps, header part */
.shell { position: sticky; top: 0; z-index: 100; font-family: Inter, 'Segoe UI', Arial, sans-serif; }
.t-Header-branding {
  height: 64px; display: flex; align-items: center; gap: 14px; padding: 0 20px;
  background: linear-gradient(180deg, #013F48 0%, #014B55 100%);
}
.fx-logo { width: 38px; height: 38px; border-radius: 10px; display: block; box-shadow: 0 0 0 1px rgba(255,255,255,.12); }
.fx-brand { display: inline-flex; align-items: center; gap: 10px; color: #fff; text-decoration: none; font: 700 16px/1 Inter, Arial, sans-serif; letter-spacing: .01em; }
/* !important: some copied APEX pages style a global .fx-btn of their own, which must not reach the header */
.fx-btn {
  height: 40px !important; min-height: 0 !important; padding: 0 14px !important; border-radius: 10px !important;
  border: 1px solid rgba(255, 255, 255, .22) !important; background: rgba(255, 255, 255, .06) !important;
  color: #fff !important; font: 600 13.5px/1 Inter, Arial, sans-serif !important; cursor: pointer;
  display: inline-flex; align-items: center; gap: 8px; text-decoration: none !important; box-shadow: none !important;
}
.fx-home { background: rgba(255, 255, 255, .14) !important; }
[dir=rtl] .fx-btn { font-family: Cairo, 'Segoe UI', Arial, sans-serif !important; }
.fx-btn:hover, .fx-btn[aria-expanded=true] { background: rgba(255, 255, 255, .2) !important; }
.fx-right { margin-inline-start: auto; display: flex; align-items: center; gap: 12px; }
.fxh-lang { display: flex; padding: 3px; border-radius: 10px; background: rgba(255, 255, 255, .08); border: 1px solid rgba(255, 255, 255, .14); }
.fxh-lang button { appearance: none; border: 0; background: transparent; cursor: pointer; padding: 6px 11px; border-radius: 7px; font: 600 13px/1 Inter, Arial, sans-serif; color: rgba(255, 255, 255, .72); }
.fxh-lang button:hover { color: #fff; }
.fxh-lang button.on { background: #fff; color: #014B55; box-shadow: 0 1px 3px rgba(0, 0, 0, .18); }
.fxh-lang button.ar { font-family: Cairo, 'Segoe UI', Arial, sans-serif; font-size: 14px; }
.fx-inbox {
  position: relative; display: inline-flex; align-items: center; gap: 8px; height: 36px; padding: 0 14px; border-radius: 9px; background: #E87722; color: #fff;
  text-decoration: none; font: 600 13.5px/1 Inter, Arial, sans-serif; box-shadow: 0 8px 18px -8px rgba(232, 119, 34, .8);
}
[dir=rtl] .fx-inbox { font-family: Cairo, 'Segoe UI', Arial, sans-serif; }
.fx-count {
  position: absolute; top: -8px; inset-inline-end: -8px; min-width: 20px; height: 20px; padding: 0 5px; border-radius: 99px; background: #D92D20; color: #fff;
  border: 2px solid #fff; font: 700 11px/16px Inter, Arial, sans-serif; text-align: center;
}
.fx-user {
  height: 44px; min-width: 120px; padding: 0 16px; border-radius: 10px; border: 1px solid rgba(255, 255, 255, .14); background: rgba(255, 255, 255, .08);
  color: #fff; font: 500 14px/1 Inter, Arial, sans-serif; cursor: pointer; white-space: nowrap;
}
.fx-user:hover, .fx-user[aria-expanded=true] { background: rgba(255, 255, 255, .14); }
.fxh-userwrap { position: relative; }
/* the user menu: ums-header.css */
.fxh-menu { position: absolute; inset-inline-end: 0; top: calc(100% + 10px); width: 290px; background: #fff; color: #0B2533; border-radius: 14px;
  box-shadow: 0 24px 60px rgba(3, 24, 30, .25); border: 1px solid #DFE8EC; overflow: hidden; z-index: 1000; animation: fxhIn .14s ease-out;
  font-family: Inter, 'Segoe UI', Arial, sans-serif; text-align: start; }
@keyframes fxhIn { from { opacity: 0; transform: translateY(-6px); } to { opacity: 1; transform: none; } }
.fxh-av { width: 32px; height: 32px; border-radius: 9px; display: grid; place-items: center; font: 700 13px/1 Inter, Arial, sans-serif; background: linear-gradient(135deg, #F0932B, #E87722); color: #fff; flex: 0 0 auto; }
.fxh-mh { display: flex; gap: 12px; align-items: center; padding: 16px; border-bottom: 1px solid #EDF2F4; background: linear-gradient(180deg, #F6FAFA, #fff); }
.fxh-mh .fxh-av { width: 42px; height: 42px; font-size: 15px; border-radius: 12px; }
.fxh-mh b { display: block; font-size: 14.5px; color: #0B2533; }
.fxh-mh small { display: block; color: #5F7B8A; font-size: 12.5px; word-break: break-all; }
.fxh-info { display: grid; grid-template-columns: auto 1fr; gap: 6px 12px; padding: 12px 16px; border-bottom: 1px solid #EDF2F4; font-size: 12.5px; }
.fxh-info span { color: #5F7B8A; }
.fxh-info b { font-weight: 600; color: #0B2533; }
.fxh-menu a { display: flex; align-items: center; gap: 12px; padding: 11px 16px; color: #0B2533 !important; text-decoration: none !important; font-size: 14px; cursor: pointer; }
.fxh-menu a:hover, .fxh-menu a:focus { background: #F2F7F8; outline: 0; }
.fxh-menu a :deep(svg) { color: #5F7B8A; flex: 0 0 auto; display: block; }
.fxh-menu hr { border: 0; border-top: 1px solid #EDF2F4; margin: 4px 0; }
.fxh-menu a.out, .fxh-menu a.out :deep(svg) { color: #C43325 !important; }
.fxh-mega {
  position: fixed; top: 66px; inset-inline-start: 16px; width: min(980px, calc(100vw - 32px)); max-height: calc(100vh - 90px); overflow: auto; z-index: 1001;
  background: #fff; color: #0B2533; border: 1px solid #DFE8EC; border-radius: 18px; box-shadow: 0 30px 70px rgba(3, 24, 30, .28); animation: fxhIn .16s ease-out;
}
.fxh-mega-h { display: flex; align-items: center; gap: 12px; padding: 14px 18px; border-bottom: 1px solid #EDF2F4; background: linear-gradient(180deg, #F6FAFA, #fff); }
.fxh-mega-h b { font-size: 15px; }
.fxh-mega-h input { margin-inline-start: auto; width: min(280px, 50%); height: 36px; border: 1px solid #DCE5E9; border-radius: 10px; padding: 0 12px; font: inherit; font-size: 13.5px; outline: 0; }
.fxh-mega-h input:focus { border-color: #00636D; box-shadow: 0 0 0 3px rgba(0, 99, 109, .12); }
.fxh-mega-b { display: grid; grid-template-columns: repeat(auto-fill, minmax(230px, 1fr)); gap: 6px 18px; padding: 16px 18px 20px; }
.fxh-grp { min-width: 0; padding: 4px 0 8px; }
.fxh-grp-t { display: flex; align-items: center; gap: 10px; padding: 8px; border-radius: 10px; font-size: 13px; font-weight: 700; color: #014B55; letter-spacing: .01em; }
.fxh-gi { width: 30px; height: 30px; border-radius: 9px; display: grid; place-items: center; background: #E6F2F2; color: #00636D; font-size: 14px; flex: 0 0 auto; }
.fxh-grp a.fxh-l { display: block; padding: 7px 8px 7px 48px; border-radius: 9px; font-size: 13.5px; color: #29495A; text-decoration: none; line-height: 1.3; }
[dir=rtl] .fxh-grp a.fxh-l { padding: 7px 48px 7px 8px; }
.fxh-grp a.fxh-l:hover { background: #F2F7F8; color: #00636D; }
.fxh-grp .is-current { background: #FFF3E8; color: #B4570E; font-weight: 600; }
@media (max-width: 900px) { .fxh-mega { top: 62px; inset-inline: 8px; width: auto; } }
@media (max-width: 600px) { .fxh-lang { display: none; } }
</style>
