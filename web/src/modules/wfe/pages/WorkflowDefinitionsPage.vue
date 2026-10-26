<template>
  <div class="workflow-page">
    <!-- Page heading -->
    <section class="page-head">
      <div>
        <h1>Workflow Engine</h1>
        <p>Design and manage workflow definitions for business processes.</p>
      </div>

      <div class="breadcrumbs">
        <span><i class="pi pi-home"></i> Home</span>
        <i class="pi pi-angle-right"></i>
        <span>Workflow Engine</span>
        <i class="pi pi-angle-right"></i>
        <strong>Workflow Definitions</strong>
      </div>
    </section>

    <!-- Summary cards -->
    <section class="summary-grid">
      <div class="summary-card">
        <div class="summary-icon summary-icon--blue">
          <i class="pi pi-clone"></i>
        </div>
        <div>
          <strong>{{ rows.length }}</strong>
          <span>Total Workflows</span>
        </div>
      </div>

      <div class="summary-card">
        <div class="summary-icon summary-icon--green">
          <i class="pi pi-check-circle"></i>
        </div>
        <div>
          <strong>{{ activeCount }}</strong>
          <span>Active Workflows</span>
        </div>
      </div>

      <div class="summary-card">
        <div class="summary-icon summary-icon--orange">
          <i class="pi pi-pause-circle"></i>
        </div>
        <div>
          <strong>{{ inactiveCount }}</strong>
          <span>Inactive Workflows</span>
        </div>
      </div>

      <div class="summary-card">
        <div class="summary-icon summary-icon--purple">
          <i class="pi pi-box"></i>
        </div>
        <div>
          <strong>{{ latestVersion }}</strong>
          <span>Latest Version</span>
          <small>Current deployed version</small>
        </div>
      </div>
    </section>

    <!-- Search & Filter -->
    <section class="filter-card">
      <div class="card-title">
        <div class="title-icon">
          <i class="pi pi-filter"></i>
        </div>
        <strong>Search & Filter</strong>
      </div>

      <div class="filter-grid">
        <label class="field field--search">
          <span>Search</span>
          <div class="input-wrap">
            <i class="pi pi-search"></i>
            <input
              v-model="search"
              placeholder="Search workflow code or name..."
              @keyup.enter="load"
            />
          </div>
        </label>

        <label class="field">
          <span>Status</span>
          <select v-model="status">
            <option value="ALL">All Statuses</option>
            <option value="Y">Active</option>
            <option value="N">Inactive</option>
          </select>
        </label>

        <label class="field">
          <span>Active</span>
          <select v-model="activeFilter">
            <option value="ALL">All</option>
            <option value="Y">Active</option>
            <option value="N">Inactive</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="resetFilters">
            <i class="pi pi-refresh"></i>
            Reset
          </button>

          <button
            class="primary-button"
            type="button"
            :disabled="loading"
            @click="load"
          >
            <i class="pi pi-search"></i>
            {{ loading ? 'Loading...' : 'Search' }}
          </button>
        </div>
      </div>
    </section>

    <div v-if="errorMessage" class="message message--error">
      {{ errorMessage }}
    </div>

    <!-- Definitions table -->
    <section class="grid-card">
      <div class="grid-header">
        <div class="grid-title">
          <i class="pi pi-list"></i>
          <strong>Workflow Definitions</strong>
        </div>

        <button class="add-workflow-button" type="button" @click="createNew">
          <i class="pi pi-plus-circle"></i>
          Add Workflow
        </button>
      </div>

      <div v-if="loading" class="loading-state">
        Loading workflows...
      </div>

      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Workflow Code</th>
              <th>Workflow Name</th>
              <th>Version</th>
              <th>Status</th>
              <th>Active</th>
              <th>Last Updated</th>
              <th>Action</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="w in filtered" :key="w.dgrmId">
              <td>{{ w.dgrmId }}</td>

              <td>
                <strong class="workflow-code">
                  {{ w.workflowCode }}
                </strong>
              </td>

              <td>{{ w.dgrmName }}</td>

              <td>{{ w.dgrmVersion }}</td>

              <td>
                <span
                  class="status-pill"
                  :class="statusClass(w.dgrmStatus)"
                >
                  {{ formatStatus(w.dgrmStatus) }}
                </span>
              </td>

              <td>
                <span
                  class="status-pill"
                  :class="w.isActive === 'Y'
                    ? 'status-pill--active'
                    : 'status-pill--inactive'"
                >
                  {{ w.isActive === 'Y' ? 'Active' : 'Inactive' }}
                </span>
              </td>

              <td>
                <div class="updated-cell">
                  <strong>
                    {{ formatDate(w.dgrmLastUpdateOn || w.dgrmCreatedOn) }}
                  </strong>
                  <small>
                    {{ w.lastUpdatedBy || w.createdBy || '—' }}
                  </small>
                </div>
              </td>

              <td>
                <button
                  class="design-button"
                  type="button"
                  @click="edit(w.dgrmId)"
                >
                  <i class="pi pi-pencil"></i>
                  Design
                </button>
              </td>
            </tr>

            <tr v-if="filtered.length === 0">
              <td colspan="8" class="empty-state">
                No workflow definitions found.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="table-footer">
        <span>
          Showing {{ filtered.length ? 1 : 0 }}
          to {{ filtered.length }}
          of {{ filtered.length }} entries
        </span>

        <div class="pager">
          <button type="button"><i class="pi pi-angle-left"></i></button>
          <button type="button" class="is-active">1</button>
          <button type="button"><i class="pi pi-angle-right"></i></button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
