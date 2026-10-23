<script setup lang="ts">
  import { onMounted, onUnmounted, ref } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { login } from '@/modules/sec/api/auth.api'
  import capitraLogo from '@/assets/capitra-mark.svg'

  const router = useRouter()
  const route = useRoute()

  const userName = ref('')
  const password = ref('')
  const errorMessage = ref('')
  const loading = ref(false)
  const showPassword = ref(false)

  const splashVisible = ref(true)
  const loginVisible = ref(false)

  let splashTimer: number | undefined
  let loginTimer: number | undefined

  async function submitLogin() {
    errorMessage.value = ''

    if (!userName.value.trim() || !password.value) {
      errorMessage.value = 'Enter your username and password.'
      return
    }

    try {
      loading.value = true

      await login(
        userName.value.trim(),
        password.value
      )

      const redirect = typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/') && !route.query.redirect.startsWith('//')
        ? route.query.redirect
        : '/'
      await router.push(redirect)
    } catch (error) {
      errorMessage.value =
        error instanceof Error
          ? error.message
          : 'Login failed.'
    } finally {
      loading.value = false
    }
  }

  onMounted(() => {
    // Show splash first, then fade it away.
    splashTimer = window.setTimeout(() => {
      splashVisible.value = false

      // Reveal login page gradually after splash starts fading.
      loginTimer = window.setTimeout(() => {
        loginVisible.value = true
      }, 450)
    }, 2800)
  })

  onUnmounted(() => {
    if (splashTimer) window.clearTimeout(splashTimer)
    if (loginTimer) window.clearTimeout(loginTimer)
  })
</script>

<template>
  <main class="capitra-login-page">

    <!-- Splash screen -->
    <section class="capitra-splash"
             :class="{ 'capitra-splash-hide': !splashVisible }">
      <div class="splash-brand">
        <img :src="capitraLogo"
             class="splash-logo"
             alt="CAPITRA" />

        <div class="splash-name">CAPITRA</div>

        <div class="splash-subtitle">
          FINANCING PLATFORM
        </div>
      </div>
    </section>

    <!-- Login page -->
    <section class="capitra-login-layer"
             :class="{ 'capitra-login-layer-visible': loginVisible }">
      <div class="capitra-corner-brand">
        <img :src="capitraLogo"
             class="corner-logo"
             alt="CAPITRA" />

        <div class="corner-copy">
          <strong>CAPITRA</strong>
          <span>FINANCING PLATFORM</span>
        </div>
      </div>

      <div class="capitra-login-shell">
        <form class="capitra-login-card"
              @submit.prevent="submitLogin">
          <div class="capitra-login-heading">
            <h1>Welcome</h1>
            <p>Sign in to continue to CAPITRA</p>
          </div>

          <div v-if="errorMessage"
               class="capitra-login-error">
            <i class="pi pi-exclamation-circle"></i>
            <span>{{ errorMessage }}</span>
          </div>

          <div class="capitra-login-field">
            <label for="capitra-login-user">
              Username
            </label>

            <div class="capitra-login-input">
              <span class="capitra-input-icon">
                <i class="pi pi-user"></i>
              </span>

              <input id="capitra-login-user"
                     v-model="userName"
                     type="text"
                     autocomplete="username"
                     placeholder="Enter username" />
            </div>
          </div>

          <div class="capitra-login-field">
            <label for="capitra-login-password">
              Password
            </label>

            <div class="capitra-login-input">
              <span class="capitra-input-icon">
                <i class="pi pi-lock"></i>
              </span>

              <input id="capitra-login-password"
                     v-model="password"
                     :type="showPassword ? 'text' : 'password'"
                     autocomplete="current-password"
                     placeholder="Enter password" />

              <button class="capitra-password-toggle"
                      type="button"
                      :title="showPassword ? 'Hide password' : 'Show password'"
                      @click="showPassword = !showPassword">
                <i :class="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
              </button>
            </div>
          </div>

          <div class="capitra-login-help">
            <span>
              <i class="pi pi-lock"></i>
              Protected CAPITRA access
            </span>
          </div>

          <button class="capitra-login-button"
                  type="submit"
                  :disabled="loading">
            <i :class="loading
                ? 'pi pi-spin pi-spinner'
                : 'pi pi-sign-in'"></i>

            <span>
              {{ loading ? 'Signing in...' : 'Sign in' }}
            </span>
          </button>
        </form>

        <div class="capitra-login-footer">
          CAPITRA Financing Platform
        </div>
      </div>
    </section>
  </main>
