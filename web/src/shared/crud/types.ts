// Configuration for CrudPage: one list + one modal form, the Vue version of an APEX report page and its modal form page.

export type ColumnType = 'text' | 'code' | 'number' | 'amount' | 'pct' | 'date' | 'bool'

export type CrudColumn<T> = {
  field: keyof T & string
  label: string
  type?: ColumnType
  rtl?: boolean
}

export type FieldOption = { value: string | number; label: string }

export type FieldType = 'text' | 'textarea' | 'number' | 'amount' | 'pct' | 'date' | 'bool' | 'select'

export type CrudField = {
  name: string
  label: string
  type: FieldType
  required?: boolean
  maxLength?: number
  min?: number
  max?: number
  options?: FieldOption[]
  rtl?: boolean
  /** Spans both columns of the form grid. */
  wide?: boolean
  /** Shown but not editable once the record exists (codes). */
  lockedOnEdit?: boolean
  uppercase?: boolean
  /** Shown only while it returns true for the form's values; a hidden field is sent as null and is not required. */
  showIf?: (values: FormValues) => boolean
}

export type FormValues = Record<string, unknown>

export type CrudApi<T> = {
  list: () => Promise<T[]>
  create: (values: FormValues) => Promise<T>
  update: (row: T, values: FormValues) => Promise<T>
  remove: (row: T) => Promise<void>
}
