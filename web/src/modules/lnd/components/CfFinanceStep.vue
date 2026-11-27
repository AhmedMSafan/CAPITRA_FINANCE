<template>
  <!-- CF_FINANCE (Ahmed, 26 Nov 2026): the step a consumer finance from a supplier is waiting at; the task holder works on it here.
       Credit approval decides each charge (upfront or added to the schedule) and approving creates the claim (27 Nov 2026; older finances
       create it at Charges and claim); Schedule and contract waits until the claim is paid, then builds the schedule. -->
  <section v-if="step" class="fx-card cfs">
    <header class="cfs-head">
      <span class="cfs-ic" aria-hidden="true"><span class="fa fa-random"></span></span>
      <h3>{{ tr(step.stepName) }}</h3>
      <p v-if="step.isMine">— {{ tr(HINT[step.stepCode] ?? 'Check the finance, then accept or reject the step.') }}</p>
      <p v-else>— {{ tr('Waiting for') }} {{ tr(ROLE[step.stepCode] ?? 'the step holder') }}. {{ tr('Open it from the inbox to work on it.') }}</p>
      <span class="cfs-grow"></span>
      <CfRulesButton :loan-id="loan.loanId" :step="step.stepCode" :title="`${loan.loanNumber} · ${tr(step.stepName)}`" :choices="rulesChoices" />
    </header>

    <!-- the deal at a glance, for the step holder -->
    <div v-if="step.isMine && loan.goods" class="cfs-deal">
      <span>{{ tr('Customer') }} <b>{{ loan.clientName }}</b></span>
      <span>{{ tr('Supplier') }} <b>{{ loan.goods.merchantName }}</b></span>
      <span>{{ tr('Item') }} <b>{{ loan.goods.description }}<template v-if="loan.goods.brandModel"> · {{ loan.goods.brandModel }}</template></b></span>
      <span>{{ tr('Price') }} <b>{{ money(loan.goods.priceAmount) }}</b></span>
      <template v-if="!deciding">
        <span>{{ tr('Down payment') }} <b>{{ money(loan.goods.downPaymentAmount) }}</b> ({{ tr(loan.goods.downPaymentCollectorCode === 'COMPANY' ? 'us' : 'supplier') }})</span>
        <span>{{ tr('Financed') }} <b>{{ money(loan.goods.priceAmount - loan.goods.downPaymentAmount) }}</b></span>
        <span>{{ tr('Term') }} <b>{{ loan.termMonths }} {{ tr('months') }}</b></span>
      </template>
      <span>{{ tr('Commission') }} <b>{{ loan.goods.supplierCommissionPct ?? 0 }}%</b></span>
    </div>

    <!-- one layout for every step (Ahmed, 27 Nov 2026): the values in boxed groups on the left, the step's decision or money in one
         framed box on the right, the actions at the bottom -->

    <!-- Credit approval: every financial value is this finance's own copy and Credit can change any of them; what was entered shows on
         the right of each row, a changed one is highlighted -->
    <div v-if="deciding && t && o" class="cfl-cols">
      <div class="cfl-stack">
        <div class="cfl-box">
          <h3>{{ tr('Amount and term') }}<small>{{ tr('entered value on the right') }}</small></h3>
          <table class="cfl-rows"><tbody>
            <tr :class="{ chg: ch('downPaymentAmount') }"><td class="cfl-l">{{ tr('Down payment') }} ({{ tr(loan.goods?.downPaymentCollectorCode === 'COMPANY' ? 'us' : 'supplier') }})</td>
              <td class="cfl-v"><input v-model.number="t.downPaymentAmount" type="number" min="0" step="any" class="fx-search" :class="{ chg: ch('downPaymentAmount') }" @input="downChanged"></td>
              <td class="cfl-h">{{ tr('entered') }} {{ money(o.downPaymentAmount) }}<template v-if="o.minDownPaymentAmount"> · {{ tr('at least') }} {{ money(o.minDownPaymentAmount) }}</template></td></tr>
            <tr><td class="cfl-l">{{ tr('Financed') }}</td><td class="cfl-v">{{ money(o.priceAmount - (Number(t.downPaymentAmount) || 0)) }}</td><td class="cfl-h">{{ tr('price − down payment') }}</td></tr>
            <tr :class="{ chg: ch('termMonths') }"><td class="cfl-l">{{ tr('Term (months)') }}</td>
              <td class="cfl-v"><input v-model.number="t.termMonths" type="number" min="1" max="120" class="fx-search" :class="{ chg: ch('termMonths') }"></td>
              <td class="cfl-h">{{ tr('entered') }} {{ o.termMonths }}</td></tr>
            <tr :class="{ chg: ch('repaymentFrequencyCode') }"><td class="cfl-l">{{ tr('Repayment') }}</td>
              <td class="cfl-v"><select v-model="t.repaymentFrequencyCode" class="fx-select" :class="{ chg: ch('repaymentFrequencyCode') }"><option v-for="x in o.options.frequencies" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select></td>
              <td class="cfl-h">{{ tr('entered') }} {{ tr(lbl(o.options.frequencies, o.repaymentFrequencyCode)) }}</td></tr>
          </tbody></table>
        </div>
        <div class="cfl-box">
          <h3>{{ tr('Interest') }}</h3>
          <table class="cfl-rows"><tbody>
            <tr :class="{ chg: ch('annualRatePct') }"><td class="cfl-l">{{ tr('Yearly rate') }} (%)</td>
              <td class="cfl-v"><input v-model.number="t.annualRatePct" type="number" min="0" max="100" step="any" class="fx-search" :class="{ chg: ch('annualRatePct') }"></td>
              <td class="cfl-h">{{ tr('product') }} {{ step.productRatePct }}%<template v-if="marginNow"> · {{ tr('margin') }} {{ marginNow > 0 ? '+' : '' }}{{ marginNow }}</template></td></tr>
            <tr :class="{ chg: ch('interestMethodCode') }"><td class="cfl-l">{{ tr('Interest method') }}</td>
              <td class="cfl-v"><select v-model="t.interestMethodCode" class="fx-select" :class="{ chg: ch('interestMethodCode') }"><option v-for="x in o.options.interestMethods" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select></td>
              <td class="cfl-h">{{ tr('entered') }} {{ tr(lbl(o.options.interestMethods, o.interestMethodCode)) }}</td></tr>
            <tr :class="{ chg: ch('dayCountCode') }"><td class="cfl-l">{{ tr('Day count') }}</td>
              <td class="cfl-v"><select v-model="t.dayCountCode" class="fx-select" :class="{ chg: ch('dayCountCode') }"><option v-for="x in o.options.dayCounts" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select></td>
              <td class="cfl-h">{{ tr('entered') }} {{ tr(lbl(o.options.dayCounts, o.dayCountCode)) }}</td></tr>
          </tbody></table>
        </div>
        <div class="cfl-box">
          <h3>{{ tr('Late fee and other') }}<button type="button" class="cfl-link" @click="moreOpen = !moreOpen">{{ tr(moreOpen ? 'Close' : 'Change') }}</button></h3>
          <table v-if="!moreOpen" class="cfl-rows"><tbody>
            <tr :class="{ chg: ch('lateFeeTypeCode') || ch('lateFeeRatePct') || ch('lateFeeFlatAmount') || ch('lateFeeBaseCode') || ch('lateFeeGraceDays') }">
              <td class="cfl-l">{{ tr('Late fee') }}</td><td class="cfl-v" colspan="2">{{ lateFeeText }}</td></tr>
            <tr :class="{ chg: ch('isInsuranceRequired') || ch('isPrepaymentAllowed') || ch('prepaymentRecalcCode') || ch('prepaymentFeePct') || ch('earlySettlementFeePct') }">
              <td class="cfl-l">{{ tr('Insurance') }} · {{ tr('Paying early') }}</td><td class="cfl-v" colspan="2">{{ otherText }}</td></tr>
            <tr :class="{ chg: ch('paymentOrder') }"><td class="cfl-l">{{ tr('Payment order') }}</td>
              <td class="cfl-v" colspan="2">{{ t.paymentOrder.map((x) => tr(lbl(o!.options.paymentParts, x))).join(' › ') }}</td></tr>
          </tbody></table>
          <table v-else class="cfl-rows"><tbody>
            <tr :class="{ chg: ch('lateFeeTypeCode') }"><td class="cfl-l">{{ tr('Late fee type') }}</td>
              <td class="cfl-v"><select v-model="t.lateFeeTypeCode" class="fx-select" :class="{ chg: ch('lateFeeTypeCode') }"><option v-for="x in o.options.lateFeeTypes" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select></td>
              <td class="cfl-h">{{ tr('entered') }} {{ tr(lbl(o.options.lateFeeTypes, o.lateFeeTypeCode)) }}</td></tr>
            <tr v-if="t.lateFeeTypeCode === 'FLAT'" :class="{ chg: ch('lateFeeFlatAmount') }"><td class="cfl-l">{{ tr('Fee per late instalment') }}</td>
              <td class="cfl-v"><input v-model.number="t.lateFeeFlatAmount" type="number" min="0" step="any" class="fx-search" :class="{ chg: ch('lateFeeFlatAmount') }"></td>
              <td class="cfl-h">{{ tr('entered') }} {{ o.lateFeeFlatAmount ?? '—' }}</td></tr>
            <template v-if="t.lateFeeTypeCode === 'PCT_ONCE' || t.lateFeeTypeCode === 'PCT_DAILY'">
              <tr :class="{ chg: ch('lateFeeRatePct') }"><td class="cfl-l">{{ tr('Late fee rate') }} (%)</td>
                <td class="cfl-v"><input v-model.number="t.lateFeeRatePct" type="number" min="0" max="100" step="any" class="fx-search" :class="{ chg: ch('lateFeeRatePct') }"></td>
                <td class="cfl-h">{{ tr('entered') }} {{ o.lateFeeRatePct ?? '—' }}</td></tr>
              <tr :class="{ chg: ch('lateFeeBaseCode') }"><td class="cfl-l">{{ tr('Calculated on') }}</td>
                <td class="cfl-v"><select v-model="t.lateFeeBaseCode" class="fx-select" :class="{ chg: ch('lateFeeBaseCode') }"><option v-for="x in o.options.lateFeeBases" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select></td>
                <td class="cfl-h">{{ tr('entered') }} {{ tr(lbl(o.options.lateFeeBases, o.lateFeeBaseCode)) }}</td></tr>
            </template>
            <tr v-if="t.lateFeeTypeCode !== 'NONE'" :class="{ chg: ch('lateFeeGraceDays') }"><td class="cfl-l">{{ tr('Grace days') }}</td>
              <td class="cfl-v"><input v-model.number="t.lateFeeGraceDays" type="number" min="0" max="90" class="fx-search" :class="{ chg: ch('lateFeeGraceDays') }"></td>
              <td class="cfl-h">{{ tr('entered') }} {{ o.lateFeeTypeCode === 'NONE' ? '—' : o.lateFeeGraceDays }}</td></tr>
            <tr :class="{ chg: ch('isInsuranceRequired') }"><td class="cfl-l">{{ tr('Insurance required') }}</td>
              <td class="cfl-v"><select v-model="t.isInsuranceRequired" class="fx-select" :class="{ chg: ch('isInsuranceRequired') }"><option :value="false">{{ tr('No') }}</option><option :value="true">{{ tr('Yes') }}</option></select></td>
              <td class="cfl-h">{{ tr('entered') }} {{ tr(o.isInsuranceRequired ? 'Yes' : 'No') }}</td></tr>
            <tr :class="{ chg: ch('isPrepaymentAllowed') }"><td class="cfl-l">{{ tr('Paying early') }}</td>
              <td class="cfl-v"><select v-model="t.isPrepaymentAllowed" class="fx-select" :class="{ chg: ch('isPrepaymentAllowed') }"><option :value="true">{{ tr('Allowed') }}</option><option :value="false">{{ tr('Not allowed') }}</option></select></td>
              <td class="cfl-h">{{ tr('entered') }} {{ tr(o.isPrepaymentAllowed ? 'Allowed' : 'Not allowed') }}</td></tr>
            <template v-if="t.isPrepaymentAllowed">
              <tr :class="{ chg: ch('prepaymentRecalcCode') }"><td class="cfl-l">{{ tr('After a part prepayment') }}</td>
                <td class="cfl-v"><select v-model="t.prepaymentRecalcCode" class="fx-select" :class="{ chg: ch('prepaymentRecalcCode') }"><option v-for="x in o.options.prepaymentRecalcs" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select></td>
                <td class="cfl-h">{{ tr('entered') }} {{ tr(lbl(o.options.prepaymentRecalcs, o.prepaymentRecalcCode)) }}</td></tr>
              <tr :class="{ chg: ch('prepaymentFeePct') }"><td class="cfl-l">{{ tr('Part prepayment fee') }} (%)</td>
                <td class="cfl-v"><input v-model.number="t.prepaymentFeePct" type="number" min="0" max="100" step="any" class="fx-search" :class="{ chg: ch('prepaymentFeePct') }"></td>
                <td class="cfl-h">{{ tr('entered') }} {{ o.prepaymentFeePct }}%</td></tr>
              <tr :class="{ chg: ch('earlySettlementFeePct') }"><td class="cfl-l">{{ tr('Early settlement fee') }} (%)</td>
                <td class="cfl-v"><input v-model.number="t.earlySettlementFeePct" type="number" min="0" max="100" step="any" class="fx-search" :class="{ chg: ch('earlySettlementFeePct') }"></td>
                <td class="cfl-h">{{ tr('entered') }} {{ o.earlySettlementFeePct }}%</td></tr>
            </template>
            <tr :class="{ chg: ch('paymentOrder') }"><td class="cfl-l">{{ tr('Payment order') }}</td>
              <td class="cfl-v" colspan="2"><span class="cfs-order">
                <span v-for="(part, i) in t.paymentOrder" :key="part" class="cfs-part">
                  <button type="button" :disabled="i === 0" :aria-label="tr('Move up')" @click="moveOrder(i)"><span class="fa fa-angle-left" aria-hidden="true"></span></button>{{ tr(lbl(o.options.paymentParts, part)) }}
                </span></span></td></tr>
          </tbody></table>
        </div>
      </div>
      <div class="cfl-stack">
        <div v-if="charges" class="cfl-key">
          <h3>{{ tr('Charges — your decision') }}<small>{{ tr('rule') }} CF_CHARGES</small></h3>
          <p v-if="charges.problem" class="cfl-note bad cfl-pad">{{ charges.problem }}</p>
          <table v-else-if="charges.charges.length" class="cfl-rows"><tbody>
            <tr v-for="c in charges.charges" :key="c.chargeCode">
              <td><b>{{ tr(c.chargeName) }}</b><br><small class="cfs-basis">{{ tr(c.basisText) || '—' }} · {{ tr('rule') }} {{ money(c.ruleAmount) }}</small></td>
              <td><input v-model.number="amounts[c.chargeCode]" type="number" min="0" step="any" class="fx-search cfs-amt" :class="{ chg: amountOf(c) !== c.ruleAmount }" @input="touched.add(c.chargeCode)"></td>
              <td class="cfl-h">
                <span class="cfs-seg" role="group" :aria-label="tr('How paid')">
                  <button type="button" :class="{ on: timing(c) === UPFRONT }" @click="setTiming(c, UPFRONT)">{{ tr('Upfront') }}</button>
                  <button type="button" :class="{ on: timing(c) === FINANCED }" @click="setTiming(c, FINANCED)">{{ tr('Add to schedule') }}</button>
                </span>
                <small v-if="timing(c) !== c.defaultTimingCode || amountOf(c) !== c.ruleAmount" class="cfs-ovr">{{ tr('changed from the rule') }}</small>
              </td>
            </tr>
          </tbody></table>
          <p v-else class="cfl-note cfl-pad">{{ tr('The rule has no charges.') }}</p>
          <div v-if="!charges.problem" class="cfl-tiles">
            <div class="cfl-tile"><small>{{ tr('Financed') }}</small><b>{{ money(totals.financed) }}</b></div>
            <div class="cfl-tile"><small>{{ tr('Added to schedule') }}</small><b>{{ money(totals.added) }}</b></div>
            <div class="cfl-tile hl"><small>{{ tr('Finance amount') }}</small><b>{{ money(totals.financed + totals.added) }}</b></div>
            <div class="cfl-tile hl"><small>{{ tr('Upfront claim') }}</small><b>{{ money(totals.upfront + claimDown) }}</b></div>
          </div>
          <p v-if="step.claimAtCredit && !charges.problem" class="cfl-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
            <template v-if="totals.upfront + claimDown > 0">{{ tr('Approve books the claim') }} {{ money(totals.upfront + claimDown) }}<template v-if="totals.upfront"> ({{ tr('rule CF_CHARGES posts the upfront charges') }} {{ money(totals.upfront) }})</template>.
              {{ tr('It is paid in Collection; the schedule step waits until it is paid.') }}</template>
            <template v-else>{{ tr('Nothing upfront: no claim, nothing posts on Approve.') }}</template>
            <template v-if="totals.added"> {{ tr('Charges added to the schedule post at Delivery as part of the finance.') }}</template>
          </p>
          <p v-if="changedCount" class="cfl-note">{{ changedCount }} {{ tr('values changed from what was entered') }}</p>
        </div>
      </div>
    </div>

    <!-- Charges and claim (finances started before 27 Nov 2026): Credit's decision on the left, the claim on the right -->
    <div v-if="step.stepCode === 'CF_CHARGES' && step.isMine" class="cfl-cols">
      <div class="cfl-box">
        <h3>{{ tr('Charges') }}<small>{{ tr('decided by Credit') }}</small></h3>
        <p v-if="charges?.problem" class="cfl-note bad cfl-pad">{{ charges.problem }}</p>
        <table v-else class="cfl-rows"><tbody>
          <tr v-for="c in charges?.charges ?? []" :key="c.chargeCode"><td class="cfl-l">{{ tr(c.chargeName) }}</td>
            <td class="cfl-v">{{ money(c.amount + c.taxAmount) }}</td>
            <td class="cfl-h"><span class="fx-chip" :class="c.timingCode === UPFRONT ? 'fx-chip--warn' : 'fx-chip--teal'">{{ tr(c.timingCode === UPFRONT ? 'Upfront' : 'Added to schedule') }}</span></td></tr>
        </tbody></table>
      </div>
      <div class="cfl-key">
        <h3>{{ tr('Upfront claim') }}<button v-if="loan.claim && loan.claim.statusCode !== 'NONE'" type="button" class="fx-btn fx-btn--sm" @click="claimShown = true"><span class="fa fa-flag-o" aria-hidden="true"></span>{{ tr('Open claim') }}</button></h3>
        <template v-if="!loan.claim">
          <div class="cfl-tiles">
            <div class="cfl-tile"><small>{{ tr('Upfront charges') }}</small><b>{{ money(claimCharges) }}</b></div>
            <div class="cfl-tile"><small>{{ tr('Down payment we take') }}</small><b>{{ money(claimDown) }}</b></div>
            <div class="cfl-tile hl"><small>{{ tr('Claim') }}</small><b>{{ money(claimCharges + claimDown) }}</b></div>
          </div>
          <p class="cfl-note"><button type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="busy" @click="createClaim">
            <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-file-text-o'" aria-hidden="true"></span>{{ tr(claimCharges + claimDown ? 'Create claim' : 'Confirm nothing upfront') }}</button></p>
        </template>
        <p v-else-if="loan.claim.statusCode === 'NONE'" class="cfl-note cfl-pad">{{ tr('Nothing is paid upfront. Accept to go on to the schedule.') }}</p>
        <template v-else>
          <div class="cfl-tiles">
            <div class="cfl-tile"><small>{{ tr('Claim') }}</small><b>{{ money(loan.claim.totalAmount) }}</b></div>
            <div class="cfl-tile"><small>{{ tr('Status') }}</small><b>{{ tr(CLAIM_LABELS[loan.claim.statusCode] ?? loan.claim.statusCode) }}</b></div>
            <div class="cfl-tile"><small>{{ tr('Paid') }}</small><b>{{ money(loan.claim.paidAmount) }}</b></div>
            <div class="cfl-tile" :class="{ warn: loan.claim.outstandingAmount > 0 }"><small>{{ tr('Outstanding') }}</small><b>{{ money(loan.claim.outstandingAmount) }}</b></div>
          </div>
          <p v-if="loan.claim.outstandingAmount > 0" class="cfl-note">{{ tr('The claim is paid in Collection › Customer Payments; accept once it is paid.') }}</p>
        </template>
      </div>
    </div>

    <!-- Schedule and contract: the finance to schedule on the left, the claim (paid first) on the right -->
    <div v-if="step.stepCode === 'CF_SCHEDULE'" class="cfl-cols">
      <div class="cfl-box">
        <h3>{{ tr('Schedule to build') }}</h3>
        <table class="cfl-rows"><tbody>
          <tr><td class="cfl-l">{{ tr('Finance amount') }}</td><td class="cfl-v">{{ money(loan.principalAmount) }}</td><td class="cfl-h">{{ tr('financed + charges added') }}</td></tr>
          <tr><td class="cfl-l">{{ tr('Term') }}</td><td class="cfl-v">{{ loan.termMonths }} {{ tr('months') }}</td><td class="cfl-h"></td></tr>
          <tr><td class="cfl-l">{{ tr('Yearly rate') }}</td><td class="cfl-v">{{ loan.annualRatePct }}%</td><td class="cfl-h"></td></tr>
          <tr><td class="cfl-l">{{ tr('Starts') }}</td><td class="cfl-v">{{ tr('today') }}</td><td class="cfl-h">{{ tr('dates move to the delivery day') }}</td></tr>
        </tbody></table>
      </div>
      <div v-if="loan.claim && loan.claim.statusCode !== 'NONE'" class="cfl-key">
        <h3>{{ tr('Upfront claim') }}<button type="button" class="fx-btn fx-btn--sm" @click="claimShown = true"><span class="fa fa-flag-o" aria-hidden="true"></span>{{ tr('Open claim') }}</button></h3>
        <div class="cfl-tiles">
          <div class="cfl-tile"><small>{{ tr('Claim') }}</small><b>{{ money(loan.claim.totalAmount) }}</b></div>
          <div class="cfl-tile"><small>{{ tr('Status') }}</small><b>{{ tr(CLAIM_LABELS[loan.claim.statusCode] ?? loan.claim.statusCode) }}</b></div>
          <div class="cfl-tile"><small>{{ tr('Paid') }}</small><b>{{ money(loan.claim.paidAmount) }}</b></div>
          <div class="cfl-tile" :class="{ warn: loan.claim.outstandingAmount > 0 }"><small>{{ tr('Outstanding') }}</small><b>{{ money(loan.claim.outstandingAmount) }}</b></div>
        </div>
        <p v-if="loan.claim.outstandingAmount > 0" class="cfl-note bad">{{ tr('Accept waits until the claim is paid in Collection › Customer Payments.') }}</p>
      </div>
      <div v-else class="cfl-key">
        <h3>{{ tr('Upfront claim') }}</h3>
        <p class="cfl-note cfl-pad">{{ tr('Nothing upfront: no claim.') }}</p>
      </div>
    </div>
    <CfClaimDialog v-model:visible="claimShown" :loan-id="loan.loanId" :show-finance="false" />

    <!-- Delivery and customer receipt: the delivery on the left, what Accept books on the right -->
    <div v-if="step.stepCode === 'CF_DELIVERY' && step.isMine" class="cfl-cols">
      <div class="cfl-box">
        <h3>{{ tr('Delivery') }}</h3>
        <table class="cfl-rows"><tbody>
          <tr><td class="cfl-l">{{ tr('Delivered on') }} *</td><td class="cfl-v"><input v-model="delivery.deliveryDate" type="date" class="fx-search" :max="todayIso"></td>
            <td class="cfl-h">{{ tr('not before the schedule step, not in the future') }}</td></tr>
          <tr><td class="cfl-l">{{ tr('Delivery note no.') }} *</td><td class="cfl-v"><input v-model="delivery.deliveryNoteNo" class="fx-search" maxlength="50"></td><td class="cfl-h"></td></tr>
          <tr><td class="cfl-l">{{ tr('Serial / IMEI') }}</td><td class="cfl-v"><input v-model="delivery.serialNo" class="fx-search" maxlength="100"></td><td class="cfl-h"></td></tr>
          <tr><td class="cfl-l">{{ tr('Delivery note (file)') }}</td><td class="cfl-v" colspan="2"><span class="c3-muted">{{ tr('Add the signed delivery note under Documents below.') }}</span></td></tr>
          <tr><td colspan="3"><label class="cfs-confirm"><input v-model="delivery.customerConfirmed" type="checkbox">{{ tr('The customer received the goods and confirms they match the contract') }}</label></td></tr>
        </tbody></table>
      </div>
      <div class="cfl-key">
        <h3>{{ tr('On accept') }}<small>{{ tr('rule') }} CF_DELIVERY</small></h3>
        <div class="cfl-tiles">
          <div class="cfl-tile hl"><small>{{ tr('Dr Finance') }}</small><b>{{ money(loan.principalAmount) }}</b></div>
          <div class="cfl-tile"><small>{{ tr('Cr Payable to the supplier') }}</small><b>{{ money(financedNow) }}</b></div>
          <div v-if="loan.principalAmount - financedNow > 0" class="cfl-tile"><small>{{ tr('Cr Charges not earned yet') }}</small><b>{{ money(loan.principalAmount - financedNow) }}</b></div>
          <div v-if="heldDown" class="cfl-tile"><small>{{ tr('Down payment held, now due to the supplier') }}</small><b>{{ money(heldDown) }}</b></div>
          <div class="cfl-tile"><small>{{ tr('Supplier paid by') }}</small><b>{{ supplierDue }}</b></div>
        </div>
        <p class="cfl-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Status') }} {{ tr('Approved') }} → {{ tr('Active') }}; {{ tr('the schedule dates move to the delivery day (amounts unchanged)') }}.</p>
      </div>
    </div>

    <!-- Supplier payment: the payment on the left, what the supplier gets on the right -->
    <div v-if="step.stepCode === 'CF_SUPPLIER_PAYMENT' && step.isMine && step.supplierDue" class="cfl-cols">
      <div class="cfl-box">
        <h3>{{ tr('Supplier payment') }}</h3>
        <table class="cfl-rows"><tbody>
          <tr><td class="cfl-l">{{ tr('Paid from') }} *</td><td class="cfl-v">
            <select v-model="sp.bankAccountId" class="fx-select">
              <option :value="null" disabled>{{ tr('Choose the bank account') }}</option>
              <option v-for="a in step.accounts ?? []" :key="a.accountId" :value="a.accountId">{{ a.bankName }} · {{ a.accountRef }}</option>
            </select></td><td class="cfl-h"></td></tr>
          <tr><td class="cfl-l">{{ tr('Transfer reference') }} *</td><td class="cfl-v"><input v-model="sp.reference" class="fx-search" maxlength="100"></td><td class="cfl-h"></td></tr>
          <tr :class="{ chg: commissionNow !== step.supplierDue.commissionAmount }"><td class="cfl-l">{{ tr('Commission') }}</td>
            <td class="cfl-v"><input v-model.number="sp.commissionAmount" type="number" min="0" step="any" class="fx-search" :class="{ chg: commissionNow !== step.supplierDue.commissionAmount }"></td>
            <td class="cfl-h">{{ tr('agreed') }} {{ step.supplierDue.commissionPct }}% = {{ money(step.supplierDue.commissionAmount) }}</td></tr>
          <tr v-if="commissionNow !== step.supplierDue.commissionAmount"><td class="cfl-l">{{ tr('Why the commission changed') }} *</td>
            <td class="cfl-v" colspan="2"><input v-model="sp.commissionReason" class="fx-search" maxlength="500"></td></tr>
        </tbody></table>
      </div>
      <div class="cfl-key">
        <h3>{{ tr('On accept') }}<small>{{ tr('rule') }} CF_SUPPLIER_PAYMENT</small></h3>
        <div class="cfl-tiles">
          <div class="cfl-tile"><small>{{ tr('Owed to the supplier') }} ({{ tr('Dr') }})</small><b>{{ money(step.supplierDue.payableAmount) }}</b></div>
          <div class="cfl-tile"><small>{{ tr('Commission income') }} ({{ tr('Cr') }})</small><b>{{ money(commissionNow) }}</b></div>
          <div class="cfl-tile hl"><small>{{ tr('Net paid from the bank') }} ({{ tr('Cr') }})</small><b>{{ money(step.supplierDue.payableAmount - commissionNow) }}</b></div>
          <div class="cfl-tile"><small>{{ tr('Pay by') }}</small><b>{{ step.supplierDue.dueDate ?? '—' }}</b></div>
        </div>
        <p class="cfl-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('The workflow ends; the finance stays active and is collected by its schedule.') }}</p>
      </div>
    </div>

    <div v-if="step.isMine" class="cfs-actions">
      <input v-model="comment" class="fx-search cfs-comment" maxlength="2000" :placeholder="tr('Comment (required to reject)')" :aria-label="tr('Comment')">
        <button type="button" class="fx-btn fc-btn-reject" :disabled="busy" @click="complete('REJECT')"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="complete('ACCEPT')">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr(ACCEPT[step.stepCode] ?? 'Accept') }}
        </button>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import './cf-layout.css'
