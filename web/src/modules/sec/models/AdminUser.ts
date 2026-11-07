export interface AdminUser {
  id: string
  userName: string
  email?: string | null
  fullName?: string | null
  employeeNo?: string | null
  department?: string | null
  jobTitle?: string | null
  isActive: boolean
  roles: string[]
}
