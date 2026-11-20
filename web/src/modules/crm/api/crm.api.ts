// CRM clients (api/crm/clients), from FINOVA_FINANCEING api/clients and its LOVs.
import { http } from '@/shared/api/http'

/** CRM_CLIENTS, as the CRM home lists them */
export type CrmClientRow = {
  clientId: number
  clientNumber: string
  clientName: string
  clientType: string
  phoneNumber: string | null
  email: string | null
  isActive: boolean
  /** the newest of the client's applications, agreements and disbursement requests; null when it has none */
  lastActivityAt: string | null
  lastActivityType: 'Application' | 'Agreement' | 'Disbursement' | null
  lastActivityRef: string | null
}

/** The fields of the client dialog */
export type CrmClientForm = {
  clientId: number | null
  clientNumber: string
  clientName: string
  clientNameAr: string
  shortName: string | null
  clientType: string
  clientRoleCode: string | null
  legalStructure: string
  registrationNumber: string | null
  taxNumber: string | null
  email: string | null
  phoneNumber: string | null
  mobileNumber: string | null
  website: string | null
  countryId: number
  baseCurrencyId: number
  statusId: number
  isActive: boolean
  isSupplier: boolean
  isRelatedParty: boolean
  /** only when a customer is created: its first role, which starts as Onboarding */
  initialRoleCode?: string | null
}

export type CrmOption = { value: string; label: string }
export type CrmClientOptions = { countries: CrmOption[]; currencies: CrmOption[]; clientRoles: CrmOption[] }

const API = '/api/crm/clients'

export const crmApi = {
  clients: () => http.get<CrmClientRow[]>(API),
  options: () => http.get<CrmClientOptions>(`${API}/options`),
  client: (clientId: number) => http.get<CrmClientForm>(`${API}/${clientId}`),
  create: (form: CrmClientForm) => http.post<CrmClientForm>(API, form),
  update: (clientId: number, form: CrmClientForm) => http.put<CrmClientForm>(`${API}/${clientId}`, form)
}

// ---------------------------------------------------------------- Customer 360 (api/crm/clients/{id}/360)

export type CrmBadge = { code: string; label: string; reasons: string[] }

export type CrmProfile = {
  clientId: number; clientNumber: string; clientName: string; clientNameAr: string; shortName: string | null; clientType: string
  legalStructure: string; taxNumber: string | null; registrationNumber: string | null; vatNumber: string | null; nationalId: string | null
  countryName: string | null; currencyCode: string | null; statusName: string | null; email: string | null; phoneNumber: string | null
  mobileNumber: string | null; website: string | null; groupName: string | null; isRelatedParty: boolean; isActive: boolean
}

export type CrmCompliance = {
  riskGrade: string | null; kycStatus: string | null; kycExpiryDate: string | null; amlStatus: string | null; amlCheckDate: string | null
  sanctionsStatus: string | null; isPep: boolean; isBlacklisted: boolean; blacklistReason: string | null; nextReviewDate: string | null
  notes: string | null
}

export type CrmComplianceState = {
  profile: CrmCompliance; changedBy: string | null; changedAt: string | null; verifiedBy: string | null; verifiedAt: string | null
  canVerify: boolean; verifyBlockedReason: string | null
}

export type CrmComplianceHistory = {
  complianceHistoryId: number; actionCode: string; kycStatus: string | null; kycExpiryDate: string | null; amlStatus: string | null
  sanctionsStatus: string | null; isPep: boolean; isBlacklisted: boolean; riskGrade: string | null; nextReviewDate: string | null
  notes: string | null; by: string; at: string
}

export type CrmRole = {
  clientRoleId: number; roleCode: string; roleLabel: string; statusCode: string; activeSinceDate: string | null; notes: string | null
  activationBlockers: string[]
}

export type CrmBankAccount = {
  bankAccountId: number; bankName: string; branchName: string | null; swiftCode: string | null; accountNumber: string | null; iban: string | null
  accountHolderName: string; accountHolderNameAr: string | null; currencyId: number; currencyCode: string; accountTypeCode: string
  isPrimary: boolean; statusCode: string; changedBy: string; changedAt: string; verifiedBy: string | null; verifiedAt: string | null
  verificationMethodCode: string | null; notes: string | null; payableFromAt: string; isPayable: boolean
}

export type CrmBankAccountHistory = {
  bankAccountHistoryId: number; bankAccountId: number; actionCode: string; bankName: string; accountNumber: string | null; iban: string | null
  currencyCode: string; isPrimary: boolean; statusCode: string; verificationMethodCode: string | null; notes: string | null; by: string; at: string
}

export type CrmProgram = {
  agreementId: number; agreementNumber: string; programRole: string; factoringType: string | null; statusCode: string | null
  currencyCode: string | null; approvedLimit: number | null; utilizedAmount: number | null; availableAmount: number | null
}

export type CrmCustomer360 = {
  profile: CrmProfile; complianceBadge: CrmBadge; bankBadge: CrmBadge; readyToPay: CrmBadge; compliance: CrmComplianceState
  complianceHistory: CrmComplianceHistory[]; roles: CrmRole[]; bankAccounts: CrmBankAccount[]; bankAccountHistory: CrmBankAccountHistory[]
  programs: CrmProgram[]; bankAccountWaitingDays: number
  /** Module Settings: whether a second user must verify a bank account change / a compliance change. */
  bankAccountMakerChecker: boolean; complianceMakerChecker: boolean
}

export type CrmBankAccountForm = {
  bankName: string; branchName: string | null; swiftCode: string | null; accountNumber: string | null; iban: string | null
  accountHolderName: string; accountHolderNameAr: string | null; currencyId: number; accountTypeCode: string; isPrimary: boolean; notes: string | null
}

const c360 = (id: number) => `${API}/${id}`

export const crm360Api = {
  get: (id: number) => http.get<CrmCustomer360>(`${c360(id)}/360`),
  saveCompliance: (id: number, body: CrmCompliance) => http.put<void>(`${c360(id)}/compliance`, body),
  verifyCompliance: (id: number, notes: string | null) => http.post<void>(`${c360(id)}/compliance/verify`, { notes }),
  addRole: (id: number, roleCode: string, notes: string | null) => http.post<void>(`${c360(id)}/roles`, { roleCode, notes }),
  activateRole: (id: number, roleId: number) => http.post<void>(`${c360(id)}/roles/${roleId}/activate`, {}),
  suspendRole: (id: number, roleId: number, notes: string) => http.post<void>(`${c360(id)}/roles/${roleId}/suspend`, { notes }),
  addBankAccount: (id: number, body: CrmBankAccountForm) => http.post<number>(`${c360(id)}/bank-accounts`, body),
  updateBankAccount: (id: number, accountId: number, body: CrmBankAccountForm) => http.put<number>(`${c360(id)}/bank-accounts/${accountId}`, body),
  verifyBankAccount: (id: number, accountId: number, verificationMethodCode: string, notes: string | null) =>
    http.post<void>(`${c360(id)}/bank-accounts/${accountId}/verify`, { verificationMethodCode, notes }),
  suspendBankAccount: (id: number, accountId: number, notes: string) => http.post<void>(`${c360(id)}/bank-accounts/${accountId}/suspend`, { notes }),
  reactivateBankAccount: (id: number, accountId: number) => http.post<void>(`${c360(id)}/bank-accounts/${accountId}/reactivate`, {})
}
