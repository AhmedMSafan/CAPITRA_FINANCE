<script setup lang="ts">
// An APEX interactive report on one table, as Universal Theme shows it: search bar with Go and Actions,
// a Create button right of the search bar, sortable column headings, an edit link per row and pagination.
import { computed, onMounted, ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { formatValue, resolveSet, type WizardButton, type WizardRegion, type WizardTarget } from './wizard'

const props = defineProps<{ app: number; page: number; region: WizardRegion; items: Record<string, string> }>()
// open: a link to a page with the items it sets (row link, column link, a button right of the search bar)
const emit = defineEmits<{ open: [page: number | null, items: Record<string, string>] }>()

type Row = Record<string, unknown>
const rows = ref<Row[]>([])
const total = ref(0)
const search = ref('')
const searchColumn = ref('')
const applied = ref('')
const sort = ref<string | null>(null)
const dir = ref<'asc' | 'desc'>('asc')
const first = ref(1)
const perPage = ref(15)
const loading = ref(false)
const menu = ref<'actions' | 'columns' | 'rows' | null>(null)
const headMenu = ref<string | null>(null)
const error = ref('')

const columns = computed(() => props.region.columns ?? [])

async function load() {
  loading.value = true
  error.value = ''
  try {
    const res = await http.post<{ ok: boolean; message?: string; total: number; rows: Row[] }>(
      `/api/apex/${props.app}/${props.page}/IR_${props.region.key}`,
      { x01: JSON.stringify({ search: applied.value, column: searchColumn.value, sort: sort.value, dir: dir.value, first: first.value, rows: perPage.value, items: props.items }) }
    )
    if (!res.ok) throw new Error(res.message)
    rows.value = res.rows
    total.value = res.total
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'The report could not be loaded.'
  } finally {
    loading.value = false
  }
}

function go() {
  applied.value = search.value.trim()
  first.value = 1
  void load()
}

function sortBy(column: string, direction: 'asc' | 'desc') {
  sort.value = column
  dir.value = direction
  headMenu.value = null
  void load()
}

function setRows(n: number) {
  perPage.value = n
  first.value = 1
  menu.value = null
  void load()
}

function turn(step: number) {
  first.value = Math.max(1, first.value + step * perPage.value)
  void load()
}

function reset() {
  search.value = ''
  applied.value = ''
  sort.value = null
  first.value = 1
  perPage.value = 15
  menu.value = null
  void load()
}

function show(value: unknown, type: string, format?: string) {
  return formatValue(value, type, format)
}

// the row link: the items the detail link sets, or the form page's key item
function openRow(row: Row) {
  const link = props.region.link!
  emit('open', link.page, link.set ? resolveSet(link.set, row, props.items) : { [link.item!]: row[props.region.pk!] == null ? '' : String(row[props.region.pk!]) })
}

function openLink(target: WizardTarget, row: Row) {
  emit('open', target.page, resolveSet(target.set, row, props.items))
}

const toolbar = computed<WizardButton[]>(() =>
  props.region.toolbar ?? (props.region.create ? [{ name: 'CREATE', ...props.region.create }] : [])
)

async function download() {
  menu.value = null
  const res = await http.post<{ rows: Row[] }>(`/api/apex/${props.app}/${props.page}/IR_${props.region.key}`, {
    x01: JSON.stringify({ search: applied.value, sort: sort.value, dir: dir.value, first: 1, rows: 1000, items: props.items })
  })
  const q = (v: unknown) => (v == null ? '' : `"${String(v).replace(/"/g, '""')}"`)
  const csv = [columns.value.map((c) => q(c.heading)).join(','), ...res.rows.map((r) => columns.value.map((c) => q(r[c.name])).join(','))].join('\r\n')
  const a = document.createElement('a')
  a.href = URL.createObjectURL(new Blob([csv], { type: 'text/csv' }))
  a.download = `${props.region.title.replace(/\s+/g, '_').toLowerCase()}.csv`
  a.click()
  URL.revokeObjectURL(a.href)
}

defineExpose({ refresh: load })
onMounted(load)
// the page items the query binds (a filter applied, the parent row of a detail report)
watch(() => JSON.stringify(props.items), () => {
  first.value = 1
  void load()
})

const last = computed(() => Math.min(total.value, first.value + rows.value.length - 1))
</script>

<template>
  <div :id="region.staticId" class="t-Region t-Region--scrollBody" :class="region.cls">
    <div v-if="!region.hideHeader" class="t-Region-header"><h2 class="t-Region-title">{{ region.title }}</h2></div>
    <div class="t-Region-body">
      <div class="a-IRR">
        <div class="a-IRR-toolbar">
          <div class="a-IRR-search">
            <button type="button" class="a-IRR-colSel" :title="'Select columns to search'" @click="menu = menu === 'columns' ? null : 'columns'">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" aria-hidden="true"><circle cx="11" cy="11" r="7" /><path d="m20 20-3.5-3.5" /></svg>
              <svg width="8" height="8" viewBox="0 0 10 6" aria-hidden="true"><path d="M0 0h10L5 6z" fill="currentColor" /></svg>
            </button>
            <ul v-if="menu === 'columns'" class="a-Menu a-Menu--cols">
              <li :class="{ on: !searchColumn }" @click="(searchColumn = ''), (menu = null)">All Text Columns</li>
              <li v-for="c in columns" :key="c.name" :class="{ on: searchColumn === c.name }" @click="(searchColumn = c.name), (menu = null)">{{ c.heading }}</li>
            </ul>
            <input v-model="search" class="a-IRR-search-field" type="search" placeholder="Search" :aria-label="'Search: ' + region.title" @keydown.enter.prevent="go" />
          </div>
          <button type="button" class="t-Button" @click="go">Go</button>
          <div class="a-IRR-actions">
            <button type="button" class="t-Button t-Button--actions" @click="menu = menu === 'actions' ? null : 'actions'">
              Actions
              <svg width="9" height="9" viewBox="0 0 10 6" aria-hidden="true"><path d="M0 0h10L5 6z" fill="currentColor" /></svg>
            </button>
            <ul v-if="menu === 'actions' || menu === 'rows'" class="a-Menu">
              <li @click="menu = 'rows'">Rows Per Page ›</li>
              <li @click="download">Download</li>
              <li @click="reset">Reset</li>
              <ul v-if="menu === 'rows'" class="a-Menu a-Menu--sub">
                <li v-for="n in [1, 5, 10, 15, 20, 25, 50, 100, 1000]" :key="n" :class="{ on: perPage === n }" @click="setRows(n)">{{ n === 1000 ? 'All' : n }}</li>
              </ul>
            </ul>
          </div>
          <button
            v-for="b in toolbar"
            :key="b.name"
            type="button"
            class="t-Button a-IRR-create"
            :class="{ 't-Button--hot': b.name === 'CREATE' }"
            @click="emit('open', b.page, resolveSet(b.set, {}, items))"
          >
            {{ b.label }}
          </button>
        </div>

        <div v-if="applied" class="a-IRR-controls">
          <span class="a-IRR-filter">Row text contains '{{ applied }}'</span>
          <button type="button" class="a-IRR-remove" aria-label="Remove filter" @click="(search = ''), go()">&times;</button>
        </div>

        <div class="a-IRR-tableWrap" :class="{ busy: loading }">
          <p v-if="error" class="a-IRR-error">{{ error }}</p>
          <table v-else-if="rows.length" class="a-IRR-table">
            <thead>
              <tr>
                <th v-if="region.link" class="a-IRR-header a-IRR-header--linkCol"><span class="u-VisuallyHidden">Edit</span></th>
                <th v-for="c in columns" :key="c.name" class="a-IRR-header" :class="{ 'u-tR': c.type === 'NUMBER' }">
                  <button type="button" class="a-IRR-headerLink" @click="headMenu = headMenu === c.name ? null : c.name">
                    {{ c.heading }}
                    <span v-if="sort === c.name" class="a-IRR-sortIcon">{{ dir === 'asc' ? '▲' : '▼' }}</span>
                  </button>
                  <div v-if="headMenu === c.name" class="a-IRR-sortWidget">
                    <button type="button" title="Sort Ascending" @click="sortBy(c.name, 'asc')">▲</button>
                    <button type="button" title="Sort Descending" @click="sortBy(c.name, 'desc')">▼</button>
                  </div>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(r, i) in rows" :key="i">
                <td v-if="region.link" class="a-IRR-linkCol">
                  <a href="#" :title="'Edit'" @click.prevent="openRow(r)">
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="M12 20h9" /><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z" /></svg>
                  </a>
                </td>
                <td v-for="c in columns" :key="c.name" :class="{ 'u-tR': c.type === 'NUMBER' }">
                  <a v-if="c.link" href="#" @click.prevent="openLink(c.link, r)">{{ show(r[c.name], c.type, c.format) }}</a>
                  <template v-else>{{ show(r[c.name], c.type, c.format) }}</template>
                </td>
              </tr>
            </tbody>
          </table>
          <div v-else-if="!loading" class="a-IRR-noDataMsg">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true"><circle cx="11" cy="11" r="7" /><path d="m20 20-3.5-3.5" /></svg>
            <span>No data found</span>
          </div>
        </div>

        <div v-if="rows.length" class="a-IRR-pagination">
          <button type="button" class="a-IRR-pageBtn" :disabled="first === 1" aria-label="Previous" @click="turn(-1)">‹</button>
          <span class="a-IRR-pagination-label">{{ first }} - {{ last }} of {{ total }}</span>
          <button type="button" class="a-IRR-pageBtn" :disabled="last >= total" aria-label="Next" @click="turn(1)">›</button>
        </div>
      </div>
    </div>
  </div>
</template>