import CfClaimDialog from './CfClaimDialog.vue'
import CfRulesButton from './CfRulesButton.vue'
import { CLAIM_LABELS, loansApi, type CfCharge, type CfCreditDecision, type CfStep, type CfTerms, type Loan } from '../api/loans.api'

const props = defineProps<{ loan: Loan; version: number }>()
const emit = defineEmits<{ changed: [] }>()
const route = useRoute()
const router = useRouter()

const UPFRONT = 'PAID_UPFRONT'
const FINANCED = 'FINANCE_WITH_PRINCIPAL'
const HINT: Record<string, string> = {
  CF_CREDIT: "Check the deal; every value below is this finance's own copy, change any of them. Approving needs the customer's compliance to be clear and creates the upfront claim.",
  CF_CHARGES: 'Create the claim of the upfront charges; once Collection has it paid, accept. Charges added to the schedule are part of the principal.',
  CF_SCHEDULE: 'Once the claim is paid, check the finance amount and term, then accept to build the schedule.',
  CF_DELIVERY: "Record the delivery and the customer's receipt; accepting books the finance.",
  CF_SUPPLIER_PAYMENT: 'Check what the supplier is owed and its commission, choose the bank account, then pay.',
}
const ACCEPT: Record<string, string> = { CF_CREDIT: 'Approve', CF_SCHEDULE: 'Build schedule and accept', CF_DELIVERY: 'Accept and book', CF_SUPPLIER_PAYMENT: 'Pay the supplier' }
const ROLE: Record<string, string> = {
  CF_CREDIT: 'Credit (CREDIT_OFFICER)', CF_CHARGES: 'Operations (OPERATIONS_OFFICER)', CF_SCHEDULE: 'Operations (OPERATIONS_OFFICER)',
  CF_DELIVERY: 'Operations (OPERATIONS_OFFICER)', CF_SUPPLIER_PAYMENT: 'Treasury (TREASURY_OFFICER)',
}

