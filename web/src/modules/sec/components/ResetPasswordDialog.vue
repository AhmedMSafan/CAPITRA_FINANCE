<script setup lang="ts">
import { ref } from 'vue'
import type { AdminUser } from '../models/AdminUser'
import { resetAdminUserPassword } from '../services/adminUserService'

const props = defineProps<{
  user: AdminUser
}>()

const emit = defineEmits<{
  close: []
  saved: []
}>()

const newPassword = ref('')
const confirmPassword = ref('')
const isSaving = ref(false)
const errorMessage = ref('')

async function submitReset() {
  errorMessage.value = ''

  if (!newPassword.value) {
    errorMessage.value = 'New password is required.'
    return
  }

  if (newPassword.value !== confirmPassword.value) {
    errorMessage.value = 'Passwords do not match.'
    return
  }

  isSaving.value = true

  try {
    await resetAdminUserPassword(props.user.id, newPassword.value)
    emit('saved')
    emit('close')
  } catch (error) {
    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to reset password.'
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <Teleport to="body">
    <div class="finova-admin-dialog-backdrop" @click.self="emit('close')">
      <section class="finova-admin-dialog finova-admin-dialog--small">
        <header class="finova-admin-dialog__header">
          <div>
            <div class="finova-admin-dialog__eyebrow">Security</div>
            <h2>Reset Password</h2>
            <p>{{ user.fullName || user.userName }}</p>
          </div>

          <button
            type="button"
            class="finova-dialog-close"
            :disabled="isSaving"
            @click="emit('close')"
          >
            <i class="pi pi-times"></i>
          </button>
        </header>

        <div class="finova-admin-dialog__body">
          <div v-if="errorMessage" class="finova-admin-inline-error">
            <i class="pi pi-exclamation-circle"></i>
            <span>{{ errorMessage }}</span>
          </div>

          <label class="finova-admin-field">
            <span>New Password</span>
            <input v-model="newPassword" type="password" />
          </label>

          <label class="finova-admin-field">
            <span>Confirm Password</span>
            <input v-model="confirmPassword" type="password" />
          </label>
        </div>

        <footer class="finova-admin-dialog__footer">
          <button
            type="button"
            class="finova-action-button finova-action-secondary"
            :disabled="isSaving"
            @click="emit('close')"
          >
            <i class="pi pi-times"></i>
            <span>Cancel</span>
          </button>

          <button
            type="button"
            class="finova-action-button finova-action-update"
            :disabled="isSaving"
            @click="submitReset"
          >
            <i :class="isSaving ? 'pi pi-spin pi-spinner' : 'pi pi-key'"></i>
            <span>{{ isSaving ? 'Resetting...' : 'Reset Password' }}</span>
          </button>
        </footer>
      </section>
    </div>
  </Teleport>
</template>
