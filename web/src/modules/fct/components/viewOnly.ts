// A completed workflow step's page opened from the customer's Disbursements tab (Ahmed, 7 Nov 2026): ?view=1 shows it view-only,
// without the step's actions or any save.
import { computed } from 'vue'
import { useRoute } from 'vue-router'

export function useViewOnly() {
  const route = useRoute()
  return computed(() => route.query.view === '1')
}
