<script setup lang="ts">
// Portfolio cases (APEX App 105 page 201): every acquisition case, its status and value, filtered by developer.
import { computed, ref, shallowRef, watch } from 'vue'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { useRouter } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import type { FieldOption } from '@/shared/crud/types'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount, formatShort } from '@/shared/format/money'
import { getDeveloperOptions } from '../api/masterData.api'
import { portfoliosApi, type Portfolio, type PortfolioCase } from '../api/portfolios.api'
import PortfolioFormDialog from '../components/PortfolioFormDialog.vue'

type SortKey = 'date' | 'face' | 'no'

const router = useRouter()
const canEdit = hasPermission('MTG.PORTFOLIO.EDIT')

const cases = shallowRef<PortfolioCase[]>([])
const developers = shallowRef<FieldOption[]>([])
const developerFilter = ref<number | ''>('')
const statusFilter = ref('')
const searchText = ref('')
const sortKey = ref<SortKey>('date')
const loading = ref(false)
const error = ref('')
const creating = ref(false)

getDeveloperOptions().then(o => (developers.value = o)).catch(() => undefined)

async function load() {
  loading.value = true
  error.value = ''
  try {
    cases.value = await portfoliosApi.cases(developerFilter.value === '' ? undefined : developerFilter.value)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Unable to load portfolio cases.'
  } finally {
    loading.value = false
  }
}

watch(developerFilter, () => {
  statusFilter.value = ''
  load()
}, { immediate: true })

/** Pipeline by status, in the status list's display order. */
const pipeline = computed(() => {
  const byCode = new Map<string, { code: string; name: string; order: number; count: number }>()
  for (const c of cases.value) {
    const s = byCode.get(c.portfolioStatusCode) ?? { code: c.portfolioStatusCode, name: c.portfolioStatusName, order: c.statusDisplayOrder, count: 0 }
    s.count++
    byCode.set(c.portfolioStatusCode, s)
  }
  return [...byCode.values()].sort((a, b) => a.order - b.order || a.code.localeCompare(b.code))
})

const totals = computed(() => ({
  count: cases.value.length,
  face: cases.value.reduce((t, c) => t + c.faceValueAmount, 0),
  outstanding: cases.value.reduce((t, c) => t + c.outstandingAmount, 0),
  accounts: cases.value.reduce((t, c) => t + c.accountCount, 0)
}))

const visible = computed(() => {
  const q = searchText.value.trim().toLowerCase()
  const rows = cases.value.filter(c =>
    (!statusFilter.value || c.portfolioStatusCode === statusFilter.value) &&
    (!q || [c.portfolioNo, c.developerName, c.projectName, c.portfolioStatusName, c.portfolioStatusNameAr ?? '']
      .some(v => v.toLowerCase().includes(q))))
  const sorted = [...rows]
  if (sortKey.value === 'face') sorted.sort((a, b) => b.faceValueAmount - a.faceValueAmount)
  else if (sortKey.value === 'no') sorted.sort((a, b) => a.portfolioNo.localeCompare(b.portfolioNo))
  return sorted
})

/** Colour of a status, by the same keywords page 201 uses. */
function tone(code: string): string {
  const c = code.toUpperCase()
  if (c.includes('REJECT')) return 'var(--cap-danger)'
  if (c.includes('CANCEL')) return 'var(--cap-muted)'
  if (c.includes('DRAFT')) return 'var(--cap-orange)'
  if (['ACCEPT', 'COMPLET', 'APPROV'].some(k => c.includes(k))) return 'var(--cap-success)'
  if (c.includes('INPROG')) return 'var(--cap-primary)'
  if (c.includes('REVIEW') || c.includes('AML')) return 'var(--cap-orange-dark)'
  if (c.includes('LOAD')) return 'var(--cap-primary)'
  return 'var(--cap-primary)'
}

function open(c: PortfolioCase) {
  router.push({ name: 'mtg-portfolio', params: { portfolioId: c.portfolioId } })
}

function onCreated(p: Portfolio) {
  creating.value = false
  router.push({ name: 'mtg-portfolio', params: { portfolioId: p.portfolioId } })
}

// Errors open the app-wide pop-up.
useFeedback({ errors: [error] })
</script>

