// the icon the admin dialogs put on the same action (Cancel ×, Save, Delete, Back ←, Next →)
export function buttonIcon(name: string, action?: string | null): string {
  const n = name.toUpperCase()
  if (['CANCEL', 'CLOSE'].includes(n)) return 'pi pi-times'
  if (n.includes('DELETE') || n.includes('REMOVE')) return 'pi pi-trash'
  if (['BACK', 'PREVIOUS', 'PREV'].includes(n)) return 'pi pi-arrow-left'
  if (n.includes('APPROVE')) return 'pi pi-check'
  if (n.includes('REJECT')) return 'pi pi-ban'
  if (['NEXT', 'CONTINUE'].includes(n)) return 'pi pi-arrow-right'
  if (n.includes('CREATE') || n.includes('SAVE') || n.includes('APPLY') || n.includes('SUBMIT')) return 'pi pi-save'
  return action === 'SUBMIT' ? 'pi pi-check' : ''
}
