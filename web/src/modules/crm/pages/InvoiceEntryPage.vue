<template>
  <!-- APEX App 148 page 705 "Invoice Entry & Verification", the Invoice Upload step of DISBURSEMENT_REQUEST_WF: the disbursement
       terms, the batch header, the invoice and cheque uploads and their grids. Opened from the workflow task, the step's Complete
       button saves the page first and moves the request to Treasury. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-upload"></span></span>
        <div>
          <h1>{{ tr('Invoice Entry & Verification') }}</h1>
          <p>{{ terms.disbursementNumber }}</p>
        </div>
      </div>
      <div class="col-actions">
        <RouterLink class="fx-btn" :to="{ name: 'crm-disbursement-requests' }"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to list') }}</RouterLink>
        <FctStepActions :save="saveForStep" :disabled="busy" />
      </div>
    </div>

    <!-- the Cycle region -->
    <div class="ie-cycle" :aria-label="tr('Cycle')">
      <div class="ie-step is-now"><span class="ie-dot">2</span><div><b>{{ tr('Invoices & Cheques Entry') }}</b><small>{{ tr('Risk Officer') }}</small></div></div>
      <span class="ie-line is-done"></span>
      <div class="ie-step is-done"><span class="ie-dot"><span class="fa fa-check" aria-hidden="true"></span></span><div><b>{{ tr('Disbursement Request') }}</b><small>{{ tr('Operations Officer') }}</small></div></div>
      <span class="ie-line is-done"></span>
      <div class="ie-step is-next"><span class="ie-dot">3</span><div><b>{{ tr('Disbursement Execution') }}</b><small>{{ tr('Treasury Officer') }}</small></div></div>
    </div>

    <div v-if="ready" class="col-stack">
      <!-- the Summary region -->
      <section v-if="summary" class="ie-sum" :aria-label="tr('Summary')">
        <div><small>{{ tr('Limit Number') }}</small><b>{{ summary.limitNumber }}</b></div>
        <div><small>{{ tr('Client') }}</small><b class="ie-nowrap">{{ summary.clientName }}</b></div>
        <div><small>{{ tr('Approved Limit') }}</small><b>{{ whole(summary.approvedLimit) }} <i>{{ summary.currencyCode }}</i></b></div>
        <div><small>{{ tr('Available Limit') }}</small><b class="ie-nowrap">{{ whole(summary.availableAmount) }} <i>{{ summary.currencyCode }}</i></b></div>
        <div><small>{{ tr('Total Invoices') }}</small><b class="ie-nowrap">{{ summary.totalInvoices }}</b></div>
        <div><small>{{ tr('Approved Invoiced') }}</small><b class="ie-nowrap">{{ summary.approvedInvoiced }}</b></div>
        <div><small>{{ tr('Advance Rate') }}</small><b class="ie-orange">{{ summary.advanceRate.toFixed(2) }}%</b></div>
        <div class="ie-util">
          <div class="ie-util-head"><small>{{ tr('Utilization') }}</small><span class="ie-orange">{{ summary.utilizationPercent }}%</span></div>
          <div class="ie-bar"><span :style="{ width: `${summary.utilizationPercent}%` }"></span></div>
          <div class="ie-util-foot">{{ whole(summary.utilizationAmount) }} <i>/</i> {{ whole(summary.approvedLimit) }} {{ summary.currencyCode }}</div>
        </div>
      </section>

      <!-- Disbursement Request Detail -->
      <section class="col-sec">
        <h2><span class="fa fa-money" aria-hidden="true"></span>{{ tr('Disbursement Request Detail') }}</h2>
        <div class="col-grid">
          <div class="fc-field">
            <span>{{ tr('Disbursement Number') }}</span>
            <div class="col-ro">{{ terms.disbursementNumber }}</div>
          </div>
          <label class="fc-field" :class="{ 'col-bad': bad('terms.disbursementDate') }">
            <span>{{ tr('Disbursement Date') }} <b>*</b></span>
            <input v-model="terms.disbursementDate" class="fx-search" type="date" :disabled="readOnly">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('terms.clientId') }">
            <span>{{ tr('Client Id') }} <b>*</b></span>
            <select v-model="terms.clientId" class="fx-select" :disabled="readOnly" @change="clientChanged">
              <option :value="null">{{ tr('- Select -') }}</option>
              <option v-for="c in options.clients" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
            </select>
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('terms.limitId') }">
            <span>{{ tr('Available Limit') }}</span>
            <select v-model="terms.limitId" class="fx-select" :disabled="readOnly" @change="limitChanged">
              <option :value="null">{{ tr('Select Limit') }}</option>
              <option v-for="l in options.limits" :key="l.value" :value="Number(l.value)">{{ l.label }}</option>
            </select>
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('terms.currencyId') }">
            <span>{{ tr('Currency') }} <b>*</b></span>
            <select v-model="terms.currencyId" class="fx-select" :disabled="readOnly">
              <option v-for="c in options.currencies" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
            </select>
          </label>
          <label class="fc-field">
            <span>{{ tr('Product ID') }}</span>
            <select v-model="terms.productId" class="fx-select" :disabled="readOnly">
              <option :value="null">{{ tr('- Select -') }}</option>
              <option v-for="p in options.products" :key="p.value" :value="Number(p.value)">{{ p.label }}</option>
            </select>
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('terms.grossAmount') }">
            <span>{{ tr('Invoices Gross Amount') }} <b>*</b></span>
            <input v-model.number="terms.grossAmount" class="fx-search" type="number" min="0" step="0.01" inputmode="decimal" :readonly="readOnly">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('terms.advanceRatePct') }">
            <span>{{ tr('Advance Rate %') }}</span>
            <input v-model.number="terms.advanceRatePct" class="fx-search" type="number" min="0" max="100" step="0.01" inputmode="decimal" :readonly="readOnly">
          </label>
          <label class="fc-field">
            <span>{{ tr('Tenor Months') }}</span>
            <input v-model.number="terms.tenorMonth" class="fx-search" type="number" min="0" step="1" inputmode="numeric" :readonly="readOnly">
          </label>
          <div class="fc-field">
            <span>{{ tr('Index Rate') }}</span>
            <div class="col-ro">{{ terms.indexRate ?? '' }}</div>
          </div>
          <label class="fc-field">
            <span>{{ tr('Spread Rate') }}</span>
            <input v-model.number="terms.spreadRate" class="fx-search" type="number" step="0.01" inputmode="decimal" :readonly="readOnly">
          </label>
          <label class="fc-field">
            <span>{{ tr('Base Day Count') }}</span>
            <select v-model="terms.baseDayCount" class="fx-select" :disabled="readOnly">
              <option v-for="o in dayCounts" :key="o" :value="o">{{ o }}</option>
            </select>
          </label>
          <label class="fc-field">
            <span>{{ tr('Repayment Start Date') }}</span>
            <input v-model="terms.repaymentStartDate" class="fx-search" type="date" :disabled="readOnly">
          </label>
          <label class="fc-field">
            <span>{{ tr('Principal Repayment Schedule Type') }}</span>
            <select v-model="terms.principalRepaymentScheduleType" class="fx-select" :disabled="readOnly">
              <option v-for="o in scheduleTypes" :key="o" :value="o">{{ o }}</option>
            </select>
          </label>
          <label class="fc-field">
            <span>{{ tr('Principal Repayment Method') }}</span>
            <select v-model="terms.principalRepaymentMethod" class="fx-select" :disabled="readOnly">
              <option v-for="o in principalMethods" :key="o" :value="o">{{ o }}</option>
            </select>
          </label>
          <label class="fc-field">
            <span>{{ tr('Principal Distribution Method') }}</span>
            <select v-model="terms.principalDistributionMethod" class="fx-select" :disabled="readOnly">
              <option v-for="o in distributionMethods" :key="o" :value="o">{{ o }}</option>
            </select>
          </label>
          <label class="fc-field">
            <span>{{ tr('Interest Repayment Method') }}</span>
            <select v-model="terms.interestRepaymentMethod" class="fx-select" :disabled="readOnly">
              <option v-for="o in interestMethods" :key="o.value" :value="o.value">{{ o.label }}</option>
            </select>
          </label>
          <label class="fc-field">
            <span>{{ tr('Payment Method') }}</span>
            <select v-model="terms.paymentMethod" class="fx-select" :disabled="readOnly">
              <option v-for="o in paymentMethods" :key="o" :value="o">{{ o }}</option>
            </select>
          </label>
          <label class="fc-field ie-wide">
            <span>{{ tr('Remarks') }}</span>
            <input v-model="terms.remarks" class="fx-search" type="text" maxlength="2000" :readonly="readOnly">
          </label>
        </div>
      </section>

      <!-- Invoice Batch Details -->
      <section class="col-sec">
        <h2><span class="fa fa-files-o" aria-hidden="true"></span>{{ tr('Invoice Batch Details') }}</h2>
        <div class="col-grid">
          <label class="fc-field" :class="{ 'col-bad': bad('batch.batchNumber') }">
            <span>{{ tr('Batch Number') }} <b>*</b></span>
            <input v-model="batch.batchNumber" class="fx-search" type="text" maxlength="50" :readonly="readOnly">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('batch.clientId') }">
            <span>{{ tr('Client') }} <b>*</b></span>
            <select v-model="batch.clientId" class="fx-select" :disabled="readOnly">
              <option :value="null">{{ tr('- Select -') }}</option>
              <option v-for="c in options.clients" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
            </select>
          </label>
          <div class="fc-field">
            <span>{{ tr('Disbursement Id') }}</span>
            <div class="col-ro">{{ disbursementId }}</div>
          </div>
          <label class="fc-field">
            <span>{{ tr('Batch Name') }}</span>
            <input v-model="batch.batchName" class="fx-search" type="text" maxlength="200" :readonly="readOnly">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('batch.batchDate') }">
            <span>{{ tr('Batch Date') }} <b>*</b></span>
            <input v-model="batch.batchDate" class="fx-search" type="date" :disabled="readOnly">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('batch.batchType') }">
            <span>{{ tr('Batch Type') }} <b>*</b></span>
            <select v-model="batch.batchType" class="fx-select" :disabled="readOnly">
              <option value="BULK">{{ tr('BULK') }}</option>
              <option value="SINGLE">{{ tr('SINGLE') }}</option>
            </select>
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('batch.currencyId') }">
            <span>{{ tr('Currency') }} <b>*</b></span>
            <select v-model="batch.currencyId" class="fx-select" :disabled="readOnly">
              <option v-for="c in options.currencies" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
            </select>
          </label>
          <div class="fc-field">
            <span>{{ tr('Total Net Amount') }}</span>
            <div class="col-ro">{{ batch.totalNetAmount == null ? '' : formatAmount(batch.totalNetAmount) }}</div>
          </div>
          <label class="fc-field ie-wide">
            <span>{{ tr('Remarks') }}</span>
            <input v-model="batch.remarks" class="fx-search" type="text" maxlength="2000" :readonly="readOnly">
          </label>
          <label v-if="!readOnly" class="fc-field ie-wide">
            <span>{{ tr('Invoices File (XLSX / CSV)') }}</span>
            <input ref="invoiceInput" class="fx-search" type="file" accept=".xlsx,.csv,.txt" @change="invoiceFile = pick($event)">
          </label>
        </div>
        <div v-if="!readOnly" class="col-actions ie-buttons">
          <button type="button" class="fx-btn ie-upl" :disabled="busy" @click="upload"><span class="fa fa-upload" aria-hidden="true"></span>{{ tr('Upload Invoices') }}</button>
        </div>
      </section>

      <!-- Invoice Entry -->
      <section class="col-sec">
        <h2><span class="fa fa-list-alt" aria-hidden="true"></span>{{ tr('Invoice Entry') }} <span class="ie-count">{{ invoices.length }}</span></h2>
        <div class="fx-table-wrap">
          <table class="fx-table ie-grid">
            <thead>
              <tr>
                <th>{{ tr('Invoice Number') }}</th>
                <th>{{ tr('Client Invoice Date') }}</th>
                <th>{{ tr('Due Date') }}</th>
                <th class="fx-r">{{ tr('Gross Amount') }}</th>
                <th class="fx-r">{{ tr('Advance Rate Pct') }}</th>
                <th class="fx-r">{{ tr('Advance Amount') }}</th>
                <th>{{ tr('Eligibility Status') }}</th>
                <th>{{ tr('Ineligible Reason') }}</th>
                <th v-if="!readOnly"><span class="fx-sr">{{ tr('Delete') }}</span></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in invoices" :key="r.invoiceId">
                <td><input v-model="r.invoiceNumber" class="ie-cell" type="text" :readonly="readOnly" @input="touchInvoice(r)"></td>
                <td><input v-model="r.clientInvoiceDate" class="ie-cell" type="date" :disabled="readOnly" @change="touchInvoice(r)"></td>
                <td><input v-model="r.dueDate" class="ie-cell" type="date" :disabled="readOnly" @change="touchInvoice(r)"></td>
                <td class="fx-r"><input v-model.number="r.grossAmount" class="ie-cell fx-r" type="number" step="0.01" :readonly="readOnly" @input="touchInvoice(r)"></td>
                <td class="fx-r"><input v-model.number="r.advanceRatePct" class="ie-cell ie-num-sm fx-r" type="number" step="0.01" :readonly="readOnly" @input="touchInvoice(r)"></td>
                <td class="fx-r"><input v-model.number="r.advanceAmount" class="ie-cell fx-r" type="number" step="0.001" :readonly="readOnly" @input="touchInvoice(r)"></td>
                <td>
                  <select v-model="r.statusCode" class="ie-cell" :class="r.statusCode === 'ELIGIBLE' ? 'is-ok' : 'is-bad'" :disabled="readOnly" @change="touchInvoice(r)">
                    <option value="ELIGIBLE">{{ tr('Eligible') }}</option>
                    <option value="NOT_ELIGIBLE">{{ tr('Not Eligible') }}</option>
                  </select>
                </td>
                <td><input v-model="r.ineligibleReason" class="ie-cell" type="text" :readonly="readOnly" @input="touchInvoice(r)"></td>
                <td v-if="!readOnly">
                  <button type="button" class="fx-btn fx-btn--sm" :title="tr('Delete')" :disabled="busy" @click="dropInvoice(r)"><span class="fa fa-trash-o" aria-hidden="true"></span></button>
                </td>
              </tr>
              <tr v-if="!invoices.length"><td :colspan="readOnly ? 8 : 9" class="fx-empty">{{ tr('No data found') }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Cheques Entry -->
      <section class="col-sec">
        <h2><span class="fa fa-credit-card" aria-hidden="true"></span>{{ tr('Cheques Entry') }} <span class="ie-count">{{ cheques.length }}</span></h2>
        <div v-if="!readOnly" class="ie-upload-row">
          <label class="fc-field">
            <span>{{ tr('Cheques File (XLSX / CSV)') }}</span>
            <input ref="chequeInput" class="fx-search" type="file" accept=".xlsx,.csv,.txt" @change="chequeFile = pick($event)">
          </label>
          <button type="button" class="fx-btn ie-upl" :disabled="busy" @click="upload"><span class="fa fa-upload" aria-hidden="true"></span>{{ tr('Upload Cheques') }}</button>
        </div>
        <div class="fx-table-wrap">
          <table class="fx-table ie-grid">
            <thead>
              <tr>
                <th>{{ tr('Cheque No') }}</th>
                <th>{{ tr('Cheque Date') }}</th>
                <th>{{ tr('Due Date') }}</th>
                <th>{{ tr('Issuer Bank Name') }}</th>
                <th>{{ tr('Drawer Name En') }}</th>
                <th class="fx-r">{{ tr('Deposit Bank Account Id') }}</th>
                <th class="fx-r">{{ tr('Cheque Amount') }}</th>
                <th>{{ tr('Currency Code') }}</th>
                <th>{{ tr('Source Module') }}</th>
                <th>{{ tr('Received Dt') }}</th>
                <th>{{ tr('Deposit Dt') }}</th>
                <th>{{ tr('Status Code') }}</th>
                <th v-if="!readOnly"><span class="fx-sr">{{ tr('Delete') }}</span></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in cheques" :key="c.chequeId">
                <td><input v-model="c.chequeNo" class="ie-cell" type="text" :readonly="readOnly" @input="touchCheque(c)"></td>
                <td><input v-model="c.chequeDate" class="ie-cell" type="date" :disabled="readOnly" @change="touchCheque(c)"></td>
                <td><input v-model="c.dueDate" class="ie-cell" type="date" :disabled="readOnly" @change="touchCheque(c)"></td>
                <td><input v-model="c.issuerBankName" class="ie-cell" type="text" :readonly="readOnly" @input="touchCheque(c)"></td>
                <td><input v-model="c.drawerNameEn" class="ie-cell" type="text" :readonly="readOnly" @input="touchCheque(c)"></td>
                <td class="fx-r"><input v-model.number="c.depositBankAccountId" class="ie-cell ie-num-sm fx-r" type="number" :readonly="readOnly" @input="touchCheque(c)"></td>
                <td class="fx-r"><input v-model.number="c.chequeAmount" class="ie-cell fx-r" type="number" step="0.01" :readonly="readOnly" @input="touchCheque(c)"></td>
                <td><input v-model="c.currencyCode" class="ie-cell ie-num-sm" type="text" maxlength="3" :readonly="readOnly" @input="touchCheque(c)"></td>
                <td>{{ c.sourceModule }}</td>
                <td><input v-model="c.receivedDate" class="ie-cell" type="date" :disabled="readOnly" @change="touchCheque(c)"></td>
                <td><input v-model="c.depositDate" class="ie-cell" type="date" :disabled="readOnly" @change="touchCheque(c)"></td>
                <td><input v-model="c.statusCode" class="ie-cell" type="text" :readonly="readOnly" @input="touchCheque(c)"></td>
                <td v-if="!readOnly">
                  <button type="button" class="fx-btn fx-btn--sm" :title="tr('Delete')" :disabled="busy" @click="dropCheque(c)"><span class="fa fa-trash-o" aria-hidden="true"></span></button>
                </td>
              </tr>
              <tr v-if="!cheques.length"><td :colspan="readOnly ? 12 : 13" class="fx-empty">{{ tr('No data found') }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- the result of Process Disbursement Request: not a region of page 705, shown so the calculation can be checked -->
      <template v-if="result && (result.charges.length || result.schedule.length)">
        <section class="col-sec">
          <h2>
            <span class="fa fa-calculator" aria-hidden="true"></span>{{ tr('Disbursement Charges') }}
            <span v-if="result.statusCode" class="fx-chip fx-chip--teal ie-chip">{{ result.statusCode }}</span>
          </h2>
          <div class="ie-totals">
            <div><small>{{ tr('Approved Invoice Amount') }}</small><b>{{ formatAmount(result.approvedInvoiceAmount) }}</b></div>
            <div><small>{{ tr('Approved Advance Amount') }}</small><b>{{ formatAmount(result.approvedAdvanceAmount) }}</b></div>
            <div><small>{{ tr('Fee Deducted') }}</small><b>{{ formatAmount(result.feeDeducted) }}</b></div>
            <div><small>{{ tr('Tax Deducted') }}</small><b>{{ formatAmount(result.taxDeducted) }}</b></div>
            <div><small>{{ tr('Net Amount') }}</small><b class="ie-net">{{ formatAmount(result.netAmount) }}</b></div>
          </div>
          <div class="fx-table-wrap">
            <table class="fx-table">
              <thead>
                <tr>
                  <th>{{ tr('Charge') }}</th>
                  <th>{{ tr('Calculation Method') }}</th>
                  <th>{{ tr('Amount Base') }}</th>
                  <th class="fx-r">{{ tr('Base Amount') }}</th>
                  <th class="fx-r">{{ tr('Rate %') }}</th>
                  <th class="fx-r">{{ tr('Charge Amount') }}</th>
                  <th class="fx-r">{{ tr('Tax Amount') }}</th>
                  <th class="fx-r">{{ tr('Total Amount') }}</th>
                  <th>{{ tr('Charge Timing') }}</th>
                  <th>{{ tr('Due Date') }}</th>
                  <th>{{ tr('Status') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="c in result.charges" :key="c.disbursementChargeId">
                  <td><b>{{ c.ruleCode }}</b><div class="ie-sub">{{ c.ruleName }}</div></td>
                  <td>{{ c.calculationMethod }}</td>
                  <td>{{ c.baseCode }}</td>
                  <td class="fx-r">{{ c.baseAmount == null ? '' : formatAmount(c.baseAmount) }}</td>
                  <td class="fx-r">{{ c.ratePercent ?? '' }}</td>
                  <td class="fx-r">{{ formatAmount(c.chargeAmount) }}</td>
                  <td class="fx-r">{{ formatAmount(c.taxAmount) }}</td>
                  <td class="fx-r"><b>{{ formatAmount(c.totalAmount) }}</b></td>
                  <td>{{ c.chargeTimingCode }}</td>
                  <td>{{ formatDate(c.dueDate) }}</td>
                  <td>{{ c.statusCode }}</td>
                </tr>
                <tr v-if="!result.charges.length"><td colspan="11" class="fx-empty">{{ tr('No data found') }}</td></tr>
              </tbody>
            </table>
          </div>
        </section>

        <section class="col-sec">
          <h2><span class="fa fa-calendar" aria-hidden="true"></span>{{ tr('Repayment Schedule') }} <span class="ie-count">{{ result.schedule.length }}</span></h2>
          <div class="fx-table-wrap">
            <table class="fx-table">
              <thead>
                <tr>
                  <th class="fx-r">{{ tr('Installment No') }}</th>
                  <th>{{ tr('Due Date') }}</th>
                  <th class="fx-r">{{ tr('Opening Principal') }}</th>
                  <th class="fx-r">{{ tr('Principal Due') }}</th>
                  <th class="fx-r">{{ tr('Interest Due') }}</th>
                  <th class="fx-r">{{ tr('Total Installment') }}</th>
                  <th class="fx-r">{{ tr('Closing Principal') }}</th>
                  <th class="fx-r">{{ tr('Interest Rate') }}</th>
                  <th class="fx-r">{{ tr('Day Count') }}</th>
                  <th>{{ tr('Status') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="r in result.schedule" :key="r.installmentNo">
                  <td class="fx-r">{{ r.installmentNo }}</td>
                  <td>{{ formatDate(r.dueDate) }}</td>
                  <td class="fx-r">{{ formatAmount(r.openingPrincipal) }}</td>
                  <td class="fx-r">{{ formatAmount(r.principalDue) }}</td>
                  <td class="fx-r">{{ formatAmount(r.interestDue) }}</td>
                  <td class="fx-r"><b>{{ formatAmount(r.totalInstallment) }}</b></td>
                  <td class="fx-r">{{ formatAmount(r.closingPrincipal) }}</td>
                  <td class="fx-r">{{ r.interestRate ?? '' }}</td>
                  <td class="fx-r">{{ r.dayCount ?? '' }}</td>
                  <td>{{ r.statusCode }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </template>

      <div v-if="!readOnly" class="col-actions ie-buttons">
        <RouterLink class="fx-btn" :to="{ name: 'crm-disbursement-requests' }">{{ tr('Cancel') }}</RouterLink>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="apply"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Apply Changes') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="processRequest"><span class="fa fa-cogs" aria-hidden="true"></span>{{ tr('Process Disbursement Request') }}</button>
      </div>
    </div>
    <div v-else class="fx-card fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '@/modules/collection/pages/collection.css'
import { computed, reactive, ref, watch } from 'vue'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatAmount } from '@/shared/format/money'
import { formatDate } from '@/shared/format/datetime'
import { fieldErrors, messageOf } from '@/modules/collection/pages/collection'
import FctStepActions from '@/modules/fct/components/FctStepActions.vue'
import {
  invoiceEntryApi, processApi, type ChequeRow, type DisbursementResult, type DisbursementTerms, type InvoiceBatch, type InvoiceEntry,
  type InvoiceEntrySave, type InvoiceRow, type InvoiceSummary
} from '../api/invoiceEntry.api'

const props = defineProps<{ disbursementId: number }>()

// the page's static lists
const dayCounts = ['30/360', 'ACTUAL/360']
const scheduleTypes = ['NORMAL_SCHEDULE', 'CUSTOM_SCHEDULE']
const principalMethods = ['MONTHLY', 'QUARTERLY', 'SEMI-ANNUAL', 'INVOICE_DUE_DATE', 'CHEQUE_DUE_DATE', 'AT_MATURITY']
const distributionMethods = ['EQUAL_AMOUNT', 'INVOICE_AMOUNT', 'CHEQUE_AMOUNT']
const interestMethods = [
  { value: 'MONTHLY', label: 'MONTHLY (EOM)' }, { value: 'AT_MATURITY', label: 'AT_MATURITY' }, { value: 'SAME_AS_PRINCIPAL', label: 'SAME_AS_PRINCIPAL' }
]
const paymentMethods = ['CHEQUE', 'TRANSFER', 'INTERNAL']

const canEdit = hasPermission('CRM.EDIT')
const readOnly = computed(() => !canEdit)
const ready = ref(false)
const busy = ref(false)
const errors = ref<Record<string, string[]>>({})
const bad = (field: string) => !!errors.value[field]

const terms = reactive({} as DisbursementTerms)
const batch = reactive({} as InvoiceBatch)
const summary = ref<InvoiceSummary | null>(null)
const result = ref<DisbursementResult | null>(null)
const options = reactive<InvoiceEntry['options']>({ clients: [], limits: [], currencies: [], products: [] })
const invoices = ref<InvoiceRow[]>([])
const cheques = ref<ChequeRow[]>([])
const changedInvoices = new Set<number>()
const changedCheques = new Set<number>()
const deletedInvoices = ref<number[]>([])
const deletedCheques = ref<number[]>([])
const invoiceFile = ref<File | null>(null)
const chequeFile = ref<File | null>(null)
const invoiceInput = ref<HTMLInputElement | null>(null)
const chequeInput = ref<HTMLInputElement | null>(null)

const whole = (n: number) => Math.round(n).toLocaleString('en-US')
const pick = (e: Event) => (e.target as HTMLInputElement).files?.[0] ?? null

function show(page: InvoiceEntry) {
  Object.assign(terms, page.terms)
  Object.assign(batch, page.batch)
  Object.assign(options, page.options)
  summary.value = page.summary
  invoices.value = page.invoices
  cheques.value = page.cheques
  changedInvoices.clear()
  changedCheques.clear()
  deletedInvoices.value = []
  deletedCheques.value = []
}

async function load() {
  ready.value = false
  errors.value = {}
  try {
    const [page, processed] = await Promise.all([invoiceEntryApi.get(props.disbursementId), processApi.result(props.disbursementId)])
    show(page)
    result.value = processed
  } catch (err) {
    showError(messageOf(err))
  } finally {
    ready.value = true
  }
}

async function refreshSummary() {
  summary.value = await invoiceEntryApi.summary(props.disbursementId, terms.clientId, terms.limitId)
}

// the limit list cascades from the client, and the Summary follows the client
async function clientChanged() {
  terms.limitId = null
  try {
    options.limits = terms.clientId == null ? [] : await invoiceEntryApi.limits(terms.clientId)
    await refreshSummary()
  } catch (err) {
    showError(messageOf(err))
  }
}

// SET_ADV_PCT: the limit's advance rate
async function limitChanged() {
  try {
    if (terms.limitId != null) {
      const rate = await invoiceEntryApi.advanceRate(terms.limitId)
      if (rate != null) terms.advanceRatePct = rate
    }
    await refreshSummary()
  } catch (err) {
    showError(messageOf(err))
  }
}

const touchInvoice = (r: InvoiceRow) => changedInvoices.add(r.invoiceId)
const touchCheque = (c: ChequeRow) => changedCheques.add(c.chequeId)

function dropInvoice(r: InvoiceRow) {
  invoices.value = invoices.value.filter((x) => x !== r)
  changedInvoices.delete(r.invoiceId)
  deletedInvoices.value.push(r.invoiceId)
}

function dropCheque(c: ChequeRow) {
  cheques.value = cheques.value.filter((x) => x !== c)
  changedCheques.delete(c.chequeId)
  deletedCheques.value.push(c.chequeId)
}

// the page's required items, checked before the submit as APEX does
function check(): boolean {
  const e: Record<string, string[]> = {}
  if (!terms.disbursementDate) e['terms.disbursementDate'] = ['Disbursement Date must have some value.']
  if (terms.clientId == null) e['terms.clientId'] = ['Client Id must have some value.']
  if (terms.currencyId == null) e['terms.currencyId'] = ['Currency must have some value.']
  if (terms.grossAmount == null || (terms.grossAmount as unknown) === '') e['terms.grossAmount'] = ['Invoices Gross Amount must have some value.']
  if (!batch.batchNumber?.trim()) e['batch.batchNumber'] = ['Batch Number must have some value.']
  if (batch.clientId == null) e['batch.clientId'] = ['Client must have some value.']
  if (!batch.batchDate) e['batch.batchDate'] = ['Batch Date must have some value.']
  if (!batch.batchType) e['batch.batchType'] = ['Batch Type must have some value.']
  if (batch.currencyId == null) e['batch.currencyId'] = ['Currency must have some value.']
  errors.value = e
  if (Object.keys(e).length) showError(Object.values(e).flat().map((m) => tr(m)).join('\n'))
  return !Object.keys(e).length
}

// empty number inputs come back as '' from v-model.number
const num = (v: unknown) => (v === '' || v == null ? null : Number(v))

const payload = (): InvoiceEntrySave => ({
  terms: {
    ...terms, grossAmount: num(terms.grossAmount), advanceRatePct: num(terms.advanceRatePct), tenorMonth: num(terms.tenorMonth),
    spreadRate: num(terms.spreadRate), remarks: terms.remarks?.trim() || null
  },
  batch: { ...batch, batchNumber: batch.batchNumber?.trim() || null, remarks: batch.remarks?.trim() || null },
  invoices: invoices.value.filter((r) => changedInvoices.has(r.invoiceId))
    .map((r) => ({ ...r, grossAmount: Number(r.grossAmount), advanceRatePct: num(r.advanceRatePct), advanceAmount: num(r.advanceAmount) })),
  deletedInvoiceIds: deletedInvoices.value,
  cheques: cheques.value.filter((c) => changedCheques.has(c.chequeId))
    .map((c) => ({ ...c, chequeAmount: Number(c.chequeAmount), depositBankAccountId: num(c.depositBankAccountId) })),
  deletedChequeIds: deletedCheques.value
})

async function run(work: () => Promise<void>): Promise<boolean> {
  busy.value = true
  try {
    await work()
    return true
  } catch (err) {
    errors.value = fieldErrors(err) ?? {}
    showError(messageOf(err))
    return false
  } finally {
    busy.value = false
  }
}

const apply = () => check() && run(async () => {
  show(await invoiceEntryApi.save(props.disbursementId, payload()))
  showSuccess(tr('Action Processed.'))
})

// Upload Invoices: saves the forms, then imports whichever files are chosen
const upload = () => check() && run(async () => {
  if (!invoiceFile.value && !chequeFile.value) throw new Error(tr('Select at least one invoice or cheque XLSX/CSV file.'))
  const result = await invoiceEntryApi.upload(props.disbursementId, payload(), invoiceFile.value, chequeFile.value)
  show(result.page)
  invoiceFile.value = null
  chequeFile.value = null
  if (invoiceInput.value) invoiceInput.value.value = ''
  if (chequeInput.value) chequeInput.value.value = ''
  showSuccess(tr(result.message))
})

// Process Disbursement Request: saves the page, then the charges and the repayment schedule
const processRequest = () => check() && run(async () => {
  const done = await processApi.process(props.disbursementId, payload())
  show(done.page)
  result.value = done.result
  showSuccess(tr(done.message))
})

// the step's Complete saves the page first
const saveForStep = async () => (readOnly.value ? true : check() && (await run(async () => {
  show(await invoiceEntryApi.save(props.disbursementId, payload()))
})))

watch(() => props.disbursementId, load, { immediate: true })
</script>

<style scoped>
/* the Cycle region of page 705 */
.ie-cycle { display: flex; align-items: center; justify-content: center; gap: 12px; padding: 18px 24px; margin-bottom: 16px; background: var(--fin-surface); border: 1px solid var(--fin-line); border-radius: 12px; overflow-x: auto; }
.ie-step { display: flex; align-items: center; gap: 10px; white-space: nowrap; }
.ie-step b { display: block; font-size: 13px; }
.ie-step small { display: block; font-size: 11px; color: #888780; }
.ie-dot { width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; }
.ie-step.is-now .ie-dot { background: #E87722; color: #fff; box-shadow: 0 0 0 4px rgba(232, 119, 34, .15); }
.ie-step.is-now b { color: #c25a0a; }
.ie-step.is-done .ie-dot { background: #00636D; color: #fff; box-shadow: 0 0 0 4px rgba(0, 99, 109, .15); }
.ie-step.is-done b { color: #00636D; }
.ie-step.is-next { opacity: .6; }
.ie-step.is-next .ie-dot { background: #eceae3; color: #888780; border: 1px solid #d3d1c7; }
.ie-line { width: 40px; height: 1px; background: #e0ded7; }
.ie-line.is-done { background: #00636D; }

/* the Summary region */
.ie-sum { display: flex; align-items: center; gap: 20px; padding: 12px 20px; border-radius: 10px; color: #fff; background: linear-gradient(135deg, #00636D 0%, #014b55 100%); box-shadow: 0 4px 12px rgba(0, 0, 0, .15); overflow-x: auto; }
.ie-sum > div { flex: 0 0 auto; padding-inline-end: 20px; border-inline-end: 1px solid rgba(255, 255, 255, .15); }
.ie-sum small { display: block; margin-bottom: 2px; font-size: 9px; font-weight: 600; color: rgba(255, 255, 255, .6); text-transform: uppercase; }
.ie-sum b { font-size: 16px; font-weight: 700; }
.ie-sum i { font-style: normal; font-size: 11px; font-weight: 400; color: rgba(255, 255, 255, .8); }
.ie-nowrap { white-space: nowrap; }
.ie-orange { color: #ff9d4d; }
.ie-sum > .ie-util { flex: 2; min-width: 180px; border: 0; padding: 0; }
.ie-util-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 4px; font-size: 11px; font-weight: 700; }
.ie-bar { height: 6px; margin-bottom: 4px; border-radius: 3px; overflow: hidden; background: rgba(255, 255, 255, .1); }
.ie-bar span { display: block; height: 100%; border-radius: 3px; background: linear-gradient(90deg, #ff9d4d, #E87722); }
.ie-util-foot { font-size: 10px; font-weight: 500; text-align: right; color: rgba(255, 255, 255, .8); }
.ie-util-foot i { color: rgba(255, 255, 255, .4); }

.ie-wide { grid-column: span 2; }
.ie-chip { margin-inline-start: 6px; }
.ie-sub { font-size: 12px; color: #6b7c7b; }
.ie-totals { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 12px; margin-bottom: 14px; }
.ie-totals > div { padding: 10px 14px; border: 1px solid var(--fin-line); border-radius: 10px; }
.ie-totals small { display: block; font-size: 11px; color: #6b7c7b; text-transform: uppercase; letter-spacing: .04em; }
.ie-totals b { font-size: 16px; }
.ie-net { color: #00636D; }
.ie-buttons { justify-content: flex-end; margin-top: 16px; }
.ie-upl { background: rgb(245, 240, 240); color: #000; border-color: transparent; font-weight: 600; }
.ie-upload-row { display: flex; align-items: flex-end; gap: 12px; margin-bottom: 14px; }
.ie-upload-row .fc-field { flex: 1; max-width: 420px; }
.ie-count { margin-inline-start: 6px; padding: 1px 8px; border-radius: 999px; background: #E6F3F4; color: #00636D; font-size: 12px; }

/* the editable grids */
.ie-grid td { padding: 4px 6px; }
.ie-cell { width: 100%; min-width: 110px; padding: 5px 7px; border: 1px solid transparent; border-radius: 6px; background: transparent; font: inherit; color: inherit; }
.ie-cell:hover:not([readonly]):not(:disabled), .ie-cell:focus { border-color: var(--fin-line); background: var(--fin-surface); outline: none; }
.ie-num-sm { min-width: 70px; }
select.ie-cell.is-ok { color: #2f9e64; font-weight: 600; }
select.ie-cell.is-bad { color: #c0392b; font-weight: 600; }
@media (max-width: 760px) {
  .ie-wide { grid-column: auto; }
  .ie-totals { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .ie-upload-row { flex-direction: column; align-items: stretch; }
}
</style>