<template>
  <div class="cp-page">
    <header class="cp-card cases-head">
      <div class="cases-head__icon"><i class="pi pi-briefcase"></i></div>
      <div>
        <h2>Portfolio cases</h2>
        <p>Every acquisition case, where it stands and what it is worth.</p>
      </div>
      <span class="cp-spacer"></span>
      <select v-model="developerFilter" class="cases-select" aria-label="Developer">
        <option value="">All developers</option>
        <option v-for="o in developers" :key="o.value" :value="o.value">{{ o.label }}</option>
      </select>
      <button v-if="canEdit" type="button" class="cp-btn cp-btn--create" @click="creating = true">
        <i class="pi pi-plus"></i><span>New portfolio</span>
      </button>
    </header>


    <section class="cp-card cases-gap">
      <div class="cp-figures">
        <div class="cp-figure">
          <div class="cp-figure__label">Cases</div>
          <div class="cp-figure__value">{{ totals.count }}</div>
          <div class="cp-figure__sub">{{ totals.accounts }} accounts</div>
        </div>
        <div class="cp-figure">
          <div class="cp-figure__label">Face value</div>
          <div class="cp-figure__value" :title="formatAmount(totals.face)">{{ formatShort(totals.face) }}</div>
          <div class="cp-figure__sub">all cases</div>
        </div>
        <div class="cp-figure">
          <div class="cp-figure__label">Outstanding</div>
          <div class="cp-figure__value" :title="formatAmount(totals.outstanding)">{{ formatShort(totals.outstanding) }}</div>
          <div class="cp-figure__sub">{{ totals.face > 0 ? Math.round((totals.outstanding / totals.face) * 100) : 0 }}% of face value</div>
        </div>
        <div class="cp-figure">
          <div class="cp-figure__label">Statuses</div>
          <div class="cp-figure__value">{{ pipeline.length }}</div>
          <div class="cp-figure__sub">in the pipeline</div>
        </div>
      </div>

      <div v-if="pipeline.length" class="pipe">
        <div class="pipe__bar" role="img" aria-label="Cases by status">
          <i v-for="s in pipeline" :key="s.code" :style="{ flex: s.count, background: tone(s.code) }" :title="`${s.name}: ${s.count}`"></i>
        </div>
        <div class="pipe__chips">
          <button type="button" :aria-pressed="statusFilter === ''" @click="statusFilter = ''">All <b>{{ totals.count }}</b></button>
          <button
            v-for="s in pipeline"
            :key="s.code"
            type="button"
            :aria-pressed="statusFilter === s.code"
            :style="{ '--c': tone(s.code) }"
            @click="statusFilter = statusFilter === s.code ? '' : s.code"
          >
            <i></i>{{ s.name }} <b>{{ s.count }}</b>
          </button>
        </div>
      </div>
    </section>

    <div class="cases-tools">
      <label class="cases-search">
        <i class="pi pi-search"></i>
        <input v-model="searchText" type="search" placeholder="Search portfolio no., developer, project or status" />
      </label>
      <select v-model="sortKey" class="cases-select" aria-label="Sort">
        <option value="date">Newest first</option>
        <option value="face">Highest face value</option>
        <option value="no">Portfolio no.</option>
      </select>
      <span class="cp-muted cases-count">{{ visible.length }} case(s)</span>
    </div>

    <div v-if="loading" class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>
    <div v-else-if="visible.length === 0" class="cp-card cp-empty">
      <b>{{ cases.length === 0 ? 'No cases yet' : 'No cases match' }}</b><br />
      {{ cases.length === 0 ? (canEdit ? 'Create the first portfolio with New portfolio.' : '') : 'Try another search or clear the status filter.' }}
    </div>
    <div v-else class="cases-grid">
      <button v-for="c in visible" :key="c.portfolioId" type="button" class="cp-card case" :style="{ '--c': tone(c.portfolioStatusCode) }" @click="open(c)">
        <div class="case__top">
          <strong>{{ c.portfolioNo }}</strong>
          <span class="case__status">{{ c.portfolioStatusName }}</span>
        </div>
        <div class="case__sub">{{ c.developerName }} · {{ c.projectName }}</div>
        <div class="case__sub">Requested {{ formatDate(c.requestDate + 'T00:00:00Z') }} · {{ c.ageDays }} days</div>
        <div class="case__figs">
          <div><span>Face value</span><b :title="formatAmount(c.faceValueAmount)">{{ formatShort(c.faceValueAmount) }}</b></div>
          <div><span>Outstanding</span><b :title="formatAmount(c.outstandingAmount)">{{ formatShort(c.outstandingAmount) }}</b></div>
          <div><span>Accounts</span><b>{{ c.accountCount }}</b></div>
        </div>
        <div v-if="c.arrearsAmount > 0" class="case__arrears">{{ formatShort(c.arrearsAmount) }} {{ c.currencyCode }} in arrears</div>
      </button>
    </div>

    <p class="apex-ref">Replaces APEX App 105 page 201</p>

    <PortfolioFormDialog v-if="creating" :portfolio="null" :can-edit="canEdit" @close="creating = false" @saved="onCreated" />
  </div>
