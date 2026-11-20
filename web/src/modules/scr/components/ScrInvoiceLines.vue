<template>
  <!-- An invoice's lines, one per user (months billed, inquiries made), and its totals: for the preview and the invoice page. -->
  <div class="scr-inv">
    <div class="fx-table-wrap">
      <table class="fx-table scr-inv-table">
        <thead>
          <tr><th>#</th><th>{{ tr('User') }}</th><th>{{ tr('Months') }}</th><th class="fx-r">{{ tr('User fee') }}</th><th class="fx-r">{{ tr('Inquiries') }}</th>
            <th class="fx-r">{{ tr('Inquiry fee') }}</th><th class="fx-r">{{ tr('Amount') }}</th></tr>
        </thead>
        <tbody>
          <tr v-for="l in invoice.lines" :key="l.lineNo">
            <td>{{ l.lineNo }}</td>
            <td><b dir="ltr">{{ l.userName }}</b><span v-if="l.fullName" class="fx-cell-sub">{{ l.fullName }}</span></td>
            <td><span dir="ltr">{{ l.userMonths }}</span><span v-if="l.monthsText" class="fx-cell-sub" dir="ltr">{{ l.monthsText }}</span></td>
            <td class="fx-r" dir="ltr">{{ egp(l.userAmount) }}</td>
            <td class="fx-r" dir="ltr">{{ l.inquiryCount }}</td>
            <td class="fx-r" dir="ltr">{{ egp(l.inquiryAmount) }}</td>
            <td class="fx-r" dir="ltr"><b>{{ egp(l.lineAmount) }}</b></td>
          </tr>
          <tr v-if="!invoice.lines.length"><td colspan="7" class="fx-empty-cell">{{ tr('No active users and no inquiries in this period.') }}</td></tr>
        </tbody>
      </table>
    </div>
    <dl class="scr-inv-totals">
      <div><dt>{{ tr('Users') }} ({{ tr('user-months') }}): <span dir="ltr">{{ invoice.userMonths }} × {{ egp(invoice.userMonthlyFee) }}</span></dt><dd dir="ltr">{{ egp(invoice.userAmount) }}</dd></div>
      <div><dt>{{ tr('Inquiries') }}: <span dir="ltr">{{ invoice.inquiryCount }} × {{ egp(invoice.inquiryFee) }}</span></dt><dd dir="ltr">{{ egp(invoice.inquiryAmount) }}</dd></div>
      <div class="scr-inv-total"><dt>{{ tr('Total') }}</dt><dd dir="ltr">{{ egp(invoice.totalAmount) }}</dd></div>
    </dl>
  </div>
</template>

<script setup lang="ts">
import { tr } from '@/shared/layout/language'
import { egp, type ScrInvoice } from '../api/scr.api'

defineProps<{ invoice: ScrInvoice }>()
</script>
