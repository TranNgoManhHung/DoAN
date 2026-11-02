import {createContext, useContext, useEffect, useMemo, useState, type FormEvent, type ReactNode, type SetStateAction, type Dispatch} from 'react'
import {apiRequest, emptyData, endpointFor, getToken, loadBootstrap, setToken, signIn, TOKEN_STORAGE_KEY} from '../api'
import type {AppData, DataKey, Employee, Entity, FormValues, Kpi, ModalState, ModalType, SessionUser} from '../types'

interface AppContextValue {
  data: AppData
  user: SessionUser | null
  authLoading: boolean
  login: (username: string, password: string) => Promise<string>
  logout: () => void
  toast: string
  setToast: (value: string) => void
  search: string
  setSearch: (value: string) => void
  periodFilter: string
  setPeriodFilter: (value: string) => void
  modal: ModalState | null
  setModal: (value: ModalState | null) => void
  form: FormValues
  setForm: Dispatch<SetStateAction<FormValues>>
  openModal: (type: ModalType, item?: Entity | null) => void
  saveModal: (e: FormEvent) => Promise<void>
  remove: (key: DataKey, item: Entity) => void
  toggleAccountStatus: (id: number, status: string) => void
  updateKpiResult: (id: number, result: number, note: string) => Promise<boolean>
  evaluateKpi: (id: number, score: number, published: boolean) => Promise<boolean>
  changePassword: (currentPassword: string, newPassword: string) => Promise<boolean>
  employee: (id: number | string | undefined) => Employee | undefined
  department: (id: number | string | undefined) => string
  position: (id: number | string | undefined) => string
  criterion: (id: number | string | undefined) => string
  period: (id: number | string | undefined) => string
  isAdmin: boolean
  isManager: boolean
  isEmployee: boolean
}

const AppContext = createContext<AppContextValue | null>(null)

function getErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : 'Đã xảy ra lỗi khi kết nối máy chủ.'
}

function isValidEmail(email: string) {
  return email.length <= 150 && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
}

