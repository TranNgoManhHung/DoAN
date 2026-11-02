import {Download, Plus} from 'lucide-react'
import {useApp} from '../context/AppContext'
import {exportCsv} from '../constants'
import {ActionButtons, SearchBox, SectionTitle, Status, Table} from '../components/ui'

export default function Employees() {
  const {data, user, search, openModal, department, position} = useApp()
  const filteredEmployees = data.employees.filter(x =>
    (x.name + ' ' + x.code + ' ' + x.email).toLowerCase().includes(search.toLowerCase()) &&
    (user?.role !== 'Nhân viên' || x.id === user.employeeId)
  )
  return <>
    <SectionTitle title="Quản lý nhân viên" desc="Quản lý hồ sơ, đơn vị công tác, chức vụ và trạng thái nhân sự." action={<button className="btn primary" onClick={() => openModal('employee')}><Plus size={17}/> Thêm nhân viên</button>}/>
    <div className="panel">
      <div className="toolbar">
        <SearchBox placeholder="Tìm theo mã, họ tên, email..."/>
        <button className="btn secondary" onClick={() => exportCsv([['Mã nhân viên','Họ tên','Email','Điện thoại','Phòng ban','Chức vụ','Trạng thái'], ...filteredEmployees.map(e => [e.code, e.name, e.email, e.phone, department(e.departmentId), position(e.positionId), e.status])], 'nhan-vien.csv')}><Download size={16}/> Xuất CSV</button>
      </div>
      <Table headers={['Mã nhân viên','Thông tin nhân viên','Phòng ban','Chức vụ','Trạng thái','Thao tác']}>{filteredEmployees.map(e => <tr key={e.id}>
        <td><b className="code">{e.code}</b></td>
        <td><div className="person-cell"><div className="mini-avatar">{e.name.split(' ').slice(-1)[0][0]}</div><span>{e.name}<small>{e.email}<br/>{e.phone}</small></span></div></td>
        <td>{department(e.departmentId)}</td>
        <td>{position(e.positionId)}</td>
        <td><Status value={e.status}/></td>
        <td><ActionButtons storeKey="employees" item={e}/></td>
      </tr>)}</Table>
      <div className="table-foot">Hiển thị {filteredEmployees.length} / {data.employees.length} nhân viên</div>
    </div>
  </>
}
