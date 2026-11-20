export type FlowStep = { id: string; name: string; gateway: boolean }
export type Flow = { steps: FlowStep[]; flows: number; gateways: number }

const TASKS = ['task', 'userTask', 'serviceTask', 'scriptTask', 'manualTask', 'businessRuleTask', 'sendTask', 'receiveTask', 'callActivity', 'subProcess']
const GATEWAYS = ['exclusiveGateway', 'parallelGateway', 'inclusiveGateway', 'eventBasedGateway', 'complexGateway']

/** The steps of a BPMN diagram in the order the flow reaches them from its start event. */
export function readFlow(xml: string): Flow {
  const flow: Flow = { steps: [], flows: 0, gateways: 0 }
  if (!xml) return flow
  const doc = new DOMParser().parseFromString(xml, 'application/xml')
  const process = [...doc.getElementsByTagName('*')].find((e) => e.localName === 'process')
  if (!process) return flow
  const nodes = new Map<string, Element>()
  const next = new Map<string, string[]>()
  for (const e of process.children) {
    const id = e.getAttribute('id') || ''
    if (e.localName === 'sequenceFlow') {
      flow.flows++
      const src = e.getAttribute('sourceRef') || ''
      next.set(src, [...(next.get(src) ?? []), e.getAttribute('targetRef') || ''])
    } else nodes.set(id, e)
  }
  const seen = new Set<string>()
  const queue = [...nodes.values()].filter((e) => e.localName === 'startEvent').map((e) => e.getAttribute('id') || '')
  const ordered: Element[] = []
  while (queue.length) {
    const id = queue.shift()!
    if (seen.has(id)) continue
    seen.add(id)
    const e = nodes.get(id)
    if (e) ordered.push(e)
    queue.push(...(next.get(id) ?? []))
  }
  // steps the flow does not reach still count
  for (const e of nodes.values()) if (!seen.has(e.getAttribute('id') || '')) ordered.push(e)
  for (const e of ordered) {
    const gateway = GATEWAYS.includes(e.localName)
    if (!gateway && !TASKS.includes(e.localName)) continue
    if (gateway) flow.gateways++
    const id = e.getAttribute('id') || ''
    flow.steps.push({ id, name: e.getAttribute('name') || id, gateway })
  }
  return flow
}
