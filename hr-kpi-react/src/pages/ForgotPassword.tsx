import {useState, type FormEvent} from 'react'
import {Link} from 'react-router-dom'
import {requestPasswordReset} from '../api'
import './ForgotPassword.css'

export default function ForgotPassword() {
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setMessage('')
    setError('')
    try {
      const response = await requestPasswordReset(username.trim(), email.trim())
      setMessage(response.message)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Không thể gửi yêu cầu lúc này.')
    } finally {
      setSubmitting(false)
    }
  }

  return <main className="forgot-password-page">
    <form className="forgot-password-card" onSubmit={submit}>
      <h1>Quên mật khẩu?</h1>
      <p>Nhập tên đăng nhập và email đã đăng ký. Nếu khớp, hệ thống sẽ gửi mật khẩu tạm thời để bạn đổi mật khẩu.</p>
      <label htmlFor="forgot-username">Tên đăng nhập</label>
      <input
        id="forgot-username"
        autoComplete="username"
        value={username}
        onChange={event => setUsername(event.target.value)}
        required
      />
      <label htmlFor="forgot-email">Email nhân viên</label>
      <input
        id="forgot-email"
        type="email"
        autoComplete="email"
        value={email}
        onChange={event => setEmail(event.target.value)}
        required
      />
      {message && <div className="forgot-message" role="status">{message}</div>}
      {error && <div className="error" role="alert">{error}</div>}
      <button className="btn primary full" disabled={submitting}>
        {submitting ? 'Đang gửi yêu cầu…' : 'Gửi mật khẩu tạm thời'}
      </button>
      <Link to="/login">Quay lại đăng nhập</Link>
    </form>
  </main>
}
