import { http } from '@/shared/api/http'

export type WriteOffFigures = {
  daysLate: number; principalAmount: number; interestAmount: number; interestSuspenseAmount: number; lateFeeAmount: number
  lateFeeSuspenseAmount: number; chargesAmount: number; insuranceAmount: number; totalAmount: number
}

export type WriteOff = {
  writeOffId: number; statusCode: 'PENDING' | 'APPROVED' | 'REJECTED' | 'WITHDRAWN'; reason: string; figures: WriteOffFigures
  requestedBy: string; requestedAt: string; decidedBy: string | null; decidedAt: string | null; decisionNotes: string | null
  writeOffDate: string | null; glTranSetId: string | null; canApprove: boolean; approveBlockedReason: string | null; canWithdraw: boolean
}

export type WriteOffsPage = {
  loanId: number; loanStatusCode: string; isWriteOffProposed: boolean; isNonAccrual: boolean; lastEodDate: string | null; today: string
  figures: WriteOffFigures; canRequest: boolean; requestBlockedReason: string | null; requests: WriteOff[]
}

const api = (loanId: number) => `/api/lnd/loans/${loanId}/write-offs`

export const writeOffsApi = {
  get: (loanId: number) => http.get<WriteOffsPage>(api(loanId)),
  request: (loanId: number, reason: string) => http.post<WriteOffsPage>(api(loanId), { reason }),
  withdraw: (loanId: number, id: number) => http.post<WriteOffsPage>(`${api(loanId)}/${id}/withdraw`, {}),
  approve: (loanId: number, id: number, notes: string | null) => http.post<WriteOffsPage>(`${api(loanId)}/${id}/approve`, { notes }),
  reject: (loanId: number, id: number, notes: string | null) => http.post<WriteOffsPage>(`${api(loanId)}/${id}/reject`, { notes })
}
