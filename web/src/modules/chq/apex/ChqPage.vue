<script setup lang="ts">
// A page of App 122 (Cheques Wallet), rendered from its definition generated from the APEX export, in the app's
// Universal Theme shell: the header with the app logo, the Navigation Bar list (user with Sign Out, Digital Factoring
// Platform, Main Menu, FINOVA Home) and the Navigation Menu at the top as tabs (js-tabLike: Home).
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ApexWizardPage from '@/modules/apex/ApexWizardPage.vue'
import { appPageUrl } from '@/modules/apex/wizard'
import { getUser } from '@/shared/auth/session'
import { logout as endSession } from '@/modules/sec/api/auth.api'
import { wizardPages } from './wizard/wizardPages'

const props = defineProps<{ page: number }>()
const route = useRoute()
const router = useRouter()
const opened = ref(0)
const user = getUser()?.userName ?? ''
// the app's CSS File URLs (#APP_FILES#finova-theme.css)
const appCss = ['/apex/122/finova-theme.css']
const home = computed(() => appPageUrl(122, 1))
const userMenu = ref(false)

function go(event: MouseEvent, to: string) {
  if (event.ctrlKey || event.metaKey || event.shiftKey) return
  event.preventDefault()
  void router.push(to)
}
async function signOut() {
  endSession()
  await router.push({ name: 'login' })
}
</script>

<template>
  <component :is="'link'" v-for="href in appCss" :key="href" rel="stylesheet" :href="href" />
  <div class="chq-app t-PageBody t-PageBody--topNav">
    <header class="t-Header chq-header">
      <div class="t-Header-branding">
        <a class="chq-logo" :href="home" @click="go($event, home)">
          <div style="height: 58px; display: flex; align-items: center; background: #014b55; justify-content: flex-start; gap: 2px">
            <img src="/apex/122/bar4.png" alt="FINOVA" style="height: 100%; width: auto; object-fit: contain; display: block" />
            <span style="color: #ffffff; font-family: 'Cairo', sans-serif; font-size: 1rem; font-weight: 400; line-height: 1">Cheques Wallet</span>
          </div>
        </a>
        <ul class="t-Header-navBar chq-navbar">
          <li class="chq-user">
            <button type="button" class="t-Button t-Button--navBar" :aria-expanded="userMenu" @click="userMenu = !userMenu">
              <span class="fa fa-user" aria-hidden="true"></span><span class="t-Button-label">{{ user }}</span><span class="fa fa-angle-down" aria-hidden="true"></span>
            </button>
            <ul v-if="userMenu" class="chq-menu" @mouseleave="userMenu = false">
              <li><button type="button" @click="signOut"><span class="fa fa-sign-out" aria-hidden="true"></span> Sign Out</button></li>
            </ul>
          </li>
          <li>
            <span class="t-Button t-Button--navBar chq-off" title="Digital Factoring Platform"><span class="fa fa-bolt" aria-hidden="true"></span><span class="t-Button-label">Digital Factoring Platform</span></span>
          </li>
          <li>
            <a class="t-Button t-Button--navBar" :href="home" @click="go($event, home)"><span class="fa fa-home" aria-hidden="true"></span><span class="t-Button-label">Main Menu</span></a>
          </li>
          <li>
            <a class="t-Button t-Button--navBar" href="/" @click="go($event, '/')"><span class="fa fa-th-large" aria-hidden="true"></span><span class="t-Button-label">FINOVA Home</span></a>
          </li>
        </ul>
      </div>
      <nav class="t-Header-nav chq-tabs" aria-label="Main Navigation">
        <a class="chq-tab" :class="{ 'is-current': page === 1 }" :href="home" @click="go($event, home)"><span class="fa fa-home" aria-hidden="true"></span> Home</a>
      </nav>
    </header>
    <main class="t-Body-main chq-main">
      <ApexWizardPage :key="`${route.fullPath}#${opened}`" :app="122" :pages="wizardPages" :page="props.page" @reload="opened++" />
    </main>
  </div>
</template>

<style scoped>
.chq-app { min-height: calc(100vh - 64px); background: var(--bg, #eef3f4); }
.chq-header { position: sticky; top: 0; z-index: 20; background: #014b55; color: #fff; }
.t-Header-branding { display: flex; align-items: center; height: 58px; padding: 0 0 0 4px; }
.chq-logo { display: block; height: 58px; text-decoration: none; }
.chq-navbar { display: flex; align-items: center; margin: 0 8px 0 auto; padding: 0; list-style: none; }
.chq-navbar .t-Button--navBar { color: #fff; text-decoration: none; gap: 6px; font-size: 13px; cursor: pointer; }
.chq-navbar .chq-off { cursor: default; opacity: 0.8; }
.chq-user { position: relative; }
.chq-menu { position: absolute; inset-inline-end: 8px; top: 40px; margin: 0; padding: 4px; list-style: none; background: #fff; border-radius: 8px; box-shadow: 0 8px 24px rgba(1, 30, 36, 0.2); min-width: 140px; }
.chq-menu button { display: flex; align-items: center; gap: 8px; width: 100%; border: 0; background: transparent; padding: 8px 10px; color: #011e24; font-size: 13px; cursor: pointer; border-radius: 6px; }
.chq-menu button:hover { background: #e8f4f5; }
.chq-tabs { display: flex; gap: 2px; padding: 0 12px; background: #01696f; }
.chq-tab { color: #fff; padding: 10px 16px; font-size: 13px; text-decoration: none; border-bottom: 3px solid transparent; }
.chq-tab.is-current { border-bottom-color: #e87722; font-weight: 600; }
.chq-main { padding: 0 0 24px; }
</style>
