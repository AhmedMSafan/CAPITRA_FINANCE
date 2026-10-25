<script setup lang="ts">
// Workflow step "Portfolio Purchase Agreement" (APEX App 105 page 216): the bilingual draft agreement for legal review,
// filled in from the portfolio, the board resolution and the financial review, with the kept contracts (annex A) and
// their cheques (annex B). Blanks are left for what Capitra does not hold (register numbers, signatories, terms to
// negotiate). Print or save as PDF from the browser.
import { computed, ref, shallowRef, watch } from 'vue'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount, formatPct } from '@/shared/format/money'
import StepLayout from '../StepLayout.vue'
import { messageOf } from '../stepUtils'
import { purchaseAgreementApi, type PurchaseAgreement } from './purchaseAgreement.api'

/** A filled-in value (shown in bold) or a blank line to complete by hand. */
type Fill = { v: string | null }
type Part = string | Fill
type Clause = { no: string; titleEn: string; titleAr: string; en: Part[]; ar: Part[] }

const props = defineProps<{ portfolioId: string }>()

const data = shallowRef<PurchaseAgreement | null>(null)
const error = ref('')
const id = computed(() => Number(props.portfolioId))

watch(id, async () => {
  data.value = null
  error.value = ''
  try {
    data.value = await purchaseAgreementApi.get(id.value)
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the purchase agreement.')
  }
}, { immediate: true })

const BLANK: Fill = { v: null }
const f = (v: string | number | null | undefined): Fill => ({ v: v === null || v === undefined || v === '' ? null : String(v) })
const money = (v: number | null) => f(v === null ? null : `${formatAmount(v)} ${data.value?.currencyCode ?? ''}`.trim())
const pct = (v: number | null) => f(v === null ? null : formatPct(v))
const day = (d: string | null) => f(d ? formatDate(d + 'T00:00:00Z') : null)