</template>

<style scoped>
.cases-head { display: flex; align-items: center; gap: 15px; padding: 20px 28px; }
.cases-head__icon { width: 54px; height: 54px; flex: 0 0 54px; display: grid; place-items: center; border-radius: 14px; background: var(--cap-warning-bg); color: var(--cap-warning); font-size: 22px; }
.cases-head h2 { margin: 0; color: var(--cap-heading); font-size: 20px; font-weight: 600; }
.cases-head p { margin: 5px 0 0; color: var(--cap-muted); font-size: 13px; }
.cases-gap { margin-top: 16px; }
.cases-select { height: 40px; min-width: 200px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 8px; background: #fff; color: var(--cap-ink); font: inherit; font-size: 12px; }
.pipe { padding: 14px 20px 18px; border-top: 1px solid var(--cap-border-soft); }
.pipe__bar { display: flex; height: 10px; overflow: hidden; border-radius: 999px; background: var(--cap-bg); gap: 2px; }
.pipe__chips { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; }
.pipe__chips button { display: inline-flex; align-items: center; gap: 7px; height: 30px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 999px; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.pipe__chips button i { width: 8px; height: 8px; border-radius: 50%; background: var(--c, var(--cap-primary-dark)); }
.pipe__chips button[aria-pressed="true"] { border-color: var(--cap-selected); background: var(--cap-selected-tint); color: var(--cap-selected); }
.cases-tools { display: flex; flex-wrap: wrap; align-items: center; gap: 12px; margin: 18px 0 14px; }
.cases-search { flex: 1 1 320px; height: 42px; display: flex; align-items: center; gap: 9px; padding: 0 13px; border: 1px solid var(--cap-border); border-radius: 8px; background: #fff; }
.cases-search i { color: var(--cap-muted); font-size: 12px; }
.cases-search input { width: 100%; border: 0; outline: 0; background: transparent; color: var(--cap-ink); font: inherit; font-size: 12px; }
.cases-count { font-size: 11px; }
.cases-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 14px; }
.case { display: block; width: 100%; padding: 16px 18px; border-top: 4px solid var(--c); text-align: start; font: inherit; cursor: pointer; transition: box-shadow .15s, transform .15s; }
.case:hover { box-shadow: 0 12px 28px rgba(14, 58, 134, .12); transform: translateY(-1px); }
.case__top { display: flex; align-items: center; justify-content: space-between; gap: 10px; }
.case__top strong { color: var(--cap-heading); font-size: 15px; }
.case__status { padding: 3px 9px; border-radius: 999px; background: color-mix(in srgb, var(--c) 12%, #fff); color: var(--c); font-size: 11px; font-weight: 600; white-space: nowrap; }
.case__sub { margin-top: 5px; color: var(--cap-muted); font-size: 12px; }
.case__figs { display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; margin-top: 14px; padding-top: 12px; border-top: 1px solid var(--cap-bg); }
.case__figs span { display: block; color: var(--cap-faint); font-size: 10px; text-transform: uppercase; letter-spacing: .04em; }
.case__figs b { display: block; margin-top: 3px; color: var(--cap-ink); font-size: 15px; font-weight: 600; }
.case__arrears { margin-top: 10px; color: var(--cap-danger); font-size: 12px; font-weight: 500; }
.apex-ref { margin: 14px 2px 0; color: var(--cap-faint); font-size: 10px; }
@media (max-width: 760px) { .cases-head { flex-wrap: wrap; padding: 18px; } }
</style>
