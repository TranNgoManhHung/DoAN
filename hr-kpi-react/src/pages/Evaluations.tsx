import {Eye} from 'lucide-react'
import {useApp} from '../context/AppContext'
import {SearchBox, SectionTitle, Status, Table} from '../components/ui'

export default function Evaluations() {
  const {data, search, periodFilter, setPeriodFilter, openModal, employee, criterion, period} = useApp()
  const rows = data.kpis.filter(k =>
    (periodFilter === 'all' || k.periodId === Number(periodFilter)) &&
    (employee(k.employeeId)?.name + ' ' + criterion(k.criterionId)).toLowerCase().includes(search.toLowerCase())
  )
  return <>
    <SectionTitle title="Đánh giá KPI" desc="Kiểm tra kết quả thực hiện, xác nhận điểm và công bố đánh giá cho nhân viên."/>
    <div className="panel">
      <div className="toolbar">
        <SearchBox placeholder="Tìm theo nhân viên, tiêu chí..."/>
        <select className="select-control" value={periodFilter} onChange={e => setPeriodFilter(e.target.value)}>
          <option value="all">Tất cả kỳ</option>
          {data.periods.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
        </select>
      </div>
      <Table headers={['Nhân viên','Kỳ đánh giá','Tiêu chí','Mục tiêu','Kết quả','Trọng số','Điểm','Trạng thái','Thao tác']}>{rows.map(k => <tr key={k.id}>
        <td><b>{employee(k.employeeId)?.name}</b></td>
        <td>{period(k.periodId)}</td>
        <td>{criterion(k.criterionId)}</td>
        <td className="target-cell">{k.target}</td>
        <td>{k.result || <span className="muted-text">Chưa cập nhật</span>}</td>
        <td>{k.weight}%</td>
        <td><b>{k.score ?? '—'}</b></td>
        <td><Status value={k.published ? 'Đã công bố' : k.status}/></td>
        <td><div className="row-actions"><button title="Đánh giá" onClick={() => openModal('evaluate', k)}><Eye size={15}/></button></div></td>
      </tr>)}</Table>
    </div>
  </>
}
