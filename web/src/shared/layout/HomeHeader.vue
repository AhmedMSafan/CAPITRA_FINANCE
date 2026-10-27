<script setup lang="ts">
  import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import capitraLogo from '@/assets/capitra-mark.svg'
  import { getUser, type SessionUser } from '@/shared/auth/session'
  import { logout as endSession, refreshCurrentUser } from '@/modules/sec/api/auth.api'
  import { workflowService } from '@/modules/wfe/services/workflowService'
  import { appMenu } from './appMenu'

  const props = defineProps<{
    moduleName: string
    functionName: string
  }>()

  const router = useRouter()
  const route = useRoute()
  const menuOpen = ref(false)
  const userOpen = ref(false)
  const search = ref('')
  const searchInput = ref<HTMLInputElement | null>(null)
  const inboxCount = ref(0)
  const user = ref<SessionUser | null>(getUser())

  const displayName = computed(() =>
    user.value?.fullName?.trim() ||
    user.value?.userName?.trim() ||
    'CAPITRA User'
  )
  const initials = computed(() =>
    displayName.value.split(/\s+/).filter(Boolean).slice(0, 2).map((w) => w[0]!.toUpperCase()).join('')
  )
  const employeeNo = computed(() => user.value?.employeeNo?.trim() || '-')
  const department = computed(() => user.value?.department?.trim() || '-')
  const roles = computed(() => user.value?.roles ?? [])
  const isHome = computed(() => route.name === 'home')

  // The search box finds pages of the menu; typing opens the menu with the matches.
  const groups = computed(() => {
    const q = search.value.trim().toLowerCase()
    if (!q) return appMenu
    return appMenu
      .map((g) => ({
        ...g,
        links: g.title.toLowerCase().includes(q) ? g.links : g.links.filter((l) => l.label.toLowerCase().includes(q))
      }))
      .filter((g) => g.links.length)
  })

  function closeAll() {
    menuOpen.value = false
    userOpen.value = false
  }

  function go(to: Parameters<typeof router.push>[0]) {
    closeAll()
    search.value = ''
    router.push(to)
  }

  function onSearchInput() {
    menuOpen.value = !!search.value.trim()
    userOpen.value = false
  }

  function openFirstMatch() {
    const first = groups.value[0]?.links[0]
    if (search.value.trim() && first) go(first.to)
  }

  async function logout() {
    endSession()
    closeAll()
    await router.push({ name: 'login' })
  }

  function onKey(e: KeyboardEvent) {
    if (e.key === 'Escape') closeAll()
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
      e.preventDefault()
      searchInput.value?.focus()
    }
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'm') {
      e.preventDefault()
      menuOpen.value = !menuOpen.value
      userOpen.value = false
    }
  }

  async function loadInboxCount() {
    try {
      inboxCount.value = (await workflowService.getMyInbox()).length
    } catch {
      inboxCount.value = 0
    }
  }

  watch(() => route.fullPath, closeAll)

  onMounted(async () => {
    window.addEventListener('keydown', onKey)
    loadInboxCount()
    try {
      user.value = await refreshCurrentUser()
    } catch (error) {
      console.error('Unable to load the current user profile.', error)
    }
  })

  onBeforeUnmount(() => window.removeEventListener('keydown', onKey))
</script>

