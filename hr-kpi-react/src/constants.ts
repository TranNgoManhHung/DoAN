import {LayoutDashboard,Users,Building2,BriefcaseBusiness,UserCog,ShieldCheck,CalendarDays,Target,ClipboardList,ChartNoAxesCombined,CheckCircle2} from 'lucide-react'
import type {ModalType, NavGroup, PageKey, SessionUser} from './types'

export const NAV: NavGroup[] = [
  {section:'TỔNG QUAN',items:[['dashboard','Tổng quan',LayoutDashboard]]},
  {section:'QUẢN LÝ NHÂN SỰ',items:[['employees','Nhân viên',Users],['departments','Phòng ban',Building2],['positions','Chức vụ',BriefcaseBusiness]]},
  {section:'QUẢN TRỊ HỆ THỐNG',items:[['accounts','Tài khoản',UserCog],['permissions','Vai trò & quyền',ShieldCheck]]},
  {section:'QUẢN LÝ KPI',items:[['periods','Kỳ đánh giá',CalendarDays],['criteria','Tiêu chí KPI',Target],['setup','Thiết lập & giao KPI',ClipboardList],['mykpi','KPI của tôi',CheckCircle2],['evaluations','Đánh giá KPI',ChartNoAxesCombined],['reports','Thống kê & báo cáo',ChartNoAxesCombined]]}
]

export const TITLES: Record<PageKey, string> = {
  dashboard:'Tổng quan',
  employees:'Quản lý nhân viên',
  departments:'Quản lý phòng ban',
  positions:'Quản lý chức vụ',
  accounts:'Quản lý tài khoản',
  permissions:'Vai trò và phân quyền',
  periods:'Kỳ đánh giá KPI',
  criteria:'Tiêu chí KPI',
  setup:'Thiết lập và giao KPI',
  mykpi:'KPI của tôi',
  evaluations:'Đánh giá KPI',
  reports:'Thống kê và báo cáo',
  changepassword:'Đổi mật khẩu'
}

export const MODAL_KIND: Record<string, ModalType> = {
  employees:'employee',
  departments:'department',
  positions:'position',
  accounts:'account',
  periods:'period',
  criteria:'criterion',
  kpis:'kpi'
}

export function exportCsv(rows: (string | number | null | undefined)[][], filename: string) {
  const csv = '\ufeff' + rows.map(r => r.map(v => `"${String(v ?? '').replaceAll('"','""')}"`).join(',')).join('\r\n')
  const a = document.createElement('a')
  a.href = URL.createObjectURL(new Blob([csv], {type:'text/csv;charset=utf-8;'}))
  a.download = filename
  a.click()
  URL.revokeObjectURL(a.href)
}

export function allowedFor(user: SessionUser | null, key: string) {
  if (!user) return false
  const isAdmin = user.role === 'Quản trị viên'
  const isManager = user.role === 'Người quản lý'
  if (key === 'dashboard' || key === 'changepassword') return true
  if (['employees','departments','positions','accounts','permissions'].includes(key)) return isAdmin || (isManager && key === 'employees')
  if (['periods','criteria','setup','evaluations','reports'].includes(key)) return isAdmin || isManager
  if (key === 'mykpi') return true
  return false
}
