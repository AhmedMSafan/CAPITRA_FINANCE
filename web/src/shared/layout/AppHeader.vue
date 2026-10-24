<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import { useRouter } from 'vue-router'
  import capitraLogo from '@/assets/capitra-mark.svg'
  import { getUser, type SessionUser } from '@/shared/auth/session'
  import { formatDate } from '@/shared/format/datetime'
  import { logout as endSession, refreshCurrentUser } from '@/modules/sec/api/auth.api'

  defineProps<{
    moduleName: string
    functionName: string
  }>()

  const router = useRouter()
  const menuOpen = ref(false)
  const user = ref<SessionUser | null>(getUser())
  const today = formatDate(new Date())

  const displayName = computed(() =>
    user.value?.fullName?.trim() ||
    user.value?.userName?.trim() ||
    'CAPITRA User'
  )

  const employeeNo = computed(() => user.value?.employeeNo?.trim() || '-')
  const department = computed(() => user.value?.department?.trim() || '-')
  const roles = computed(() => user.value?.roles ?? [])

  function goHome() {
    router.push({ name: 'home' })
  }

  async function logout() {
    endSession()
    menuOpen.value = false
    await router.push({ name: 'login' })
  }

  onMounted(async () => {
    try {
      user.value = await refreshCurrentUser()
    } catch (error) {
      console.error('Unable to load the current user profile.', error)
    }
  })
</script>

<template>
  <header class="capitra-app-header">
    <div class="capitra-app-brand">
      <div class="capitra-logo-wrap">
        <img
          :src="capitraLogo"
          alt="CAPITRA"
          class="capitra-header-logo"
        />
      </div>

      <div class="capitra-brand-text">
        <span class="capitra-brand-name">CAPITRA</span>
        <span class="capitra-brand-divider"></span>
        <span class="capitra-module-name">{{ moduleName }}</span>
        <span class="capitra-function-name">{{ functionName }}</span>
      </div>
    </div>

    <div class="capitra-header-actions">
      <nav class="capitra-header-shortcuts" aria-label="CAPITRA shortcuts">

        <button type="button"
                class="capitra-shortcut capitra-shortcut--home"
                title="CAPITRA Home" aria-label="CAPITRA Home"
                @click="goHome">
          <span class="capitra-shortcut-icon">
            <i class="pi pi-home"></i>
          </span>
        </button>
      </nav>

      <div class="capitra-header-pill capitra-date-pill">
        {{ today }}
      </div>

      <div class="capitra-user-area">
        <button type="button"
                class="capitra-user-button capitra-name-button"
                @click="menuOpen = !menuOpen">
          <i class="pi pi-user"></i>
          <span>{{ displayName }}</span>
          <i class="pi"
             :class="menuOpen ? 'pi-chevron-up' : 'pi-chevron-down'"></i>
        </button>

        <div v-if="menuOpen" class="capitra-user-menu">
          <div class="capitra-user-menu-name">
            {{ displayName }}
          </div>

          <div class="capitra-user-menu-login">
            {{ user?.userName ?? '' }}
          </div>

          <div class="capitra-user-menu-section">
            <div class="capitra-user-menu-section-title">
              Assigned Roles
            </div>

            <div v-if="roles.length"
                 class="capitra-role-list">
              <div v-for="role in roles"
                   :key="role"
                   class="capitra-role-item">
                <i class="pi pi-shield"></i>
                <span>{{ role }}</span>
              </div>
            </div>

            <div v-else
                 class="capitra-role-empty">
              No assigned roles
            </div>
          </div>

          <button type="button"
                  class="capitra-logout-button"
                  @click="logout">
            <i class="pi pi-sign-out"></i>
            Logout
          </button>
        </div>
      </div>

      <div class="capitra-user-info-tag employee-tag">
        <span class="capitra-tag-label">Employee</span>
        <strong>{{ employeeNo }}</strong>
      </div>

      <div class="capitra-user-info-tag department-tag">
        <span class="capitra-tag-label">Department</span>
        <strong>{{ department }}</strong>
      </div>
    </div>
  </header>
</template>