const clauses = computed<Clause[]>(() => {
  const a = data.value
  if (!a) return []
  return [
    {
      no: '', titleEn: 'Parties', titleAr: 'الأطراف',
      en: ['First Party (the Buyer): Capitra, a company established under the laws of the Arab Republic of Egypt, Commercial Register No. ', BLANK, ', address ', BLANK, ', represented by ', BLANK, ' in his capacity as ', BLANK, '.\n\nSecond Party (the Seller): ', f(a.developerNameEn), ', Commercial Register No. ', BLANK, ', address ', BLANK, ', represented by ', BLANK, ' in his capacity as ', BLANK, '.'],
      ar: ['الطرف الأول (المشتري): شركة كابيترا، شركة مؤسسة وفقاً لقوانين جمهورية مصر العربية، سجل تجاري رقم ', BLANK, '، وعنوانها ', BLANK, '، ويمثلها السيد/ ', BLANK, ' بصفته ', BLANK, '.\n\nالطرف الثاني (البائع): ', f(a.developerNameAr), '، سجل تجاري رقم ', BLANK, '، وعنوانها ', BLANK, '، ويمثلها السيد/ ', BLANK, ' بصفته ', BLANK, '.']
    },
    {
      no: '', titleEn: 'Preamble', titleAr: 'تمهيد',
      en: ['Whereas the Second Party has sold residential units in the project ', f(a.projectNameEn), ' under instalment sale contracts with its customers (the “Investors”) and wishes to sell to the First Party its rights to the outstanding instalments under the contracts listed in Annex (A); and whereas the First Party has reviewed the portfolio and its Board of Directors approved the purchase by decision No. ', f(a.boardDecisionNo), ' dated ', day(a.boardDecisionDate), '; the Parties, having acknowledged their full legal capacity to contract, have agreed as follows:'],
      ar: ['حيث إن الطرف الثاني قد باع وحدات سكنية في مشروع ', f(a.projectNameAr), ' بموجب عقود بيع بالتقسيط مع عملائه («المستثمرين»)، ويرغب في بيع حقوقه في الأقساط المتبقية بموجب العقود الواردة في الملحق (أ) إلى الطرف الأول؛ وحيث إن الطرف الأول قد قام بدراسة المحفظة ووافق مجلس إدارته على الشراء بموجب القرار رقم ', f(a.boardDecisionNo), ' بتاريخ ', day(a.boardDecisionDate), '؛ فقد اتفق الطرفان، بعد أن أقرا بأهليتهما القانونية الكاملة للتعاقد، على ما يلي:']
    },
    {
      no: '1. ', titleEn: 'Preamble and annexes', titleAr: 'البند الأول: التمهيد والملاحق',
      en: ['The preamble above and the annexes form an integral part of this Agreement.'],
      ar: ['يعتبر التمهيد السابق والملاحق جزءاً لا يتجزأ من هذا العقد ومكملاً له.']
    },
    {
      no: '2. ', titleEn: 'Subject of the agreement', titleAr: 'البند الثاني: موضوع العقد',
      en: ['The Second Party sells and assigns to the First Party, who accepts, all its rights to the outstanding balances of ', f(a.contractCount), ' instalment contracts covering ', f(a.unitCount), ' units in portfolio ', f(a.portfolioNo), ', with a total outstanding balance of ', money(a.outstandingAmount), ', together with ', f(a.chequeCount), ' post-dated cheques of a total face value of ', money(a.chequeAmount), ' listed in Annex (B).'],
      ar: ['باع وتنازل الطرف الثاني للطرف الأول، القابل لذلك، عن كافة حقوقه في الأرصدة المتبقية لعدد ', f(a.contractCount), ' عقد بيع بالتقسيط عن عدد ', f(a.unitCount), ' وحدة ضمن المحفظة رقم ', f(a.portfolioNo), '، بإجمالي رصيد قائم قدره ', money(a.outstandingAmount), '، بالإضافة إلى عدد ', f(a.chequeCount), ' شيكاً آجلاً بقيمة اسمية إجمالية قدرها ', money(a.chequeAmount), ' والمبينة في الملحق (ب).']
    },
    {
      no: '3. ', titleEn: 'Purchase price', titleAr: 'البند الثالث: الثمن',
      en: ['The purchase price is ', money(a.proposedPurchaseAmount), '. It represents the present value of the cheques discounted at ', pct(a.pvRate), ' per annum, less the agreed margin and the charges stated in clause (9).'],
      ar: ['حُدد ثمن الشراء بمبلغ ', money(a.proposedPurchaseAmount), '، ويمثل القيمة الحالية للشيكات مخصومة بمعدل ', pct(a.pvRate), ' سنوياً، مخصوماً منها الهامش المتفق عليه والمصروفات المبينة في البند التاسع.']
    },
    {
      no: '4. ', titleEn: 'Payment and reserved amount', titleAr: 'البند الرابع: سداد الثمن والمبلغ المحتجز',
      en: ['The First Party pays the price by bank transfer to the Second Party within ', BLANK, ' business days after the cheques are endorsed and delivered, retaining ', pct(a.reservedAmountPct), ' of the price as a reserved amount. The reserved amount is released after ', BLANK, ' months provided no cheque has been returned, after deducting any amount due under clause (7).'],
      ar: ['يسدد الطرف الأول الثمن بتحويل بنكي إلى حساب الطرف الثاني خلال ', BLANK, ' يوم عمل من تاريخ تظهير الشيكات وتسليمها، مع احتجاز نسبة ', pct(a.reservedAmountPct), ' من الثمن كمبلغ محتجز يُفرج عنه بعد ', BLANK, ' شهراً بشرط عدم ارتداد أي شيك، وذلك بعد خصم أي مبالغ مستحقة طبقاً للبند السابع.']
    },
    {
      no: '5. ', titleEn: 'Transfer of rights and cheques', titleAr: 'البند الخامس: نقل الحقوق والشيكات',
      en: ['On signing, the Second Party endorses all cheques listed in Annex (B) to the order of the First Party, delivers them together with copies of the investor contracts, and notifies the investors in writing that all payments are now due to the First Party.'],
      ar: ['يلتزم الطرف الثاني عند التوقيع بتظهير جميع الشيكات الواردة في الملحق (ب) لأمر الطرف الأول وتسليمها مع صور عقود المستثمرين، وإخطار المستثمرين كتابياً بأن جميع المدفوعات أصبحت مستحقة للطرف الأول.']
    },
    {
      no: '6. ', titleEn: 'Representations and warranties', titleAr: 'البند السادس: الإقرارات والضمانات',
      en: ['The Second Party warrants that the contracts are valid and enforceable, that the units are free of any third-party right or encumbrance, that the title documents and building licences are valid, that the cheques are genuine, and that it has not assigned these rights to any third party.'],
      ar: ['يقر الطرف الثاني ويضمن أن العقود صحيحة ونافذة، وأن الوحدات خالية من أي حقوق أو أعباء للغير، وأن مستندات الملكية وتراخيص البناء سارية، وأن الشيكات صحيحة، وأنه لم يسبق له التنازل عن هذه الحقوق للغير.']
    },
    {
      no: '7. ', titleEn: 'Returned cheques', titleAr: 'البند السابع: الشيكات المرتدة',
      en: ['If any cheque is returned unpaid, the Second Party shall, within ', BLANK, ' days of being notified, replace it with a valid cheque or pay its value in cash; otherwise the First Party may deduct the amount from the reserved amount or recover it from the Second Party.'],
      ar: ['في حالة ارتداد أي شيك دون صرف، يلتزم الطرف الثاني خلال ', BLANK, ' يوماً من تاريخ إخطاره باستبداله بشيك صالح أو سداد قيمته نقداً، وإلا جاز للطرف الأول خصم قيمته من المبلغ المحتجز أو الرجوع بها على الطرف الثاني.']
    },
    {
      no: '8. ', titleEn: 'Late payment', titleAr: 'البند الثامن: غرامة التأخير',
      en: ['Any amount due from the Second Party and not paid on time bears a late fee of ', pct(a.lateFeeRate), ' per annum from its due date until full payment.'],
      ar: ['يستحق على أي مبلغ مستحق على الطرف الثاني ولم يُسدد في موعده غرامة تأخير بنسبة ', pct(a.lateFeeRate), ' سنوياً من تاريخ الاستحقاق وحتى تمام السداد.']
    },
    {
      no: '9. ', titleEn: 'Fees and expenses', titleAr: 'البند التاسع: المصروفات والرسوم',
      en: ['The following charges were deducted when determining the price: administrative expenses ', money(a.adminDeductedAmount), ', legal fees ', money(a.legalDeductedAmount), ' and valuation expenses ', money(a.valuationDeductedAmount), '. Each Party otherwise bears its own costs.'],
      ar: ['تم خصم المصروفات التالية عند تحديد الثمن: مصروفات إدارية ', money(a.adminDeductedAmount), '، ورسوم قانونية ', money(a.legalDeductedAmount), '، ومصروفات تقييم ', money(a.valuationDeductedAmount), '. ويتحمل كل طرف فيما عدا ذلك مصروفاته الخاصة.']
    },
    {
      no: '10. ', titleEn: 'Confidentiality', titleAr: 'البند العاشر: السرية',
      en: ["Each Party keeps the terms of this Agreement and the investors' data confidential and uses them only to perform this Agreement, subject to the applicable laws."],
      ar: ['يلتزم كل طرف بالحفاظ على سرية شروط هذا العقد وبيانات المستثمرين وعدم استخدامها إلا في تنفيذ هذا العقد، وذلك مع مراعاة القوانين المعمول بها.']
    },
    {
      no: '11. ', titleEn: 'Governing law and disputes', titleAr: 'البند الحادي عشر: القانون الواجب التطبيق وفض المنازعات',
      en: ['This Agreement is governed by the laws of the Arab Republic of Egypt. The courts of ', BLANK, ' have jurisdiction over any dispute arising from it.'],
      ar: ['يخضع هذا العقد لقوانين جمهورية مصر العربية، وتختص محاكم ', BLANK, ' بنظر أي نزاع ينشأ عن تفسيره أو تنفيذه.']
    },
    {
      no: '12. ', titleEn: 'Notices', titleAr: 'البند الثاني عشر: الإخطارات',
      en: ['Notices are valid when delivered to the addresses stated above, or to any other address a Party notifies to the other in writing.'],
      ar: ['تكون الإخطارات صحيحة ومنتجة لآثارها متى أُرسلت إلى العناوين المبينة أعلاه، أو إلى أي عنوان آخر يخطر به أحد الطرفين الآخر كتابياً.']
    },
    {
      no: '13. ', titleEn: 'Copies and language', titleAr: 'البند الثالث عشر: النسخ واللغة',
      en: ['This Agreement is made in two originals, one for each Party, in Arabic and English. In case of any difference, the Arabic text prevails.'],
      ar: ['حُرر هذا العقد من نسختين أصليتين بيد كل طرف نسخة للعمل بموجبها، باللغتين العربية والإنجليزية، وفي حالة الاختلاف يُعتد بالنص العربي.']
    }
  ]
})

