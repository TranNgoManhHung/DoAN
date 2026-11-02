import {useState, type InputHTMLAttributes} from 'react'
import {Eye, EyeOff} from 'lucide-react'
import './PasswordInput.css'

type PasswordInputProps = Omit<InputHTMLAttributes<HTMLInputElement>, 'type'>

export default function PasswordInput(props: PasswordInputProps) {
  const [visible, setVisible] = useState(false)

  return <span className="password-input-wrap">
    <input {...props} type={visible ? 'text' : 'password'}/>
    <button
      type="button"
      className="password-toggle"
      aria-label={visible ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
      aria-pressed={visible}
      onClick={() => setVisible(value => !value)}
    >
      {visible ? <EyeOff size={16}/> : <Eye size={16}/>}
    </button>
  </span>
}
