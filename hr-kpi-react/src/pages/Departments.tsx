import {useApp} from '../context/AppContext'
import SimpleCrud from '../components/SimpleCrud'

export default function Departments() {
  const {data} = useApp()
  return <SimpleCrud
    title="Quản lý phòng ban"
    kind="department"
    keyName="departments"
    items={data.departments}
    headers={['Mã phòng ban','Tên phòng ban','Mô tả','Số nhân sự','Thao tác']}
    desc="Thiết lập cơ cấu đơn vị và quản lý thông tin phòng ban."
    addLabel="Thêm phòng ban"
    searchPlaceholder="Tìm mã hoặc tên phòng ban..."
    renderRow={d => <><td><b className="code">{d.code}</b></td><td><b>{d.name}</b></td><td>{d.description || '—'}</td><td><span className="count-pill">{data.employees.filter(e => e.departmentId === d.id).length} nhân sự</span></td></>}
  />
}
