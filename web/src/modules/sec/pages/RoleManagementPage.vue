<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import type { AdminRole } from '../models/AdminRole'
  import { createAdminRole, getAdminRoles, getPermissionCatalog, setRolePermissions } from '../services/adminRoleService'
  import { getAdminUsers } from '../services/adminUserService'
  import { tr } from '@/shared/layout/language'
  import '@/shared/styles/apex-fx-inbox.css'
  import '../styles/admin.css'

  const roles = ref<AdminRole[]>([])
  const newRoleName = ref('')
  const isLoading = ref(false)
  const isSaving = ref(false)
  const errorMessage = ref('')

  // Capitra checks permissions, so each role also lists the permission codes it grants.
  const catalog = ref<string[]>([])
  const selectedRole = ref<AdminRole | null>(null)
  const selectedPermissions = ref<string[]>([])
  const savedMessage = ref('')

  const searchText = ref('')
  const showCreate = ref(false)
  // users per role, from the user list
  const userCounts = ref<Record<string, number>>({})

  const sortedRoles = computed(() => {
    const q = searchText.value.trim().toLowerCase()
    return [...roles.value]
      .filter(r => !q || `${r.name} ${r.description ?? ''}`.toLowerCase().includes(q))
      .sort((a, b) => a.name.localeCompare(b.name))
  })

  async function loadRoles() {
    isLoading.value = true
    errorMessage.value = ''

    try {
      const [loadedRoles, loadedCatalog, users] = await Promise.all([getAdminRoles(), getPermissionCatalog(), getAdminUsers()])
      roles.value = loadedRoles
      catalog.value = loadedCatalog
      const counts: Record<string, number> = {}
      for (const u of users) for (const r of u.roles) counts[r] = (counts[r] ?? 0) + 1
      userCounts.value = counts
      if (selectedRole.value)
        selectRole(loadedRoles.find(r => r.id === selectedRole.value?.id) ?? null)
    } catch (error) {
      errorMessage.value =
        error instanceof Error
          ? error.message
          : 'Unable to load roles.'
    } finally {
      isLoading.value = false
    }
  }

  async function addRole() {
    const roleName = newRoleName.value.trim()

    if (!roleName) {
      errorMessage.value = 'Role name is required.'
      return
    }

    isSaving.value = true
    errorMessage.value = ''

    try {
      await createAdminRole(roleName)
      newRoleName.value = ''
      showCreate.value = false
      await loadRoles()
    } catch (error) {
      errorMessage.value =
        error instanceof Error
          ? error.message
          : 'Unable to create role.'
    } finally {
      isSaving.value = false
    }
  }

  function selectRole(role: AdminRole | null) {
    selectedRole.value = role
    selectedPermissions.value = role ? [...role.permissions] : []
    if (role) savedMessage.value = ''
  }

  function openCreate() {
    newRoleName.value = ''
    errorMessage.value = ''
    showCreate.value = true
  }

  async function savePermissions() {
    if (!selectedRole.value)
      return

    isSaving.value = true
    errorMessage.value = ''
    savedMessage.value = ''

    try {
      const saved = await setRolePermissions(selectedRole.value.id, selectedPermissions.value)
      roles.value = roles.value.map(r => r.id === saved.id ? saved : r)
      selectRole(null)
      savedMessage.value = `Permissions for '${saved.name}' saved.`
    } catch (error) {
      errorMessage.value =
        error instanceof Error
          ? error.message
          : 'Unable to save permissions.'
    } finally {
      isSaving.value = false
    }
  }

  onMounted(loadRoles)
</script>