const step = ref<CfStep | null>(null)
/** What Credit has on the screen, for Rules and GL (not saved until Approve). */
const rulesChoices = computed(() => (deciding.value && charges.value ? {
  charges: charges.value.charges.map((c) => ({ code: c.chargeCode, timing: timing(c), amount: amountOf(c) })),
  down: t.value ? Number(t.value.downPaymentAmount) || 0 : null
} : null))
/** Late fee and other: one summary line each until Change opens the fields. */
const moreOpen = ref(false)
const lateFeeText = computed(() => {
  const x = t.value
  if (!x) return ''
  if (x.lateFeeTypeCode === 'NONE') return tr('None')
  const what = x.lateFeeTypeCode === 'FLAT' ? money(Number(x.lateFeeFlatAmount) || 0) : `${x.lateFeeRatePct ?? 0}%`
  return `${tr(lbl(o.value?.options.lateFeeTypes ?? [], x.lateFeeTypeCode))} · ${what} · ${x.lateFeeGraceDays} ${tr('grace days')}`
})
const otherText = computed(() => {
  const x = t.value
  if (!x) return ''
  return `${tr(x.isInsuranceRequired ? 'Yes' : 'No')} · ${tr(x.isPrepaymentAllowed ? 'Allowed' : 'Not allowed')}`
    + (x.isPrepaymentAllowed ? ` (${tr('fees')} ${x.prepaymentFeePct}% / ${x.earlySettlementFeePct}%)` : '')
})
const claimShown = ref(false)
const comment = ref('')
const busy = ref(false)
const timings = ref<Record<string, string>>({})
const amounts = ref<Record<string, number | string>>({})
/** Credit's values (t) and what was entered (o). */
const t = ref<CfTerms | null>(null)
const o = computed(() => step.value?.terms ?? null)
const touched = new Set<string>()
const todayIso = new Date().toISOString().slice(0, 10)
const delivery = reactive({ deliveryDate: todayIso as string | null, deliveryNoteNo: '', serialNo: '', customerConfirmed: false })
const financedNow = computed(() => (props.loan.goods ? props.loan.goods.priceAmount - props.loan.goods.downPaymentAmount : props.loan.principalAmount))
const heldDown = computed(() => (props.loan.claim?.statusCode === 'PAID' ? props.loan.claim.downPaymentAmount : 0))
const supplierDue = computed(() => {
  if (!delivery.deliveryDate) return '—'
  const d = new Date(`${delivery.deliveryDate}T00:00:00Z`)
  d.setUTCDate(d.getUTCDate() + (props.loan.goods?.supplierPaymentDays ?? 0))
  return d.toISOString().slice(0, 10)
})
const sp = reactive({ bankAccountId: null as number | null, reference: '', commissionAmount: null as number | string | null, commissionReason: '' })
const commissionNow = computed(() => {
  const v = sp.commissionAmount
  return v === null || v === '' || Number.isNaN(Number(v)) ? step.value?.supplierDue?.commissionAmount ?? 0 : Math.round(Number(v) * 100) / 100
})
const inboxId = () => (route.query.inboxId ? Number(route.query.inboxId) : null)
const money = (n: number) => new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n)

