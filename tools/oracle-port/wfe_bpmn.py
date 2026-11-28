"""Builds a BPMN 2.0 diagram for the C# workflow engine from an APEX WFE workflow definition.

The APEX steps become user tasks (role, target screen, the step's actions) and the APEX transitions become sequence
flows named after the action that takes them (APPROVE, REJECT; a COMPLETED or ANY transition is the unnamed default).
A step whose action has no transition ends the workflow, as WFE_OPERATIONS_PKG.FINISH_TASK does, so the diagram gets
an explicit flow to the end event for it.

usage: python3 wfe_bpmn.py WORKFLOW_CODE out.bpmn [route-prefix]
"""
import csv
import json
import sys
from pathlib import Path
from xml.sax.saxutils import quoteattr

DATA = Path('/mnt/project-files/dotnet-migration/apex-export/data')
ACTION_OF_OUTCOME = {'APPROVED': 'APPROVE', 'REJECTED': 'REJECT', 'RETURNED': 'RETURN'}


def rows(name):
    with open(DATA / f'{name}.csv', encoding='utf-8-sig') as f:
        return list(csv.DictReader(f))


def build(code, route_prefix):
    definition = next(r for r in rows('WFE_WORKFLOW_DEFINITIONS') if r['WORKFLOW_CODE'] == code)
    wid = definition['WORKFLOW_DEF_ID']
    steps = sorted((r for r in rows('WFE_WORKFLOW_STEPS') if r['WORKFLOW_DEF_ID'] == wid and r['IS_ACTIVE'] != 'N'),
                   key=lambda r: int(r['STEP_ORDER'] or 0))
    by_id = {s['STEP_ID']: s for s in steps}
    actions = {}
    for a in sorted(rows('WFE_WORKFLOW_STEP_ACTIONS'), key=lambda r: int(r['DISPLAY_SEQ'] or 0)):
        if a['STEP_ID'] in by_id and a['IS_ACTIVE'] != 'N':
            actions.setdefault(a['STEP_ID'], []).append(a['ACTION_CODE'])
    transitions = sorted((t for t in rows('WFE_WORKFLOW_STEP_TRANSITIONS') if t['WORKFLOW_DEF_ID'] == wid and t['IS_ACTIVE'] != 'N'),
                         key=lambda t: int(t['TRANSITION_ORDER'] or 0))

    def element_id(step):
        return 'EndEvent_1' if step['IS_END_STEP'] == 'Y' else f"Task_{step['STEP_CODE']}"

    tasks = [s for s in steps if s['IS_END_STEP'] != 'Y']
    end = next(s for s in steps if s['IS_END_STEP'] == 'Y')
    start = next(s for s in tasks if s['IS_START_STEP'] == 'Y')

    # Sequence flows: start -> first step, then the APEX transitions. A forward (default) flow is listed first,
    # because the engine takes the first flow when no flow name matches the action.
    flows = [('Flow_Start', 'StartEvent_1', element_id(start), None)]
    for s in tasks:
        own = [t for t in transitions if t['FROM_STEP_ID'] == s['STEP_ID']]
        own.sort(key=lambda t: (t['REQUIRED_OUTCOME'] in ACTION_OF_OUTCOME and t['REQUIRED_OUTCOME'] != 'APPROVED', int(t['TRANSITION_ORDER'] or 0)))
        names = set()
        for t in own:
            name = ACTION_OF_OUTCOME.get(t['REQUIRED_OUTCOME'])
            names.add(name)
            flows.append((f"Flow_{s['STEP_CODE']}_{name or 'NEXT'}", element_id(s), element_id(by_id[t['TO_STEP_ID']]), name))
        # An action with no transition finishes the workflow (WFE_OPERATIONS_PKG.FINISH_TASK).
        for action in ('APPROVE', 'REJECT'):
            if action in actions.get(s['STEP_ID'], []) and action not in names and None not in names:
                flows.append((f"Flow_{s['STEP_CODE']}_{action}", element_id(s), 'EndEvent_1', action))

    # Layout: the steps in one row, rejections drop to a lane below and run to the end event.
    x0, y, w, h, gap = 120, 160, 130, 80, 50
    pos = {'StartEvent_1': (x0, y + 22, 36, 36)}
    for i, s in enumerate(tasks):
        pos[element_id(s)] = (x0 + 80 + i * (w + gap), y, w, h)
    pos['EndEvent_1'] = (x0 + 80 + len(tasks) * (w + gap), y + 22, 36, 36)

    def centre_right(eid):
        x, yy, ww, hh = pos[eid]
        return x + ww, yy + hh // 2

    def centre_left(eid):
        x, yy, ww, hh = pos[eid]
        return x, yy + hh // 2

    def bottom(eid):
        x, yy, ww, hh = pos[eid]
        return x + ww // 2, yy + hh

    out = []
    add = out.append
    add('<?xml version="1.0" encoding="UTF-8"?>')
    add('<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" '
        'xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" '
        'xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" '
        f'id="Definitions_{code}" targetNamespace="http://finova/workflow">')
    variables = json.dumps(['ENTITY_ID', 'ENTITY_REF', 'ENTITY_TYPE', 'MODULE_CODE'])
    add(f'  <bpmn:process id="Process_{code}" name={quoteattr(definition["WORKFLOW_NAME"])} isExecutable="true" finova:variables={quoteattr(variables)}>')

    def io(eid):
        parts = [f'<bpmn:incoming>{f[0]}</bpmn:incoming>' for f in flows if f[2] == eid]
        parts += [f'<bpmn:outgoing>{f[0]}</bpmn:outgoing>' for f in flows if f[1] == eid]
        return ''.join(parts)

    add(f'    <bpmn:startEvent id="StartEvent_1" name="Start">{io("StartEvent_1")}</bpmn:startEvent>')
    for s in tasks:
        page = s['TARGET_PAGE_ID']
        # The screen's first item (e.g. P209_PORTFOLIO_ID) gets the business record, as TARGET_ITEM_VALUES_TEMPLATE #ENTITY_ID#.
        item = (s['TARGET_ITEM_NAMES'] or '').split(',')[0].strip()
        params = json.dumps([{'name': item, 'value': 'ENTITY_ID'}]) if page and item else ''
        attrs = {
            'finova:stepCode': s['STEP_CODE'],
            'finova:stepType': 'HumanTask',
            'finova:assignmentType': 'Role' if s['ASSIGNEE_TYPE'] == 'ROLE' else 'User',
            'finova:assignee': s['ASSIGNEE_ROLE_CODE'] or s['ASSIGNEE_ID'],
            'finova:route': f'{route_prefix}{page}' if page else '',
            'finova:routeParameters': params,
            'finova:actions': ','.join(actions.get(s['STEP_ID'], [])),
        }
        a = ' '.join(f'{k}={quoteattr(v)}' for k, v in attrs.items() if v)
        add(f'    <bpmn:userTask id="{element_id(s)}" name={quoteattr(s["STEP_NAME"])} {a}>{io(element_id(s))}</bpmn:userTask>')
    add(f'    <bpmn:endEvent id="EndEvent_1" name={quoteattr(end["STEP_NAME"])}>{io("EndEvent_1")}</bpmn:endEvent>')
    for fid, src, dst, name in flows:
        n = f' name="{name}" finova:outcome="{name}"' if name else ''
        add(f'    <bpmn:sequenceFlow id="{fid}" sourceRef="{src}" targetRef="{dst}"{n} />')
    add('  </bpmn:process>')

    add('  <bpmndi:BPMNDiagram id="BPMNDiagram_1">')
    add(f'    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_{code}">')
    for eid, (x, yy, ww, hh) in pos.items():
        add(f'      <bpmndi:BPMNShape id="{eid}_di" bpmnElement="{eid}"><dc:Bounds x="{x}" y="{yy}" width="{ww}" height="{hh}" /></bpmndi:BPMNShape>')
    lane = y + h + 70
    for fid, src, dst, name in flows:
        if dst == 'EndEvent_1' and name in ('REJECT', 'RETURN') or (dst == 'EndEvent_1' and src != tasks[-1] and pos[src][0] + pos[src][2] + gap < pos[dst][0]):
            bx, by = bottom(src)
            ex, ey = bottom(dst)
            points = [(bx, by), (bx, lane), (ex, lane), (ex, ey)]
        else:
            points = [centre_right(src), centre_left(dst)]
        wps = ''.join(f'<di:waypoint x="{px}" y="{py}" />' for px, py in points)
        add(f'      <bpmndi:BPMNEdge id="{fid}_di" bpmnElement="{fid}">{wps}</bpmndi:BPMNEdge>')
    add('    </bpmndi:BPMNPlane>')
    add('  </bpmndi:BPMNDiagram>')
    add('</bpmn:definitions>')
    return definition, '\n'.join(out) + '\n'


if __name__ == '__main__':
    definition, xml = build(sys.argv[1], sys.argv[3] if len(sys.argv) > 3 else 'mtg-')
    Path(sys.argv[2]).write_text(xml, encoding='utf-8')
    print(f"{definition['WORKFLOW_CODE']}: {xml.count('<bpmn:userTask')} steps, {xml.count('<bpmn:sequenceFlow')} flows")