/* eslint-disable @typescript-eslint/no-explicit-any -- bpmn-js and its moddle objects are untyped */
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { getWorkflowDefinitions } from '../services/workflowDefinitionService'

const router = useRouter()

const rows = ref<any[]>([])
const search = ref('')
const status = ref('ALL')
const activeFilter = ref('ALL')
const loading = ref(false)
const errorMessage = ref('')

const filtered = computed(() => {
  let result = rows.value

  if (status.value !== 'ALL') {
    result = result.filter(x => x.isActive === status.value)
  }

  if (activeFilter.value !== 'ALL') {
    result = result.filter(x => x.isActive === activeFilter.value)
  }

  return result
})

const activeCount = computed(() =>
  rows.value.filter(x => x.isActive === 'Y').length
)

const inactiveCount = computed(() =>
  rows.value.filter(x => x.isActive !== 'Y').length
)

const latestVersion = computed(() => {
  if (!rows.value.length)
    return 0

  return Math.max(
    ...rows.value.map(x =>
      Number(x.dgrmVersion || 0)
    )
  )
})

async function load() {
  loading.value = true
  errorMessage.value = ''

  try {
    rows.value = await getWorkflowDefinitions(search.value)
  }
  catch (e) {
    errorMessage.value =
      e instanceof Error
        ? e.message
        : 'Unable to load workflows.'
  }
  finally {
    loading.value = false
  }
}

function resetFilters() {
  search.value = ''
  status.value = 'ALL'
  activeFilter.value = 'ALL'
  load()
}

function createNew() {
  router.push('/workflowengine/designer/new')
}

function edit(id: number) {
  router.push(`/workflowengine/designer/${id}`)
}

function formatDate(v?: string | null) {
  if (!v)
    return '—'

  const d = new Date(v)

  if (Number.isNaN(d.getTime()))
    return v

  return new Intl.DateTimeFormat('en-GB', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  }).format(d)
}

function formatStatus(value?: string | null) {
  if (!value)
    return '—'

  return value.charAt(0).toUpperCase() +
    value.slice(1).toLowerCase()
}

function statusClass(value?: string | null) {
  const normalized = (value || '').toLowerCase()

  if (normalized === 'released')
    return 'status-pill--released'

  if (normalized === 'draft')
    return 'status-pill--draft'

  return 'status-pill--neutral'
}

onMounted(load)
</script>

<style scoped>
.workflow-page {
  min-height: 100vh;
  padding: 12px 24px 18px;
  background: #f5f8fb;
  color: #153553;
}

.page-head {
  display: flex;
  justify-content: space-between;
  gap: 20px;
  align-items: center;
  margin-bottom: 10px;
}

.page-head h1 {
  margin: 0;
  color: #0d2f58;
  font-size: 26px;
  line-height: 1;
  font-weight: 900;
}

.page-head p {
  margin: 5px 0 0;
  color: #536f88;
  font-size: 12px;
  font-weight: 600;
}

.breadcrumbs {
  display: flex;
  align-items: center;
  gap: 8px;
  padding-top: 6px;
  color: #8191a1;
  font-size: 10.5px;
  white-space: nowrap;
}

.breadcrumbs strong {
  color: #205d96;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
}

.summary-card {
  min-height: 72px;
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 9px 14px;
  border: 1px solid #e0e7ec;
  border-radius: 12px;
  background: #ffffff;
  box-shadow: 0 4px 12px rgba(20, 61, 87, .035);
}

.summary-icon {
  width: 54px;
  height: 54px;
  flex: 0 0 54px;
  display: grid;
  place-items: center;
  border-radius: 12px;
}

.summary-icon i {
  font-size: 29px !important;
  line-height: 1;
  font-weight: 700;
}

.summary-icon--blue {
  color: #1f7be8;
  background: #e8f2ff;
}

.summary-icon--green {
  color: #139b58;
  background: #e5f7ed;
}

.summary-icon--orange {
  color: #ef850d;
  background: #fff0df;
}

.summary-icon--purple {
  color: #8050e0;
  background: #f0e9ff;
}

.summary-card strong {
  display: block;
  color: #102f50;
  font-size: 20px;
  line-height: 1;
  font-weight: 900;
}

.summary-card span {
  display: block;
  margin-top: 5px;
  color: #315779;
  font-size: 11px;
  font-weight: 800;
}

.summary-card small {
  display: block;
  margin-top: 2px;
  color: #7f91a1;
  font-size: 8.8px;
  font-weight: 600;
}

.filter-card,
.grid-card {
  margin-top: 12px;
  border: 1px solid #dce6ec;
  border-radius: 12px;
  background: #fff;
  box-shadow: 0 4px 12px rgba(20, 61, 87, .035);
}

