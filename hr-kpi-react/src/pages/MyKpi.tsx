import {CalendarDays, Edit2, Target} from 'lucide-react'
import {useApp} from '../context/AppContext'
import {SectionTitle, Status} from '../components/ui'

export default function MyKpi() {
  const {data, user, openModal, criterion, period} = useApp()
  if (!user) return null
  const mine = data.kpis.filter(k => k.employeeId === user.employeeId)
  return <>
    <SectionTitle title="KPI của tôi" desc="Theo dõi mục tiêu được giao, cập nhật kết quả thực hiện và xem đánh giá đã công bố."/>
    <div className="notice-strip"><Target size={18}/><span>Chỉ hiển thị KPI được giao cho tài khoản <b>{user.username}</b>.</span></div>
    <div className="kpi-cards">
      {mine.map(k => <div className="kpi-card" key={k.id}>
        <div className="kpi-card-top"><span className="kpi-label">{criterion(k.criterionId)}</span><Status value={k.published ? 'Đã công bố' : k.status}/></div>
        <h3>{k.target}</h3>
        <div className="kpi-meta"><span><CalendarDays size={14}/>{period(k.periodId)}</span><span><Target size={14}/>Trọng số {k.weight}%</span></div>
        {k.result !== '' && k.result != null && <div className="result-box"><span>Kết quả thực hiện</span><b>{k.result}</b></div>}
        {k.published && <div className="published-score"><span>Điểm đánh giá</span><strong>{k.score}/100</strong></div>}
        {!k.published && <button className="btn primary full" onClick={() => openModal('result', k)}><Edit2 size={15}/>{k.result !== '' && k.result != null ? 'Cập nhật kết quả' : 'Nhập kết quả thực hiện'}</button>}
      </div>)}
      {!mine.length && <div className="panel empty-state">Chưa có KPI nào được giao cho bạn.</div>}
    </div>
  </>
}
