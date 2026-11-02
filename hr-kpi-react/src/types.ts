import type {LucideIcon} from 'lucide-react'

export type Role = 'Quản trị viên' | 'Người quản lý' | 'Nhân viên'

export type EmployeeStatus = 'Đang làm việc' | 'Ngừng hoạt động'
export type AccountStatus = 'Hoạt động' | 'Bị khóa'
export type PeriodStatus = 'Chưa bắt đầu' | 'Đang diễn ra' | 'Đã kết thúc'
export type KpiStatus = 'Chờ cập nhật' | 'Chờ đánh giá' | 'Đã đánh giá'

export interface Employee {
  id: number
  code: string
  name: string
  email: string
  phone: string
  departmentId: number
  positionId: number
  status: EmployeeStatus | string
}

export interface Department {
  id: number
  code: string
  name: string
  description?: string
}

export interface Position {
  id: number
  code: string
  name: string
}

export interface Account {
  id: number
  username: string
  employeeId: number
  role: Role | string
  status: AccountStatus | string
}

export interface Period {
  id: number
  name: string
  start: string
  end: string
  status: PeriodStatus | string
}

export interface Criterion {
  id: number
  name: string
  description?: string
  defaultWeight: number
}

export interface Kpi {
  id: number
  periodId: number
  employeeId: number
  criterionId: number
  target: string
  weight: number
  result: string | number | null
  note?: string
  score: number | null
  status: KpiStatus | string
  published: boolean
}

export interface AppData {
  employees: Employee[]
  departments: Department[]
  positions: Position[]
  accounts: Account[]
  periods: Period[]
  criteria: Criterion[]
  kpis: Kpi[]
}

export type DataKey = keyof AppData
export type Entity = AppData[DataKey][number]

export interface SessionUser {
  username: string
  role: Role | string
  employeeId: number
  name: string
  mustChangePassword: boolean
}

export type PageKey =
  | 'dashboard'
  | 'employees'
  | 'departments'
  | 'positions'
  | 'accounts'
  | 'permissions'
  | 'periods'
  | 'criteria'
  | 'setup'
  | 'mykpi'
  | 'evaluations'
  | 'reports'
  | 'changepassword'

export type ModalType =
  | 'employee'
  | 'department'
  | 'position'
  | 'account'
  | 'period'
  | 'criterion'
  | 'kpi'
  | 'result'
  | 'evaluate'

export interface ModalState {
  type: ModalType
  item: Entity | null
}

export type FormValues = Record<string, string | number | boolean | null | undefined>

export type NavItem = [PageKey, string, LucideIcon]
export interface NavGroup {
  section: string
  items: NavItem[]
}
