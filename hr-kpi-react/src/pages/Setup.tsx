import {useNavigate} from 'react-router-dom'
import {ArrowUpRight, CalendarDays, Edit2, Plus} from 'lucide-react'
import {useApp} from '../context/AppContext'
import {SearchBox, SectionTitle, Status, Table} from '../components/ui'

export default function Setup() {
  const {data, search, periodFilter, setPeriodFilter, openModal, setToast, employee, criterion, period} = useApp()
  const navigate = useNavigate()
  const rows = data.kpis.filter(k =>
    (periodFilter === 'all' || k.periodId === Number(periodFilter)) &&
    (employee(k.employeeId)?.name + ' ' + k.target + ' ' + criterion(k.criterionId)).toLowerCase().includes(search.toLowerCase())
  )
  return <>
    <SectionTitle title="Thiết lập và giao KPI" desc="Tạo mục tiêu theo kỳ đánh giá, tiêu chí và giao KPI cho từng nhân viên." action={<button className="btn primary" onClick={() => openModal('kpi')}><Plus size={17}/> Thiết lập KPI</button>}/>
    <div className="panel">
      <div className="toolbar">
        <SearchBox placeholder="Tìm theo nhân viên hoặc mục tiêu..."/>
        <select className="select-control" value={periodFilter} onChange={e => setPeriodFilter(e.target.value)}>
          <option value="all">Tất cả kỳ</option>
          {data.periods.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
        </select>
        <button className="btn secondary" onClick={() => navigate('/periods')}><CalendarDays size={16}/> Quản lý kỳ</button>
      </div>
      <Table headers={['Kỳ đánh giá','Nhân viên','Tiêu chí KPI','Mục tiêu','Trọng số','Trạng thái','Thao tác']}>{rows.map(k => <tr key={k.id}>
        <td>{period(k.periodId)}</td>
        <td><b>{employee(k.employeeId)?.name}</b></td>
        <td>{criterion(k.criterionId)}</td>
        <td className="target-cell">{k.target}</td>
        <td><b>{k.weight}%</b></td>
        <td><Status value={k.status}/></td>
        <td><div className="row-actions">
          <button title="Chỉnh sửa" onClick={() => openModal('kpi', k)}><Edit2 size={15}/></button>
          <button title="Giao KPI" onClick={() => setToast(`KPI đã được giao cho ${employee(k.employeeId)?.name}.`)}><ArrowUpRight size={15}/></button>
        </div></td>
      </tr>)}</Table>
      <div className="table-foot">Tổng số: {data.kpis.length} mục tiêu KPI</div>
    </div>
  </>
}