const contractTotal = computed(() => data.value?.contracts.reduce((t, c) => t + c.outstandingAmount, 0) ?? 0)
const chequeTotal = computed(() => data.value?.cheques.reduce((t, c) => t + c.chequeAmount, 0) ?? 0)
const date = (d: string) => formatDate(d + 'T00:00:00Z')
const printed = ref(false)
const print = () => {
  window.print()
  printed.value = true
}

/** Progress inside the step: the board approval it rests on, reviewing the draft, then printing it for signature. */
const stages = computed(() => {
  const approved = !!data.value?.boardDecisionNo
  return [
    { label: 'Board approval', state: approved ? 'done' : 'now' },
    { label: 'Review the agreement & annexes', state: printed.value ? 'done' : approved ? 'now' : 'todo' },
    { label: 'Print or save as PDF', state: printed.value ? 'done' : 'todo' }
  ] as { label: string; state: 'done' | 'now' | 'todo' }[]
})

// Errors open the app-wide pop-up.
useFeedback({ errors: [error] })
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    title="Portfolio purchase agreement"
    lead="The draft agreement for legal review, in English and Arabic, filled in from the portfolio, the board resolution and the financial review. Complete the blank lines by hand or after legal review, then print or save it as PDF."
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectNameEn"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    :stages="stages"
    apex-page="216"
  >
    <div v-if="!data.isBoardApproved" class="warn"><i class="pi pi-exclamation-triangle"></i> The board has not approved this purchase yet, so the price and the resolution may still change.</div>

    <div class="cp-card bar">
      <i class="pi pi-file"></i>
      <b>Proposed purchase agreement · {{ data.portfolioNo }}</b>
      <span class="cp-spacer"></span>
      <button type="button" class="cp-btn cp-btn--primary" @click="print"><i class="pi pi-print"></i><span>Print / Save as PDF</span></button>
    </div>

    <article class="doc">
      <header class="row head">
        <div>
          <span class="draft">Draft · for legal review</span>
          <h1>Mortgage Portfolio Purchase Agreement</h1>
          <p>Ref. CAP-PPA-{{ data.portfolioNo }} · Made on <span class="blank"></span> at <span class="blank"></span></p>
        </div>
        <div class="ar" lang="ar" dir="rtl">
          <span class="draft">مسودة للمراجعة القانونية</span>
          <h1>عقد شراء محفظة تمويل عقاري</h1>
          <p>مرجع CAP-PPA-{{ data.portfolioNo }} · حُرر في <span class="blank"></span> بمدينة <span class="blank"></span></p>
        </div>
      </header>

      <section v-for="c in clauses" :key="c.titleEn" class="row">
        <div>
          <h3>{{ c.no }}{{ c.titleEn }}</h3>
          <p><template v-for="(part, k) in c.en" :key="k"><template v-if="typeof part === 'string'">{{ part }}</template><b v-else-if="part.v !== null">{{ part.v }}</b><span v-else class="blank"></span></template></p>
        </div>
        <div class="ar" lang="ar" dir="rtl">
          <h3>{{ c.titleAr }}</h3>
          <p><template v-for="(part, k) in c.ar" :key="k"><template v-if="typeof part === 'string'">{{ part }}</template><b v-else-if="part.v !== null">{{ part.v }}</b><span v-else class="blank"></span></template></p>
        </div>
      </section>

      <div class="sign">
        <div>
          <h4>First Party (Buyer) · الطرف الأول (المشتري)</h4>
          <p>Capitra</p>
          <p>Name / الاسم: <span class="blank"></span></p>
          <p>Title / الصفة: <span class="blank"></span></p>
          <p>Signature / التوقيع: <span class="blank"></span></p>
        </div>
        <div>
          <h4>Second Party (Seller) · الطرف الثاني (البائع)</h4>
          <p>{{ data.developerNameEn }}</p>
          <p>Name / الاسم: <span class="blank"></span></p>
          <p>Title / الصفة: <span class="blank"></span></p>
          <p>Signature / التوقيع: <span class="blank"></span></p>
        </div>
      </div>

      <section class="annex">
        <h2><span>Annex (A) · Investor contracts</span><span lang="ar">الملحق (أ) · عقود المستثمرين</span></h2>
        <div class="cp-table-wrap">
          <table>
            <thead><tr><th>#</th><th>Investor<small>المستثمر</small></th><th>Contract no.<small>رقم العقد</small></th><th>Unit<small>الوحدة</small></th><th class="n">Outstanding<small>الرصيد القائم</small></th></tr></thead>
            <tbody>
              <tr v-for="(c, n) in data.contracts" :key="n">
                <td>{{ n + 1 }}</td>
                <td>{{ c.investorNameEn }}<span v-if="c.investorNameAr !== c.investorNameEn" class="ar-line" lang="ar" dir="rtl">{{ c.investorNameAr }}</span></td>
                <td>{{ c.sellerContractNo }}</td>
                <td>{{ c.unitCode }}</td>
                <td class="n">{{ formatAmount(c.outstandingAmount) }}</td>
              </tr>
              <tr class="tot"><td colspan="4">Total · الإجمالي</td><td class="n">{{ formatAmount(contractTotal) }} {{ data.currencyCode }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="annex">
        <h2><span>Annex (B) · Cheques</span><span lang="ar">الملحق (ب) · الشيكات</span></h2>
        <div class="cp-table-wrap">
          <table>
            <thead><tr><th>#</th><th>Investor<small>المستثمر</small></th><th>Cheque no.<small>رقم الشيك</small></th><th>Bank<small>البنك</small></th><th>Due date<small>تاريخ الاستحقاق</small></th><th class="n">Amount<small>المبلغ</small></th></tr></thead>
            <tbody>
              <tr v-for="(c, n) in data.cheques" :key="n">
                <td>{{ n + 1 }}</td>
                <td>{{ c.investorName }}</td>
                <td>{{ c.chequeNo }}</td>
                <td>{{ c.bankName }}</td>
                <td>{{ date(c.dueDate) }}</td>
                <td class="n">{{ formatAmount(c.chequeAmount) }}</td>
              </tr>
              <tr class="tot"><td colspan="5">Total · الإجمالي</td><td class="n">{{ formatAmount(chequeTotal) }} {{ data.currencyCode }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>
    </article>
  </StepLayout>
</template>

<style scoped>
.warn { padding: 10px 14px; border: 1px solid var(--cap-warning-border); border-radius: 8px; background: var(--cap-warning-bg); color: var(--cap-warning); font-size: 13px; }
.bar { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; padding: 12px 16px; font-size: 13px; color: var(--cap-primary-dark); }
.doc { padding: 40px 44px; border: 1px solid var(--cap-border-soft); border-radius: 12px; background: #fff; color: var(--cap-ink); font-size: 13px; line-height: 1.65; }
.row { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); gap: 32px; padding: 14px 0; border-bottom: 1px solid var(--cap-bg); }
.row h3 { margin: 0 0 6px; font-size: 13px; color: var(--cap-primary-dark); }
.row p { margin: 0; white-space: pre-line; }
.head { padding-top: 0; border-bottom: 2px solid var(--cap-primary-dark); }
.head h1 { margin: 6px 0 4px; font-size: 20px; color: var(--cap-primary-dark); }
.head p { color: var(--cap-ink-soft); }
.draft { display: inline-block; padding: 2px 8px; border-radius: 4px; background: var(--cap-danger-bg); color: var(--cap-danger-dark); font-size: 11px; font-weight: 600; }
.ar { text-align: right; font-family: 'Noto Naskh Arabic', 'Segoe UI', Tahoma, sans-serif; }
.blank { display: inline-block; min-width: 90px; border-bottom: 1px dotted var(--cap-muted); vertical-align: baseline; height: 1em; }
.sign { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); gap: 32px; margin: 28px 0 8px; }
.sign h4 { margin: 0 0 8px; font-size: 13px; color: var(--cap-primary-dark); }
.sign p { margin: 0 0 10px; }
.annex { margin-top: 28px; break-before: page; }
.annex h2 { display: flex; flex-wrap: wrap; justify-content: space-between; gap: 8px; margin: 0 0 10px; font-size: 15px; color: var(--cap-primary-dark); }
.annex table { width: 100%; border-collapse: collapse; font-size: 12px; }
.annex th, .annex td { padding: 6px 8px; border: 1px solid var(--cap-border-soft); text-align: start; vertical-align: top; }
.annex th { background: var(--cap-bg); font-weight: 600; }
.annex th small { display: block; font-weight: 400; color: var(--cap-muted); }
.annex .n { text-align: end; font-variant-numeric: tabular-nums; white-space: nowrap; }
.annex .tot td { font-weight: 600; background: var(--cap-bg); }
.ar-line { display: block; color: var(--cap-ink-soft); }
@media (max-width: 760px) {
  .doc { padding: 20px 16px; }
  .row, .sign { grid-template-columns: minmax(0, 1fr); gap: 12px; }
}
</style>

<style>
/* Print only the agreement. */
@media print {
  .capitra-app-header, .crumbs, .hero, .apex-ref, .bar, .warn { display: none !important; }
  .capitra-app-content, .cp-page { padding: 0 !important; margin: 0 !important; background: #fff !important; }
  .doc { border: 0 !important; padding: 0 !important; }
  .doc .cp-table-wrap { overflow: visible !important; }
}
</style>