<style scoped>
  .capitra-app-header {
    position: relative;
    z-index: 3000;
    min-height: 64px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 18px;
    padding: 0 22px;
    box-sizing: border-box;
    color: #ffffff;
    background:
      radial-gradient(circle at 18% -40%, rgba(48, 112, 190, .18), transparent 42%),
      linear-gradient(110deg, var(--cap-navy) 0%, var(--cap-primary-dark) 52%, var(--cap-navy) 100%);
    border-bottom: 0;
    box-shadow: 0 8px 22px rgba(3, 24, 55, .22);
  }

  .capitra-app-brand {
    min-width: 320px;
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    gap: 16px;
    margin-right: 24px;
  }

  .capitra-logo-wrap {
    width: 58px;
    height: 54px;
    flex: 0 0 58px;
    display: grid;
    place-items: center;
    border: 0;
    border-radius: 0;
    background: transparent;
    box-shadow: none;
  }

  .capitra-header-logo {
    width: 44px;
    height: 44px;
    display: block;
    overflow: visible;
    filter: drop-shadow(0 2px 3px rgba(0, 0, 0, .16));
  }

  .capitra-logo-main {
    fill: var(--cap-accent);
  }

  .capitra-logo-dark {
    fill: var(--cap-accent-dark);
  }

  .capitra-brand-text {
    min-width: 0;
    display: flex;
    align-items: center;
    gap: 9px;
    white-space: nowrap;
  }

  .capitra-brand-name {
    color: #ffffff;
    font-size: 19px;
    font-weight: 900;
    letter-spacing: .055em;
  }

  .capitra-brand-divider {
    width: 1px;
    height: 24px;
    display: inline-block;
    background: rgba(255, 255, 255, .30);
  }

  .capitra-module-name {
    color: var(--cap-primary-tint);
    font-size: 12px;
    font-weight: 800;
    letter-spacing: .025em;
  }

  .capitra-function-name {
    max-width: 260px;
    overflow: hidden;
    text-overflow: ellipsis;
    color: var(--cap-primary-tint-2);
    font-size: 11px;
    font-weight: 600;
  }

  .capitra-header-actions {
    min-width: 0;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 9px;
  }

  .capitra-header-pill,
  .capitra-user-button,
  .capitra-user-info-tag {
    height: 38px;
    box-sizing: border-box;
    display: inline-flex;
    align-items: center;
    border: 1px solid rgba(255, 255, 255, .18);
    border-radius: 20px;
    background: rgba(255, 255, 255, .08);
    color: #ffffff;
    backdrop-filter: blur(5px);
  }

  .capitra-header-pill {
    gap: 7px;
    padding: 0 13px;
    color: var(--cap-primary-tint);
    font-size: 11px;
    font-weight: 700;
  }

  .capitra-header-pill:first-child {
    background: rgba(255, 255, 255, .11);
  }

  .capitra-header-pill i {
    color: var(--cap-accent);
  }

  .capitra-user-area {
    position: relative;
  }

  .capitra-name-button {
    min-width: 0;
    padding: 0 13px;
    gap: 8px;
    cursor: pointer;
    font-size: 12px;
    font-weight: 800;
    transition:
      background .16s ease,
      border-color .16s ease;
  }

  .capitra-name-button:hover {
    background: rgba(255, 255, 255, .15);
    border-color: rgba(255, 255, 255, .30);
  }

  .capitra-name-button > .pi-user {
    width: 27px;
    height: 27px;
    display: grid;
    place-items: center;
    border-radius: 50%;
    background: linear-gradient(145deg, var(--cap-primary), var(--cap-primary));
    box-shadow: inset 0 0 0 1px rgba(255,255,255,.16);
  }

  .capitra-user-info-tag {
    gap: 7px;
    padding: 0 13px;
    white-space: nowrap;
    font-size: 11px;
    font-weight: 700;
  }

  .employee-tag {
    background: rgba(255, 255, 255, .09);
  }

  .department-tag {
    background: rgba(230, 123, 30, .14);
    border-color: rgba(255, 173, 83, .28);
  }

  .capitra-tag-label {
    color: var(--cap-primary-tint-2);
    font-size: 9px;
    font-weight: 700;
    letter-spacing: .04em;
    text-transform: uppercase;
  }

  .department-tag .capitra-tag-label {
    color: var(--cap-warning-border);
  }

  .capitra-user-menu {
    position: absolute;
    top: calc(100% + 10px);
    right: 0;
    z-index: 5000;
    width: 286px;
    overflow: hidden;
    border: 1px solid var(--cap-border);
    border-radius: 13px;
    background: #ffffff;
    color: var(--cap-ink);
    box-shadow: 0 18px 42px rgba(13, 53, 100, .20);
  }

  .capitra-user-menu-name {
    padding: 15px 16px 2px;
    color: var(--cap-primary-dark);
    font-weight: 900;
  }

  .capitra-user-menu-login {
    padding: 0 16px 13px;
    color: var(--cap-muted);
    font-size: 11px;
    border-bottom: 1px solid var(--cap-border-soft);
  }

  .capitra-user-menu-section {
    padding: 12px 16px;
    border-bottom: 1px solid var(--cap-border-soft);
  }

  .capitra-user-menu-section-title {
    margin-bottom: 8px;
    color: var(--cap-muted);
    font-size: 9px;
    font-weight: 900;
    letter-spacing: .07em;
    text-transform: uppercase;
  }

  .capitra-role-list {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .capitra-role-item {
    display: flex;
    align-items: center;
    gap: 7px;
    padding: 8px 9px;
    border-radius: 8px;
    background: var(--cap-primary-tint);
    color: var(--cap-primary);
    font-size: 11px;
    font-weight: 800;
  }

  .capitra-role-item i {
    color: var(--cap-accent-dark);
    font-size: 11px;
  }

  .capitra-role-empty {
    color: var(--cap-muted);
    font-size: 11px;
  }

  .capitra-logout-button {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 9px;
    padding: 13px 16px;
    border: 0;
    background: #ffffff;
    color: var(--cap-danger);
    cursor: pointer;
    font-weight: 800;
    text-align: left;
  }

  .capitra-logout-button:hover {
    background: var(--cap-danger-bg);
  }

  @media (max-width: 1180px) {
    .capitra-header-pill {
      display: none;
    }

    .capitra-function-name {
      display: none;
    }
  }

  @media (max-width: 900px) {
    .capitra-app-header {
      padding: 0 14px;
    }

    .employee-tag,
    .department-tag {
      display: none;
    }

    .capitra-module-name {
      display: none;
    }

    .capitra-brand-divider {
      display: none;
    }
  }

  @media (max-width: 620px) {
    .capitra-name-button span {
      display: none;
    }

    .capitra-name-button {
      width: 40px;
      padding: 0;
      justify-content: center;
    }

    .capitra-name-button > .pi-user {
      width: 28px;
      height: 28px;
    }

    .capitra-name-button > .pi-chevron-up,
    .capitra-name-button > .pi-chevron-down {
      display: none;
    }
  }

  .capitra-header-shortcuts {
    display: flex;
    align-items: center;
    gap: 11px;
    margin-right: 5px;
  }

  .capitra-shortcut {
    width: 66px;
    min-width: 66px;
    height: 66px;
    padding: 2px;
    border: 0;
    border-radius: 12px;
    background: transparent;
    color: #fff;
    cursor: pointer;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 4px;
    font-family: inherit;
    transition: transform .15s ease, background .15s ease;
  }

  .capitra-shortcut:hover {
    transform: translateY(-1px);
    background: rgba(255,255,255,.06);
  }

  .capitra-shortcut-icon {
    width: 44px;
    height: 44px;
    border-radius: 11px;
    display: grid;
    place-items: center;
    border: 1px solid rgba(255,255,255,.11);
    box-shadow:
      inset 0 1px 0 rgba(255,255,255,.10),
      0 4px 10px rgba(0,0,0,.10);
  }

  .capitra-shortcut-icon i {
    font-size: 22px;
    font-weight: 700;
  }

  .capitra-shortcut-label {
    font-size: 11px;
    line-height: 1;
    font-weight: 800;
    color: rgba(255,255,255,.97);
    white-space: nowrap;
  }

  .capitra-shortcut--admin .capitra-shortcut-icon {
    color: #7af0a5;
    background: rgba(31,95,209,.22);
  }

  .capitra-shortcut--tools .capitra-shortcut-icon {
    color: var(--cap-accent);
    background: rgba(210,100,20,.20);
  }

  .capitra-shortcut--platform .capitra-shortcut-icon {
    color: #71c9f7;
    background: rgba(58,146,194,.20);
  }

  .capitra-shortcut--home .capitra-shortcut-icon {
    color: #ffffff;
    background: rgba(255,255,255,.11);
  }

  .capitra-shortcut--home {
    border-left: 0 !important;
    border-right: 0 !important;
  }

  .capitra-date-pill {
    min-width: 96px;
    justify-content: center;
  }

  @media (max-width: 1500px) {
    .capitra-app-brand {
      min-width: 290px;
      margin-right: 16px;
    }

    .capitra-header-shortcuts {
      gap: 7px;
    }

    .capitra-shortcut {
      width: 58px;
      min-width: 58px;
      height: 62px;
    }

    .capitra-shortcut-icon {
      width: 40px;
      height: 40px;
    }

    .capitra-shortcut-icon i {
      font-size: 20px;
    }

    .capitra-shortcut-label {
      font-size: 10px;
    }
  }


  /* FINAL shortcut sizing: icon-only, no text labels underneath. */
  .capitra-header-shortcuts {
    gap: 10px !important;
  }

  .capitra-shortcut {
    width: 50px !important;
    min-width: 50px !important;
    height: 50px !important;
    padding: 0 !important;
    gap: 0 !important;
  }

  .capitra-shortcut-icon {
    width: 42px !important;
    height: 42px !important;
    border-radius: 11px !important;
  }

  .capitra-shortcut-icon i {
    font-size: 22px !important;
  }

  .capitra-shortcut-label {
    display: none !important;
  }

  /* Correct transparent CAPITRA logo: no tile, border or background. */
  .capitra-logo-wrap {
    width: 62px !important;
    height: 54px !important;
    flex: 0 0 62px !important;
    display: grid !important;
    place-items: center !important;
    border: 0 !important;
    border-radius: 0 !important;
    background: transparent !important;
    box-shadow: none !important;
    overflow: visible !important;
  }

  .capitra-header-logo {
    width: 58px !important;
    height: 50px !important;
    object-fit: contain !important;
    display: block !important;
    filter: none !important;
    background: transparent !important;
    border: 0 !important;
  }

</style>
