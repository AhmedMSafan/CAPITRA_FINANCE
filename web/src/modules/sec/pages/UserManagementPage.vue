<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import AdminUserDialog from '../components/AdminUserDialog.vue'
  import ResetPasswordDialog from '../components/ResetPasswordDialog.vue'
  import type { AdminRole } from '../models/AdminRole'
  import type { AdminUser } from '../models/AdminUser'
  import { getAdminRoles } from '../services/adminRoleService'
  import { getAdminUsers } from '../services/adminUserService'
  import '../styles/admin.css'

  const users = ref<AdminUser[]>([])
  const roles = ref<AdminRole[]>([])
  const searchText = ref('')
  const isLoading = ref(false)
  const errorMessage = ref('')

  const editingUser = ref<AdminUser | null>(null)
  const resetPasswordUser = ref<AdminUser | null>(null)
  const showUserDialog = ref(false)

  const filteredUsers = computed(() => {
    const q = searchText.value.trim().toLowerCase()

    if (!q)
      return users.value

    return users.value.filter(user => {
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
  <section class="finova-admin-page">

    <!-- UPDATED HEADER ONLY -->
    <div class="users-header-row">
      <div class="users-header">
        <div class="users-header-icon">
          <i class="pi pi-users"></i>
          <i class="pi pi-shield users-mini-icon"></i>
        </div>

        <div class="users-header-copy">
          <h1>User Management</h1>
          <p>Manage Capitra users, business roles, status and security access.</p>
        </div>
      </div>

      <button type="button"
              class="finova-action-button finova-action-create"
              @click="openCreateUser">
        <i class="pi pi-user-plus"></i>
        <span>Add User</span>
      </button>
    </div>

    <div class="users-header-space"></div>

    <div class="finova-admin-toolbar">
      <div class="finova-admin-search">
        <i class="pi pi-search"></i>
        <input v-model="searchText"
               type="text"
               placeholder="Search users..." />
      </div>

      <div class="finova-admin-toolbar__meta">
        {{ filteredUsers.length }} user(s)
      </div>
    </div>

    <div v-if="errorMessage" class="finova-admin-inline-error">
      <i class="pi pi-exclamation-circle"></i>
      <span>{{ errorMessage }}</span>
    </div>

    <div class="finova-admin-panel">
      <div class="finova-admin-panel__title">
        <div>
          <i class="pi pi-users"></i>
          <span>Capitra Users</span>
        </div>
      </div>

      <div v-if="isLoading" class="finova-admin-loading">
        <i class="pi pi-spin pi-spinner"></i>
        <span>Loading users...</span>
      </div>

      <div v-else class="finova-admin-table-wrap">
        <table class="finova-admin-table">
          <thead>
            <tr>
              <th>User</th>
              <th>Employee</th>
              <th>Department / Title</th>
              <th>Roles</th>
              <th>Status</th>
              <th class="finova-admin-table__actions">Actions</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="user in filteredUsers" :key="user.id">
              <td>
                <div class="finova-admin-user-cell">
                  <div class="finova-admin-avatar">
                    <i class="pi pi-user"></i>
                  </div>
                  <div>
                    <strong>{{ user.fullName || user.userName }}</strong>
                    <span>{{ user.userName }}</span>
                    <small>{{ user.email || '—' }}</small>
                  </div>
                </div>
              </td>

              <td>
                <strong>{{ user.employeeNo || '—' }}</strong>
              </td>

              <td>
                <div class="finova-admin-two-line">
                  <span>{{ user.department || '—' }}</span>
                  <small>{{ user.jobTitle || '—' }}</small>
                </div>
              </td>

              <td>
                <div v-if="user.roles.length" class="finova-admin-chip-list">
                  <span v-for="role in user.roles"
                        :key="role"
                        class="finova-admin-chip">
                    {{ role }}
                  </span>
                </div>
                <span v-else>—</span>
              </td>

              <td>
                <span class="finova-admin-status"
                      :class="user.isActive
                    ? 'finova-admin-status--active'
                    : 'finova-admin-status--inactive'">
                  <i :class="user.isActive
                      ? 'pi pi-check-circle'
                      : 'pi pi-ban'"></i>
                  {{ user.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>

              <td class="finova-admin-table__actions">
                <button type="button"
                        class="finova-admin-icon-button"
                        title="Edit user"
                        @click="openEditUser(user)">
                  <i class="pi pi-pencil"></i>
                </button>

                <button type="button"
                        class="finova-admin-icon-button"
                        title="Reset password"
                        @click="openPasswordReset(user)">
                  <i class="pi pi-key"></i>
                </button>
              </td>
            </tr>

            <tr v-if="filteredUsers.length === 0">
              <td colspan="6" class="finova-admin-empty">
                No users found.
              </td>
            </tr>
          </tbody>
        </table>
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
  /* ONLY THE HEADER STYLING ADDED/CHANGED */

  .users-header-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 20px;
  }

  .users-header {
    display: flex;
    align-items: center;
    gap: 15px;
  }

  .users-header-icon {
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

    .users-header-icon > .pi-users {
      font-size: 22px;
    }

  .users-mini-icon {
    position: absolute;
    right: 8px;
    bottom: 8px;
    font-size: 11px;
  }

  .users-header-copy h1 {
    margin: 0;
    color: #244b3a;
    font-size: 22px;
    line-height: 1.05;
    font-weight: 800;
  }

  .users-header-copy p {
    margin: 5px 0 0;
    color: #718579;
    font-size: 14px;
  }

  .users-header-space {
    height: 26px;
  }

  @media (max-width: 650px) {
    .users-header-row {
      align-items: flex-start;
      flex-direction: column;
    }

    .users-header-space {
      height: 24px;
    }
  }
</style>
