<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import type { AdminRole } from '../models/AdminRole'
  import { createAdminRole, getAdminRoles, getPermissionCatalog, setRolePermissions } from '../services/adminRoleService'
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

  const sortedRoles = computed(() =>
    [...roles.value].sort((a, b) => a.name.localeCompare(b.name))
  )

  async function loadRoles() {
    isLoading.value = true
    errorMessage.value = ''

    try {
      const [loadedRoles, loadedCatalog] = await Promise.all([getAdminRoles(), getPermissionCatalog()])
      roles.value = loadedRoles
      catalog.value = loadedCatalog
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
    savedMessage.value = ''
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
      selectRole(saved)
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
  <section class="finova-admin-page">

    <!-- UNIFIED FINOVA HEADER -->
    <div class="roles-header">
      <div class="roles-header-icon">
        <i class="pi pi-shield"></i>
        <i class="pi pi-users roles-mini-icon"></i>
      </div>

      <div class="roles-header-copy">
        <h1>Role Management</h1>
        <p>
          Maintain business roles used by Capitra security and workflow routing.
        </p>
      </div>
    </div>

    <div class="roles-header-space"></div>

    <div v-if="errorMessage" class="finova-admin-inline-error">
      <i class="pi pi-exclamation-circle"></i>
      <span>{{ errorMessage }}</span>
    </div>

    <div class="finova-admin-panel">
      <div class="finova-admin-panel__title">
        <div>
          <i class="pi pi-shield"></i>
          <span>Business Roles</span>
        </div>
      </div>

      <div class="finova-admin-role-create">
        <input v-model="newRoleName"
               type="text"
               placeholder="Example: CREDIT_APPROVER"
               @keyup.enter="addRole" />

        <button type="button"
                class="finova-action-button finova-action-create"
                :disabled="isSaving"
                @click="addRole">
          <i :class="isSaving ? 'pi pi-spin pi-spinner' : 'pi pi-plus-circle'"></i>
          <span>{{ isSaving ? 'Creating...' : 'Create Role' }}</span>
        </button>
      </div>

      <div v-if="isLoading" class="finova-admin-loading">
        <i class="pi pi-spin pi-spinner"></i>
        <span>Loading roles...</span>
      </div>

      <div v-else class="finova-admin-role-cards">
        <div v-for="role in sortedRoles"
             :key="role.id"
             class="finova-admin-role-card"
             :class="{ 'roles-card--selected': selectedRole?.id === role.id }"
             role="button"
             tabindex="0"
             @click="selectRole(role)"
             @keyup.enter="selectRole(role)">
          <div class="finova-admin-role-card__icon">
            <i class="pi pi-shield"></i>
          </div>

          <div>
            <strong>{{ role.name }}</strong>
            <span>{{ role.permissions.length }} permission(s)</span>
          </div>
        </div>

        <div v-if="sortedRoles.length === 0" class="finova-admin-empty">
          No roles have been created yet.
        </div>
      </div>
    </div>

    <div v-if="selectedRole" class="finova-admin-panel roles-permissions">
      <div class="finova-admin-panel__title">
        <div>
          <i class="pi pi-lock"></i>
          <span>Permissions of {{ selectedRole.name }}</span>
        </div>
      </div>

      <div class="roles-permission-grid">
        <label v-for="code in catalog" :key="code" class="finova-admin-role-option">
          <input v-model="selectedPermissions" type="checkbox" :value="code" />
          <span>{{ code === '*' ? '* (all permissions)' : code }}</span>
        </label>
      </div>

      <div class="roles-permissions-footer">
        <span v-if="savedMessage" class="roles-saved">
          <i class="pi pi-check-circle"></i> {{ savedMessage }}
        </span>
        <button type="button"
                class="finova-action-button finova-action-update"
                :disabled="isSaving"
                @click="savePermissions">
          <i :class="isSaving ? 'pi pi-spin pi-spinner' : 'pi pi-save'"></i>
          <span>Save Permissions</span>
        </button>
      </div>
    </div>
  </section>
</template>

<style scoped>
  /* =========================================
   UNIFIED FINOVA HEADER
   ========================================= */

  .roles-header {
    display: flex;
    align-items: center;
    gap: 15px;
  }

  .roles-header-icon {
    position: relative;
    width: 54px;
    height: 54px;
    flex: 0 0 54px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 14px;
    background: #fff0df;
    color: #c47b2c;
  }

    .roles-header-icon > .pi-shield {
      font-size: 22px;
    }

  .roles-mini-icon {
    position: absolute;
    right: 8px;
    bottom: 8px;
    font-size: 11px;
  }

  .roles-header-copy h1 {
    margin: 0;
    color: #244b3a;
    font-size: 22px;
    line-height: 1.05;
    font-weight: 800;
  }

  .roles-header-copy p {
    margin: 5px 0 0;
    color: #718579;
    font-size: 14px;
  }

  .roles-header-space {
    height: 26px;
  }

  .finova-admin-role-card {
    cursor: pointer;
  }

  .roles-card--selected {
    outline: 2px solid #c47b2c;
  }

  .roles-permissions {
    margin-top: 20px;
  }

  .roles-permission-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    gap: 8px;
    padding: 14px 18px;
  }

  .roles-permissions-footer {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 14px;
    padding: 0 18px 16px;
  }

  .roles-saved {
    color: #2f7a4f;
    font-size: 14px;
  }

  @media (max-width: 650px) {
    .roles-header-space {
      height: 24px;
    }
  }
</style>
