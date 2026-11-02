import {CheckCircle2, ClipboardList, Download, Target, Users} from 'lucide-react'
import {useState} from 'react'
import {useApp} from '../context/AppContext'
import {exportCsv} from '../constants'
import {SearchBox, SectionTitle, Stat, Table} from '../components/ui'

export default function Reports() {
  const {data, search, periodFilter, setPeriodFilter, employee, department, criterion} = useApp()
  const [departmentFilter, setDepartmentFilter] = useState('')
  const rows = data.kpis.filter(k =>
    (periodFilter === 'all' || k.periodId === Number(periodFilter)) &&
    (!departmentFilter || department(employee(k.employeeId)?.departmentId) === departmentFilter) &&
    (employee(k.employeeId)?.name + ' ' + criterion(k.criterionId)).toLowerCase().includes(search.toLowerCase())
  )
  const employees = [...new Set(rows.map(k => k.employeeId))].map(id => {
    const arr = rows.filter(k => k.employeeId === id)
    const scored = arr.filter(k => k.score != null)
    return {
      id,
      name: employee(id)?.name || '—',
      dept: department(employee(id)?.departmentId),
      count: arr.length,
      done: scored.length,
      score: scored.length ? scored.reduce((s, k) => s + Number(k.score) * k.weight / 100, 0) : null
    }
  })
  return <>
    <SectionTitle title="Thống kê và báo cáo" desc="Tra cứu và tổng hợp kết quả KPI theo kỳ đánh giá, nhân viên và phòng ban." action={<button className="btn secondary" onClick={() => exportCsv([['Nhân viên','Phòng ban','Số KPI','Đã đánh giá','Điểm tổng hợp'], ...employees.map(e => [e.name, e.dept, e.count, e.done, e.score == null ? '' : e.score.toFixed(1)])], 'bao-cao-kpi.csv')}><Download size={16}/> Xuất báo cáo CSV</button>}/>
    <div className="filter-panel">
      <div><label>Kỳ đánh giá</label><select className="select-control" value={periodFilter} onChange={e => setPeriodFilter(e.target.value)}><option value="all">Tất cả kỳ</option>{data.periods.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}</select></div>
      <div><label>Tra cứu</label><SearchBox placeholder="Tên nhân viên hoặc tiêu chí..."/></div>
      <div><label>Phòng ban</label><select className="select-control" value={departmentFilter} onChange={e => setDepartmentFilter(e.target.value)}><option value="">Tất cả phòng ban</option>{data.departments.map(d => <option key={d.id} value={d.name}>{d.name}</option>)}</select></div>
    </div>
    <div className="stat-grid compact">
      <Stat icon={Users} label="Nhân viên có KPI" value={new Set(rows.map(k => k.employeeId)).size} sub="Theo bộ lọc hiện tại" tone="blue"/>
      <Stat icon={ClipboardList} label="Tổng mục tiêu" value={rows.length} sub="KPI được thiết lập" tone="violet"/>
      <Stat icon={CheckCircle2} label="Đã đánh giá" value={rows.filter(k => k.score != null).length} sub="Có điểm đánh giá" tone="green"/>
      <Stat icon={Target} label="Chờ đánh giá" value={rows.filter(k => k.score == null).length} sub="Chưa có điểm" tone="orange"/>
    </div>
    <div className="panel">
      <div className="panel-head"><div><h3>Tổng hợp kết quả theo nhân viên</h3><p>Điểm tổng hợp được tính từ điểm từng KPI nhân trọng số</p></div></div>
      <Table headers={['Nhân viên','Phòng ban','Số KPI','Đã đánh giá','Tiến độ','Điểm tổng hợp']}>{employees.map(e => <tr key={e.id}>
        <td><b>{e.name}</b></td>
        <td>{e.dept}</td>
        <td>{e.count}</td>
        <td>{e.done}</td>
        <td><div className="mini-progress"><span style={{width: `${e.count ? e.done / e.count * 100 : 0}%`}}/></div><small>{e.count ? Math.round(e.done / e.count * 100) : 0}%</small></td>
        <td>{e.score == null ? <span className="muted-text">Chưa đủ dữ liệu</span> : <b className="score">{e.score.toFixed(1)}/100</b>}</td>
      </tr>)}</Table>
    </div>
  </>
}
