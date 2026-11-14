import './shared/styles/capitra-theme.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import { definePreset } from '@primeuix/themes'
import 'primeicons/primeicons.css'

// PrimeVue components follow the Capitra Navy theme (primary = blue instead of Aura's emerald).
const CapitraPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{blue.50}', 100: '{blue.100}', 200: '{blue.200}', 300: '{blue.300}', 400: '{blue.400}',
      500: '{blue.600}', 600: '{blue.700}', 700: '{blue.800}', 800: '{blue.900}', 900: '{blue.950}', 950: '{blue.950}'
    }
  }
})

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: CapitraPreset
  }
})

app.mount('#app')
