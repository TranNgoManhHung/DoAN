import {useEffect, useRef, useState} from 'react'
import {NavLink, Outlet, useLocation, useNavigate} from 'react-router-dom'
import {Bell, CheckCircle2, ChevronDown, KeyRound, LogOut, Menu, X} from 'lucide-react'
import {useApp} from '../context/AppContext'
import {allowedFor, NAV, TITLES} from '../constants'
import Modal from '../components/Modal'
import type {PageKey} from '../types'
import './AppLayout.css'

export default function AppLayout() {
  const {user, logout, toast, setSearch, data} = useApp()
  const [mobileNav, setMobileNav] = useState(false)
  const [userMenuOpen, setUserMenuOpen] = useState(false)
  const userMenuRef = useRef<HTMLDivElement>(null)
  const location = useLocation()
  const navigate = useNavigate()
  const page = (location.pathname.replace(/^\//, '') || 'dashboard') as PageKey
  const heading = TITLES[page] || 'Tổng quan'
  const visibleNav = NAV.map(g => ({...g, items: g.items.filter(i => allowedFor(user, i[0]))})).filter(g => g.items.length)

  useEffect(() => {
    setSearch('')
    setMobileNav(false)
    setUserMenuOpen(false)
  }, [location.pathname, setSearch])

  useEffect(() => {
    if (!userMenuOpen) return

    function handlePointerDown(event: MouseEvent) {
      if (!userMenuRef.current?.contains(event.target as Node)) setUserMenuOpen(false)
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') setUserMenuOpen(false)
    }

    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [userMenuOpen])

  function handleLogout() {
    setUserMenuOpen(false)
    logout()
    navigate('/login')
  }

  if (!user) return null

  return <div className="app-shell">
    <aside className={`sidebar ${mobileNav ? 'show' : ''}`}>
      <div className="brand">
        <span className="brand-mark">H</span>
        <span>HUMG <b>HR & KPI</b></span>
        <button className="close-nav" onClick={() => setMobileNav(false)}><X size={18}/></button>
      </div>
      <div className="school-label">HỆ THỐNG QUẢN TRỊ NHÂN SỰ</div>
      <div className="nav-scroll">{visibleNav.map(group => <div className="nav-group" key={group.section}>
        <div className="nav-section">{group.section}</div>
        {group.items.map(([key, label, Icon]) => <NavLink key={key} to={`/${key}`} className={({isActive}) => `nav-item ${isActive ? 'active' : ''}`}>
          <Icon size={18}/><span>{label}</span>
          {key === 'evaluations' && data.kpis.some(k => k.score == null) && <i className="nav-badge">{data.kpis.filter(k => k.score == null).length}</i>}
        </NavLink>)}
      </div>)}</div>
      <div className="sidebar-bottom">
        <div className="support-card"><div className="support-symbol">✦</div><b>HUMG HR & KPI</b><p>Quản trị nhân sự hiệu quả hơn mỗi ngày.</p></div>
        <div className="sidebar-version">© 2026 Tran Ngo Manh Hung · Demo frontend</div>
      </div>
    </aside>
    {mobileNav && <div className="nav-overlay" onClick={() => setMobileNav(false)}/>}
    <main className="main-area">
      <header className="topbar">
        <button className="icon-btn mobile-toggle" onClick={() => setMobileNav(!mobileNav)}><Menu/></button>
        <div className="crumb">Hệ thống <span>/</span> <b>{heading}</b></div>
        <div className="top-actions">
          <span className="today">{new Date().toLocaleDateString('vi-VN', {weekday:'short', day:'2-digit', month:'short', year:'numeric'})}</span>
          <button className="icon-btn notification" title="Thông báo"><Bell size={19}/><i/></button>
          <div className="user-menu-container" ref={userMenuRef}>
            <button
              className="user-menu"
              type="button"
              aria-expanded={userMenuOpen}
              aria-haspopup="menu"
              aria-label={`Tùy chọn tài khoản ${user.name}`}
              onClick={() => setUserMenuOpen(open => !open)}
            >
              <div className="avatar">{user.name.split(' ').slice(-1)[0].slice(0, 1)}</div>
              <div><b>{user.name}</b><small>{user.role}</small></div>
              <ChevronDown size={15} className={userMenuOpen ? 'user-menu-chevron open' : 'user-menu-chevron'}/>
            </button>
            {userMenuOpen && <div className="user-menu-dropdown" role="menu">
              <NavLink to="/changepassword" role="menuitem" onClick={() => setUserMenuOpen(false)}>
                <KeyRound size={16}/><span>Đổi mật khẩu</span>
              </NavLink>
              <button type="button" role="menuitem" className="logout" onClick={handleLogout}>
                <LogOut size={16}/><span>Đăng xuất</span>
              </button>
            </div>}
          </div>
        </div>
      </header>
      <div className="page-content">
        <Outlet/>
        <footer>Hệ thống quản lý nhân sự và đánh giá KPI <span>•</span> Trường Đại học Mỏ - Địa chất</footer>
      </div>
    </main>
    {toast && <div className="toast"><CheckCircle2 size={18}/>{toast}</div>}
    <Modal/>
  </div>
}