const charges = computed(() => step.value?.charges ?? null)
/** Credit decides on its own task; later steps only show the decision. */
const deciding = computed(() => step.value?.stepCode === 'CF_CREDIT' && step.value.isMine)
const timing = (c: CfCharge) => timings.value[c.chargeCode] ?? c.timingCode
/** The amount Credit typed (an empty box counts as the rule's amount); the tax follows it. */
const amountOf = (c: CfCharge) => {
  const v = amounts.value[c.chargeCode]
  return v === undefined || v === '' || Number.isNaN(Number(v)) ? c.amount : Math.round(Number(v) * 100) / 100
}
const taxOf = (c: CfCharge) => (c.amount ? Math.round((amountOf(c) * c.taxAmount) / c.amount * 100) / 100 : 0)
const lbl = (list: { value: string; label: string }[], v: string) => list.find((x) => x.value === v)?.label ?? v
const TERM_KEYS = ['downPaymentAmount', 'termMonths', 'repaymentFrequencyCode', 'interestMethodCode', 'annualRatePct', 'dayCountCode', 'lateFeeTypeCode',
  'lateFeeRatePct', 'lateFeeFlatAmount', 'lateFeeBaseCode', 'lateFeeGraceDays', 'paymentOrder', 'isInsuranceRequired', 'isPrepaymentAllowed',
  'prepaymentRecalcCode', 'prepaymentFeePct', 'earlySettlementFeePct'] as const
