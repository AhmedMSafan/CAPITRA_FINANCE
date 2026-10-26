<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import type { AdminRole } from '../models/AdminRole'
import type { AdminUser } from '../models/AdminUser'
import type { CreateAdminUserRequest } from '../models/CreateAdminUserRequest'
import type { UpdateAdminUserRequest } from '../models/UpdateAdminUserRequest'
import {
  createAdminUser,
  setAdminUserRoles,
  updateAdminUser
} from '../services/adminUserService'

const props = defineProps<{
  user: AdminUser | null
  roles: AdminRole[]
}>()

const emit = defineEmits<{
  close: []
  saved: []
}>()

const isSaving = ref(false)
const errorMessage = ref('')

const form = reactive({
  userName: '',
  password: '',
  email: '',
  fullName: '',
  employeeNo: '',
  department: '',
  jobTitle: '',
  isActive: true,
  roles: [] as string[]
})

const isEditMode = computed(() => !!props.user)

watch(
  () => props.user,
  user => {
    errorMessage.value = ''

    if (user) {
      form.userName = user.userName
      form.password = ''
      form.email = user.email ?? ''
      form.fullName = user.fullName ?? ''
      form.employeeNo = user.employeeNo ?? ''
      form.department = user.department ?? ''
      form.jobTitle = user.jobTitle ?? ''
      form.isActive = user.isActive
      form.roles = [...user.roles]
    } else {
      form.userName = ''
      form.password = ''
      form.email = ''
      form.fullName = ''
      form.employeeNo = ''
      form.department = ''
      form.jobTitle = ''
      form.isActive = true
      form.roles = []
    }
  },
  { immediate: true }
)

function closeDialog() {
  if (!isSaving.value)
    emit('close')
}

function toggleRole(roleName: string) {
  if (form.roles.includes(roleName)) {
    form.roles = form.roles.filter(x => x !== roleName)
  } else {
    form.roles = [...form.roles, roleName]
  }
}

async function saveUser() {
  if (isSaving.value)
    return

  errorMessage.value = ''

  if (!form.userName.trim()) {
    errorMessage.value = 'Username is required.'
    return
  }

  if (!isEditMode.value && !form.password) {
    errorMessage.value = 'Password is required.'
    return
  }

  isSaving.value = true

  try {
    if (props.user) {
      const updateRequest: UpdateAdminUserRequest = {
        email: form.email || null,
        fullName: form.fullName || null,
        employeeNo: form.employeeNo || null,
        department: form.department || null,
        jobTitle: form.jobTitle || null,
        isActive: form.isActive
      }

      await updateAdminUser(props.user.id, updateRequest)
      await setAdminUserRoles(props.user.id, form.roles)
    } else {
      const createRequest: CreateAdminUserRequest = {
        userName: form.userName.trim(),
        password: form.password,
        email: form.email || null,
        fullName: form.fullName || null,
        employeeNo: form.employeeNo || null,
        department: form.department || null,
        jobTitle: form.jobTitle || null,
        isActive: form.isActive,
        roles: form.roles
      }

      await createAdminUser(createRequest)
    }

    emit('saved')
    emit('close')
  } catch (error) {
    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to save user.'
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <Teleport to="body">
    <div class="finova-admin-dialog-backdrop" @click.self="closeDialog">
      <section class="finova-admin-dialog">
        <header class="finova-admin-dialog__header">
          <div>
            <div class="finova-admin-dialog__eyebrow">Administration</div>
            <h2>{{ isEditMode ? 'Edit User' : 'Create User' }}</h2>
          </div>

          <button
            type="button"
            class="finova-dialog-close"
            :disabled="isSaving"
            @click="closeDialog"
          >
            <i class="pi pi-times"></i>
          </button>
        </header>

        <div class="finova-admin-dialog__body">
          <div v-if="errorMessage" class="finova-admin-inline-error">
            <i class="pi pi-exclamation-circle"></i>
            <span>{{ errorMessage }}</span>
          </div>

          <div class="finova-admin-form-grid">
            <label class="finova-admin-field">
              <span>Username</span>
              <input
                v-model="form.userName"
                type="text"
                :disabled="isEditMode"
              />
            </label>

            <label v-if="!isEditMode" class="finova-admin-field">
              <span>Initial Password</span>
              <input v-model="form.password" type="password" />
            </label>

            <label class="finova-admin-field">
              <span>Full Name</span>
              <input v-model="form.fullName" type="text" />
            </label>

            <label class="finova-admin-field">
              <span>Email</span>
              <input v-model="form.email" type="email" />
            </label>

            <label class="finova-admin-field">
              <span>Employee No.</span>
              <input v-model="form.employeeNo" type="text" />
            </label>

            <label class="finova-admin-field">
              <span>Department</span>
              <input v-model="form.department" type="text" />
            </label>

            <label class="finova-admin-field">
              <span>Job Title</span>
              <input v-model="form.jobTitle" type="text" />
            </label>

            <label class="finova-admin-field finova-admin-field--toggle">
              <span>Status</span>
              <div class="finova-admin-toggle-row">
                <input v-model="form.isActive" type="checkbox" />
                <span>{{ form.isActive ? 'Active' : 'Inactive' }}</span>
              </div>
            </label>
          </div>

          <div class="finova-admin-role-section">
            <div class="finova-admin-role-section__title">
              <i class="pi pi-shield"></i>
              <span>Assigned Roles</span>
            </div>

            <div v-if="roles.length" class="finova-admin-role-grid">
              <label
                v-for="role in roles"
                :key="role.id"
                class="finova-admin-role-option"
              >
                <input
                  type="checkbox"
                  :checked="form.roles.includes(role.name)"
                  @change="toggleRole(role.name)"
                />
                <span>{{ role.name }}</span>
              </label>
            </div>

            <div v-else class="finova-admin-empty-mini">
              No roles have been created yet.
            </div>
          </div>
        </div>

        <footer class="finova-admin-dialog__footer">
          <button
            type="button"
            class="finova-action-button finova-action-secondary"
            :disabled="isSaving"
            @click="closeDialog"
          >
            <i class="pi pi-times"></i>
            <span>Cancel</span>
          </button>

          <button
            type="button"
            class="finova-action-button finova-action-update"
            :disabled="isSaving"
            @click="saveUser"
          >
            <i
              :class="isSaving ? 'pi pi-spin pi-spinner' : 'pi pi-save'"
            ></i>
            <span>{{ isSaving ? 'Saving...' : 'Save User' }}</span>
          </button>
        </footer>
      </section>
    </div>
  </Teleport>
</template>