</template>

<style scoped>
  .capitra-login-page {
    position: relative;
    min-height: 100vh;
    overflow: hidden;
    background: #ffffff;
    font-family: Inter, "Segoe UI", Arial, sans-serif;
    color: #173b61;
  }

  /* =========================================================
   SPLASH
   ========================================================= */

  .capitra-splash {
    position: fixed;
    inset: 0;
    z-index: 20;
    display: grid;
    place-items: center;
    background: #ffffff;
    opacity: 1;
    visibility: visible;
    transition: opacity 1.2s ease, visibility 1.2s ease;
  }

  .capitra-splash-hide {
    opacity: 0;
    visibility: hidden;
    pointer-events: none;
  }

  .splash-brand {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
  }

  .splash-logo {
    width: 180px;
    height: 180px;
    object-fit: contain;
    display: block;
    background: transparent;
    border: 0;
    box-shadow: none;
  }

  .splash-name {
    margin-top: -8px;
    color: #0b4fc4;
    font-size: clamp(52px, 6vw, 78px);
    line-height: 1;
    font-weight: 800;
    letter-spacing: .08em;
  }

  .splash-subtitle {
    margin-top: 16px;
    color: #90a3b4;
    font-size: 13px;
    font-weight: 600;
    letter-spacing: .42em;
    white-space: nowrap;
    background: transparent;
  }

  /* =========================================================
   LOGIN LAYER
   ========================================================= */

  .capitra-login-layer {
    position: absolute;
    inset: 0;
    min-height: 100vh;
    display: grid;
    place-items: center;
    padding: 34px;
    background: #ffffff;
    opacity: 0;
    pointer-events: none;
    transition: opacity 1.1s ease;
  }

  .capitra-login-layer-visible {
    opacity: 1;
    pointer-events: auto;
  }

  /* Final brand - fixed top-left, no movement animation */
  .capitra-corner-brand {
    position: absolute;
    top: 28px;
    left: 34px;
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .corner-logo {
    width: 54px;
    height: 54px;
    object-fit: contain;
  }

  .corner-copy {
    display: flex;
    flex-direction: column;
    line-height: 1;
  }

    .corner-copy strong {
      color: #0b4fc4;
      font-size: 22px;
      font-weight: 800;
      letter-spacing: .06em;
    }

    .corner-copy span {
      margin-top: 7px;
      color: #8da0b0;
      font-size: 7px;
      font-weight: 600;
      letter-spacing: .24em;
      white-space: nowrap;
    }

  /* =========================================================
   LOGIN CARD
   ========================================================= */

  .capitra-login-shell {
    width: 100%;
    max-width: 460px;
  }

  .capitra-login-card {
    padding: 38px 38px 34px;
    display: flex;
    flex-direction: column;
    justify-content: flex-start;
    background:
      linear-gradient(180deg, rgba(249,252,255,.98), rgba(255,255,255,1));
    border: 1px solid #d9e6f0;
    border-radius: 22px;
    box-shadow:
      0 24px 70px rgba(27, 74, 119, .10),
      0 3px 10px rgba(27, 74, 119, .04);
  }

  .capitra-login-heading {
    margin-bottom: 28px;
    text-align: center;
  }


    .capitra-login-heading h1 {
      margin: 0 0 7px;
      color: #123c64;
      font-family: "Palatino Linotype", "Book Antiqua", Palatino, Georgia, serif;
      font-size: 32px;
      line-height: 1.05;
      font-weight: 700;
      letter-spacing: -.03em;
    }

    .capitra-login-heading p {
      margin: 0;
      color: #7890a4;
      font-size: 13px;
    }

  .capitra-login-error {
    display: flex;
    align-items: flex-start;
    gap: 9px;
    margin-bottom: 18px;
    padding: 11px 13px;
    border: 1px solid #fecaca;
    border-radius: 10px;
    background: #fff5f5;
    color: #b91c1c;
    font-size: 12px;
    line-height: 1.4;
  }

  .capitra-login-field {
    margin-bottom: 18px;
  }

    .capitra-login-field label {
      display: block;
      margin-bottom: 8px;
      color: #244a6c;
      font-size: 12px;
      font-weight: 800;
    }

  .capitra-login-input {
    min-height: 52px;
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 5px 10px 5px 7px;
    border: 1px solid #cfdeea;
    border-radius: 11px;
    background: #ffffff;
    transition:
      border-color .18s ease,
      box-shadow .18s ease,
      background .18s ease;
  }

    .capitra-login-input:focus-within {
      border-color: #2b78d0;
      background: #fbfdff;
      box-shadow: 0 0 0 4px rgba(43, 120, 208, .08);
    }

  .capitra-input-icon {
    width: 39px;
    height: 39px;
    flex: 0 0 39px;
    display: grid;
    place-items: center;
    border-radius: 9px;
    background: #eef5ff;
    color: #2d70bd;
  }

    .capitra-input-icon i {
      font-size: 15px;
    }

    .capitra-login-input input {
      width: 100%;
      min-width: 0;
      border: 0;
      outline: 0;
      background: #ffffff !important;
      color: #173b61;
      font-size: 13px;
      font-weight: 500;
      box-shadow: 0 0 0 1000px #ffffff inset !important;
      -webkit-box-shadow: 0 0 0 1000px #ffffff inset !important;
      -webkit-text-fill-color: #173b61 !important;
    }

    .capitra-login-input input:-webkit-autofill,
    .capitra-login-input input:-webkit-autofill:hover,
    .capitra-login-input input:-webkit-autofill:focus,
    .capitra-login-input input:-webkit-autofill:active {
      -webkit-box-shadow: 0 0 0 1000px #ffffff inset !important;
      box-shadow: 0 0 0 1000px #ffffff inset !important;
      -webkit-text-fill-color: #173b61 !important;
      caret-color: #173b61;
      transition: background-color 9999s ease-out 0s;
    }

      .capitra-login-input input::placeholder {
        color: #9aabba;
        font-weight: 400;
      }

  .capitra-password-toggle {
    width: 34px;
    height: 34px;
    flex: 0 0 34px;
    border: 0;
    border-radius: 8px;
    display: grid;
    place-items: center;
    background: transparent;
    color: #7790a6;
    cursor: pointer;
    transition: background .15s ease, color .15s ease;
  }

    .capitra-password-toggle:hover {
      background: #f0f5f9;
      color: #275f9b;
    }

  .capitra-login-help {
    display: flex;
    justify-content: flex-end;
    align-items: center;
    min-height: 20px;
    margin-top: -2px;
    margin-bottom: 18px;
    color: #8a9baa;
    font-size: 10px;
  }

    .capitra-login-help span {
      display: inline-flex;
      align-items: center;
      gap: 5px;
    }

  .capitra-login-button {
    width: 100%;
    height: 46px;
    border: 0;
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 9px;
    cursor: pointer;
    background: linear-gradient(180deg, #1555aa, #0b3f91);
    color: #ffffff;
    font-size: 13px;
    font-weight: 800;
    box-shadow: 0 10px 22px rgba(11, 63, 145, .20);
    transition:
      background .16s ease,
      transform .16s ease,
      box-shadow .16s ease;
  }

    .capitra-login-button:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 13px 26px rgba(8, 47, 111, .24);
    }

    .capitra-login-button:active:not(:disabled) {
      transform: translateY(0);
    }

    .capitra-login-button:disabled {
      opacity: .65;
      cursor: default;
    }

  .capitra-login-footer {
    margin-top: 16px;
    text-align: center;
    color: #8799a7;
    font-size: 11px;
    letter-spacing: .01em;
  }

  /* =========================================================
   RESPONSIVE
   ========================================================= */

  @media (max-width: 600px) {
    .capitra-login-layer {
      padding: 20px;
    }

    .capitra-corner-brand {
      top: 18px;
      left: 20px;
    }

    .corner-logo {
      width: 44px;
      height: 44px;
    }

    .corner-copy strong {
      font-size: 18px;
    }

    .capitra-login-card {
      padding: 30px 24px 28px;
      border-radius: 18px;
    }

    .splash-logo {
      width: 145px;
      height: 145px;
    }

    .splash-subtitle {
      letter-spacing: .28em;
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .capitra-splash,
    .capitra-login-layer {
      transition-duration: .01ms;
    }
  }
</style>