<template>
  <header class="cap-hdr">
    <div class="cap-hdr__bar">
      <button type="button" class="cap-hdr__menu-btn" :class="{ 'is-open': menuOpen }"
              aria-label="Menu" title="Menu (Ctrl+M)"
              @click="menuOpen = !menuOpen; userOpen = false">
        <i class="pi" :class="menuOpen ? 'pi-times' : 'pi-bars'"></i>
        <span>Menu</span>
      </button>

      <RouterLink :to="{ name: 'home' }" class="cap-hdr__brand" aria-label="CAPITRA Home">
        <img :src="capitraLogo" alt="" class="cap-hdr__logo" />
        <span class="cap-hdr__word">CAPITRA<em>FINANCE</em></span>
      </RouterLink>

      <div v-if="!isHome && props.moduleName" class="cap-hdr__where">
        <small>{{ props.moduleName }}</small>
        <b>{{ props.functionName }}</b>
      </div>

      <label class="cap-hdr__search">
        <i class="pi pi-search"></i>
        <input ref="searchInput" v-model="search" type="text" placeholder="Search pages…"
               @input="onSearchInput" @keydown.enter="openFirstMatch" />
        <kbd>Ctrl K</kbd>
      </label>

      <span class="cap-hdr__grow"></span>

      <nav class="cap-hdr__tools" aria-label="Shortcuts">
        <RouterLink :to="{ name: 'home' }" class="cap-hdr__tool" :class="{ 'is-on': isHome }" title="Home">
          <i class="pi pi-home"></i>
        </RouterLink>
        <RouterLink to="/workflowengine" class="cap-hdr__tool" title="Workflow Engine">
          <i class="pi pi-sitemap"></i>
        </RouterLink>
        <RouterLink to="/admin" class="cap-hdr__tool" title="Administration">
          <i class="pi pi-cog"></i>
        </RouterLink>
        <RouterLink to="/workflow/inbox" class="cap-hdr__tool" title="My Inbox">
          <i class="pi pi-bell"></i>
          <span v-if="inboxCount" class="cap-hdr__badge">{{ inboxCount }}</span>
        </RouterLink>
      </nav>

      <span class="cap-hdr__sep"></span>

      <div class="cap-hdr__who-wrap">
        <button type="button" class="cap-hdr__who" @click="userOpen = !userOpen; menuOpen = false">
          <span class="cap-hdr__ava">{{ initials }}</span>
          <span class="cap-hdr__who-text">
            <b>{{ displayName }}</b>
            <small>
              <i class="pi pi-id-card"></i>{{ employeeNo }}
              <span class="cap-hdr__dot">·</span>
              <i class="pi pi-building"></i>{{ department }}
            </small>
          </span>
          <i class="pi cap-hdr__cv" :class="userOpen ? 'pi-chevron-up' : 'pi-chevron-down'"></i>
        </button>

        <div v-if="userOpen" class="cap-hdr__user-menu">
          <div class="cap-hdr__um-head">
            <span class="cap-hdr__ava">{{ initials }}</span>
            <div><b>{{ displayName }}</b><small>{{ user?.userName ?? '' }}</small></div>
          </div>
          <div class="cap-hdr__um-row"><label>Employee</label><span><i class="pi pi-id-card"></i>{{ employeeNo }}</span></div>
          <div class="cap-hdr__um-row"><label>Department</label><span><i class="pi pi-building"></i>{{ department }}</span></div>
          <div class="cap-hdr__um-row">
            <label>Assigned roles</label>
            <div v-if="roles.length" class="cap-hdr__roles">
              <span v-for="role in roles" :key="role">{{ role }}</span>
            </div>
            <span v-else class="cap-hdr__muted">No assigned roles</span>
          </div>
          <button type="button" class="cap-hdr__logout" @click="logout">
            <i class="pi pi-sign-out"></i> Sign out
          </button>
        </div>
      </div>
    </div>

    <div v-if="menuOpen || userOpen" class="cap-hdr__shade" @click="closeAll"></div>

    <div v-if="menuOpen" class="cap-hdr__mega" role="menu">
      <div v-for="g in groups" :key="g.title" class="cap-hdr__group" :class="{ 'is-workspace': g.workspace }">
        <h4><i :class="g.icon"></i>{{ g.title }}</h4>
        <a v-for="l in g.links" :key="l.label" href="#" @click.prevent="go(l.to)">
          <i v-if="l.icon" :class="l.icon"></i>{{ l.label }}
        </a>
      </div>
      <div v-if="!groups.length" class="cap-hdr__none">No page matches “{{ search }}”.</div>
      <div class="cap-hdr__mega-foot">
        <span>Type in the search box to find a page</span>
        <span><kbd>Ctrl</kbd> <kbd>M</kbd> opens the menu</span>
      </div>
    </div>
  </header>
