<template>
  <!-- App 100 (FINOVA launcher) page 2 "Admin Setup Main", region "Admin Setup": the same markup and CSS,
       with the card opening this app's users and roles pages instead of APEX application 103. -->
  <div class="fm">
    <section class="fm-hero">
      <div class="fm-hero-l">
        <span class="fm-eyebrow"><i></i>{{ tr('CAPITRA · Admin Setup') }}</span>
        <h1 v-if="!ar">Admin <em>Setup</em></h1><h1 v-else>إعدادات <em>النظام</em></h1>
        <p>{{ tr('Platform administration for all CAPITRA applications. Choose a module to continue.') }}</p>
      </div>
      <div class="fm-kpis">
        <div><b>{{ activeUsers ?? '–' }}</b><span>{{ tr('Active users') }}</span></div>
        <div><b>{{ roles ?? '–' }}</b><span>{{ tr('Roles') }}</span></div>
        <div><b>{{ today }}</b><span>{{ tr('Today') }}</span></div>
      </div>
    </section>

    <div class="fm-sec"><h2>{{ tr('Administration modules') }}</h2></div>
    <div class="fm-grid fm-grid--2">
      <router-link to="/admin/users" class="fm-card fm-card--t2" :style="{ '--c': '#014B55', '--cl': '#E6F0F1' }">
        <div class="fm-top"><span class="fm-ico" v-html="icon(USERS)"></span><span class="fm-pill fm-pill--live">{{ tr('Live') }}</span></div>
        <div class="fm-body">
          <small>{{ tr('Access Control') }}</small><h3>{{ tr('Users and Roles') }}</h3>
          <p>{{ tr('Create users, give them roles and choose the pages each role can open in every CAPITRA application.') }}</p>
          <ul class="fm-feats"><li>{{ tr('User accounts') }}</li><li>{{ tr('Roles') }}</li><li>{{ tr('Role permissions') }}</li></ul>
        </div>
        <div class="fm-foot"><span class="fm-go">{{ tr('Open Access Control') }}<span v-html="icon(ARROW, 16)"></span></span></div>
      </router-link>
    </div>
    <router-link class="fm-back" to="/"><span v-html="icon(BACK, 16)"></span>{{ tr('All applications') }}</router-link>
  </div>
</template>

<script setup lang="ts">
import { language, tr } from '@/shared/layout/language'
import { onMounted, ref } from 'vue'
import { getAdminUsers } from '@/modules/sec/services/adminUserService'
import { getAdminRoles } from '@/modules/sec/services/adminRoleService'

const USERS = '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9"/><path d="M16 3.1a4 4 0 0 1 0 7.8"/>'
const ARROW = '<path d="M5 12h14M13 5l7 7-7 7"/>'
const BACK = '<path d="M19 12H5M11 19l-7-7 7-7"/>'
// the APEX page's svg() function
const icon = (paths: string, size = 24) =>
  `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${paths}</svg>`

const ar = language.value === 'AR'
const today = new Date().toLocaleDateString(ar ? 'ar-EG-u-nu-latn' : undefined, { day: 'numeric', month: 'short' })
const activeUsers = ref<number | null>(null)
const roles = ref<number | null>(null)

onMounted(() => {
  getAdminUsers().then((u) => { activeUsers.value = u.filter((x) => x.isActive).length }).catch(() => {})
  getAdminRoles().then((r) => { roles.value = r.length }).catch(() => {})
})
</script>
