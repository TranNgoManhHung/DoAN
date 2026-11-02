import {useApp} from '../context/AppContext'
import {Field} from '../components/ui'
import PasswordInput from '../components/PasswordInput'
import {useNavigate} from 'react-router-dom'
import type {FormEvent} from 'react'

export default function ChangePassword() {
  const {user, setToast, changePassword} = useApp()
  const navigate = useNavigate()
  async function submit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    const formElement = e.currentTarget
    const form = new FormData(formElement)
    const currentPassword = String(form.get('currentPassword') || '')
    const newPassword = String(form.get('newPassword') || '')
    if (newPassword !== String(form.get('confirmPassword') || '')) {
      setToast('Mật khẩu xác nhận không khớp.')
      return
    }
    if (await changePassword(currentPassword, newPassword)) {
      formElement.reset()
      if (user?.mustChangePassword) {
        setToast('Đã đổi mật khẩu. Bạn có thể tiếp tục sử dụng hệ thống.')
        navigate('/dashboard')
      } else {
        setToast('Đã đổi mật khẩu.')
      }
    }
  }
  return <div className="panel form-panel">
    <h3>Đổi mật khẩu</h3>
    <p>{user?.mustChangePassword
      ? 'Bạn đang dùng mật khẩu tạm thời. Hãy đổi mật khẩu để tiếp tục sử dụng hệ thống.'
      : 'Mật khẩu được cập nhật an toàn trên máy chủ.'}</p>
    <form onSubmit={submit}>
      <Field label="Mật khẩu hiện tại"><PasswordInput name="currentPassword" autoComplete="current-password" required/></Field>
      <Field label="Mật khẩu mới"><PasswordInput name="newPassword" autoComplete="new-password" minLength={6} required/></Field>
      <Field label="Xác nhận mật khẩu mới"><PasswordInput name="confirmPassword" autoComplete="new-password" minLength={6} required/></Field>
      <button className="btn primary">Cập nhật mật khẩu</button>
    </form>
  </div>
}
