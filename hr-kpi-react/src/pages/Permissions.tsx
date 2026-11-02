import {AlertCircle, ChartNoAxesCombined, CheckCircle2, ShieldCheck, Users} from 'lucide-react'
import {SectionTitle} from '../components/ui'

export default function Permissions() {
  return <>
    <SectionTitle title="Vai trò và phân quyền" desc="Phân quyền theo ba nhóm người dùng được xác định trong tài liệu phân tích."/>
    <div className="role-grid">{[
      {name:'Quản trị viên', desc:'Quản lý tài khoản, vai trò, quyền, nhân viên, phòng ban và chức vụ.', icon: ShieldCheck, items:['Quản lý tài khoản','Quản lý vai trò và quyền','Quản lý nhân viên','Quản lý phòng ban','Quản lý chức vụ'], tone:'violet'},
      {name:'Người quản lý', desc:'Theo dõi nhân sự trong phạm vi, thiết lập và đánh giá KPI.', icon: ChartNoAxesCombined, items:['Xem nhân viên','Thiết lập và giao KPI','Theo dõi kết quả','Đánh giá KPI','Xem báo cáo'], tone:'blue'},
      {name:'Nhân viên', desc:'Theo dõi thông tin cá nhân và mục tiêu KPI được giao.', icon: Users, items:['Xem thông tin cá nhân','Xem KPI được giao','Cập nhật kết quả','Xem kết quả đã công bố'], tone:'green'}
    ].map(r => <div className="role-card" key={r.name}>
      <div className={`role-icon ${r.tone}`}><r.icon size={22}/></div>
      <h3>{r.name}</h3>
      <p>{r.desc}</p>
      <div className="permission-list">{r.items.map(i => <div key={i}><CheckCircle2 size={15}/>{i}</div>)}</div>
      <span className="role-count">{r.items.length} nhóm quyền mẫu</span>
    </div>)}</div>
    <div className="panel note-panel"><AlertCircle size={19}/><p><b>Lưu ý triển khai:</b> giao diện chỉ điều hướng theo vai trò để minh họa. Backend .NET 8 cần kiểm tra JWT và quyền tại từng API; không dựa vào việc ẩn menu phía ReactJS để bảo vệ dữ liệu.</p></div>
  </>
}
