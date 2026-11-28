import { currentLang } from '@/shared/layout/language'

/**
 * Opens the Workflow user manual (served from public/manuals) in a new tab, in the app's language, at a section:
 * 3 designing, 6 documents and data sources, 7 working tasks, 8 start a request, 9 out of office, 10 rules on paths,
 * 11 parallel approvals, 12 document sections, 13 process monitor.
 */
export function openWorkflowManual(section?: number) {
  const lang = currentLang() === 'AR' ? 'ar' : 'en'
  const hash = section === undefined ? lang : `${lang}-${section}`
  window.open(`${import.meta.env.BASE_URL}manuals/workflow/workflow-manual.html#${hash}`, '_blank', 'noopener')
}
