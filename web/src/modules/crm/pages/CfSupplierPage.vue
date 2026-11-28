<template>
  <!-- A supplier application's link (the inbox opens it for every CF_SUPPLIER_APPROVAL step): it opens the customer's Consumer Finance
       Supplier tab in Customer 360, with the task (?inboxId=). -->
  <div class="fx fc"><div class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div></div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { errorText } from '@/modules/scr/api/scr.api'
import { cfSuppliersApi } from '../api/cfSuppliers.api'

const props = defineProps<{ applicationId: number }>()
const route = useRoute()
const router = useRouter()

onMounted(async () => {
  try {
    const app = await cfSuppliersApi.get(props.applicationId)
    const query: Record<string, string> = { tab: 'cf-supplier' }
    if (route.query.inboxId) query.inboxId = String(route.query.inboxId)
    await router.replace({ name: 'crm-customer-workspace', params: { clientId: app.clientId }, query })
  } catch (err) {
    showError(errorText(err))
  }
})
</script>