.filter-card {
  padding: 11px 16px 13px;
}

.card-title {
  display: flex;
  align-items: center;
  gap: 9px;
  margin-bottom: 9px;
}

.title-icon {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border-radius: 8px;
  color: #ffffff;
  background: #216fd2;
  font-size: 15px;
}

.card-title strong,
.grid-title strong {
  color: #123553;
  font-size: 15px;
  font-weight: 900;
}

.filter-grid {
  display: grid;
  grid-template-columns:
    minmax(280px, 1.7fr)
    minmax(150px, .7fr)
    minmax(150px, .7fr)
    auto;
  gap: 14px;
  align-items: end;
}

.field {
  display: grid;
  gap: 6px;
}

.field span {
  color: #234866;
  font-size: 11px;
  font-weight: 850;
}

.field input,
.field select {
  width: 100%;
  height: 38px;
  box-sizing: border-box;
  border: 1px solid #cfdce5;
  border-radius: 9px;
  background: #fff;
  color: #173d5d;
  padding: 0 12px;
  font: inherit;
  font-size: 11.5px;
  outline: none;
}

.input-wrap {
  position: relative;
}

.input-wrap > i {
  position: absolute;
  left: 13px;
  top: 50%;
  transform: translateY(-50%);
  color: #587792;
  font-size: 13px;
}

.input-wrap input {
  padding-left: 36px;
}

.filter-actions {
  display: flex;
  gap: 9px;
}

.primary-button,
.secondary-button,
.add-workflow-button,
.design-button {
  height: 38px;
  border-radius: 8px;
  padding: 0 14px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  border: 0;
  cursor: pointer;
  font: inherit;
  font-size: 11px;
  font-weight: 750;
}

.primary-button {
  min-width: 96px;
  color: #fff;
  background: #0b4f97;
}

.primary-button:disabled {
  opacity: .55;
  cursor: default;
}

.secondary-button {
  min-width: 86px;
  color: #496175;
  background: #f5f7f9;
}

.message--error {
  margin-top: 12px;
  color: #9a3f3f;
}

.grid-card {
  min-height: 430px;
  padding: 0 18px 10px;
}

.grid-header {
  min-height: 52px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.grid-title {
  display: flex;
  align-items: center;
  gap: 9px;
}

.grid-title > i {
  color: #0b5ea8;
  font-size: 18px;
}

.add-workflow-button {
  color: #fff;
  background: #e64444;
}

.table-wrap {
  min-height: 305px;
  overflow: auto;
}

table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
}

thead th {
  padding: 11px 10px;
  background: #dcecff;
  color: #12395d;
  font-size: 11px;
  font-weight: 900;
  text-align: left;
  white-space: nowrap;
}

thead th:first-child {
  border-radius: 8px 0 0 8px;
}

thead th:last-child {
  border-radius: 0 8px 8px 0;
}

tbody td {
  padding: 13px 10px;
  border-bottom: 1px solid #e7edf1;
  color: #2f506a;
  font-size: 11px;
  vertical-align: middle;
}

.workflow-code {
  color: #12355d;
  font-weight: 850;
}

.status-pill {
  display: inline-flex;
  align-items: center;
  min-height: 24px;
  padding: 0 10px;
  border-radius: 6px;
  font-size: 9.5px;
  font-weight: 800;
}

.status-pill--released,
.status-pill--active {
  color: #10884c;
  background: #e6f7ee;
  border: 1px solid #bfe8cf;
}

.status-pill--inactive {
  color: #b35a2d;
  background: #fff0e8;
  border: 1px solid #ffd2bd;
}

.status-pill--draft {
  color: #8d6814;
  background: #fff7dc;
  border: 1px solid #f4e3a8;
}

.status-pill--neutral {
  color: #5c7082;
  background: #eef2f5;
  border: 1px solid #dce3e8;
}

.updated-cell {
  display: grid;
  gap: 2px;
}

.updated-cell strong {
  color: #284a66;
  font-size: 10.5px;
  font-weight: 700;
}

.updated-cell small {
  color: #8a98a5;
  font-size: 8.5px;
}

.design-button {
  height: 32px;
  padding: 0 11px;
  color: #fff;
  background: #27a36d;
}

.loading-state,
.empty-state {
  padding: 28px 12px;
  text-align: center;
  color: #7d8e9c;
}

.table-footer {
  min-height: 42px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #6f8292;
  font-size: 9.5px;
}

.pager {
  display: flex;
  gap: 6px;
}

.pager button {
  width: 32px;
  height: 32px;
  border: 1px solid #d2dde5;
  border-radius: 7px;
  background: #fff;
  color: #49657c;
  cursor: pointer;
}

.pager button.is-active {
  color: #fff;
  border-color: #2f7fe4;
  background: #2f7fe4;
}

@media (max-width: 1100px) {
  .summary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .filter-grid {
    grid-template-columns: 1fr 1fr;
  }

  .filter-actions {
    justify-content: flex-end;
  }

  .breadcrumbs {
    display: none;
  }
}
</style>
