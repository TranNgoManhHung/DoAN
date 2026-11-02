import {useApp} from '../context/AppContext'
import SimpleCrud from '../components/SimpleCrud'
import {Status} from '../components/ui'

export default function Periods() {
  const {data} = useApp()
  return <SimpleCrud
    title="Kỳ đánh giá KPI"
    kind="period"
    keyName="periods"
    items={data.periods}
    headers={['Tên kỳ đánh giá','Thời gian bắt đầu','Thời gian kết thúc','Số KPI','Trạng thái','Thao tác']}
    desc="Quản lý kỳ đánh giá theo tháng, quý hoặc năm."
    addLabel="Tạo kỳ đánh giá"
    searchPlaceholder="Tìm kỳ đánh giá..."
    renderRow={p => <><td><b>{p.name}</b></td><td>{p.start}</td><td>{p.end}</td><td>{data.kpis.filter(k => k.periodId === p.id).length}</td><td><Status value={p.status}/></td></>}
  />
}
