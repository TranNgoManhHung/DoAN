import {useApp} from '../context/AppContext'
import SimpleCrud from '../components/SimpleCrud'

export default function Positions() {
  const {data} = useApp()
  return <SimpleCrud
    title="Quản lý chức vụ"
    kind="position"
    keyName="positions"
    items={data.positions}
    headers={['Mã chức vụ','Tên chức vụ','Số nhân sự','Thao tác']}
    desc="Quản lý chức vụ dùng trong hồ sơ nhân sự và xác định mục tiêu KPI."
    addLabel="Thêm chức vụ"
    searchPlaceholder="Tìm mã hoặc tên chức vụ..."
    renderRow={p => <><td><b className="code">{p.code}</b></td><td><b>{p.name}</b></td><td><span className="count-pill">{data.employees.filter(e => e.positionId === p.id).length} nhân sự</span></td></>}
  />
}
