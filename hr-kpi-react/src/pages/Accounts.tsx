import {useApp} from '../context/AppContext'
import SimpleCrud from '../components/SimpleCrud'
import {Status} from '../components/ui'

export default function Accounts() {
  const {data, employee, toggleAccountStatus} = useApp()
  return <SimpleCrud
    title="Quản lý tài khoản"
    kind="account"
    keyName="accounts"
    items={data.accounts}
    headers={['Tên đăng nhập','Nhân viên liên kết','Vai trò','Trạng thái','Thao tác']}
    desc="Cấp tài khoản, gán vai trò và khóa/mở quyền truy cập."
    addLabel="Tạo tài khoản"
    searchPlaceholder="Tìm tài khoản hoặc nhân viên..."
    renderRow={a => <><td><b>{a.username}</b></td><td>{employee(a.employeeId)?.name || '—'}</td><td><span className="role-pill">{a.role}</span></td><td><button className={`status-button ${a.status === 'Bị khóa' ? 'locked' : ''}`} onClick={() => toggleAccountStatus(a.id, a.status)}><Status value={a.status}/></button></td></>}
  />
}