</template>

<style scoped>
  .cap-hdr {
    position: relative;
    z-index: 3000;
    font-family: Inter, "Segoe UI", Arial, sans-serif;
  }

  /* Pearl bar with faint slate and gold contour lines, and a gold hairline that fades at both ends. */
  .cap-hdr__bar {
    position: relative;
    height: 68px;
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 0 24px;
    color: #14232b;
    background:
      url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='1440' height='68' viewBox='0 0 1440 68' preserveAspectRatio='none'%3E%3Cg fill='none' stroke-width='1'%3E%3Cpath stroke='%23334155' stroke-opacity='.07' d='M0 58 C240 20 420 74 720 40 S1200 6 1440 34'/%3E%3Cpath stroke='%23334155' stroke-opacity='.06' d='M0 66 C260 30 440 82 740 50 S1220 16 1440 44'/%3E%3Cpath stroke='%23e0b04a' stroke-opacity='.22' d='M0 48 C220 12 400 64 700 30 S1180 -4 1440 24'/%3E%3Cpath stroke='%23334155' stroke-opacity='.05' d='M0 38 C200 4 380 56 680 22 S1160 -12 1440 14'/%3E%3C/g%3E%3C/svg%3E") center / 100% 100% no-repeat,
      radial-gradient(420px 120px at 100% 0%, rgba(224, 176, 74, .10), transparent 70%),
      linear-gradient(180deg, #ffffff 0%, #f7f9fb 100%);
    box-shadow: 0 1px 0 #e6ebef, 0 6px 18px rgba(15, 23, 42, .05);
  }

  .cap-hdr__bar::after {
    content: "";
    position: absolute;
    left: 0;
    right: 0;
    bottom: 0;
    height: 1.5px;
    background: linear-gradient(90deg, transparent 0%, #e0b04a 30%, #e0b04a 70%, transparent 100%);
    opacity: .9;
  }

  .cap-hdr__menu-btn {
    display: flex;
    align-items: center;
    gap: 8px;
    height: 40px;
    padding: 0 14px 0 12px;
    border: 0;
    border-radius: 11px;
    background: #0b2530;
    color: #fff;
    font-family: inherit;
    font-size: 13.5px;
    font-weight: 600;
    cursor: pointer;
    white-space: nowrap;
  }

  .cap-hdr__menu-btn i { font-size: 14px; color: #e0b04a; }
  .cap-hdr__menu-btn:hover, .cap-hdr__menu-btn.is-open { background: #16323f; }

  .cap-hdr__brand {
    display: flex;
    align-items: center;
    gap: 10px;
    text-decoration: none;
    color: #0b2530;
  }

  .cap-hdr__logo { width: 38px; height: 38px; border-radius: 10px; display: block; }

  .cap-hdr__word {
    font-size: 16px;
    font-weight: 800;
    letter-spacing: .08em;
    line-height: 1.05;
  }

  .cap-hdr__word em {
    display: block;
    font-style: normal;
    font-size: 9.5px;
    font-weight: 600;
    letter-spacing: .32em;
    color: #a87a14;
  }

  .cap-hdr__where {
    display: flex;
    flex-direction: column;
    padding-left: 14px;
    border-left: 1px solid #e2e8f0;
    white-space: nowrap;
    min-width: 0;
  }

  .cap-hdr__where small { font-size: 10.5px; font-weight: 700; letter-spacing: .1em; color: #94a3b8; }
  .cap-hdr__where b { max-width: 260px; overflow: hidden; text-overflow: ellipsis; font-size: 14px; color: #0b2530; }

  .cap-hdr__search {
    flex: 0 1 380px;
    min-width: 200px;
    height: 42px;
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 0 12px 0 14px;
    border: 1px solid #e2e8f0;
    border-radius: 11px;
    background: rgba(255, 255, 255, .85);
  }

  .cap-hdr__search i { color: #0f766e; }

  .cap-hdr__search input {
    flex: 1;
    min-width: 0;
    border: 0;
    outline: 0;
    background: transparent;
    font: inherit;
    font-size: 14px;
    color: #14232b;
  }

  .cap-hdr__search:focus-within { border-color: #e0b04a; box-shadow: 0 0 0 3px rgba(224, 176, 74, .18); }

  kbd {
    padding: 2px 6px;
    border: 1px solid #d6e0de;
    border-radius: 6px;
    background: #fff;
    font-family: inherit;
    font-size: 11px;
    line-height: 1.3;
    color: #64748b;
  }

  .cap-hdr__grow { flex: 1; }

  .cap-hdr__tools { display: flex; gap: 4px; }

  .cap-hdr__tool {
    position: relative;
    width: 40px;
    height: 40px;
    border-radius: 10px;
    display: grid;
    place-items: center;
    color: #475569;
    font-size: 16px;
    text-decoration: none;
  }

  .cap-hdr__tool:hover { background: #f1f5f9; color: #0b2530; }
  .cap-hdr__tool.is-on { background: #f1f5f9; color: #0b2530; }

  .cap-hdr__badge {
    position: absolute;
    top: 3px;
    right: 2px;
    min-width: 17px;
    height: 17px;
    padding: 0 4px;
    border-radius: 9px;
    background: #e11d48;
    color: #fff;
    font-size: 10px;
    font-weight: 700;
    display: grid;
    place-items: center;
    box-shadow: 0 0 0 2px #fff;
  }

  .cap-hdr__sep { width: 1px; height: 28px; background: #e2e8f0; }

  .cap-hdr__who-wrap { position: relative; }

  .cap-hdr__who {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 4px 8px 4px 4px;
    border: 0;
    border-radius: 12px;
    background: transparent;
    font-family: inherit;
    cursor: pointer;
    text-align: left;
  }

  .cap-hdr__who:hover { background: #f1f5f9; }

  .cap-hdr__ava {
    width: 38px;
    height: 38px;
    flex: none;
    border-radius: 50%;
    display: grid;
    place-items: center;
    background: linear-gradient(135deg, #334155, #0b2530);
    color: #fff;
    font-size: 13px;
    font-weight: 700;
    box-shadow: 0 0 0 2px #fff, 0 0 0 3.5px #e0b04a;
  }

  .cap-hdr__who-text b { display: block; font-size: 13.5px; color: #14232b; white-space: nowrap; }

  .cap-hdr__who-text small {
    display: flex;
    align-items: center;
    gap: 5px;
    margin-top: 2px;
    font-size: 11.5px;
    color: #64748b;
    white-space: nowrap;
  }

  .cap-hdr__who-text small i { font-size: 10.5px; color: #a87a14; }
  .cap-hdr__dot { margin: 0 3px; opacity: .5; }
  .cap-hdr__cv { font-size: 11px; color: #94a3b8; }

  .cap-hdr__user-menu {
    position: absolute;
    right: 0;
    top: 56px;
    width: 300px;
    padding: 16px;
    border-radius: 14px;
    background: #fff;
    box-shadow: 0 20px 50px rgba(15, 23, 42, .22), 0 0 0 1px #e6ebef;
    color: #14232b;
    z-index: 3;
  }

  .cap-hdr__um-head {
    display: flex;
    gap: 12px;
    align-items: center;
    padding-bottom: 14px;
    margin-bottom: 6px;
    border-bottom: 1px solid #eef2f4;
  }

  .cap-hdr__um-head .cap-hdr__ava { box-shadow: none; }
  .cap-hdr__um-head b { display: block; font-size: 14px; }
  .cap-hdr__um-head small { color: #64748b; font-size: 12px; }

  .cap-hdr__um-row { padding: 8px 0; }

  .cap-hdr__um-row label {
    display: block;
    margin-bottom: 4px;
    font-size: 10.5px;
    font-weight: 700;
    letter-spacing: .1em;
    text-transform: uppercase;
    color: #94a3b8;
  }

  .cap-hdr__um-row > span { display: flex; gap: 8px; align-items: center; font-size: 13.5px; }
  .cap-hdr__um-row > span i { color: #a87a14; font-size: 12px; }

  .cap-hdr__roles { display: flex; flex-wrap: wrap; gap: 6px; }

  .cap-hdr__roles span {
    padding: 3px 8px;
    border-radius: 6px;
    background: #f1f5f9;
    color: #334155;
    font-size: 11.5px;
    font-weight: 600;
  }

  .cap-hdr__muted { color: #94a3b8; font-size: 13px; }

  .cap-hdr__logout {
    width: 100%;
    margin-top: 10px;
    padding: 10px 4px;
    display: flex;
    gap: 10px;
    align-items: center;
    border: 0;
    border-top: 1px solid #eef2f4;
    background: transparent;
    font-family: inherit;
    font-size: 13.5px;
    color: #b42318;
    cursor: pointer;
  }

  .cap-hdr__shade {
    position: fixed;
    inset: 68px 0 0 0;
    background: rgba(15, 23, 42, .18);
    z-index: 1;
  }

  .cap-hdr__mega {
    position: absolute;
    left: 24px;
    right: 24px;
    top: 76px;
    max-width: 1320px;
    margin: 0 auto;
    padding: 22px 24px 18px;
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr)) minmax(0, 1.05fr);
    gap: 8px 26px;
    border-radius: 18px;
    background: #fff;
    box-shadow: 0 30px 70px rgba(15, 23, 42, .22), 0 0 0 1px #e6ebef;
    z-index: 2;
  }

  .cap-hdr__group h4 {
    margin: 0 0 10px;
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 12px;
    letter-spacing: .1em;
    text-transform: uppercase;
    color: #94a3b8;
  }

  .cap-hdr__group h4 i {
    width: 26px;
    height: 26px;
    border-radius: 8px;
    display: grid;
    place-items: center;
    background: #f1f5f9;
    color: #334155;
    font-size: 12px;
  }

  .cap-hdr__group a {
    display: flex;
    align-items: center;
    gap: 8px;
    margin: 0 -8px;
    padding: 7px 8px;
    border-radius: 8px;
    font-size: 13.5px;
    color: #1e293b;
    text-decoration: none;
  }

  .cap-hdr__group a:hover { background: #f6f3ea; }

  .cap-hdr__group.is-workspace {
    margin: -6px -8px -6px 0;
    padding: 16px;
    border-radius: 14px;
    background: #f8fafc;
  }

  .cap-hdr__group.is-workspace a { font-weight: 600; }
  .cap-hdr__group.is-workspace a i { width: 18px; color: #a87a14; font-size: 13px; }

  .cap-hdr__none { grid-column: 1 / -1; padding: 10px 0; color: #64748b; font-size: 14px; }

  .cap-hdr__mega-foot {
    grid-column: 1 / -1;
    display: flex;
    justify-content: space-between;
    margin-top: 8px;
    padding-top: 14px;
    border-top: 1px solid #eef2f4;
    font-size: 12.5px;
    color: #64748b;
  }

  @media (max-width: 1180px) {
    .cap-hdr__where { display: none; }
    .cap-hdr__mega { grid-template-columns: repeat(3, minmax(0, 1fr)); }
  }

  @media (max-width: 900px) {
    .cap-hdr__search, .cap-hdr__who-text, .cap-hdr__word { display: none; }
    .cap-hdr__mega { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  }
</style>
