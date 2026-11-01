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
            <span>{{ tr('Tenor Months') }}<em v-if="isReverse" class="ie-src">{{ tr('From cheques') }}</em></span>
            <input v-model.number="terms.tenorMonth" class="fx-search" type="number" min="0" step="1" inputmode="numeric" :readonly="locked" :class="{ 'ie-locked': isReverse }">
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
          <label class="fc-field">
            <span>{{ tr('Repayment Start Date') }}<em v-if="isReverse" class="ie-src">{{ tr('First cheque') }}</em></span>
            <input v-model="terms.repaymentStartDate" class="fx-search" type="date" :disabled="locked" :class="{ 'ie-locked': isReverse }">
          </label>
          <label class="fc-field">
            <span>{{ tr('Principal Repayment Schedule Type') }}<em v-if="isReverse" class="ie-src">{{ tr('From cheques') }}</em></span>
            <select v-model="terms.principalRepaymentScheduleType" class="fx-select" :disabled="locked" :class="{ 'ie-locked': isReverse }">
              <option v-for="o in scheduleTypes" :key="o" :value="o">{{ o }}</option>
            </select>
          </label>
          <label class="fc-field">
            <span>{{ tr('Principal Repayment Method') }}<em v-if="isReverse" class="ie-src">{{ tr('From cheques') }}</em></span>
            <select v-model="terms.principalRepaymentMethod" class="fx-select" :disabled="locked" :class="{ 'ie-locked': isReverse }">
              <option v-for="o in principalMethods" :key="o" :value="o">{{ o }}</option>
            </select>
          </label>
          <label class="fc-field">
            <span>{{ tr('Principal Distribution Method') }}<em v-if="isReverse" class="ie-src">{{ tr('From cheques') }}</em></span>
            <select v-model="terms.principalDistributionMethod" class="fx-select" :disabled="locked" :class="{ 'ie-locked': isReverse }">
              <option v-for="o in distributionMethods" :key="o" :value="o">{{ o }}</option>
            </select>
          </label>
          <label class="fc-field">
            <span>{{ tr('Interest Repayment Method') }}<em v-if="isReverse" class="ie-src">{{ tr('From cheques') }}</em></span>
            <select v-model="terms.interestRepaymentMethod" class="fx-select" :disabled="locked" :class="{ 'ie-locked': isReverse }">
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
          <input ref="invoiceInput" type="file" hidden accept=".xlsx,.csv,.txt" @change="invoiceFile = pick($event)">
          <input ref="chequeInput" type="file" hidden accept=".xlsx,.csv,.txt" @change="chequeFile = pick($event)">
          <div class="ie-files">
            <div v-for="f in fileSlots" :key="f.key" class="fc-drop ie-drop" :class="{ 'is-set': !!f.file }" role="button" tabindex="0"
                 @click="f.browse()" @keydown.enter="f.browse()" @dragover.prevent @drop.prevent="f.drop($event)">
              <span class="fc-drop-ic" aria-hidden="true"><span class="fa" :class="f.file ? 'fa-file-excel-o' : f.icon"></span></span>
              <b>{{ f.file ? f.file.name : tr(f.title) }}</b>
              <small v-if="f.file">{{ fileSize(f.file.size) }} · <a href="#" @click.prevent.stop="f.clear()">{{ tr('Remove') }}</a></small>
              <small v-else>{{ tr('Drop the file here or click to browse') }} · XLSX, CSV</small>
            </div>
          </div>
          <div class="col-actions ie-buttons">
            <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || (!invoiceFile && !chequeFile)" @click="upload">
              <span class="fa fa-cloud-upload" aria-hidden="true"></span>{{ tr('Upload Files') }}</button>
          </div>
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
                    <select v-model="r.statusCode" class="ie-pill" :class="r.statusCode === 'ELIGIBLE' ? 'is-ok' : 'is-bad'" :disabled="readOnly" @change="touchInvoice(r)">
                      <option value="ELIGIBLE">{{ tr('Eligible') }}</option>
                      <option value="NOT_ELIGIBLE">{{ tr('Not Eligible') }}</option>
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
              <tr v-if="!invoices.length"><td :colspan="readOnly ? 7 : 8" class="fx-empty">{{ tr('No invoices yet. Upload the invoices file above.') }}</td></tr>
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
                    <td>{{ c.issuerBankName }}</td>
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
              <tr v-if="!cheques.length"><td :colspan="readOnly ? 7 : 8" class="fx-empty">{{ tr('No cheques yet. Upload the cheques file above.') }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- page 705 region Calculated Disbursement Charges (FNV_DISBURSEMENT_CHARGES): a preview as soon as the product is chosen
           (Ahmed, 1 Nov 2026), kept when Process Disbursement Request runs; and the GL entries they post, collapsed -->
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

        <details v-if="charges.length" class="ie-gl">
          <summary>
            <span class="fa fa-book" aria-hidden="true"></span>{{ tr('GL Entries') }}
            <span class="ie-count">{{ glEntries.length }}</span>
            <span v-if="payment?.glTranSetId" class="ie-ts">{{ tr('GL Tran Set ID') }} <b>{{ payment.glTranSetId }}</b></span>
            <span v-else class="ie-ts">{{ tr('Posted when the step is completed') }}</span>
          </summary>
          <div v-if="!glEntries.length" class="fx-empty">{{ tr('No GL lines are set up for these charges. Run the charges GL setup script.') }}</div>
          <div v-for="j in glJournals" :key="j.no" class="ie-journal">
            <h3 class="ie-subhead">{{ tr('Journal') }} {{ j.no }} · {{ tr(j.no === 1 ? 'Charges' : j.no === 2 ? 'Charges Payment' : 'Other') }}</h3>
            <div class="fx-table-wrap">
              <table class="fx-table ie-tbl">
                <thead>
                  <tr><th>{{ tr('Account') }}</th><th>{{ tr('Account Name') }}</th><th>{{ tr('Charge') }}</th>
                    <th class="fx-r">{{ tr('Debit') }}</th><th class="fx-r">{{ tr('Credit') }}</th></tr>
                </thead>
                <tbody>
                  <tr v-for="l in j.lines" :key="`${l.chargeCode}-${l.lineNo}`">
                    <td><b class="ie-acct">{{ l.accountCode ?? '–' }}</b></td>
                    <td :class="{ 'ie-cr': l.debitCreditCode === 'C' }">{{ l.accountName ?? '–' }}</td>
                    <td><span class="ie-code">{{ l.chargeCode }}</span></td>
                    <td class="fx-r">{{ l.debitCreditCode === 'D' ? formatAmount(l.amount) : '' }}</td>
                    <td class="fx-r">{{ l.debitCreditCode === 'C' ? formatAmount(l.amount) : '' }}</td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr><td colspan="3">{{ tr('Total') }}</td><td class="fx-r"><b>{{ formatAmount(j.debit) }}</b></td><td class="fx-r"><b>{{ formatAmount(j.credit) }}</b></td></tr>
                </tfoot>
              </table>
            </div>
          </div>
        </details>
      </section>

      <!-- Charges Payment (agreed with Ahmed, 1 Nov 2026): on the draft the user ticks the customer's payment sources that pay the charges;
           Complete pays from them in that order, and the payments show here once it ran -->
      <section v-if="charges.length || payment?.payments.length" class="col-sec">
        <h2>
          <span class="fa fa-money" aria-hidden="true"></span>{{ tr('Charges Payment') }}
          <span v-if="posted" class="fx-chip fx-chip--teal ie-chip">{{ tr('Paid') }}</span>
          <span v-else class="fx-chip ie-chip" :class="cover.short > 0 ? 'fx-chip--warn' : 'fx-chip--teal'">
            {{ tr(cover.short > 0 ? 'Not Covered' : 'Covered') }}</span>
          <span v-if="payment?.glTranSetId" class="ie-ts"><span class="fa fa-book" aria-hidden="true"></span>{{ tr('GL Tran Set ID') }}
            <b>{{ payment.glTranSetId }}</b></span>
        </h2>
        <div class="ie-totals ie-totals--3">
          <div><small>{{ tr('Total Charges') }}</small><b>{{ formatAmount(posted ? payment!.totalCharges : chargeTotals.total) }}</b></div>
          <div><small>{{ tr(posted ? 'Paid' : 'Chosen Sources Cover') }}</small><b class="ie-net">{{ formatAmount(posted ? payment!.paidAmount : cover.covered) }}</b></div>
          <div><small>{{ tr(posted ? 'Outstanding' : 'Still to Cover') }}</small>
            <b :class="{ 'ie-due': (posted ? payment!.outstandingAmount : cover.short) > 0 }">{{ formatAmount(posted ? payment!.outstandingAmount : cover.short) }}</b></div>
        </div>

        <template v-if="!posted">
          <p class="ie-hint"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Tick the sources that pay the charges. Complete pays from them in the order ticked.') }}</p>
          <div class="fx-table-wrap">
            <table class="fx-table ie-tbl">
              <thead>
                <tr>
                  <th class="ie-tick"><span class="fx-sr">{{ tr('Use') }}</span></th>
                  <th>{{ tr('Repayment No.') }}</th><th>{{ tr('Source') }}</th><th>{{ tr('Reference') }}</th><th>{{ tr('Value Date') }}</th>
                  <th class="fx-r">{{ tr('Amount') }}</th><th class="fx-r">{{ tr('Used') }}</th><th class="fx-r">{{ tr('Available') }}</th>
                  <th class="fx-r">{{ tr('Order') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="src in payment?.sources ?? []" :key="src.repaymentSourceId" :class="{ 'is-chosen': chosen.includes(src.repaymentSourceId) }">
                  <td class="ie-tick"><input :id="`src-${src.repaymentSourceId}`" type="checkbox" :checked="chosen.includes(src.repaymentSourceId)"
                                            :disabled="readOnly" @change="toggleSource(src.repaymentSourceId)"></td>
                  <td><label :for="`src-${src.repaymentSourceId}`"><b>{{ src.repaymentNumber }}</b></label></td>
                  <td>{{ tr(sourceLabel(src.sourceType)) }}</td>
                  <td>{{ src.reference || '–' }}</td>
                  <td>{{ formatDate(src.valueDate) }}</td>
                  <td class="fx-r">{{ formatAmount(src.sourceAmount) }}</td>
                  <td class="fx-r">{{ formatAmount(src.usedAmount) }}</td>
                  <td class="fx-r"><b>{{ formatAmount(src.availableAmount) }}</b></td>
                  <td class="fx-r">{{ chosen.includes(src.repaymentSourceId) ? chosen.indexOf(src.repaymentSourceId) + 1 : '' }}</td>
                </tr>
                <tr v-if="!payment?.sources.length">
                  <td colspan="9" class="fx-empty">{{ tr('This customer has no payment source with balance. Record the customer\'s payment (collected cheque or bank transfer) first.') }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>

        <template v-if="payment?.payments.length">
          <h3 class="ie-subhead">{{ tr('Payments') }}</h3>
          <div class="fx-table-wrap">
            <table class="fx-table ie-tbl">
              <thead>
                <tr>
                  <th>{{ tr('Charge') }}</th><th>{{ tr('Repayment No.') }}</th><th>{{ tr('Source') }}</th><th>{{ tr('Reference') }}</th>
                  <th>{{ tr('Date') }}</th><th class="fx-r">{{ tr('Amount') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="p in payment.payments" :key="p.allocationId">
                  <td><b>{{ p.ruleCode }}</b></td>
                  <td>{{ p.repaymentNumber }}</td>
                  <td>{{ tr(sourceLabel(p.sourceType)) }}</td>
                  <td>{{ p.reference || '–' }}</td>
                  <td>{{ formatDate(p.allocationDate) }}</td>
                  <td class="fx-r"><b>{{ formatAmount(p.amount) }}</b></td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>
      </section>

      <section class="col-sec">
        <h2><span class="fa fa-credit-card" aria-hidden="true"></span>{{ tr('Repayment Schedule') }} <span class="ie-count">{{ schedule.length }}</span></h2>
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
        <span class="ie-foot-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Save Draft keeps your work. Complete calculates, pays the charges, loads the cheques into the wallet and posts the GL.') }}</span>
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
  chargePaymentApi, invoiceEntryApi, processApi, type ChargePayment, type ChequeRow, type DisbursementResult, type DisbursementTerms, type InvoiceBatch, type InvoiceEntry,
  type InvoiceEntrySave, type InvoiceRow, type InvoiceSummary
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
const scheduleTypes = ['NORMAL_SCHEDULE', 'CUSTOM_SCHEDULE']
const principalMethods = ['MONTHLY', 'QUARTERLY', 'SEMI-ANNUAL', 'INVOICE_DUE_DATE', 'CHEQUE_DUE_DATE', 'AT_MATURITY']
const distributionMethods = ['EQUAL_AMOUNT', 'INVOICE_AMOUNT', 'CHEQUE_AMOUNT']
const interestMethods = [
  { value: 'MONTHLY', label: 'MONTHLY (EOM)' }, { value: 'AT_MATURITY', label: 'AT_MATURITY' }, { value: 'SAME_AS_PRINCIPAL', label: 'SAME_AS_PRINCIPAL' }
]
const paymentMethods = ['CHEQUE', 'TRANSFER', 'INTERNAL']

const canEdit = hasPermission('CRM.EDIT')
const readOnly = computed(() => !canEdit)
// reverse factoring (agreed with Ahmed, 31 Oct 2026): the rates are the agreement's and the repayment terms the cheques', set by the server
const isReverse = computed(() => terms.productId != null && options.reverseProductIds.includes(terms.productId))
const locked = computed(() => readOnly.value || isReverse.value)
const dayCountOptions = computed(() => !terms.baseDayCount || dayCounts.includes(terms.baseDayCount) ? dayCounts : [...dayCounts, terms.baseDayCount])
const ready = ref(false)
const busy = ref(false)
const errors = ref<Record<string, string[]>>({})
const bad = (field: string) => !!errors.value[field]

const terms = reactive({} as DisbursementTerms)
const batch = reactive({} as InvoiceBatch)
const summary = ref<InvoiceSummary | null>(null)
const result = ref<DisbursementResult | null>(null)
const charges = computed(() => result.value?.charges ?? [])
const schedule = computed(() => result.value?.schedule ?? [])
const payment = ref<ChargePayment | null>(null)
const posted = computed(() => !!payment.value?.glTranSetId)
// the payment sources ticked for the charges, in the order ticked (saved with the draft)
const chosen = ref<number[]>([])
const toggleSource = (id: number) => {
  chosen.value = chosen.value.includes(id) ? chosen.value.filter((x) => x !== id) : [...chosen.value, id]
}
const SOURCE_LABELS: Record<string, string> = { CHEQUE: 'Collected Cheque', BANK_TRANSFER: 'Bank Transfer', CREDIT_BALANCE: 'Credit Balance' }
const sourceLabel = (t: string) => SOURCE_LABELS[t] ?? t
const CHEQUE_STATUS: Record<string, string> = { DRAFT: 'Draft', IN_WALLET: 'In Wallet', CANCELLED: 'Cancelled' }
const chequeStatus = (code: string) => CHEQUE_STATUS[code] ?? code
const completeNote = 'Your draft is saved, then in one step: the charges and repayment schedule are calculated, the charges are paid from the ticked sources, the cheques are loaded into the wallet and all GL is posted under one GL Tran Set ID. If any step fails, nothing is posted and only the draft stays.'
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
const invoiceInput = ref<HTMLInputElement | null>(null)
const chequeInput = ref<HTMLInputElement | null>(null)

const whole = (n: number) => Math.round(n).toLocaleString('en-US')
const pick = (e: Event) => (e.target as HTMLInputElement).files?.[0] ?? null
const fileSize = (n: number) => (n >= 1048576 ? `${(n / 1048576).toFixed(1)} MB` : `${Math.max(1, Math.round(n / 1024))} KB`)

// the two upload boxes: drop or browse an invoices file and a cheques file, then Upload Files sends both
const fileSlots = computed(() => [
  {
    key: 'inv', title: 'Invoices File', icon: 'fa-file-text-o', file: invoiceFile.value,
    browse: () => invoiceInput.value?.click(),
    drop: (e: DragEvent) => { invoiceFile.value = e.dataTransfer?.files?.[0] ?? invoiceFile.value },
    clear: () => { invoiceFile.value = null; if (invoiceInput.value) invoiceInput.value.value = '' }
  },
  {
    key: 'chq', title: 'Cheques File', icon: 'fa-credit-card', file: chequeFile.value,
    browse: () => chequeInput.value?.click(),
    drop: (e: DragEvent) => { chequeFile.value = e.dataTransfer?.files?.[0] ?? chequeFile.value },
    clear: () => { chequeFile.value = null; if (chequeInput.value) chequeInput.value.value = '' }
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
const chargeTotals = computed(() => ({
  amount: round2(charges.value.reduce((t, c) => t + c.chargeAmount, 0)) ?? 0,
  tax: round2(charges.value.reduce((t, c) => t + c.taxAmount, 0)) ?? 0,
  total: round2(charges.value.reduce((t, c) => t + c.totalAmount, 0)) ?? 0
}))
// what the ticked sources cover of the charges still owed
const cover = computed(() => {
  const owed = Math.max(chargeTotals.value.total - (payment.value?.paidAmount ?? 0), 0)
  const available = (payment.value?.sources ?? []).filter((x) => chosen.value.includes(x.repaymentSourceId)).reduce((t, x) => t + x.availableAmount, 0)
  const covered = round2(Math.min(owed, available)) ?? 0
  return { covered, short: round2(owed - covered) ?? 0 }
})
const glEntries = computed(() => result.value?.glEntries ?? [])
const glJournals = computed(() => [...new Set(glEntries.value.map((l) => l.journalNo))].sort((a, b) => a - b).map((no) => {
  const lines = glEntries.value.filter((l) => l.journalNo === no)
  const side = (dc: string) => round2(lines.filter((l) => l.debitCreditCode === dc).reduce((t, l) => t + l.amount, 0)) ?? 0
  return { no, lines, debit: side('D'), credit: side('C') }
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
  changedInvoices.clear()
  changedCheques.clear()
  deletedInvoices.value = []
  deletedCheques.value = []
}

async function load() {
  ready.value = false
  errors.value = {}
  try {
    const [page, processed, paid] = await Promise.all([
      invoiceEntryApi.get(props.disbursementId), processApi.result(props.disbursementId), chargePaymentApi.get(props.disbursementId)
    ])
    show(page)
    result.value = processed
    payment.value = paid
    chosen.value = [...paid.chosenSourceIds]
    if (!paid.glTranSetId && !processed.charges.length && terms.productId != null) void nextTick(previewCharges)
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
  invoices: invoices.value.filter((r) => changedInvoices.has(r.invoiceId))
    .map((r) => ({ ...r, grossAmount: Number(r.grossAmount), advanceRatePct: num(r.advanceRatePct), advanceAmount: num(r.advanceAmount) })),
  deletedInvoiceIds: deletedInvoices.value,
  cheques: cheques.value.filter((c) => changedCheques.has(c.chequeId))
    .map((c) => ({ ...c, chequeAmount: Number(c.chequeAmount), depositBankAccountId: num(c.depositBankAccountId) })),
  deletedChequeIds: deletedCheques.value,
  chargeSourceIds: chosen.value
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
  void previewCharges()
  showSuccess(tr('Draft saved.'))
})

// Upload Invoices: saves the forms, then imports whichever files are chosen
const upload = () => check() && run(async () => {
  if (!invoiceFile.value && !chequeFile.value) throw new Error(tr('Select at least one invoice or cheque XLSX/CSV file.'))
  const result = await invoiceEntryApi.upload(props.disbursementId, payload(), invoiceFile.value, chequeFile.value)
  show(result.page)
  void previewCharges()
  invoiceFile.value = null
  chequeFile.value = null
  if (invoiceInput.value) invoiceInput.value.value = ''
  if (chequeInput.value) chequeInput.value.value = ''
  showSuccess(tr(result.message))
})

// the step's Complete saves the page first
const saveForStep = async () => (readOnly.value ? true : check() && (await run(async () => {
  show(await invoiceEntryApi.save(props.disbursementId, payload()))
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
    const preview = await processApi.preview(props.disbursementId, payload())
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
  invoices.value.map((r) => `${r.statusCode}|${r.grossAmount}|${r.advanceAmount}`)
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