type TermKey = typeof TERM_KEYS[number]
/** A value Credit changed from what was entered. */
const ch = (k: TermKey) => !!t.value && !!o.value && JSON.stringify(t.value[k] ?? null) !== JSON.stringify(o.value[k] ?? null)
const changedCount = computed(() => TERM_KEYS.filter((k) => ch(k)).length
  + (charges.value?.charges ?? []).filter((c) => timing(c) !== c.defaultTimingCode || amountOf(c) !== c.ruleAmount).length)
const marginNow = computed(() => (t.value && step.value?.productRatePct != null ? Math.round(((Number(t.value.annualRatePct) || 0) - step.value.productRatePct) * 10000) / 10000 : 0))
function moveOrder(i: number) {
  if (!t.value || i < 1) return
  const list = [...t.value.paymentOrder]
  const up = list[i]!
  list[i] = list[i - 1]!
  list[i - 1] = up
  t.value.paymentOrder = list
}
// a new down payment changes the financed amount, so the charges are worked out again (amounts Credit typed stay)
let downTimer: ReturnType<typeof setTimeout> | undefined
function downChanged() {
  clearTimeout(downTimer)
  downTimer = setTimeout(async () => {
    if (!t.value) return
    try {
      const fresh = await loansApi.cfStep(props.loan.loanId, inboxId(), Number(t.value.downPaymentAmount) || 0)
      if (step.value && fresh) step.value = { ...step.value, charges: fresh.charges }
      for (const c of fresh?.charges?.charges ?? []) if (!touched.has(c.chargeCode)) amounts.value[c.chargeCode] = c.amount
    } catch {
      // the check on Approve says what is wrong
    }
  }, 400)
}
function setTiming(c: CfCharge, t: string) {
  timings.value = { ...timings.value, [c.chargeCode]: t }
}
const totals = computed(() => {
  const list = charges.value?.charges ?? []
  const of = (t: string) => list.filter((c) => (deciding.value ? timing(c) : c.timingCode) === t)
    .reduce((s, c) => s + (deciding.value ? amountOf(c) + taxOf(c) : c.amount + c.taxAmount), 0)
  return { financed: charges.value?.financedAmount ?? 0, added: of(FINANCED), upfront: of(UPFRONT) }
})
const claimCharges = computed(() => (charges.value?.charges ?? []).filter((c) => c.timingCode === UPFRONT).reduce((s, c) => s + c.amount + c.taxAmount, 0))
const claimDown = computed(() => (props.loan.goods?.downPaymentCollectorCode === 'COMPANY' ? props.loan.goods.downPaymentAmount : 0))