<template>
  <!-- The list in the layout of App 100 page 920 "My Inbox", as every admin list. -->
  <section class="fx roles-page">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Role Management') }}</h1>
        <p>{{ tr('Configure roles and application permissions.') }}</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="loadRoles"><i class="pi pi-refresh"></i>{{ tr('Refresh') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" @click="openCreate"><i class="pi pi-plus"></i>{{ tr('Add') }}</button>
      </div>
    </div>

    <div v-if="savedMessage" class="roles-msg roles-msg--ok">{{ savedMessage }}</div>
    <div v-if="errorMessage && !showCreate && !selectedRole" class="roles-msg roles-msg--bad">{{ errorMessage }}</div>

    <div class="fx-toolbar">
      <div class="fx-seg" role="group"><button type="button" aria-pressed="true">{{ tr('All') }}<em>{{ roles.length }}</em></button></div>
      <input v-model="searchText" class="fx-search" type="search" :placeholder="tr('Search')" :aria-label="tr('Search')" />
    </div>

    <div class="fx-card">
      <div v-if="isLoading" class="fx-empty"><i class="pi pi-spin pi-spinner"></i> {{ tr('Loading...') }}</div>
      <div v-else class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr><th>{{ tr('Role') }}</th><th>{{ tr('Permissions') }}</th><th class="fx-r">{{ tr('Users') }}</th><th></th></tr>
          </thead>
          <tbody>
            <tr v-for="role in sortedRoles" :key="role.id">
              <td><a href="#" @click.prevent="selectRole(role)">{{ role.name }}</a><span class="fx-cell-sub">{{ role.description }}</span></td>
              <td>
                <span class="fx-chip fx-chip--teal">{{ role.permissions.includes('*') ? tr('All permissions') : `${role.permissions.length} ${tr('permission(s)')}` }}</span>
              </td>
              <td class="fx-r">{{ userCounts[role.name] ?? 0 }}</td>
              <td class="fx-r"><button type="button" class="fx-btn fx-btn--sm" @click="selectRole(role)">{{ tr('Edit') }}</button></td>
            </tr>
          </tbody>
        </table>
        <div v-if="sortedRoles.length === 0" class="fx-empty">{{ tr('No records found.') }}</div>
      </div>
    </div>

    <!-- Add role -->
    <div v-if="showCreate" class="finova-admin-dialog-backdrop" @click.self="showCreate = false">
      <section class="finova-admin-dialog finova-admin-dialog--small">
        <header class="finova-admin-dialog__header">
          <h2>{{ tr('Add role') }}</h2>
          <button type="button" class="finova-dialog-close" @click="showCreate = false"><i class="pi pi-times"></i></button>
        </header>
        <div class="finova-admin-dialog__body">
          <div v-if="errorMessage" class="finova-admin-inline-error"><i class="pi pi-exclamation-circle"></i><span>{{ errorMessage }}</span></div>
          <label class="finova-admin-field">
            <span>{{ tr('Role name') }}</span>
            <input v-model="newRoleName" type="text" placeholder="CREDIT_APPROVER" @keyup.enter="addRole" />
          </label>
        </div>
        <footer class="finova-admin-dialog__footer">
          <span></span>
          <button type="button" class="finova-action-button finova-action-create" :disabled="isSaving" @click="addRole">
            <i :class="isSaving ? 'pi pi-spin pi-spinner' : 'pi pi-check'"></i><span>{{ tr('Create') }}</span>
          </button>
        </footer>
      </section>
    </div>

    <!-- Edit a role's permissions -->
    <div v-if="selectedRole" class="finova-admin-dialog-backdrop" @click.self="selectRole(null)">
      <section class="finova-admin-dialog">
        <header class="finova-admin-dialog__header">
          <div>
            <h2>{{ selectedRole.name }}</h2>
            <p>{{ tr('Permissions') }}</p>
          </div>
          <button type="button" class="finova-dialog-close" @click="selectRole(null)"><i class="pi pi-times"></i></button>
        </header>
        <div class="finova-admin-dialog__body">
          <div v-if="errorMessage" class="finova-admin-inline-error"><i class="pi pi-exclamation-circle"></i><span>{{ errorMessage }}</span></div>
          <div class="roles-permission-grid">
            <label v-for="code in catalog" :key="code" class="finova-admin-role-option">
              <input v-model="selectedPermissions" type="checkbox" :value="code" />
              <span>{{ code === '*' ? '* (all permissions)' : code }}</span>
            </label>
          </div>
        </div>
        <footer class="finova-admin-dialog__footer">
          <span></span>
          <button type="button" class="finova-action-button finova-action-update" :disabled="isSaving" @click="savePermissions">
            <i :class="isSaving ? 'pi pi-spin pi-spinner' : 'pi pi-check'"></i><span>{{ tr('Save Changes') }}</span>
          </button>
        </footer>
      </section>
    </div>
  </section>
</template>

<style scoped>
  .roles-page { font-family: Inter, "Segoe UI", Arial, sans-serif; }
  html.fnv-ar .roles-page { font-family: Cairo, "Segoe UI", Arial, sans-serif; }
  .fx .fx-btn .pi { font-size: 12px; }
  .roles-msg { margin-block-end: 12px; padding: 10px 14px; border-radius: var(--fin-radius-sm); font-size: var(--fs-sm); font-weight: 600; }
  .roles-msg--ok { background: var(--fin-ok-50); color: var(--fin-ok); }
  .roles-msg--bad { background: var(--fin-bad-50); color: var(--fin-bad); }
  .roles-permission-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(290px, 1fr)); gap: 8px; }
</style>
