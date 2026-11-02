import type {AppData, DataKey, SessionUser} from './types'

const API_URL = (import.meta.env.VITE_API_URL || 'http://localhost:5145').replace(/\/$/, '')
export const TOKEN_STORAGE_KEY = 'hrkpi_token'

export class ApiError extends Error {
  constructor(message: string, readonly status: number) {
    super(message)
    this.name = 'ApiError'
  }
}

export const emptyData: AppData = {
  employees: [],
  departments: [],
  positions: [],
  accounts: [],
  periods: [],
  criteria: [],
  kpis: []
}

export function getToken() {
  const token = localStorage.getItem(TOKEN_STORAGE_KEY)
  if (token) return token

  const legacyToken = sessionStorage.getItem(TOKEN_STORAGE_KEY)
  if (legacyToken) {
    localStorage.setItem(TOKEN_STORAGE_KEY, legacyToken)
    sessionStorage.removeItem(TOKEN_STORAGE_KEY)
  }
  return legacyToken
}

export function setToken(token: string | null) {
  if (token) localStorage.setItem(TOKEN_STORAGE_KEY, token)
  else {
    localStorage.removeItem(TOKEN_STORAGE_KEY)
    sessionStorage.removeItem(TOKEN_STORAGE_KEY)
  }
}

export async function apiRequest<T>(
  path: string,
  options: {method?: string; body?: unknown; authenticated?: boolean} = {}
): Promise<T> {
  const headers = new Headers({'Content-Type': 'application/json'})
  if (options.authenticated !== false) {
    const token = getToken()
    if (token) headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_URL}${path}`, {
    method: options.method || 'GET',
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body)
  })

  if (!response.ok) {
    let message = `Yêu cầu thất bại (${response.status}).`
    try {
      const payload = await response.json() as {message?: string; title?: string; detail?: string}
      message = payload.message || payload.detail || payload.title || message
    } catch {
      if (response.status === 0) message = 'Không thể kết nối API.'
    }
    throw new ApiError(message, response.status)
  }

  if (response.status === 204) return undefined as T
  return await response.json() as T
}

export async function loadBootstrap() {
  return await apiRequest<AppData>('/api/bootstrap')
}

export async function signIn(username: string, password: string) {
  return await apiRequest<{token: string; user: SessionUser}>('/api/auth/login', {
    method: 'POST',
    authenticated: false,
    body: {username, password}
  })
}

export async function requestPasswordReset(username: string, email: string) {
  return await apiRequest<{message: string}>('/api/auth/forgot-password', {
    method: 'POST',
    authenticated: false,
    body: {username, email}
  })
}

export function endpointFor(key: DataKey) {
  const names: Record<DataKey, string> = {
    employees: 'employees',
    departments: 'departments',
    positions: 'positions',
    accounts: 'accounts',
    periods: 'periods',
    criteria: 'criteria',
    kpis: 'kpis'
  }
  return `/api/${names[key]}`
}