const message = (err: unknown) => (err instanceof Error ? err.message : String(err))

async function load() {
  try {
    step.value = await loansApi.cfStep(props.loan.loanId, inboxId())
    timings.value = {}
    touched.clear()
    amounts.value = Object.fromEntries((step.value?.charges?.charges ?? []).map((c) => [c.chargeCode, c.amount]))
    t.value = step.value?.terms ? { ...step.value.terms, paymentOrder: [...step.value.terms.paymentOrder] } : null
  } catch (err) {
    showError(message(err))
  }
}

async function createClaim() {
  if (!step.value?.inboxId) return
  busy.value = true
  try {
    await loansApi.createCfClaim(props.loan.loanId, step.value.inboxId)
    showSuccess(tr(claimCharges.value + claimDown.value ? 'Claim created.' : 'Nothing is paid upfront.'))
    emit('changed')
  } catch (err) {
    showError(message(err))
  } finally {
    busy.value = false
  }
}

async function complete(action: 'ACCEPT' | 'REJECT') {
  if (!step.value?.inboxId) return
  if (action === 'REJECT' && !comment.value.trim()) {
    showError(tr('Write why the finance is rejected.'))
    return
  }
  busy.value = true
  try {
    const list = charges.value?.charges ?? []
    const decision = deciding.value && action === 'ACCEPT' ? {
      chargeTimings: Object.fromEntries(list.map((c) => [c.chargeCode, timing(c)])),
      chargeAmounts: Object.fromEntries(list.map((c) => [c.chargeCode, amountOf(c)])),
      rateMarginPct: null,
      terms: t.value ? Object.fromEntries(TERM_KEYS.filter((k) => ch(k)).map((k) => [k, t.value![k]])) as CfCreditDecision['terms'] : null
    } : undefined
    const deliveryBody = step.value.stepCode === 'CF_DELIVERY' && action === 'ACCEPT'
      ? { deliveryDate: delivery.deliveryDate, deliveryNoteNo: delivery.deliveryNoteNo.trim() || null, serialNo: delivery.serialNo.trim() || null, customerConfirmed: delivery.customerConfirmed }
      : undefined
    const paymentBody = step.value.stepCode === 'CF_SUPPLIER_PAYMENT' && action === 'ACCEPT'
      ? { bankAccountId: sp.bankAccountId, reference: sp.reference.trim() || null, commissionAmount: commissionNow.value, commissionReason: sp.commissionReason.trim() || null }
      : undefined
    await loansApi.completeCfStep(props.loan.loanId, step.value.inboxId, action, comment.value.trim() || null, decision, deliveryBody, paymentBody)
    showSuccess(tr(action === 'ACCEPT' ? 'Step accepted.' : 'Finance rejected.'))
    comment.value = ''
    // the task is done: drop it from the address so the page shows the next step as someone else's
    const query = { ...route.query }
    delete query.inboxId
    await router.replace({ query })
    emit('changed')
  } catch (err) {
    showError(message(err))
  } finally {
    busy.value = false
  }
}

