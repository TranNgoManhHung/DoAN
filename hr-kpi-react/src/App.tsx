import type {ReactNode} from 'react'
import {BrowserRouter, Navigate, Outlet, Route, Routes, useLocation} from 'react-router-dom'
import {AppProvider, useApp} from './context/AppContext'
import {allowedFor} from './constants'
import AppLayout from './layout/AppLayout'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import Employees from './pages/Employees'
import Departments from './pages/Departments'
import Positions from './pages/Positions'
import Accounts from './pages/Accounts'
import Permissions from './pages/Permissions'
import Periods from './pages/Periods'
import Criteria from './pages/Criteria'
import Setup from './pages/Setup'
import MyKpi from './pages/MyKpi'
import Evaluations from './pages/Evaluations'
import Reports from './pages/Reports'
import ChangePassword from './pages/ChangePassword'
import ForgotPassword from './pages/ForgotPassword'
import type {PageKey} from './types'

function RequireAuth() {
  const {user, authLoading} = useApp()
  const location = useLocation()
  if (authLoading) return <div className="loading-screen">Đang kết nối máy chủ…</div>
  if (!user) return <Navigate to="/login" replace/>
  if (user.mustChangePassword && location.pathname !== '/changepassword') {
    return <Navigate to="/changepassword" replace/>
  }
  return <Outlet/>
}

function RequirePage({page, children}: {page: PageKey; children: ReactNode}) {
  const {user} = useApp()
  if (!allowedFor(user, page)) return <Navigate to="/dashboard" replace/>
  return children
}

function AppRoutes() {
  return <Routes>
    <Route path="/login" element={<Login/>}/>
    <Route path="/forgot-password" element={<ForgotPassword/>}/>
    <Route element={<RequireAuth/>}>
      <Route element={<AppLayout/>}>
        <Route path="/" element={<Navigate to="/dashboard" replace/>}/>
        <Route path="/dashboard" element={<RequirePage page="dashboard"><Dashboard/></RequirePage>}/>
        <Route path="/employees" element={<RequirePage page="employees"><Employees/></RequirePage>}/>
        <Route path="/departments" element={<RequirePage page="departments"><Departments/></RequirePage>}/>
        <Route path="/positions" element={<RequirePage page="positions"><Positions/></RequirePage>}/>
        <Route path="/accounts" element={<RequirePage page="accounts"><Accounts/></RequirePage>}/>
        <Route path="/permissions" element={<RequirePage page="permissions"><Permissions/></RequirePage>}/>
        <Route path="/periods" element={<RequirePage page="periods"><Periods/></RequirePage>}/>
        <Route path="/criteria" element={<RequirePage page="criteria"><Criteria/></RequirePage>}/>
        <Route path="/setup" element={<RequirePage page="setup"><Setup/></RequirePage>}/>
        <Route path="/mykpi" element={<RequirePage page="mykpi"><MyKpi/></RequirePage>}/>
        <Route path="/evaluations" element={<RequirePage page="evaluations"><Evaluations/></RequirePage>}/>
        <Route path="/reports" element={<RequirePage page="reports"><Reports/></RequirePage>}/>
        <Route path="/changepassword" element={<ChangePassword/>}/>
      </Route>
    </Route>
    <Route path="*" element={<Navigate to="/dashboard" replace/>}/>
  </Routes>
}

export default function App() {
  return <BrowserRouter>
    <AppProvider>
      <AppRoutes/>
    </AppProvider>
  </BrowserRouter>
}
