import {useNavigate} from 'react-router-dom'
import {AlertCircle, Building2, CalendarDays, ChartNoAxesCombined, CheckCircle2, ClipboardList, Target, Users, ArrowUpRight} from 'lucide-react'
import {useApp} from '../context/AppContext'
import {allowedFor} from '../constants'
import {SectionTitle, Stat, Status, Table} from '../components/ui'
import type {Period} from '../types'

function periodsCurrent(list: Period[]) {
  return list.find(x => x.status === 'Đang diễn ra')?.name || 'Chưa thiết lập kỳ đang diễn ra'
}

export default function Dashboard() {
  const {data, department, position, user} = useApp()
  const navigate = useNavigate()
  const active = data.employees.filter(e => e.status === 'Đang làm việc').length
  const done = data.kpis.filter(k => k.status === 'Đã đánh giá').length
  const pending = data.kpis.filter(k => k.status !== 'Đã đánh giá').length

  return <>
    <SectionTitle title="Tổng quan hệ thống" desc="Theo dõi nhanh tình hình nhân sự và tiến độ đánh giá KPI." action={<button className="btn secondary" onClick={() => navigate('/reports')}><ChartNoAxesCombined size={16}/> Xem báo cáo</button>}/>
    <div className="stat-grid">
      <Stat icon={Users} label="Tổng nhân sự" value={active} sub="Đang làm việc" tone="blue"/>
      <Stat icon={Building2} label="Phòng ban" value={data.departments.length} sub="Đơn vị tổ chức" tone="violet"/>
      <Stat icon={Target} label="KPI cần đánh giá" value={pending} sub="Đang chờ xử lý" tone="orange"/>
      <Stat icon={CheckCircle2} label="KPI đã đánh giá" value={done} sub="Đã có kết quả" tone="green"/>
    </div>
    <div className="dashboard-grid">
      <div className="panel">
        <div className="panel-head"><div><h3>Tiến độ đánh giá KPI</h3><p>Tổng hợp các KPI đang có trong hệ thống</p></div><span className="soft-badge">Tất cả kỳ</span></div>
        <div className="progress-summary">
          <div><strong>{data.kpis.length ? Math.round(done / data.kpis.length * 100) : 0}%</strong><span>Hoàn tất đánh giá</span></div>
          <div className="progress-track"><div style={{width: `${data.kpis.length ? done / data.kpis.length * 100 : 0}%`}}/></div>
          <div className="progress-legend">
            <span><i className="dot green-dot"/> Đã đánh giá <b>{done}</b></span>
            <span><i className="dot orange-dot"/> Đang xử lý <b>{pending}</b></span>
          </div>
        </div>
        <div className="panel-head bottom-head"><div><h3>Nhân sự theo phòng ban</h3><p>Phân bổ nhân sự đang hoạt động</p></div></div>
        {data.departments.map(d => {
          const count = data.employees.filter(e => e.departmentId === d.id && e.status === 'Đang làm việc').length
          return <div className="dept-bar" key={d.id}><div><span>{d.name}</span><b>{count}</b></div><div className="progress-track"><div style={{width: `${active ? count / active * 100 : 0}%`}}/></div></div>
        })}
      </div>
      <div className="panel">
        <div className="panel-head"><div><h3>Việc cần xử lý</h3><p>Các hoạt động gần đây</p></div></div>
        <div className="activity-list">{[
          {icon: ClipboardList, title: 'KPI chờ đánh giá', detail: `${pending} mục tiêu đang chờ quản lý`, tone: 'orange', go: '/evaluations', page: 'evaluations'},
          {icon: Users, title: 'Quản lý nhân sự', detail: `${active} nhân sự đang hoạt động`, tone: 'blue', go: '/employees', page: 'employees'},
          {icon: CalendarDays, title: 'Kỳ đánh giá hiện tại', detail: periodsCurrent(data.periods), tone: 'violet', go: '/periods', page: 'periods'},
          {icon: ChartNoAxesCombined, title: 'Báo cáo KPI', detail: 'Tra cứu theo kỳ và phòng ban', tone: 'green', go: '/reports', page: 'reports'}
        ].filter(a => allowedFor(user, a.page)).map((a, i) => <button className="activity" key={i} onClick={() => navigate(a.go)}>
          <span className={`activity-icon ${a.tone}`}><a.icon size={18}/></span>
          <span><b>{a.title}</b><small>{a.detail}</small></span>
          <ArrowUpRight size={16}/>
        </button>)}</div>
        <div className="tip-box"><div className="tip-icon"><AlertCircle size={18}/></div><div><b>Gợi ý</b><p>Hoàn tất cập nhật kết quả KPI trước khi thực hiện đánh giá cuối kỳ.</p></div></div>
      </div>
    </div>
    <div className="panel recent-panel">
      <div className="panel-head">
        <div><h3>Nhân sự mới nhất</h3><p>Danh sách nhân sự trong hệ thống</p></div>
        {allowedFor(user, 'employees') && <button className="text-btn" onClick={() => navigate('/employees')}>Xem tất cả <ArrowUpRight size={15}/></button>}
      </div>
      <Table headers={['Mã nhân viên','Họ và tên','Phòng ban','Chức vụ','Trạng thái']}>{data.employees.slice(0, 4).map(e => <tr key={e.id}>
        <td><b className="code">{e.code}</b></td>
        <td><div className="person-cell"><div className="mini-avatar">{e.name.split(' ').slice(-1)[0][0]}</div><span>{e.name}<small>{e.email}</small></span></div></td>
        <td>{department(e.departmentId)}</td>
        <td>{position(e.positionId)}</td>
        <td><Status value={e.status}/></td>
      </tr>)}</Table>
    </div>
  </>
}
