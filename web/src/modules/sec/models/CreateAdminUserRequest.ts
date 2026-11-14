export interface CreateAdminUserRequest {
  userName: string
  password: string
  email?: string | null
  fullName?: string | null
  employeeNo?: string | null
  department?: string | null
  jobTitle?: string | null
  isActive: boolean
  roles: string[]
}
