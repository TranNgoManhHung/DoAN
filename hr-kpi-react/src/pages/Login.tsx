import {useState, type FormEvent} from 'react'
import {Link, Navigate, useNavigate} from 'react-router-dom'
import {ArrowUpRight, LockKeyhole} from 'lucide-react'
import {useApp} from '../context/AppContext'
import PasswordInput from '../components/PasswordInput'

export default function Login() {
  const {user, login} = useApp()
  const navigate = useNavigate()
  const [form, setForm] = useState({username:'', password:''})
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  if (user) return <Navigate to="/dashboard" replace/>

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setError('')
    try {
      const message = await login(form.username, form.password)
      if (message) { setError(message); return }
      navigate('/dashboard')
    } finally {
      setSubmitting(false)
    }
  }

  return <div className="login-page">
    <div className="login-art">
      <div className="brand large"><span className="brand-mark">H</span><span>HUMG <b>HR & KPI</b></span></div>
      <div className="art-copy">
        <span className="eyebrow">NỀN TẢNG QUẢN TRỊ NHÂN SỰ</span>
        <h1>Quản lý hiệu quả.<br/><em>Phát triển bền vững.</em></h1>
        <p>Hệ thống quản lý nhân sự và đánh giá KPI dành cho Trường Đại học Mỏ - Địa chất.</p>
        <div className="art-stats">
          <div><strong>360°</strong><span>Góc nhìn nhân sự</span></div>
          <div><strong>KPI</strong><span>Đo lường mục tiêu</span></div>
          <div><strong>HR</strong><span>Dữ liệu tập trung</span></div>
        </div>
      </div>
      <div className="art-footer">HỆ THỐNG QUẢN LÝ NHÂN SỰ & ĐÁNH GIÁ KPI</div>
    </div>
    <div className="login-side">
      <form className="login-card" onSubmit={onSubmit}>
        <div className="login-icon"><LockKeyhole size={24}/></div>
        <h2>Chào mừng trở lại</h2>
        <p>Đăng nhập để tiếp tục vào hệ thống</p>
        <label>Tên đăng nhập</label>
        <input value={form.username} onChange={e => setForm({...form, username: e.target.value})} autoComplete="username" required/>
        <label>Mật khẩu</label>
        <PasswordInput value={form.password} onChange={e => setForm({...form, password: e.target.value})} autoComplete="current-password" required/>
        <Link className="login-forgot-link " to="/forgot-password">Quên mật khẩu?</Link>
        {error && <div className="error">{error}</div>}
        <button className="btn primary full" disabled={submitting}>{submitting ? 'Đang đăng nhập…' : 'Đăng nhập'} <ArrowUpRight size={17}/></button>
        <small>Tài khoản và dữ liệu được xác thực, tải từ máy chủ.</small>
      </form>
    </div>
  </div>
}
