export interface UpdateAdminUserRequest {
  email?: string | null
  fullName?: string | null
  employeeNo?: string | null
  department?: string | null
  jobTitle?: string | null
  isActive: boolean
}