watch(() => [props.loan.loanId, props.version, route.query.inboxId], load, { immediate: true })
</script>

<style scoped>
.cfs { padding: 14px 18px; }
.cfs-head { display: flex; gap: 10px; align-items: center; flex-wrap: wrap; }
.cfs-head h3 { margin: 0; font-size: 15px; }
.cfs-head p { margin: 0; color: #34495a; font-size: 13.5px; }
.cfs-deal { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 10px; }
.cfs-deal > span { padding: 3px 10px; border-radius: 999px; background: #f2f4f6; color: #34495a; font-size: 12.5px; }
.cfs-deal b { color: var(--fin-ink); }
.cfs-terms { margin-top: 10px; }
.cfs-grow { flex: 1; }
.cfs-confirm { display: flex; align-items: center; gap: 8px; margin-top: 10px; font-size: 13.5px; }
.cfs-confirm input { width: 16px; height: 16px; }
.cfs-list { margin: 0; padding-inline-start: 18px; font-size: 13px; line-height: 1.7; }
.cfs-doc { display: block; padding-top: 8px; color: var(--fin-muted); }
.cfs-sub { margin: 10px 0 6px; font-size: 11.5px; font-weight: 700; letter-spacing: .4px; text-transform: uppercase; color: var(--fin-muted); }
.cfs-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px 14px; }
.cfs-grid .fc-field small { color: var(--fin-muted); font-size: 11.5px; }
.cfs-order { display: inline-flex; flex-wrap: wrap; align-items: center; gap: 6px; margin-top: 10px; font-size: 12.5px; }
.cfs-part { display: inline-flex; align-items: center; gap: 4px; padding: 3px 10px 3px 4px; border: 1px solid var(--fin-line); border-radius: 999px; }
.cfs-part button { border: 0; background: none; padding: 0 4px; cursor: pointer; color: var(--fin-teal-700); }
.cfs-part button:disabled { color: #c5ccd3; cursor: default; }
@media (max-width: 900px) { .cfs-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
.cfs-rate { display: flex; flex-wrap: wrap; align-items: center; gap: 12px; margin-top: 10px; font-size: 13px; }
.cfs-rate label { display: inline-flex; align-items: center; gap: 6px; font-weight: 700; color: #5b6b78; font-size: 12.5px; }
.cfs-rate input { width: 90px; height: 30px; text-align: end; }
.cfs-eff { padding: 3px 12px; border-radius: 999px; background: var(--fin-teal-50); color: var(--fin-teal-700); }
.cfs-amt { width: 110px; height: 30px; text-align: end; }
.cfs-rule { margin-inline-start: 6px; white-space: nowrap; }
/* every row on one centred line; amount and timing centred in their columns */
.cfs :deep(.fx-table) th, .cfs :deep(.fx-table) td { vertical-align: middle; }
.cfs .cfs-c { text-align: center; white-space: nowrap; }
.cfs .cfs-c > * { vertical-align: middle; }
.cfs-deal { align-items: center; }
.chg { border-color: #d39b2a !important; background: #fffaf0 !important; }
.cfs-ic { display: grid; place-items: center; width: 34px; height: 34px; border-radius: 9px; background: var(--fin-teal-50); color: var(--fin-teal-700); flex: none; }
.cfs-box { margin-top: 12px; padding-top: 10px; border-top: 1px solid #f0f2f4; }
.cfs-box h4 { margin: 0 0 6px; font-size: 13px; color: var(--fin-muted); }
.cfs-box p { margin: 6px 0; font-size: 13px; }
.cfs-sum { color: #34495a; }
.cfs-bad { color: var(--fin-danger, #b42318); }
.cfs-ovr { margin-inline-start: 6px; color: #b06a00; font-weight: 600; }
.cfs-basis { color: #34495a; font-size: 12.5px; }
.cfs-seg { display: inline-flex; border: 1px solid var(--fin-line); border-radius: 8px; overflow: hidden; }
.cfs-seg button { padding: 5px 10px; border: 0; background: #fff; color: #34495a; font: 600 12px/1 inherit; cursor: pointer; }
.cfs-seg button.on { background: var(--fin-teal-600); color: #fff; }
.cfs-pay { display: flex; flex-wrap: wrap; align-items: flex-end; gap: 10px; }
.cfs-pay .fc-field { min-width: 180px; }
.cfs-note { margin: 12px 0 0; font-size: 13px; color: #34495a; }
.cfs-note .fa { color: var(--fin-teal-700); margin-inline-end: 6px; }
.cfs-actions { display: flex; gap: 8px; align-items: center; margin-top: 12px; }
.cfs-comment { flex: 1; min-width: 0; height: 36px; }
</style>
