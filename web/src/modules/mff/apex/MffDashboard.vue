<script setup lang="ts">
// App 110 page 1 (Mortgage Finance Dashboard) in the app 118 design: its page items (the Before Header process,
// PAGE_LOAD) shown as the hero counts, the application pipeline and the quick actions.
import { computed, onMounted, ref } from 'vue'
import { http } from '@/shared/api/http'
import { getLang } from './mffUi'

const ar = getLang() === 'AR'
const t = (en: string, arabic: string) => (ar ? arabic : en)

const items = ref<Record<string, string | null>>({})
const loading = ref(true)
const error = ref('')

const n = (k: string) => Number(items.value[k] ?? 0) || 0
const show = (k: string) => (loading.value ? '–' : String(n(k)))

const kpis = computed(() => [
  { k: 'P1_TOTAL', label: t('Applications', 'الطلبات'), hint: t('All cases', 'كل الحالات'), cls: '' },
  { k: 'P1_IMPORTED', label: t('Imported', 'وارد الصندوق'), hint: t('Intake', 'الاستلام'), cls: '' },
  { k: 'P1_REVIEW', label: t('Under review', 'تحت الدراسة'), hint: t('Credit & documents', 'الائتمان والمستندات'), cls: 'is-warm' },
  { k: 'P1_APPROVED', label: t('Approved', 'موافقات'), hint: t('Pre-disbursement', 'قبل الصرف'), cls: 'is-ok' },
  { k: 'P1_ACTIVE', label: t('Active loans', 'تمويل قائم'), hint: t('Servicing', 'الخدمة'), cls: '' },
  { k: 'P1_DELINQ', label: t('Delinquent', 'متعثر'), hint: t('Collections', 'التحصيل'), cls: 'is-hot' }
])

const stages = computed(() => [
  { k: 'P1_PIPE_INTAKE', label: t('Intake', 'الاستلام') },
  { k: 'P1_PIPE_CREDIT', label: t('Credit inquiry', 'الاستعلام') },
  { k: 'P1_PIPE_STUDY', label: t('Study', 'الدراسة') },
  { k: 'P1_PIPE_APPROVAL', label: t('Approval', 'الموافقة') },
  { k: 'P1_PIPE_LIVE', label: t('Live loan', 'تمويل قائم') }
])
const pipeTotal = computed(() => stages.value.reduce((s, x) => s + n(x.k), 0))

const actions = computed(() => [
  { to: '/mortgage-finance/p10', icon: 'pi pi-upload', title: t('Fund intake batches', 'ملفات الصندوق'), hint: t('Files received from the fund', 'الملفات الواردة من الصندوق') },
  { to: '/mortgage-finance/p30', icon: 'pi pi-search', title: t('Credit inquiry queue', 'الاستعلام الائتماني'), hint: t('Applications waiting for an inquiry', 'طلبات في انتظار الاستعلام') },
  { to: '/mortgage-finance/p40', icon: 'pi pi-file-excel', title: t('Missing documents', 'المستندات الناقصة'), hint: t('Document review and follow-up', 'مراجعة ومتابعة المستندات') },
  { to: '/mortgage-finance/p80', icon: 'pi pi-calculator', title: t('Mortgage calculator', 'حاسبة التمويل'), hint: t('Instalment and subsidy', 'القسط والدعم') },
  { to: '/mortgage-finance/p140', icon: 'pi pi-chart-bar', title: t('MIS & reports', 'التقارير'), hint: t('Registers and statistics', 'السجلات والإحصاءات') }
])

