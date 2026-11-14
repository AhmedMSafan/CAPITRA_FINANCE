export interface AdminRole {
  id: string
  name: string
  description?: string | null
  permissions: string[]
}
