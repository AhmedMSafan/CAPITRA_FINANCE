import { http } from '@/shared/api/http'

/** One setting of Administration > Module Settings, with the change waiting for approval if there is one. */
export type ModuleSetting = {
  moduleCode: string
  code: string
  group: string
  label: string
  help: string
  type: 'BOOL' | 'INT' | 'DECIMAL' | 'CHOICE'
  value: string
  valueLabel: string
  defaultValue: string
  isDefault: boolean
  min: number | null
  max: number | null
  unit: string | null
  choices: { value: string; label: string }[]
  /** A yes/no setting of the same module: this one is shown only while that one is Yes. */
  shownWhen: string | null
  pendingValue: string | null
  pendingLabel: string | null
  pendingReason: string | null
  requestedBy: string | null
  requestedAt: string | null
  changedBy: string | null
  changedAt: string | null
  approvedBy: string | null
  approvedAt: string | null
  canApprove: boolean
  approveBlockedReason: string | null
}

export type ModuleSettingsModule = { code: string; title: string; description: string; icon: string; pendingCount: number; settings: ModuleSetting[] }

export type ModuleSettingHistory = {
  historyId: number; code: string; label: string; actionCode: 'REQUEST' | 'APPROVE' | 'REJECT' | 'CANCEL'
  oldValue: string | null; newValue: string | null; notes: string | null; by: string; at: string
}

export type ModuleSettingsPage = { modules: ModuleSettingsModule[]; history: ModuleSettingHistory[] }

const API = '/api/adm/module-settings'
const at = (code: string, action: string) => `${API}/${encodeURIComponent(code)}/${action}`

export const moduleSettingsApi = {
  get: () => http.get<ModuleSettingsPage>(API),
  request: (code: string, value: string, reason: string) => http.post<ModuleSettingsPage>(at(code, 'request'), { value, reason }),
  approve: (code: string, notes: string | null) => http.post<ModuleSettingsPage>(at(code, 'approve'), { notes }),
  reject: (code: string, notes: string) => http.post<ModuleSettingsPage>(at(code, 'reject'), { notes }),
  cancel: (code: string) => http.post<ModuleSettingsPage>(at(code, 'cancel'), { notes: null })
}