onMounted(async () => {
  try {
    const res = await http.post<{ ok?: boolean; message?: string; items?: Record<string, string | null> }>('/api/apex/110/1/PAGE_LOAD', { x01: '{}' })
    if (res.ok === false) error.value = res.message ?? t('The dashboard could not be loaded.', 'تعذر تحميل اللوحة.')
    items.value = res.items ?? {}
  } catch (e) {
    error.value = e instanceof Error ? e.message : t('The dashboard could not be loaded.', 'تعذر تحميل اللوحة.')
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="wx md wx-page">
    <section class="wx-hero">
      <div class="wr-hero">
        <div>
          <span class="wx-eyebrow"><i></i>{{ t('FINOVA Mortgage Finance', 'التمويل العقاري') }}<template v-if="items.P1_TODAY"> &middot; {{ items.P1_TODAY }}</template></span>
          <h1>{{ t('Mortgage Finance', 'التمويل العقاري') }}</h1>
          <p>{{ t('Fund-supported mortgage origination, approval and servicing: from the fund file to the live loan.', 'متابعة دورة التمويل العقاري المدعوم من الصندوق: من ملف الصندوق حتى التمويل القائم.') }}</p>
        </div>
        <div class="wr-cta">
          <router-link class="wx-btn wx-btn--ghost" to="/mortgage-finance/p10"><i class="pi pi-upload"></i><span>{{ t('Fund intake', 'ملفات الصندوق') }}</span></router-link>
          <router-link class="wx-btn wx-btn--ghost" to="/mortgage-finance/p50"><i class="pi pi-inbox"></i><span>{{ t('My work queue', 'قائمة أعمالي') }}</span></router-link>
          <router-link class="wx-btn wx-btn--hot" to="/mortgage-finance/p20"><i class="pi pi-folder-open"></i><span>{{ t('Open applications', 'فتح الطلبات') }}</span></router-link>
        </div>
      </div>
      <div class="wr-kpis md-kpis">
        <div v-for="k in kpis" :key="k.k" class="wr-kpi" :class="n(k.k) > 0 ? k.cls : ''">
          <b>{{ show(k.k) }}</b><span>{{ k.label }} &middot; <small>{{ k.hint }}</small></span>
        </div>
      </div>
    </section>

    <div v-if="error" class="wr-error md-err">{{ error }}</div>

    <div class="wx-sec"><h2>{{ t('Application pipeline', 'مراحل الطلب') }}</h2></div>
    <section class="md-card">
      <div class="md-ph">
        <div>
          <h3>{{ t('Where the applications are', 'أين توجد الطلبات') }}</h3>
          <p>{{ t('Every open application by the stage it is in.', 'كل طلب مفتوح حسب المرحلة الحالية.') }}</p>
        </div>
        <router-link class="md-more" to="/mortgage-finance/p20">{{ t('View details', 'عرض التفاصيل') }}</router-link>
      </div>
      <ol class="md-flow">
        <li class="md-ev"><span>{{ t('Start', 'بداية') }}</span></li>
        <li v-for="(s, i) in stages" :key="s.k" class="md-n" :class="{ 'is-on': n(s.k) > 0 }">
          <span class="md-no">{{ i + 1 }}</span>
          <span class="md-tx"><b>{{ s.label }}</b><small>{{ show(s.k) }} {{ n(s.k) === 1 ? t('application', 'طلب') : t('applications', 'طلبات') }}</small></span>
          <span class="md-c">{{ show(s.k) }}</span>
        </li>
      </ol>
      <div class="md-dist" :title="t('Share of applications by stage', 'توزيع الطلبات على المراحل')">
        <i v-for="(s, i) in stages" :key="s.k" :class="'d' + i" :style="{ width: pipeTotal ? (100 * n(s.k)) / pipeTotal + '%' : '0' }"></i>
      </div>
      <div class="md-legend">
        <span v-for="(s, i) in stages" :key="s.k"><i :class="'d' + i"></i>{{ s.label }} <b>{{ show(s.k) }}</b></span>
      </div>
    </section>

    <div class="wx-sec"><h2>{{ t('Quick actions', 'إجراءات سريعة') }}</h2></div>
    <section class="md-acts">
      <router-link v-for="a in actions" :key="a.to" :to="a.to" class="md-act">
        <div class="md-top"><span class="md-ico"><i :class="a.icon"></i></span><span class="md-go"><i class="pi pi-arrow-right"></i></span></div>
        <h3>{{ a.title }}</h3>
        <p>{{ a.hint }}</p>
      </router-link>
    </section>
  </div>
</template>

<style scoped>
.md-kpis { grid-template-columns: repeat(6, minmax(0, 1fr)); }
.md-kpis small { color: rgba(255, 255, 255, .5); font-size: 11.5px; }
.md-err { margin-top: 16px; }
.md-card { display: flex; flex-direction: column; gap: 16px; padding: 20px; border-radius: 16px; background: #fff; border: 1px solid var(--wx-line); box-shadow: 0 1px 2px rgba(11, 37, 51, .04); }
.md-ph { display: flex; align-items: center; justify-content: space-between; gap: 12px; }
.md-ph h3, .md-act h3 { margin: 0; font-size: 16px; font-weight: 700; letter-spacing: -.01em; color: var(--wx-ink); }
.md-ph p, .md-act p { margin: 4px 0 0; font-size: 13px; line-height: 1.5; color: var(--wx-muted); }
.md-more {
  flex: 0 0 auto; display: inline-flex; align-items: center; height: 32px; padding: 0 12px; border-radius: 9px; border: 1px solid var(--wx-line); background: var(--wx-soft);
  font-size: 12.5px; font-weight: 600; color: var(--wx-ink2); transition: all .15s;
}
.md-more:hover { background: var(--wx-teal-d); border-color: var(--wx-teal-d); color: #fff; }
.md-flow {
  list-style: none; margin: 0; padding: 20px 16px; display: flex; flex-wrap: wrap; align-items: center; row-gap: 20px; border-radius: 14px;
  background-color: var(--wx-soft); border: 1px solid var(--wx-line); background-image: radial-gradient(#D6E3E7 1px, transparent 1.1px); background-size: 14px 14px;
}
.md-flow > li { position: relative; display: flex; align-items: center; }
.md-flow > li + li { margin-inline-start: 36px; }
.md-flow > li + li::before { content: ""; position: absolute; inset-inline-start: -32px; top: 50%; width: 24px; height: 2px; margin-top: -1px; background: #A9C0C6; }
.md-flow > li + li::after {
  content: ""; position: absolute; inset-inline-start: -10px; top: 50%; margin-top: -5px; border: 5px solid transparent; border-inline-start: 6px solid #A9C0C6; border-inline-end: 0;
}
.md-ev span {
  display: grid; place-items: center; min-width: 46px; height: 46px; padding: 0 12px; border-radius: 23px; background: #fff; border: 2.5px solid var(--wx-teal);
  font-size: 10.5px; font-weight: 800; letter-spacing: .06em; text-transform: uppercase; color: var(--wx-teal);
}
.md-n {
  flex: 1 1 0; gap: 10px; min-width: 170px; padding: 10px 12px 10px 10px; border-radius: 12px; background: #fff; border: 1.5px solid #CFDDE1;
  border-inline-start: 5px solid #9DB0B8; box-shadow: 0 1px 2px rgba(11, 37, 51, .06);
}
.md-n.is-on { border-color: #7FC3CA; border-inline-start-color: var(--wx-teal); background: #F2FAFB; }
.md-no {
  flex: 0 0 26px; width: 26px; height: 26px; border-radius: 50%; display: grid; place-items: center; font-size: 11.5px; font-weight: 800;
  background: var(--wx-soft); color: var(--wx-ink2); border: 1px solid var(--wx-line);
}
.md-n.is-on .md-no { background: var(--wx-teal); border-color: var(--wx-teal); color: #fff; }
.md-tx { display: flex; flex-direction: column; flex: 1; min-width: 0; gap: 1px; }
.md-tx b { font-size: 13px; font-weight: 700; color: var(--wx-ink); }
.md-tx small { font-size: 11.5px; color: var(--wx-muted); }
.md-c { font-size: 22px; font-weight: 800; letter-spacing: -.02em; color: #9DB0B8; }
.md-n.is-on .md-c { color: var(--wx-teal-d); }
.md-dist { display: flex; height: 10px; border-radius: 99px; overflow: hidden; background: #E6EEF1; }
.md-dist i { display: block; height: 100%; }
.d0 { background: #9DB0B8; } .d1 { background: #4FA3AB; } .d2 { background: #00636D; } .d3 { background: #E87722; } .d4 { background: #14734F; }
.md-legend { display: flex; flex-wrap: wrap; gap: 6px 18px; font-size: 12px; color: var(--wx-muted); }
.md-legend i { display: inline-block; width: 10px; height: 10px; border-radius: 3px; margin-inline-end: 6px; vertical-align: -1px; }
.md-legend b { color: var(--wx-ink); margin-inline-start: 2px; }
.md-acts { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 16px; }
.md-act {
  position: relative; display: flex; flex-direction: column; gap: 12px; padding: 20px; border-radius: 16px; background: #fff; border: 1px solid var(--wx-line);
  color: var(--wx-ink); box-shadow: 0 1px 2px rgba(11, 37, 51, .04); transition: transform .18s, box-shadow .18s, border-color .18s;
}
.md-act:hover { transform: translateY(-3px); border-color: #A9CDD1; box-shadow: 0 18px 36px -18px rgba(11, 37, 51, .25); }
.md-top { display: flex; align-items: center; justify-content: space-between; }
.md-ico { width: 44px; height: 44px; border-radius: 12px; display: grid; place-items: center; color: var(--wx-teal); background: var(--wx-teal-l); }
.md-ico .pi { font-size: 18px; }
.md-go { width: 32px; height: 32px; border-radius: 50%; display: grid; place-items: center; color: var(--wx-muted); background: var(--wx-soft); border: 1px solid var(--wx-line); transition: all .18s; }
.md-go .pi { font-size: 12px; }
.md-act:hover .md-go { background: var(--wx-teal); border-color: var(--wx-teal); color: #fff; }
@media (max-width: 1300px) { .md-kpis { grid-template-columns: repeat(3, minmax(0, 1fr)); } .md-acts { grid-template-columns: repeat(3, minmax(0, 1fr)); } }
@media (max-width: 760px) {
  .md-kpis, .md-acts { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .md-flow { flex-direction: column; align-items: stretch; }
  .md-flow > li + li { margin-inline-start: 0; margin-top: 22px; }
  .md-flow > li + li::before { inset-inline-start: 24px; top: -20px; width: 2px; height: 16px; margin-top: 0; }
  .md-flow > li + li::after { display: none; }
}
</style>
