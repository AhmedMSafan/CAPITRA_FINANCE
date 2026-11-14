<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import AdminUserDialog from '../components/AdminUserDialog.vue'
  import ResetPasswordDialog from '../components/ResetPasswordDialog.vue'
  import type { AdminRole } from '../models/AdminRole'
  import type { AdminUser } from '../models/AdminUser'
  import { getAdminRoles } from '../services/adminRoleService'
  import { getAdminUsers } from '../services/adminUserService'
  import { tr } from '@/shared/layout/language'
  import '@/shared/styles/apex-fx-inbox.css'
  import '../styles/admin.css'

  const users = ref<AdminUser[]>([])
  const roles = ref<AdminRole[]>([])
  const searchText = ref('')
  const statusFilter = ref<'' | 'Y' | 'N'>('')

  const statusViews = computed(() => {
    const active = users.value.filter(u => u.isActive).length
    return [
      { key: '' as const, label: 'All', count: users.value.length },
      { key: 'Y' as const, label: 'Active', count: active },
      { key: 'N' as const, label: 'Inactive', count: users.value.length - active }
    ]
  })
  const isLoading = ref(false)
  const errorMessage = ref('')

  const editingUser = ref<AdminUser | null>(null)
  const resetPasswordUser = ref<AdminUser | null>(null)
  const showUserDialog = ref(false)

  const filteredUsers = computed(() => {
    const q = searchText.value.trim().toLowerCase()
    const byStatus = users.value.filter(u => !statusFilter.value || u.isActive === (statusFilter.value === 'Y'))

    if (!q)
      return byStatus

    return byStatus.filter(user => {
      const values = [
        user.userName,
        user.fullName,
        user.email,
        user.employeeNo,
        user.department,
        user.jobTitle,
        user.roles.join(' ')
      ]

      return values.some(value =>
        (value ?? '').toLowerCase().includes(q)
      )
    })
  })

  async function loadPage() {
    isLoading.value = true
    errorMessage.value = ''

    try {
      const [loadedUsers, loadedRoles] = await Promise.all([
        getAdminUsers(),
        getAdminRoles()
      ])

      users.value = loadedUsers
      roles.value = loadedRoles
    } catch (error) {
      errorMessage.value =
        error instanceof Error
          ? error.message
          : 'Unable to load user management data.'
    } finally {
      isLoading.value = false
    }
  }

  function openCreateUser() {
    editingUser.value = null
    showUserDialog.value = true
  }

  function openEditUser(user: AdminUser) {
    editingUser.value = user
    showUserDialog.value = true
  }

  function openPasswordReset(user: AdminUser) {
    resetPasswordUser.value = user
  }

  async function handleSaved() {
    await loadPage()
  }

  onMounted(loadPage)
</script>

<template>
  <!-- The list in the layout of App 100 page 920 "My Inbox", as every admin list. -->
  <section class="fx users-page">
    <div class="fx-head">
      <div>
        <h1>{{ tr('User Management') }}</h1>
        <p>{{ tr('Maintain system users and access configuration.') }}</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="loadPage"><i class="pi pi-refresh"></i>{{ tr('Refresh') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" @click="openCreateUser"><i class="pi pi-plus"></i>{{ tr('Add') }}</button>
      </div>
    </div>

    <div v-if="errorMessage" class="users-msg">{{ errorMessage }}</div>

    <div class="fx-toolbar">
      <div class="fx-seg" role="group" :aria-label="tr('Status')">
        <button v-for="v in statusViews" :key="v.key" type="button" :aria-pressed="statusFilter === v.key" @click="statusFilter = v.key">
          {{ tr(v.label) }}<em>{{ v.count }}</em>
        </button>
      </div>
      <input v-model="searchText" class="fx-search" type="search" :placeholder="tr('Search')" :aria-label="tr('Search')" />
    </div>

    <div class="fx-card">
      <div v-if="isLoading" class="fx-empty"><i class="pi pi-spin pi-spinner"></i> {{ tr('Loading...') }}</div>
      <div v-else class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('User') }}</th><th>{{ tr('Employee no.') }}</th><th>{{ tr('Department') }}</th><th>{{ tr('Roles') }}</th><th>{{ tr('Status') }}</th><th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in filteredUsers" :key="user.id">
              <td>
                <a href="#" @click.prevent="openEditUser(user)">{{ user.userName }}</a>
                <span class="fx-cell-sub">{{ user.fullName }}<template v-if="user.email"> &middot; {{ user.email }}</template></span>
              </td>
              <td>{{ user.employeeNo || '-' }}</td>
              <td>{{ user.department || '-' }}<span v-if="user.jobTitle" class="fx-cell-sub">{{ user.jobTitle }}</span></td>
              <td>
                <span class="users-roles">
                  <span v-for="role in user.roles" :key="role" class="fx-chip fx-chip--plain">{{ role }}</span>
                  <template v-if="!user.roles.length">-</template>
                </span>
              </td>
              <td><span class="fx-chip" :class="{ 'fx-chip--ok': user.isActive }">{{ tr(user.isActive ? 'Active' : 'Inactive') }}</span></td>
              <td class="fx-r">
                <button type="button" class="fx-btn fx-btn--sm" @click="openEditUser(user)">{{ tr('Edit') }}</button>
                <button type="button" class="fx-btn fx-btn--sm users-key" :title="tr('Reset password')" :aria-label="tr('Reset password')" @click="openPasswordReset(user)"><i class="pi pi-key"></i></button>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredUsers.length === 0" class="fx-empty">{{ tr('No records found.') }}</div>
      </div>
    </div>

    <AdminUserDialog v-if="showUserDialog"
                     :user="editingUser"
                     :roles="roles"
                     @close="showUserDialog = false"
                     @saved="handleSaved" />

    <ResetPasswordDialog v-if="resetPasswordUser"
                         :user="resetPasswordUser"
                         @close="resetPasswordUser = null"
                         @saved="handleSaved" />
  </section>
</template>

<style scoped>
  .users-page { font-family: Inter, "Segoe UI", Arial, sans-serif; }
  html.fnv-ar .users-page { font-family: Cairo, "Segoe UI", Arial, sans-serif; }
  .fx .fx-btn .pi { font-size: 12px; }
  .users-msg { margin-block-end: 12px; padding: 10px 14px; border-radius: var(--fin-radius-sm); font-size: var(--fs-sm); font-weight: 600; background: var(--fin-bad-50); color: var(--fin-bad); }
  .users-roles { display: flex; flex-wrap: wrap; gap: 4px; max-width: 520px; }
  .users-key { margin-inline-start: 6px; }
</style>