export function AppProvider({children}: {children: ReactNode}) {
  const [data, setData] = useState<AppData>(emptyData)
  const [user, setUser] = useState<SessionUser | null>(null)
  const [authLoading, setAuthLoading] = useState(true)
  const [toast, setToast] = useState('')
  const [search, setSearch] = useState('')
  const [periodFilter, setPeriodFilter] = useState('all')
  const [modal, setModal] = useState<ModalState | null>(null)
  const [form, setForm] = useState<FormValues>({})

  useEffect(() => {
    let active = true
    let sessionRevision = 0

    async function restoreSession(token: string | null, revision: number) {
      if (!token) {
        if (active && revision === sessionRevision) {
          setUser(null)
          setData(emptyData)
          setModal(null)
          setAuthLoading(false)
        }
        return
      }

      setAuthLoading(true)
      try {
        const currentUser = await apiRequest<SessionUser>('/api/auth/me')
        const bootstrap = currentUser.mustChangePassword ? emptyData : await loadBootstrap()
        if (active && revision === sessionRevision) {
          setUser(currentUser)
          setData(bootstrap)
        }
      } catch (error) {
        if (!active || revision !== sessionRevision) return
        if (getToken() === token) setToken(null)
        setUser(null)
        setData(emptyData)
        setToast(getErrorMessage(error))
      } finally {
        if (active && revision === sessionRevision) setAuthLoading(false)
      }
    }

    function handleStorage(event: StorageEvent) {
      if (event.key !== TOKEN_STORAGE_KEY || event.storageArea !== localStorage) return
      const revision = ++sessionRevision
      if (!event.newValue) {
        void restoreSession(null, revision)
        return
      }
      void restoreSession(event.newValue, revision)
    }

    window.addEventListener('storage', handleStorage)
    void restoreSession(getToken(), ++sessionRevision)

    return () => {
      active = false
      window.removeEventListener('storage', handleStorage)
    }
  }, [])

  useEffect(() => {
    if (!toast) return
    const timer = setTimeout(() => setToast(''), 3500)
    return () => clearTimeout(timer)
  }, [toast])

  const employee = (id: number | string | undefined) => data.employees.find(x => x.id === Number(id))
  const department = (id: number | string | undefined) => data.departments.find(x => x.id === Number(id))?.name || '—'
  const position = (id: number | string | undefined) => data.positions.find(x => x.id === Number(id))?.name || '—'
  const criterion = (id: number | string | undefined) => data.criteria.find(x => x.id === Number(id))?.name || '—'
  const period = (id: number | string | undefined) => data.periods.find(x => x.id === Number(id))?.name || '—'
  const isAdmin = user?.role === 'Quản trị viên'
  const isManager = user?.role === 'Người quản lý'
  const isEmployee = user?.role === 'Nhân viên'

  async function refreshData() {
    setData(await loadBootstrap())
  }

  async function login(username: string, password: string) {
    try {
      const response = await signIn(username, password)
      setToken(response.token)
      if (response.user.mustChangePassword) setData(emptyData)
      else setData(await loadBootstrap())
      setUser(response.user)
      return ''
    } catch (error) {
      setToken(null)
      return getErrorMessage(error)
    }
  }

  function logout() {
    setToken(null)
    setUser(null)
    setData(emptyData)
    setModal(null)
  }

  function openModal(type: ModalType, item: Entity | null = null) {
    setForm(item ? {...item} : {status: type === 'account' ? 'Hoạt động' : 'Đang làm việc'})
    setModal({type, item})
  }

  async function saveModal(e: FormEvent) {
    e.preventDefault()
    if (!modal) return

    const type = modal.type
    const item: FormValues = {...form}
    let key: DataKey
    let body: Record<string, unknown>

    if (type === 'employee') {
      key = 'employees'
      body = {
        code: String(item.code || '').trim(),
        name: String(item.name || '').trim(),
        email: String(item.email || '').trim(),
        phone: String(item.phone || '').trim(),
        departmentId: Number(item.departmentId),
        positionId: Number(item.positionId),
        status: String(item.status || 'Đang làm việc')
      }
    } else if (type === 'department') {
      key = 'departments'
      body = {code: String(item.code || '').trim(), name: String(item.name || '').trim(), description: String(item.description || '')}
    } else if (type === 'position') {
      key = 'positions'
      body = {code: String(item.code || '').trim(), name: String(item.name || '').trim()}
    } else if (type === 'account') {
      key = 'accounts'
      body = {
        username: String(item.username || '').trim(),
        employeeId: Number(item.employeeId),
        role: String(item.role || 'Nhân viên'),
        status: String(item.status || 'Hoạt động')
      }
    } else if (type === 'period') {
      key = 'periods'
      body = {
        name: String(item.name || '').trim(),
        start: String(item.start || ''),
        end: String(item.end || ''),
        status: String(item.status || 'Chưa bắt đầu')
      }
    } else if (type === 'criterion') {
      key = 'criteria'
      body = {
        name: String(item.name || '').trim(),
        description: String(item.description || ''),
        defaultWeight: Number(item.defaultWeight || 0)
      }
    } else if (type === 'kpi') {
      key = 'kpis'
      body = {
        periodId: Number(item.periodId),
        employeeId: Number(item.employeeId),
        criterionId: Number(item.criterionId),
        target: String(item.target || '').trim(),
        weight: Number(item.weight)
      }
    } else {
      return
    }

    if (type === 'employee' && (!body.code || !body.name || !body.email || !body.departmentId || !body.positionId)) {
      setToast('Vui lòng nhập đầy đủ hồ sơ nhân viên.')
      return
    }
    if (type === 'employee' && !isValidEmail(String(body.email))) {
      setToast('Vui lòng nhập địa chỉ email hợp lệ (tối đa 150 ký tự).')
      return
    }
    if (type === 'department' && (!body.code || !body.name)) {
      setToast('Vui lòng nhập mã và tên phòng ban.')
      return
    }
    if (type === 'position' && (!body.code || !body.name)) {
      setToast('Vui lòng nhập mã và tên chức vụ.')
      return
    }
    if (type === 'account' && (!body.username || !body.employeeId)) {
      setToast('Vui lòng nhập tên đăng nhập và chọn nhân viên liên kết.')
      return
    }
    if (type === 'period' && (!body.name || !body.start || !body.end || String(body.start) > String(body.end))) {
      setToast('Vui lòng kiểm tra tên và thời gian kỳ đánh giá.')
      return
    }
    if (type === 'criterion' && !body.name) {
      setToast('Vui lòng nhập tên tiêu chí.')
      return
    }
    if (type === 'kpi' && (!body.target || Number(body.weight) <= 0 || Number(body.weight) > 100)) {
      setToast('Nhập mục tiêu và trọng số từ 1 đến 100.')
      return
    }

    try {
      const base = endpointFor(key)
      const id = (modal.item as Entity | null)?.id
      const result = await apiRequest<{emailSent?: boolean}>(id ? `${base}/${id}` : base, {
        method: id ? 'PUT' : 'POST',
        body
      })
      await refreshData()
      setModal(null)
      setToast(type === 'account' && !id
        ? result.emailSent
          ? 'Đã tạo tài khoản và gửi mật khẩu tạm thời tới email nhân viên.'
          : 'Đã tạo tài khoản nhưng gửi email thất bại. Hãy cấu hình SMTP rồi yêu cầu đặt lại mật khẩu.'
        : 'Đã lưu dữ liệu vào SQL Server.')
    } catch (error) {
      setToast(getErrorMessage(error))
    }
  }

  async function remove(key: DataKey, item: Entity) {
    if (!confirm('Bạn có chắc chắn muốn xóa bản ghi này?')) return
    try {
      await apiRequest(`${endpointFor(key)}/${item.id}`, {method: 'DELETE'})
      await refreshData()
      setToast(key === 'employees' && data.kpis.some(kpi => kpi.employeeId === item.id)
        ? 'Nhân viên đã được chuyển sang ngừng hoạt động.'
        : 'Đã xóa bản ghi.')
    } catch (error) {
      setToast(getErrorMessage(error))
    }
  }

  async function toggleAccountStatus(id: number, status: string) {
    const account = data.accounts.find(item => item.id === id)
    if (!account) return
    try {
      await apiRequest(`/api/accounts/${id}`, {
        method: 'PUT',
        body: {...account, status: status === 'Bị khóa' ? 'Hoạt động' : 'Bị khóa'}
      })
      await refreshData()
      setToast('Đã cập nhật trạng thái tài khoản.')
    } catch (error) {
      setToast(getErrorMessage(error))
    }
  }

  async function updateKpiResult(id: number, result: number, note: string) {
    try {
      await apiRequest(`/api/kpis/${id}/result`, {method: 'PUT', body: {result, note}})
      await refreshData()
      return true
    } catch (error) {
      setToast(getErrorMessage(error))
      return false
    }
  }

  async function evaluateKpi(id: number, score: number, published: boolean) {
    try {
      await apiRequest(`/api/kpis/${id}/evaluation`, {method: 'PUT', body: {score, published}})
      await refreshData()
      return true
    } catch (error) {
      setToast(getErrorMessage(error))
      return false
    }
  }

  async function changePassword(currentPassword: string, newPassword: string) {
    try {
      await apiRequest('/api/auth/change-password', {
        method: 'POST',
        body: {currentPassword, newPassword}
      })
      setUser(currentUser => currentUser
        ? {...currentUser, mustChangePassword: false}
        : currentUser)
      return true
    } catch (error) {
      setToast(getErrorMessage(error))
      return false
    }
  }

  const value = useMemo<AppContextValue>(() => ({
    data, user, authLoading, login, logout, toast, setToast, search, setSearch, periodFilter, setPeriodFilter,
    modal, setModal, form, setForm, openModal, saveModal, remove, toggleAccountStatus,
    updateKpiResult, evaluateKpi, changePassword,
    employee, department, position, criterion, period, isAdmin, isManager, isEmployee
  }), [data, user, authLoading, toast, search, periodFilter, modal, form, isAdmin, isManager, isEmployee])

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>
}

export function useApp() {
  const ctx = useContext(AppContext)
  if (!ctx) throw new Error('useApp must be used within AppProvider')
  return ctx
}

export type {Kpi}
