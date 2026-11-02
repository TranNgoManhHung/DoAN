import {useApp} from '../context/AppContext'
import SimpleCrud from '../components/SimpleCrud'
import {Status} from '../components/ui'

export default function Criteria() {
  const {data} = useApp()
  return <SimpleCrud
    title="Tiêu chí KPI"
    kind="criterion"
    keyName="criteria"
    items={data.criteria}
    headers={['Tên tiêu chí','Mô tả','Trọng số mặc định','Đang được dùng','Thao tác']}
    desc="Định nghĩa các tiêu chí dùng để thiết lập và chấm điểm KPI."
    addLabel="Thêm tiêu chí"
    searchPlaceholder="Tìm tiêu chí KPI..."
    renderRow={c => <><td><b>{c.name}</b></td><td>{c.description || '—'}</td><td><b>{c.defaultWeight}%</b></td><td>{data.kpis.some(k => k.criterionId === c.id) ? <Status value="Đang sử dụng"/> : <span className="muted-text">Chưa sử dụng</span>}</td></>}
  />
}
