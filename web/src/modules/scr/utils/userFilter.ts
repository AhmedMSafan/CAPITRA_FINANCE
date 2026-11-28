// The user filter of every Screening list. A list opens on the items the signed-in user entered ("Entered by me"); an Admin
// opens on everything. Where items move through the workflow, "Waiting for me" lists those whose current step is with the user
// or one of their roles. Any other user shows the items that user entered. ?user= in the address picks a view ('' = all users).
import { computed, ref, type Ref } from 'vue'
import { useRoute } from 'vue-router'
import { getUser } from '@/shared/auth/session'

export const MINE = '__mine__'
export const WAITING = '__waiting__'
export const ADMIN_ROLE = 'ADMIN'

const same = (a: string | null | undefined, b: string) => !!a && a.toLowerCase() === b.toLowerCase()

export function useUserFilter<T>(
  items: Ref<T[]>,
  enteredBy: (item: T) => string | null | undefined,
  /** The user or role the item's current step is with; only for lists of items in the workflow. */
  holder?: (item: T) => string | null | undefined
) {
  const user = getUser()
  const me = user?.userName ?? ''
  const roles = new Set((user?.roles ?? []).map((r) => r.toUpperCase()))
  const isAdmin = roles.has(ADMIN_ROLE)
  const route = useRoute()
  const who = ref<string>(typeof route.query.user === 'string' ? route.query.user : isAdmin ? '' : MINE)

  const isMine = (h: string | null | undefined) => !!h && (same(h, me) || roles.has(h.toUpperCase()))
  const waitingForMe = (item: T) => !!holder && isMine(holder(item))
  const users = computed(() => [...new Set(items.value.map(enteredBy).filter((u): u is string => !!u && !same(u, me)))].sort())
  const scoped = computed(() => items.value.filter((item) => {
    switch (who.value) {
      case '': return true
      case MINE: return same(enteredBy(item), me)
      case WAITING: return waitingForMe(item)
      default: return same(enteredBy(item), who.value)
    }
  }))
  const waitingCount = computed(() => (holder ? items.value.filter(waitingForMe).length : 0))

  return { who, me, isAdmin, users, scoped, waitingCount, isMine, withWaiting: !!holder }
}
