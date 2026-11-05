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
        <RouterLink class="fx-btn" :to="back"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Factoring Workspace') }}</RouterLink>
        <FctStepActions :save="saveForStep" :disabled="busy" :complete-note="completeNote" />
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
        <div><small>{{ tr('Total Invoices') }}</small><b class="ie-nowrap">{{ formatAmount(summary.totalInvoices) }}</b></div>
        <div><small>{{ tr('Approved Invoiced') }}</small><b class="ie-nowrap">{{ formatAmount(summary.approvedInvoiced) }}</b></div>
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
            <span>{{ tr('Invoices Gross Amount') }} <b v-if="!isReverse">*</b><em v-else class="ie-src">{{ tr('Total of eligible invoices') }}</em></span>
            <div v-if="locked" class="col-ro ie-locked">{{ formatAmount(terms.grossAmount) }}</div>
            <input v-else v-model.number="terms.grossAmount" class="fx-search" type="number" min="0" step="0.01" inputmode="decimal" @blur="terms.grossAmount = round2(terms.grossAmount)">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('terms.advanceRatePct') }">
            <span>{{ tr('Advance Rate %') }}<em v-if="isReverse" class="ie-src">{{ tr('From agreement') }}</em></span>
            <input v-model.number="terms.advanceRatePct" class="fx-search" type="number" min="0" max="100" step="0.01" inputmode="decimal" :readonly="locked" :class="{ 'ie-locked': isReverse }">
          </label>
          <label class="fc-field">
            <span>{{ tr('Tenor Months') }}<em v-if="isCustom" class="ie-src">{{ tr('Last principal date') }}</em></span>
            <input v-model.number="terms.tenorMonth" class="fx-search" type="number" min="0" step="1" inputmode="numeric" :readonly="readOnly || isCustom" :class="{ 'ie-locked': isCustom }">
          </label>
          <div class="fc-field">
            <span>{{ tr('Index Rate') }}</span>
            <div class="col-ro">{{ terms.indexRate ?? '' }}</div>
          </div>
          <label class="fc-field">
            <span>{{ tr('Spread Rate') }}<em v-if="isReverse" class="ie-src">{{ tr('From agreement') }}</em></span>
            <input v-model.number="terms.spreadRate" class="fx-search" type="number" step="0.01" inputmode="decimal" :readonly="locked" :class="{ 'ie-locked': isReverse }">
          </label>
          <label class="fc-field">
            <span>{{ tr('Base Day Count') }}<em v-if="isReverse" class="ie-src">{{ tr('From agreement') }}</em></span>
            <select v-model="terms.baseDayCount" class="fx-select" :disabled="locked" :class="{ 'ie-locked': isReverse }">
              <option v-for="o in dayCountOptions" :key="o" :value="o">{{ o }}</option>
            </select>
          </label>
          <!-- the repayment terms (Ahmed, 2 Nov 2026): the schedule is built from them, then the customer's cheques must match it -->
          <label class="fc-field">
            <span>{{ tr('Schedule Type') }}</span>
            <select v-model="terms.principalRepaymentScheduleType" class="fx-select" :disabled="readOnly">
              <option v-for="o in scheduleTypes" :key="o.value" :value="o.value">{{ tr(o.label) }}</option>
            </select>
          </label>
          <template v-if="!isCustom">
            <label class="fc-field">
              <span>{{ tr('Principal Repayment') }}</span>
              <select v-model="terms.principalRepaymentMethod" class="fx-select" :disabled="readOnly">
                <option v-for="o in principalMethods" :key="o.value" :value="o.value">{{ tr(o.label) }}</option>
              </select>
            </label>
            <label v-if="periodic(terms.principalRepaymentMethod)" class="fc-field">
              <span>{{ tr('Principal Every (Months)') }}</span>
              <input v-model.number="terms.principalFrequencyMonths" class="fx-search" type="number" min="1" step="1" inputmode="numeric" :readonly="readOnly"
                     @blur="terms.principalFrequencyMonths = wholeMonths(terms.principalFrequencyMonths)">
            </label>
            <label v-if="periodic(terms.principalRepaymentMethod)" class="fc-field">
              <span>{{ tr('Principal Start Date') }}</span>
              <input v-model="terms.repaymentStartDate" class="fx-search" type="date" :disabled="readOnly">
            </label>
            <label v-if="monthEnd(terms.principalRepaymentMethod)" class="fc-field">
              <span>{{ tr('Principal First Due') }}</span>
              <select v-model="terms.principalFirstDueCode" class="fx-select" :disabled="readOnly">
                <option v-for="o in firstDueOptions" :key="o.value" :value="o.value">{{ tr(o.label) }}</option>
              </select>
            </label>
            <label v-if="terms.principalRepaymentMethod === 'INVOICE_DUE_DATE'" class="fc-field">
              <span>{{ tr('Principal Distribution') }}</span>
              <select v-model="terms.principalDistributionMethod" class="fx-select" :disabled="readOnly">
                <option v-for="o in distributionMethods" :key="o.value" :value="o.value">{{ tr(o.label) }}</option>
              </select>
            </label>
          </template>
          <label class="fc-field">
            <span>{{ tr('Interest Repayment') }}</span>
            <select v-model="terms.interestRepaymentMethod" class="fx-select" :disabled="readOnly">
              <option v-for="o in interestMethods" :key="o.value" :value="o.value">{{ tr(o.label) }}</option>
            </select>
          </label>
          <label v-if="periodic(terms.interestRepaymentMethod)" class="fc-field">
            <span>{{ tr('Interest Every (Months)') }}</span>
            <input v-model.number="terms.interestFrequencyMonths" class="fx-search" type="number" min="1" step="1" inputmode="numeric" :readonly="readOnly"
                   @blur="terms.interestFrequencyMonths = wholeMonths(terms.interestFrequencyMonths)">
          </label>
          <label v-if="periodic(terms.interestRepaymentMethod)" class="fc-field">
            <span>{{ tr('Interest Start Date') }}</span>
            <input v-model="terms.interestStartDate" class="fx-search" type="date" :disabled="readOnly">
          </label>
          <label v-if="monthEnd(terms.interestRepaymentMethod)" class="fc-field">
            <span>{{ tr('Interest First Due') }}</span>
            <select v-model="terms.interestFirstDueCode" class="fx-select" :disabled="readOnly">
              <option v-for="o in firstDueOptions" :key="o.value" :value="o.value">{{ tr(o.label) }}</option>
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

        <!-- Custom (irregular) schedule: the principal payments the customer agreed, by date; they must add up to the amount financed -->
        <div v-if="isCustom" class="ie-custom">
          <h3 class="ie-subhead">
            {{ tr('Principal Payments') }}
            <span class="ie-ts">{{ tr('Total') }} <b>{{ formatAmount(customTotal) }}</b>
              <template v-if="financedAmount != null"> · {{ tr('Amount Financed') }} <b>{{ formatAmount(financedAmount) }}</b>
                <b v-if="Math.abs(customTotal - financedAmount) > 0.01" class="ie-due"> · {{ tr('Difference') }} {{ formatAmount(round2(financedAmount - customTotal)) }}</b>
              </template>
            </span>
          </h3>
          <div class="fx-table-wrap">
            <table class="fx-table ie-tbl">
              <thead><tr><th>{{ tr('No.') }}</th><th>{{ tr('Due Date') }}</th><th class="fx-r">{{ tr('Principal Amount') }}</th><th class="fx-r"><span class="fx-sr">{{ tr('Actions') }}</span></th></tr></thead>
              <tbody>
                <tr v-for="(r, n) in customSchedule" :key="n">
                  <td>{{ n + 1 }}</td>
                  <td><input v-model="r.dueDate" class="fx-search" type="date" :disabled="readOnly" :aria-label="`${tr('Due Date')} ${n + 1}`"></td>
                  <td class="fx-r"><input v-model.number="r.principalAmount" class="fx-search ie-num" type="number" min="0" step="0.01" inputmode="decimal" :readonly="readOnly"
                                          :aria-label="`${tr('Principal Amount')} ${n + 1}`" @blur="r.principalAmount = round2(r.principalAmount)"></td>
                  <td class="fx-r">
                    <button v-if="!readOnly" type="button" class="fx-btn fx-btn--sm" :title="tr('Delete')" :aria-label="`${tr('Delete')} ${n + 1}`" @click="customSchedule.splice(n, 1)">
                      <span class="fa fa-trash-o" aria-hidden="true"></span></button>
                  </td>
                </tr>
                <tr v-if="!customSchedule.length"><td colspan="4" class="fx-empty">{{ tr('Add the principal payments: a date and an amount each.') }}</td></tr>
              </tbody>
            </table>
          </div>
          <button v-if="!readOnly" type="button" class="fx-btn fx-btn--sm ie-add" @click="customSchedule.push({ dueDate: null, principalAmount: null })">
            <span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Add Payment') }}</button>
        </div>
      </section>

      <!-- Invoice Batch: the batch header and the invoice and cheque files (page 705 Invoice Batch Details, redesigned 1 Nov 2026) -->
      <section class="col-sec">
        <h2><span class="fa fa-files-o" aria-hidden="true"></span>{{ tr('Invoice Batch') }}</h2>
        <div class="col-grid">
          <label class="fc-field" :class="{ 'col-bad': bad('batch.batchNumber') }">
            <span>{{ tr('Batch Number') }} <b>*</b></span>
            <input v-model="batch.batchNumber" class="fx-search" type="text" maxlength="50" :readonly="readOnly">
          </label>
          <label class="fc-field">
            <span>{{ tr('Batch Name') }}</span>
            <input v-model="batch.batchName" class="fx-search" type="text" maxlength="200" :readonly="readOnly">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('batch.batchDate') }">
            <span>{{ tr('Batch Date') }} <b>*</b></span>
            <input v-model="batch.batchDate" class="fx-search" type="date" :disabled="readOnly">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('batch.clientId') }">
            <span>{{ tr('Client') }} <b>*</b></span>
            <select v-model="batch.clientId" class="fx-select" :disabled="readOnly">
              <option :value="null">{{ tr('- Select -') }}</option>
              <option v-for="c in options.clients" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
            </select>
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
          <label class="fc-field ie-wide">
            <span>{{ tr('Remarks') }}</span>
            <input v-model="batch.remarks" class="fx-search" type="text" maxlength="2000" :readonly="readOnly">
          </label>
          <div class="fc-field">
            <span>{{ tr('Total Net Amount') }}</span>
            <div class="col-ro">{{ batch.totalNetAmount == null ? '' : formatAmount(batch.totalNetAmount) }}</div>
          </div>
        </div>

        <template v-if="!readOnly">
          <input ref="invoiceInput" type="file" hidden accept=".xlsx,.csv,.txt" @change="setFile('inv', pick($event))">
          <input ref="chequeInput" type="file" hidden accept=".xlsx,.csv,.txt" @change="setFile('chq', pick($event))">
          <div class="ie-files">
            <div v-for="f in fileSlots" :key="f.key" class="fc-drop ie-drop" :class="{ 'is-set': !!f.file }" role="button" tabindex="0"
                 @click="f.browse()" @keydown.enter="f.browse()" @dragover.prevent @drop.prevent="f.drop($event)">
              <span class="fc-drop-ic" aria-hidden="true"><span class="fa" :class="f.file ? 'fa-file-excel-o' : f.icon"></span></span>
              <b>{{ f.file ? f.file.name : tr(f.title) }}</b>
              <small v-if="f.file">{{ fileSize(f.file.size) }} · <a href="#" @click.prevent.stop="f.clear()">{{ tr('Remove') }}</a></small>
              <small v-else>{{ tr('Drop the file here or click to browse') }} · XLSX, CSV</small>
            </div>
          </div>
          <p v-if="invoiceFile || chequeFile" class="ie-file-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('The rows below are a preview of the chosen files. They are saved only when you click Save Draft.') }}</p>
        </template>
      </section>

      <!-- Invoices -->
      <section class="col-sec">
        <h2><span class="fa fa-list-alt" aria-hidden="true"></span>{{ tr('Invoices') }} <span class="ie-count">{{ invoices.length }}</span></h2>
        <div class="ie-kpis">
          <div><small>{{ tr('Gross Amount') }}</small><b>{{ formatAmount(invoiceTotals.gross) }}</b></div>
          <div><small>{{ tr('Eligible Gross') }}</small><b class="ie-net">{{ formatAmount(invoiceTotals.eligible) }}</b></div>
          <div><small>{{ tr('Advance Amount') }}</small><b>{{ formatAmount(invoiceTotals.advance) }}</b></div>
          <div><small>{{ tr('Eligible') }}</small><b class="ie-net">{{ invoiceTotals.eligibleCount }}</b></div>
          <div><small>{{ tr('Not Eligible') }}</small><b :class="{ 'ie-due': invoiceTotals.rejectedCount > 0 }">{{ invoiceTotals.rejectedCount }}</b></div>
        </div>
        <div class="fx-table-wrap">
          <table class="fx-table ie-tbl">
            <thead>
              <tr>
                <th>{{ tr('Invoice No.') }}</th>
                <th>{{ tr('Invoice Date') }}</th>
                <th>{{ tr('Due Date') }}</th>
                <th class="fx-r">{{ tr('Gross Amount') }}</th>
                <th class="fx-r">{{ tr('Advance %') }}</th>
                <th class="fx-r">{{ tr('Advance Amount') }}</th>
                <th>{{ tr('Eligibility') }}</th>
                <th v-if="!readOnly" class="fx-r"><span class="fx-sr">{{ tr('Actions') }}</span></th>
              </tr>
            </thead>
            <tbody>
              <template v-for="r in invoices" :key="r.invoiceId">
                <tr :class="{ 'is-edit': editInvoice === r.invoiceId, 'is-off': r.statusCode === 'NOT_ELIGIBLE' }">
                  <template v-if="editInvoice === r.invoiceId">
                    <td><input v-model="r.invoiceNumber" class="ie-in" type="text" @input="touchInvoice(r)"></td>
                    <td><input v-model="r.clientInvoiceDate" class="ie-in" type="date" @change="touchInvoice(r)"></td>
                    <td><input v-model="r.dueDate" class="ie-in" type="date" @change="touchInvoice(r)"></td>
                    <td><input v-model.number="r.grossAmount" class="ie-in fx-r" type="number" step="0.01" @input="touchInvoice(r)" @blur="r.grossAmount = round2(r.grossAmount) ?? 0"></td>
                    <td><input v-model.number="r.advanceRatePct" class="ie-in ie-in--sm fx-r" type="number" step="0.01" @input="touchInvoice(r)"></td>
                    <td><input v-model.number="r.advanceAmount" class="ie-in fx-r" type="number" step="0.01" @input="touchInvoice(r)" @blur="r.advanceAmount = round2(r.advanceAmount)"></td>
                  </template>
                  <template v-else>
                    <td><b>{{ r.invoiceNumber }}</b></td>
                    <td>{{ formatDate(r.clientInvoiceDate) }}</td>
                    <td>{{ formatDate(r.dueDate) }}</td>
                    <td class="fx-r">{{ formatAmount(r.grossAmount) }}</td>
                    <td class="fx-r">{{ formatPct(r.advanceRatePct) }}</td>
                    <td class="fx-r"><b>{{ formatAmount(r.advanceAmount) }}</b></td>
                  </template>
                  <td>
                    <select v-model="r.statusCode" class="ie-pill" :class="r.statusCode === 'NOT_ELIGIBLE' ? 'is-bad' : 'is-ok'" :disabled="readOnly || r.statusCode === 'FUNDED'" @change="touchInvoice(r)">
                      <option value="ELIGIBLE">{{ tr('Eligible') }}</option>
                      <option value="NOT_ELIGIBLE">{{ tr('Not Eligible') }}</option>
                      <option v-if="r.statusCode === 'FUNDED'" value="FUNDED" disabled>{{ tr('Funded') }}</option>
                    </select>
                  </td>
                  <td v-if="!readOnly" class="fx-r ie-acts">
                    <button type="button" class="fx-btn fx-btn--sm" :title="tr(editInvoice === r.invoiceId ? 'Done' : 'Edit')" :disabled="busy"
                            @click="editInvoice = editInvoice === r.invoiceId ? null : r.invoiceId">
                      <span class="fa" :class="editInvoice === r.invoiceId ? 'fa-check' : 'fa-pencil'" aria-hidden="true"></span></button>
                    <button type="button" class="fx-btn fx-btn--sm" :title="tr('Delete')" :disabled="busy" @click="dropInvoice(r)"><span class="fa fa-trash-o" aria-hidden="true"></span></button>
                  </td>
                </tr>
                <tr v-if="r.statusCode === 'NOT_ELIGIBLE'" class="ie-reason">
                  <td :colspan="readOnly ? 7 : 8">
                    <label :class="{ 'is-missing': !r.ineligibleReason?.trim() }">
                      <span class="fa fa-exclamation-circle" aria-hidden="true"></span>{{ tr('Ineligible Reason') }}
                      <input v-model="r.ineligibleReason" class="ie-in" type="text" :readonly="readOnly" :placeholder="tr('Why is this invoice not eligible?')" @input="touchInvoice(r)">
                    </label>
                  </td>
                </tr>
              </template>
              <tr v-if="!invoices.length"><td :colspan="readOnly ? 7 : 8" class="fx-empty">{{ tr('No invoices yet. Choose the invoices file above, then click Save Draft.') }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Cheques -->
      <section class="col-sec">
        <h2><span class="fa fa-credit-card" aria-hidden="true"></span>{{ tr('Cheques') }} <span class="ie-count">{{ cheques.length }}</span></h2>
        <div class="ie-kpis ie-kpis--4">
          <div><small>{{ tr('Cheques Amount') }}</small><b class="ie-net">{{ formatAmount(chequeTotals.amount) }}</b></div>
          <div><small>{{ tr('First Due Date') }}</small><b>{{ formatDate(chequeTotals.first) }}</b></div>
          <div><small>{{ tr('Last Due Date') }}</small><b>{{ formatDate(chequeTotals.last) }}</b></div>
          <div><small>{{ tr('Drawers') }}</small><b>{{ chequeTotals.drawers }}</b></div>
        </div>
        <div class="fx-table-wrap">
          <table class="fx-table ie-tbl">
            <thead>
              <tr>
                <th>{{ tr('Cheque No.') }}</th>
                <th>{{ tr('Cheque Date') }}</th>
                <th>{{ tr('Due Date') }}</th>
                <th>{{ tr('Bank') }}</th>
                <th>{{ tr('Drawer') }}</th>
                <th class="fx-r">{{ tr('Amount') }}</th>
                <th>{{ tr('Status') }}</th>
                <th v-if="!readOnly" class="fx-r"><span class="fx-sr">{{ tr('Actions') }}</span></th>
              </tr>
            </thead>
            <tbody>
              <template v-for="c in cheques" :key="c.chequeId">
                <tr :class="{ 'is-edit': editCheque === c.chequeId }">
                  <template v-if="editCheque === c.chequeId">
                    <td><input v-model="c.chequeNo" class="ie-in" type="text" @input="touchCheque(c)"></td>
                    <td><input v-model="c.chequeDate" class="ie-in" type="date" @change="touchCheque(c)"></td>
                    <td><input v-model="c.dueDate" class="ie-in" type="date" @change="touchCheque(c)"></td>
                    <td><input v-model="c.issuerBankName" class="ie-in" type="text" @input="touchCheque(c)"></td>
                    <td><input v-model="c.drawerNameEn" class="ie-in" type="text" @input="touchCheque(c)"></td>
                    <td><input v-model.number="c.chequeAmount" class="ie-in fx-r" type="number" step="0.01" @input="touchCheque(c)" @blur="c.chequeAmount = round2(c.chequeAmount) ?? 0"></td>
                    <td><span class="fx-chip">{{ tr(chequeStatus(c.statusCode)) }}</span></td>
                  </template>
                  <template v-else>
                    <td><b>{{ c.chequeNo }}</b></td>
                    <td>{{ formatDate(c.chequeDate) }}</td>
                    <td>{{ formatDate(c.dueDate) }}</td>
                    <td :class="{ 'ie-placeholder': c.issuerBankName === BANK_PLACEHOLDER }">{{ c.issuerBankName }}</td>
                    <td>{{ c.drawerNameEn }}</td>
                    <td class="fx-r"><b>{{ formatAmount(c.chequeAmount) }}</b> <i class="ie-cur">{{ c.currencyCode }}</i></td>
                    <td><span class="fx-chip" :class="c.statusCode === 'IN_WALLET' ? 'fx-chip--teal' : c.statusCode === 'CANCELLED' ? 'fx-chip--warn' : ''">{{ tr(chequeStatus(c.statusCode)) }}</span></td>
                  </template>
                  <td v-if="!readOnly" class="fx-r ie-acts">
                    <span v-if="c.statusCode === 'IN_WALLET'" class="ie-locked-tag" :title="tr('In the wallet: change it from the Cheques Wallet.')"><span class="fa fa-lock" aria-hidden="true"></span></span>
                    <template v-else>
                    <button type="button" class="fx-btn fx-btn--sm" :title="tr(editCheque === c.chequeId ? 'Done' : 'Edit')" :disabled="busy"
                            @click="editCheque = editCheque === c.chequeId ? null : c.chequeId">
                      <span class="fa" :class="editCheque === c.chequeId ? 'fa-check' : 'fa-pencil'" aria-hidden="true"></span></button>
                    <button type="button" class="fx-btn fx-btn--sm" :title="tr('Delete')" :disabled="busy" @click="dropCheque(c)"><span class="fa fa-trash-o" aria-hidden="true"></span></button>
                    </template>
                  </td>
                </tr>
                <tr v-if="editCheque === c.chequeId" class="ie-more">
                  <td :colspan="8">
                    <div class="ie-more-grid">
                      <label class="fc-field"><span>{{ tr('Currency') }}</span><input v-model="c.currencyCode" class="fx-search" type="text" maxlength="3" @input="touchCheque(c)"></label>
                      <label class="fc-field"><span>{{ tr('Deposit Bank Account Id') }}</span><input v-model.number="c.depositBankAccountId" class="fx-search" type="number" @input="touchCheque(c)"></label>
                      <label class="fc-field"><span>{{ tr('Received Date') }}</span><input v-model="c.receivedDate" class="fx-search" type="date" @change="touchCheque(c)"></label>
                      <label class="fc-field"><span>{{ tr('Deposit Date') }}</span><input v-model="c.depositDate" class="fx-search" type="date" @change="touchCheque(c)"></label>
                      <div class="fc-field"><span>{{ tr('Source Module') }}</span><div class="col-ro">{{ c.sourceModule }}</div></div>
                    </div>
                  </td>
                </tr>
              </template>
              <tr v-if="!cheques.length"><td :colspan="readOnly ? 7 : 8" class="fx-empty">{{ tr('No cheques yet. Choose the cheques file above, then click Save Draft.') }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- page 705 region Calculated Disbursement Charges (FNV_DISBURSEMENT_CHARGES): a preview as soon as the product is chosen
           (Ahmed, 1 Nov 2026), kept when Process Disbursement Request runs. No GL is posted for them here (Ahmed, 2 Nov 2026). -->
      <section class="col-sec">
        <h2>
          <span class="fa fa-calculator" aria-hidden="true"></span>{{ tr('Disbursement Charges') }}
          <span v-if="result?.isPreview && charges.length" class="fx-chip fx-chip--warn ie-chip">{{ tr('Preview') }}</span>
          <span v-else-if="result?.statusCode" class="fx-chip fx-chip--teal ie-chip">{{ result.statusCode }}</span>
          <span v-if="previewing" class="ie-ts"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Calculating...') }}</span>
        </h2>
        <p v-if="result?.isPreview && charges.length" class="ie-hint">
          <span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Calculated from the values on the page. They are kept when you process the request.') }}
        </p>
        <p v-if="previewProblem" class="ie-hint ie-hint--warn">
          <span class="fa fa-exclamation-triangle" aria-hidden="true"></span>{{ previewProblem }}
        </p>
        <div v-if="charges.length && !result?.isPreview" class="ie-totals">
          <div><small>{{ tr('Approved Invoice Amount') }}</small><b>{{ formatAmount(result!.approvedInvoiceAmount) }}</b></div>
          <div><small>{{ tr('Approved Advance Amount') }}</small><b>{{ formatAmount(result!.approvedAdvanceAmount) }}</b></div>
          <div><small>{{ tr('Fee Deducted') }}</small><b>{{ formatAmount(result!.feeDeducted) }}</b></div>
          <div><small>{{ tr('Tax Deducted') }}</small><b>{{ formatAmount(result!.taxDeducted) }}</b></div>
          <div><small>{{ tr('Net Amount') }}</small><b class="ie-net">{{ formatAmount(result!.netAmount) }}</b></div>
        </div>
        <div class="fx-table-wrap">
          <table class="fx-table ie-tbl">
            <thead>
              <tr>
                <th>{{ tr('Charge') }}</th>
                <th>{{ tr('Method') }}</th>
                <th class="fx-r">{{ tr('Base Amount') }}</th>
                <th class="fx-r">{{ tr('Rate') }}</th>
                <th class="fx-r">{{ tr('Amount') }}</th>
                <th class="fx-r">{{ tr('Tax') }}</th>
                <th class="fx-r">{{ tr('Total') }}</th>
                <th>{{ tr('Timing') }}</th>
                <th>{{ tr('Due Date') }}</th>
                <th>{{ tr('Status') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in charges" :key="c.disbursementChargeId">
                <td><b>{{ c.ruleName }}</b><small class="ie-code">{{ c.ruleCode }}</small></td>
                <td>{{ c.calculationMethod }}</td>
                <td class="fx-r">{{ c.baseAmount ? formatAmount(c.baseAmount) : '–' }}</td>
                <td class="fx-r">{{ c.ratePercent == null ? '–' : formatPct(c.ratePercent) }}</td>
                <td class="fx-r">{{ formatAmount(c.chargeAmount) }}</td>
                <td class="fx-r">{{ formatAmount(c.taxAmount) }}</td>
                <td class="fx-r"><b>{{ formatAmount(c.totalAmount) }}</b></td>
                <td>{{ c.chargeTimingCode }}</td>
                <td>{{ formatDate(c.dueDate) }}</td>
                <td><span class="fx-chip" :class="c.statusCode === 'COLLECTED' ? 'fx-chip--teal' : ''">{{ result?.isPreview ? tr('Preview') : c.statusCode }}</span></td>
              </tr>
              <tr v-if="!charges.length"><td colspan="10" class="fx-empty">{{ tr('Choose the product and the limit to see its charges.') }}</td></tr>
            </tbody>
            <tfoot v-if="charges.length">
              <tr><td colspan="4">{{ tr('Total') }}</td><td class="fx-r">{{ formatAmount(chargeTotals.amount) }}</td><td class="fx-r">{{ formatAmount(chargeTotals.tax) }}</td>
                <td class="fx-r"><b>{{ formatAmount(chargeTotals.total) }}</b></td><td colspan="3"></td></tr>
            </tfoot>
          </table>
        </div>

      </section>

      <!-- Customer Claim (Ahmed, 2 Nov 2026): the charges are not paid here. Create Claim claims them from the customer; Collection
           (Customer Payments) pays the claim and posts its GL. CLAIM_STATUS_CODE is kept on the request; Complete needs it Paid. -->
      <section v-if="charges.length || claim?.claim" class="col-sec">
        <h2>
          <span class="fa fa-flag-o" aria-hidden="true"></span>{{ tr('Customer Claim') }}
          <span class="fx-chip ie-chip" :class="claimTone">{{ tr(claimLabel) }}</span>
          <b v-if="claim?.claim" class="ie-ts">{{ claim.claim.claim.claimNumber }}</b>
        </h2>
        <div class="ie-totals ie-totals--3">
          <div><small>{{ tr('Claim Total') }}</small><b>{{ formatAmount(claim?.claim ? claim.claim.claim.totalAmount : chargeTotals.total) }}</b></div>
          <div><small>{{ tr('Paid') }}</small><b class="ie-net">{{ formatAmount(claim?.claim?.claim.paidAmount ?? 0) }}</b></div>
          <div><small>{{ tr('Outstanding') }}</small>
            <b :class="{ 'ie-due': claimOutstanding > 0 }">{{ formatAmount(claimOutstanding) }}</b></div>
        </div>
        <p class="ie-hint"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr(claimHint) }}</p>

        <template v-if="claim?.claim?.payments.length">
          <h3 class="ie-subhead">{{ tr('Payments') }}</h3>
          <div class="fx-table-wrap">
            <table class="fx-table ie-tbl">
              <thead>
                <tr>
                  <th>{{ tr('Date') }}</th><th>{{ tr('Repayment No.') }}</th><th>{{ tr('Source') }}</th><th>{{ tr('Reference') }}</th>
                  <th class="fx-r">{{ tr('Amount') }}</th><th>{{ tr('GL Tran Set ID') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="p in claim.claim.payments" :key="p.claimPaymentId">
                  <td>{{ formatDate(p.paymentDate) }}</td>
                  <td>{{ p.repaymentNumber }}</td>
                  <td>{{ tr(sourceLabel(p.sourceType)) }}</td>
                  <td>{{ p.reference || '–' }}</td>
                  <td class="fx-r"><b>{{ formatAmount(p.paymentAmount) }}</b></td>
                  <td><b class="ie-acct">{{ p.glTranSetId || '–' }}</b></td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>

        <div v-if="!readOnly && !posted" class="col-actions ie-buttons">
          <button v-if="!claim?.claim" type="button" class="fx-btn fx-btn--primary" :disabled="busy || previewing || !charges.length" @click="createClaim">
            <span class="fa fa-flag" aria-hidden="true"></span>{{ tr('Create Claim') }}</button>
          <button v-else-if="claim.claim.claim.paidAmount === 0" type="button" class="fx-btn" :disabled="busy" @click="cancelClaim">
            <span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel Claim') }}</button>
        </div>
      </section>

      <section class="col-sec">
        <h2>
          <span class="fa fa-credit-card" aria-hidden="true"></span>{{ tr('Repayment Schedule') }} <span class="ie-count">{{ schedule.length }}</span>
          <button v-if="!readOnly" type="button" class="fx-btn fx-btn--sm ie-h-btn" :disabled="busy || !schedule.length"
                  :title="tr(schedule.length ? 'One cheque per due date, for its total due' : 'Available once the schedule is created')" @click="chequesFromSchedule">
            <span class="fa fa-money" aria-hidden="true"></span>{{ tr('Create Cheques from Schedule') }}</button>
        </h2>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr>
                <th class="fx-r">{{ tr('No.') }}</th>
                <th>{{ tr('Due Date') }}</th>
                <th class="fx-r">{{ tr('Days') }}</th>
                <th class="fx-r">{{ tr('Opening Principal') }}</th>
                <th class="fx-r">{{ tr('Principal') }}</th>
                <th class="fx-r">{{ tr('Interest') }}</th>
                <th class="fx-r">{{ tr('Total Due') }}</th>
                <th class="fx-r">{{ tr('Closing Principal') }}</th>
                <th class="fx-r">{{ tr('Rate %') }}</th>
                <th>{{ tr('Status') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in schedule" :key="r.installmentNo">
                <td class="fx-r">{{ r.installmentNo }}</td>
                <td>{{ formatDate(r.dueDate) }}</td>
                <td class="fx-r">{{ r.dayCount ?? '' }}</td>
                <td class="fx-r">{{ formatAmount(r.openingPrincipal) }}</td>
                <td class="fx-r">{{ formatAmount(r.principalDue) }}</td>
                <td class="fx-r">{{ formatAmount(r.interestDue) }}</td>
                <td class="fx-r"><b>{{ formatAmount(r.totalInstallment) }}</b></td>
                <td class="fx-r">{{ formatAmount(r.closingPrincipal) }}</td>
                <td class="fx-r">{{ formatPct(r.interestRate) }}</td>
                <td>{{ r.statusCode }}</td>
              </tr>
              <tr v-if="!schedule.length"><td colspan="10" class="fx-empty">{{ tr('No data found') }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- the page only saves a draft; the step's Complete (top of the page) does all the work (agreed with Ahmed, 1 Nov 2026) -->
      <div v-if="!readOnly && !posted" class="col-actions ie-buttons">
        <span class="ie-foot-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Save Draft keeps your work. Complete needs the charges\' claim paid; it then loads the cheques into the wallet.') }}</span>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="apply"><span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save Draft') }}</button>
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
import { computed, nextTick, reactive, ref, watch } from 'vue'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatAmount, formatPct } from '@/shared/format/money'
import { formatDate } from '@/shared/format/datetime'
import { fieldErrors, messageOf } from '@/modules/collection/pages/collection'
import FctStepActions from '@/modules/fct/components/FctStepActions.vue'
import {
  disbursementClaimApi, invoiceEntryApi, processApi, type ChequeRow, type DisbursementClaim, type DisbursementResult, type DisbursementTerms, type InvoiceBatch, type InvoiceEntry,
  type InvoiceEntrySave, type InvoiceRow, type CustomScheduleRow, type InvoiceSummary
} from '../api/invoiceEntry.api'

const props = defineProps<{ disbursementId: number }>()
// empty number inputs come back as '' from v-model.number
const num = (v: unknown) => (v === '' || v == null ? null : Number(v))
// amounts keep 2 decimals (Ahmed, 1 Nov 2026)
function round2(v: unknown): number | null {
  const n = num(v)
  return n == null || Number.isNaN(n) ? null : Math.round((n + Number.EPSILON) * 100) / 100
}
// back to the customer's Factoring Customer Workspace, on its Disbursements tab
const back = computed(() => terms.clientId == null ? { name: 'crm-customers' }
  : { name: 'FactoringCustomerWorkspace', params: { clientId: terms.clientId }, query: { tab: 'disbursements', from: 'customer' } })

// the page's static lists
const dayCounts = ['30/360', 'ACTUAL/360']
// the repayment terms (Ahmed, 2 Nov 2026): regular (frequencies) or custom (principal payments entered); the cheques then match the schedule
const scheduleTypes = [{ value: 'NORMAL_SCHEDULE', label: 'Regular' }, { value: 'CUSTOM_SCHEDULE', label: 'Irregular (custom)' }]
const FREQUENCIES = [
  { value: 'EVERY_N_MONTHS', label: 'Every N Months' }, { value: 'END_OF_MONTH_30', label: 'End of Month (30)' },
  { value: 'END_OF_MONTH_ACTUAL', label: 'End of Month (Actual)' }
]
const principalMethods = computed(() => [
  ...FREQUENCIES, ...(isReverse.value ? [] : [{ value: 'INVOICE_DUE_DATE', label: 'Invoice Due Dates' }]), { value: 'AT_MATURITY', label: 'At Maturity' }
])
const distributionMethods = [{ value: 'EQUAL_AMOUNT', label: 'Equal Amounts' }, { value: 'INVOICE_AMOUNT', label: 'Invoice Amounts' }]
const interestMethods = [...FREQUENCIES, { value: 'WITH_PRINCIPAL', label: 'With Principal' }, { value: 'AT_MATURITY', label: 'At Maturity' }]
const periodic = (m: string | null) => FREQUENCIES.some((f) => f.value === m)
// End of Month: the first due date is this month's end or the next one's (default next)
const monthEnd = (m: string | null) => m === 'END_OF_MONTH_30' || m === 'END_OF_MONTH_ACTUAL'
const firstDueOptions = [{ value: 'NEXT_MONTH', label: 'Next Month' }, { value: 'SAME_MONTH', label: 'Same Month' }]
// N is a whole number of 1 or more
const wholeMonths = (v: unknown) => { const n = num(v); return n == null || Number.isNaN(n) ? null : Math.max(1, Math.round(n)) }
const paymentMethods = ['CHEQUE', 'TRANSFER', 'INTERNAL']

const canEdit = hasPermission('CRM.EDIT')
const readOnly = computed(() => !canEdit)
// reverse factoring (agreed with Ahmed, 31 Oct 2026): the rates are the agreement's; the repayment terms are entered (2 Nov 2026)
const isReverse = computed(() => terms.productId != null && options.reverseProductIds.includes(terms.productId))
const locked = computed(() => readOnly.value || isReverse.value)
const dayCountOptions = computed(() => !terms.baseDayCount || dayCounts.includes(terms.baseDayCount) ? dayCounts : [...dayCounts, terms.baseDayCount])
const ready = ref(false)
const busy = ref(false)
const errors = ref<Record<string, string[]>>({})
const bad = (field: string) => !!errors.value[field]

const terms = reactive({} as DisbursementTerms)
const isCustom = computed(() => terms.principalRepaymentScheduleType === 'CUSTOM_SCHEDULE')
const customSchedule = ref<CustomScheduleRow[]>([])
const customTotal = computed(() => round2(customSchedule.value.reduce((t, r) => t + (Number(r.principalAmount) || 0), 0)) ?? 0)
const batch = reactive({} as InvoiceBatch)
const summary = ref<InvoiceSummary | null>(null)
const result = ref<DisbursementResult | null>(null)
const charges = computed(() => result.value?.charges ?? [])
const schedule = computed(() => result.value?.schedule ?? [])
// the request's claim of its charges (CLAIM_STATUS_CODE and the claim); Complete needs it Paid
const claim = ref<DisbursementClaim | null>(null)
const posted = computed(() => !!claim.value?.isCompleted)
const CLAIM_LABELS: Record<string, string> = {
  NOT_CREATED: 'Claim Not Created', CREATED_UNPAID: 'Claim Created (Unpaid)', PARTIALLY_PAID: 'Claim Partially Paid', PAID: 'Claim Paid'
}
const claimStatus = computed(() => claim.value?.claimStatusCode ?? 'NOT_CREATED')
const claimLabel = computed(() => CLAIM_LABELS[claimStatus.value] ?? claimStatus.value)
const claimTone = computed(() => claimStatus.value === 'PAID' ? 'fx-chip--teal' : claimStatus.value === 'NOT_CREATED' ? '' : 'fx-chip--warn')
const claimOutstanding = computed(() => claim.value?.claim ? claim.value.claim.claim.outstandingAmount : chargeTotals.value.total)
const claimHint = computed(() => ({
  NOT_CREATED: 'The charges are paid by the customer through a claim. Create Claim saves the request and claims the charges shown. Invoices and cheques are checked at Complete.',
  CREATED_UNPAID: 'The claim is paid in Collection and Repayments (Customer Payments), or from the Claims tab of the Factoring Workspace. Charges are locked; cancel the claim to change them.',
  PARTIALLY_PAID: 'The claim is partly paid. Complete is possible once it is fully paid in Collection and Repayments.',
  PAID: 'The claim is paid. The step can be completed.'
} as Record<string, string>)[claimStatus.value] ?? '')
const SOURCE_LABELS: Record<string, string> = { CHEQUE: 'Collected Cheque', BANK_TRANSFER: 'Bank Transfer', CREDIT_BALANCE: 'Credit Balance' }
const sourceLabel = (t: string) => SOURCE_LABELS[t] ?? t
const CHEQUE_STATUS: Record<string, string> = { DRAFT: 'Draft', IN_WALLET: 'In Wallet', CANCELLED: 'Cancelled' }
const chequeStatus = (code: string) => CHEQUE_STATUS[code] ?? code
const completeNote = 'Your draft is saved, then in one step: the request is processed, the charges\' claim must be paid, and the cheques are loaded into the wallet with their memo GL entry. If any step fails, only the draft stays.'
const options = reactive<InvoiceEntry['options']>({ clients: [], limits: [], currencies: [], products: [], reverseProductIds: [] })
const invoices = ref<InvoiceRow[]>([])
// Reverse factoring: Invoices Gross Amount follows the invoices grid, the total of the rows marked Eligible (0 when none are loaded)
watch([isReverse, invoices], () => {
  if (!isReverse.value) return
  terms.grossAmount = round2(invoices.value.filter((r) => r.statusCode === 'ELIGIBLE').reduce((t, r) => t + (Number(r.grossAmount) || 0), 0))
}, { deep: true, immediate: true })
const cheques = ref<ChequeRow[]>([])
const changedInvoices = new Set<number>()
const changedCheques = new Set<number>()
const deletedInvoices = ref<number[]>([])
const deletedCheques = ref<number[]>([])
const invoiceFile = ref<File | null>(null)
const chequeFile = ref<File | null>(null)
const invoicesPreviewed = ref(false)
const chequesPreviewed = ref(false)
const invoiceInput = ref<HTMLInputElement | null>(null)
const chequeInput = ref<HTMLInputElement | null>(null)

const whole = (n: number) => Math.round(n).toLocaleString('en-US')
const pick = (e: Event) => (e.target as HTMLInputElement).files?.[0] ?? null
const fileSize = (n: number) => (n >= 1048576 ? `${(n / 1048576).toFixed(1)} MB` : `${Math.max(1, Math.round(n / 1024))} KB`)

// the two file boxes: drop or browse an invoices file and a cheques file; Save Draft loads both
const fileSlots = computed(() => [
  {
    key: 'inv', title: 'Invoices File', icon: 'fa-file-text-o', file: invoiceFile.value,
    browse: () => invoiceInput.value?.click(),
    drop: (e: DragEvent) => { const f = e.dataTransfer?.files?.[0]; if (f) void setFile('inv', f) },
    clear: () => { if (invoiceInput.value) invoiceInput.value.value = ''; void setFile('inv', null) }
  },
  {
    key: 'chq', title: 'Cheques File', icon: 'fa-credit-card', file: chequeFile.value,
    browse: () => chequeInput.value?.click(),
    drop: (e: DragEvent) => { const f = e.dataTransfer?.files?.[0]; if (f) void setFile('chq', f) },
    clear: () => { if (chequeInput.value) chequeInput.value.value = ''; void setFile('chq', null) }
  }
])

// the row being edited in each grid (the others show formatted values)
const editInvoice = ref<number | null>(null)
const editCheque = ref<number | null>(null)

const invoiceTotals = computed(() => {
  const rows = invoices.value
  const eligible = rows.filter((r) => r.statusCode === 'ELIGIBLE')
  const sum = (list: InvoiceRow[], f: (r: InvoiceRow) => number | null) => round2(list.reduce((t, r) => t + (Number(f(r)) || 0), 0)) ?? 0
  return {
    gross: sum(rows, (r) => r.grossAmount), eligible: sum(eligible, (r) => r.grossAmount), advance: sum(eligible, (r) => r.advanceAmount),
    eligibleCount: eligible.length, rejectedCount: rows.length - eligible.length
  }
})
const chequeTotals = computed(() => {
  const live = cheques.value.filter((c) => c.statusCode !== 'CANCELLED' && c.statusCode !== 'REPLACED')
  const dues = live.map((c) => c.dueDate).filter(Boolean).sort()
  return {
    amount: round2(live.reduce((t, c) => t + (Number(c.chequeAmount) || 0), 0)) ?? 0,
    first: dues[0] ?? null, last: dues[dues.length - 1] ?? null, drawers: new Set(live.map((c) => c.drawerNameEn)).size
  }
})
// what the custom schedule must add up to: the approved advance plus the charges financed with the principal
const financedAmount = computed(() => result.value && result.value.approvedAdvanceAmount
  ? round2(result.value.approvedAdvanceAmount + charges.value.filter((c) => c.chargeTimingCode === 'FINANCE_WITH_PRINCIPAL').reduce((t, c) => t + c.totalAmount, 0))
  : null)
const chargeTotals = computed(() => ({
  amount: round2(charges.value.reduce((t, c) => t + c.chargeAmount, 0)) ?? 0,
  tax: round2(charges.value.reduce((t, c) => t + c.taxAmount, 0)) ?? 0,
  total: round2(charges.value.reduce((t, c) => t + c.totalAmount, 0)) ?? 0
}))
// show() fills the page from the server: the values it shows are the base, and only a change from them starts a charges preview
let shownKey = ''
function show(page: InvoiceEntry) {
  void nextTick(() => { shownKey = previewKey.value })
  Object.assign(terms, page.terms)
  Object.assign(batch, page.batch)
  Object.assign(options, page.options)
  summary.value = page.summary
  invoices.value = page.invoices
  cheques.value = page.cheques
  customSchedule.value = page.customSchedule.map((r) => ({ ...r }))
  changedInvoices.clear()
  changedCheques.clear()
  deletedInvoices.value = []
  deletedCheques.value = []
}

async function load() {
  ready.value = false
  errors.value = {}
  try {
    const [page, processed, claimed] = await Promise.all([
      invoiceEntryApi.get(props.disbursementId), processApi.result(props.disbursementId), disbursementClaimApi.get(props.disbursementId)
    ])
    show(page)
    markSaved()
    result.value = processed
    claim.value = claimed
    if (!claimed.isCompleted && !processed.charges.length && terms.productId != null) void nextTick(previewCharges)
  } catch (err) {
    showError(messageOf(err))
  } finally {
    ready.value = true
  }
}

// the page as last loaded or saved: Create Claim needs Save Draft first when anything changed (Ahmed, 2 Nov 2026)
let savedKey = ''
const pageKey = () => JSON.stringify(payload())
const markSaved = () => { void nextTick(() => { savedKey = pageKey() }) }
const unsaved = () => !!invoiceFile.value || !!chequeFile.value || pageKey() !== savedKey

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

// Create Cheques from Schedule (Ahmed, 2 Nov 2026): one draft cheque per due date for its total due, numbered 1, 2, 3; the user
// edits the numbers and enters the bank (Complete refuses "Enter Bank Name"). Nothing is saved until Save Draft.
const BANK_PLACEHOLDER = 'Enter Bank Name'
function chequesFromSchedule() {
  const open = cheques.value.filter((c) => c.statusCode !== 'IN_WALLET' && c.statusCode !== 'CANCELLED')
  if (open.length && !window.confirm(tr('Replace the {0} cheques with cheques from the schedule?').replace('{0}', String(open.length)))) return
  open.forEach(dropCheque)
  if (chequeFile.value) { chequeFile.value = null; chequesPreviewed.value = false; if (chequeInput.value) chequeInput.value.value = '' }
  const dues = new Map<string, number>()
  for (const r of schedule.value) dues.set(r.dueDate, (dues.get(r.dueDate) ?? 0) + (Number(r.totalInstallment) || 0))
  const today = new Date().toISOString().slice(0, 10)
  const drawer = options.clients.find((o) => o.value === String(terms.clientId))?.label ?? ''
  const currency = options.currencies.find((o) => o.value === String(terms.currencyId))?.label ?? ''
  let id = Math.min(0, ...cheques.value.map((c) => c.chequeId))
  const made: ChequeRow[] = [...dues.entries()].sort(([a], [b]) => a.localeCompare(b)).map(([dueDate, amount], i) => ({
    chequeId: --id, chequeNo: String(i + 1), chequeDate: terms.disbursementDate ?? today, dueDate, issuerBankName: BANK_PLACEHOLDER,
    drawerNameEn: drawer, depositBankAccountId: null, chequeAmount: round2(amount) ?? 0, currencyCode: currency, sourceModule: null,
    receivedDate: today, depositDate: null, statusCode: 'DRAFT'
  }))
  made.forEach((c) => changedCheques.add(c.chequeId))
  cheques.value = [...cheques.value, ...made]
  showSuccess(tr('{0} cheques created from the schedule. Enter each bank name, then click Save Draft.').replace('{0}', String(made.length)))
}

// the page's required items, checked before the submit as APEX does
function check(): boolean {
  const e: Record<string, string[]> = {}
  if (!terms.disbursementDate) e['terms.disbursementDate'] = ['Disbursement Date must have some value.']
  if (terms.clientId == null) e['terms.clientId'] = ['Client Id must have some value.']
  if (terms.currencyId == null) e['terms.currencyId'] = ['Currency must have some value.']
  if (!isReverse.value && (terms.grossAmount == null || (terms.grossAmount as unknown) === '')) e['terms.grossAmount'] = ['Invoices Gross Amount must have some value.']
  if (!batch.batchNumber?.trim()) e['batch.batchNumber'] = ['Batch Number must have some value.']
  if (batch.clientId == null) e['batch.clientId'] = ['Client must have some value.']
  if (!batch.batchDate) e['batch.batchDate'] = ['Batch Date must have some value.']
  if (!batch.batchType) e['batch.batchType'] = ['Batch Type must have some value.']
  if (batch.currencyId == null) e['batch.currencyId'] = ['Currency must have some value.']
  errors.value = e
  if (Object.keys(e).length) showError(Object.values(e).flat().map((m) => tr(m)).join('\n'))
  return !Object.keys(e).length
}


const payload = (): InvoiceEntrySave => ({
  terms: {
    ...terms, grossAmount: num(terms.grossAmount), advanceRatePct: num(terms.advanceRatePct), tenorMonth: num(terms.tenorMonth),
    spreadRate: num(terms.spreadRate), remarks: terms.remarks?.trim() || null
  },
  batch: { ...batch, batchNumber: batch.batchNumber?.trim() || null, remarks: batch.remarks?.trim() || null },
  invoices: invoices.value.filter((r) => (invoiceFile.value && invoicesPreviewed.value) || changedInvoices.has(r.invoiceId))
    .map((r) => ({ ...r, grossAmount: Number(r.grossAmount), advanceRatePct: num(r.advanceRatePct), advanceAmount: num(r.advanceAmount) })),
  deletedInvoiceIds: deletedInvoices.value,
  cheques: cheques.value.filter((c) => (chequeFile.value && chequesPreviewed.value && c.statusCode === 'DRAFT') || changedCheques.has(c.chequeId))
    .map((c) => ({ ...c, chequeAmount: Number(c.chequeAmount), depositBankAccountId: num(c.depositBankAccountId) })),
  deletedChequeIds: deletedCheques.value,
  customSchedule: isCustom.value ? customSchedule.value.map((r) => ({ dueDate: r.dueDate || null, principalAmount: num(r.principalAmount) })) : undefined,
  invoicesPreviewed: !!invoiceFile.value && invoicesPreviewed.value,
  chequesPreviewed: !!chequeFile.value && chequesPreviewed.value
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

// Save Draft: the page, and the chosen files only now (choosing a file saves nothing; Ahmed, 1 Nov 2026)
async function saveDraft(): Promise<string> {
  if (!invoiceFile.value && !chequeFile.value) {
    show(await invoiceEntryApi.save(props.disbursementId, payload()))
    markSaved()
    return tr('Draft saved.')
  }
  const result = await invoiceEntryApi.upload(props.disbursementId, payload(), invoiceFile.value, chequeFile.value)
  show(result.page)
  markSaved()
  invoiceFile.value = null
  chequeFile.value = null
  invoicesPreviewed.value = false
  chequesPreviewed.value = false
  if (invoiceInput.value) invoiceInput.value.value = ''
  if (chequeInput.value) chequeInput.value.value = ''
  return tr(result.message)
}

const apply = () => check() && run(async () => {
  const message = await saveDraft()
  void previewCharges()
  showSuccess(message)
})

// choosing or removing a file shows the files' rows at once, as a preview: nothing is saved until Save Draft (Ahmed, 1 Nov 2026)
async function setFile(kind: 'inv' | 'chq', file: File | null) {
  if (kind === 'inv') { invoiceFile.value = file; invoicesPreviewed.value = false } else { chequeFile.value = file; chequesPreviewed.value = false }
  if (!invoiceFile.value && !chequeFile.value) { await load(); return }
  if (!check()) return
  await run(async () => {
    const result = await invoiceEntryApi.upload(props.disbursementId, payload(), invoiceFile.value, chequeFile.value, true)
    show(result.page)
    invoicesPreviewed.value = !!invoiceFile.value
    chequesPreviewed.value = !!chequeFile.value
    void previewCharges()
  })
}

// Create Claim: the saved page's charges are calculated and claimed from the customer; unsaved changes need Save Draft first (Ahmed, 2 Nov 2026)
const createClaim = () => {
  if (unsaved()) { showError(tr('Click Save Draft first: the page has changes that are not saved.')); return }
  return check() && createClaimRun()
}
const createClaimRun = () => run(async () => {
  claim.value = await disbursementClaimApi.create(props.disbursementId, payload())
  const [page, processed] = await Promise.all([invoiceEntryApi.get(props.disbursementId), processApi.result(props.disbursementId)])
  show(page)
  markSaved()
  result.value = processed
  showSuccess(tr('Claim {0} created.').replace('{0}', claim.value.claim?.claim.claimNumber ?? ''))
})

// Cancel Claim, while nothing of it is paid: the charges can change again
const cancelClaim = () => claim.value?.claim && run(async () => {
  const no = claim.value!.claim!.claim.claimNumber
  if (!window.confirm(tr('Cancel claim {0}?').replace('{0}', no))) return
  claim.value = await disbursementClaimApi.cancel(props.disbursementId, claim.value!.claim!.claim.claimId)
  showSuccess(tr('Claim {0} cancelled.').replace('{0}', no))
})

// the step's Complete saves the draft first, files included
const saveForStep = async () => (readOnly.value ? true : check() && (await run(async () => {
  await saveDraft()
})))

// the charges as soon as the product is chosen (Ahmed, 1 Nov 2026): recalculated, not kept, when the terms or the invoices change
const previewing = ref(false)
const previewProblem = ref('')
let previewTimer: ReturnType<typeof setTimeout> | undefined
let previewSeq = 0
async function previewCharges() {
  if (readOnly.value || !ready.value) return
  const seq = ++previewSeq
  shownKey = previewKey.value
  previewing.value = true
  try {
    const preview = invoiceFile.value || chequeFile.value
      ? await processApi.previewFiles(props.disbursementId, payload(), invoiceFile.value, chequeFile.value)
      : await processApi.preview(props.disbursementId, payload())
    if (seq !== previewSeq) return
    result.value = preview
    previewProblem.value = ''
  } catch (err) {
    if (seq === previewSeq) previewProblem.value = messageOf(err)
  } finally {
    if (seq === previewSeq) previewing.value = false
  }
}
const previewKey = computed(() => JSON.stringify([
  terms.productId, terms.limitId, terms.disbursementDate, terms.grossAmount, terms.advanceRatePct, terms.tenorMonth,
  invoices.value.map((r) => `${r.statusCode}|${r.grossAmount}|${r.advanceAmount}`),
  // the repayment terms rebuild the schedule shown
  terms.spreadRate, terms.baseDayCount, terms.principalRepaymentScheduleType, terms.principalRepaymentMethod, terms.principalFrequencyMonths,
  terms.repaymentStartDate, terms.principalDistributionMethod, terms.interestRepaymentMethod, terms.interestFrequencyMonths, terms.interestStartDate,
  terms.principalFirstDueCode, terms.interestFirstDueCode,
  isCustom.value ? customSchedule.value.map((r) => `${r.dueDate}|${r.principalAmount}`) : []
]))
watch(previewKey, () => {
  clearTimeout(previewTimer)
  previewTimer = setTimeout(() => { if (previewKey.value !== shownKey) void previewCharges() }, 600)
})

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
.ie-locked { background: #F1F4F6; color: #3D4A55; cursor: default; }
.ie-custom { margin-top: 16px; }
.ie-custom .fx-search { width: 100%; max-width: 220px; }
.ie-custom .ie-num { text-align: right; margin-inline-start: auto; display: block; }
.ie-add { margin-top: 10px; }
.ie-totals.ie-totals--3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
.ie-due { color: #E87722; }
.ie-subhead { margin: 16px 0 8px; font-size: 13px; font-weight: 700; color: var(--fin-ink, #1d2a33); }
.ie-ts { margin-inline-start: auto; display: inline-flex; align-items: center; gap: 6px; font-size: 12px; font-weight: 500; color: var(--fin-muted, #6b7a86); }
.ie-ts b { font-family: monospace; font-size: 13px; color: #00636D; }
.ie-src { margin-inline-start: 6px; font-style: normal; font-size: 11px; font-weight: 600; color: #E87722; }
.ie-chip { margin-inline-start: 6px; }
.ie-sub { font-size: 12px; color: #6b7c7b; }
.ie-totals { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 12px; margin-bottom: 14px; }
.ie-totals > div { padding: 10px 14px; border: 1px solid var(--fin-line); border-radius: 10px; }
.ie-totals small { display: block; font-size: 11px; color: #6b7c7b; text-transform: uppercase; letter-spacing: .04em; }
.ie-totals b { font-size: 16px; }
.ie-net { color: #00636D; }
.ie-buttons { justify-content: flex-end; margin-top: 16px; }
.ie-count { margin-inline-start: 6px; padding: 1px 8px; border-radius: 999px; background: #E6F3F4; color: #00636D; font-size: 12px; }

.ie-h-btn { margin-inline-start: auto; }
.ie-placeholder { color: var(--fx-warn, #b45309); font-style: italic; }
/* upload boxes */
.ie-files { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 14px; margin-top: 16px; }
.ie-drop { padding: 16px; }
.ie-drop.is-set { border-style: solid; border-color: #00636D; background: #fff; }
.ie-drop.is-set .fc-drop-ic { color: #00636D; background: #E6F3F4; }
.ie-drop a { color: #E87722; font-weight: 600; }

/* KPI strip over a grid */
.ie-kpis { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 10px; margin-bottom: 12px; }
.ie-kpis--4 { grid-template-columns: repeat(4, minmax(0, 1fr)); }
.ie-kpis > div { padding: 8px 12px; border-radius: 10px; background: #F5F8F9; border-inline-start: 3px solid #00636D; }
.ie-kpis small { display: block; font-size: 10px; color: #6b7c7b; text-transform: uppercase; letter-spacing: .04em; }
.ie-kpis b { font-size: 15px; }

/* grids: formatted values, one row edited at a time */
.ie-tbl td { vertical-align: middle; }
.ie-tbl tfoot td { font-weight: 700; background: #F5F8F9; border-top: 2px solid var(--fin-line); }
.ie-tbl tr.is-edit > td { background: #FFF8F2; }
.ie-tbl tr.is-off > td { color: #8a949c; }
.ie-acts { white-space: nowrap; }
.ie-acts .fx-btn + .fx-btn { margin-inline-start: 4px; }
.ie-in { width: 100%; min-width: 96px; padding: 6px 8px; border: 1px solid var(--fin-line); border-radius: 6px; background: #fff; font: inherit; }
.ie-in:focus { border-color: #00636D; outline: none; box-shadow: 0 0 0 3px rgba(0, 99, 109, .12); }
.ie-in--sm { min-width: 64px; }
.ie-pill { padding: 3px 26px 3px 10px; border-radius: 999px; border: 1px solid transparent; font: inherit; font-size: 12px; font-weight: 600; cursor: pointer; }
.ie-pill.is-ok { background: #E6F3F4; color: #00636D; }
.ie-pill.is-bad { background: #FDEEE3; color: #c25a0a; }
.ie-pill:disabled { cursor: default; }
.ie-reason > td { padding-top: 0; background: #FFFBF7; }
.ie-reason label { display: flex; align-items: center; gap: 8px; font-size: 12px; font-weight: 600; color: #c25a0a; }
.ie-reason label .ie-in { flex: 1; max-width: 560px; }
.ie-reason label.is-missing .ie-in { border-color: #E87722; }
.ie-more > td { background: #FFF8F2; }
.ie-more-grid { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 10px; }
.ie-cur { font-style: normal; font-size: 11px; color: #6b7c7b; }
.ie-code { display: block; font-size: 11px; color: #6b7c7b; font-family: monospace; }
.ie-acct { font-family: monospace; color: #00636D; }
.ie-cr { padding-inline-start: 24px; }
.ie-tick { width: 36px; text-align: center; }
.ie-tick input { width: 16px; height: 16px; accent-color: #00636D; cursor: pointer; }
.ie-tbl tr.is-chosen > td { background: #EEF6F7; }
.ie-locked-tag { color: #8a949c; padding: 0 8px; }
.ie-file-note { display: flex; align-items: center; gap: 6px; margin: 10px 0 0; font-size: 12px; color: #00636D; }
.ie-foot-note { display: inline-flex; align-items: center; gap: 6px; margin-inline-end: auto; font-size: 12px; color: #6b7c7b; }
.ie-hint { display: flex; align-items: center; gap: 8px; margin: 0 0 12px; font-size: 12px; color: #6b7c7b; }
.ie-hint--warn { color: #c25a0a; }

/* GL entries, collapsed */
.ie-gl { margin-top: 14px; border: 1px solid var(--fin-line); border-radius: 10px; }
.ie-gl > summary { display: flex; align-items: center; gap: 8px; padding: 10px 14px; cursor: pointer; font-weight: 700; color: #00636D; list-style: none; }
.ie-gl > summary::-webkit-details-marker { display: none; }
.ie-gl > summary::after { content: '\f107'; font-family: FontAwesome; margin-inline-start: 8px; transition: transform .15s; }
.ie-gl[open] > summary::after { transform: rotate(180deg); }
.ie-gl[open] > summary { border-bottom: 1px solid var(--fin-line); }
.ie-journal { padding: 0 14px 14px; }
@media (max-width: 760px) {
  .ie-wide { grid-column: auto; }
  .ie-totals { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .ie-files, .ie-more-grid { grid-template-columns: 1fr; }
  .ie-kpis, .ie-kpis--4 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
</style>
