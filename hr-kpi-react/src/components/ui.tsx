import type {LucideIcon} from 'lucide-react'
import {Edit2, Search, Trash2} from 'lucide-react'
import type {ReactNode} from 'react'
import {useApp} from '../context/AppContext'
import {MODAL_KIND} from '../constants'
import type {DataKey, Entity} from '../types'

export function Field({label, children}: {label: string; children: ReactNode}) {
  return <label className="field"><span>{label}</span>{children}</label>
}

export function Status({value}: {value?: string | null}) {
  let c = 'neutral'
  if (value && ['Đang làm việc','Hoạt động','Đã đánh giá','Đã công bố'].includes(value)) c = 'success'
  if (value && ['Chờ đánh giá','Chờ cập nhật','Đang diễn ra'].includes(value)) c = 'warning'
  if (value && ['Ngừng hoạt động','Bị khóa','Đã kết thúc'].includes(value)) c = 'muted'
  return <span className={`status ${c}`}><i/>{value || 'Chưa có'}</span>
}

export function Table({headers, children}: {headers: string[]; children?: ReactNode}) {
  return <div className="table-wrap"><table><thead><tr>{headers.map(h => <th key={h}>{h}</th>)}</tr></thead><tbody>{children || <tr><td colSpan={headers.length} className="empty">Không có dữ liệu phù hợp.</td></tr>}</tbody></table></div>
}

export function SectionTitle({title, desc, action}: {title: string; desc: string; action?: ReactNode}) {
  return <div className="section-title"><div><h2>{title}</h2><p>{desc}</p></div>{action}</div>
}

export function SearchBox({placeholder = 'Tìm kiếm...', value, onChange}: {placeholder?: string; value?: string; onChange?: (value: string) => void}) {
  const app = useApp()
  const v = value ?? app.search
  const change = onChange ?? app.setSearch
  return <div className="search-box"><Search size={17}/><input placeholder={placeholder} value={v} onChange={e => change(e.target.value)}/></div>
}

export function Stat({icon: Icon, label, value, sub, tone}: {icon: LucideIcon; label: string; value: number; sub: string; tone: string}) {
  return <div className="stat-card"><div className={`stat-icon ${tone}`}><Icon size={20}/></div><div className="stat-label">{label}</div><div className="stat-value">{value}</div><div className="stat-sub">{sub}</div><span className="stat-watermark"><Icon size={52}/></span></div>
}

export function ActionButtons({storeKey, item}: {storeKey: DataKey; item: Entity}) {
  const {openModal, remove} = useApp()
  return <div className="row-actions">
    <button title="Chỉnh sửa" onClick={() => openModal(MODAL_KIND[storeKey], item)}><Edit2 size={15}/></button>
    <button className="danger-text" title="Xóa" onClick={() => remove(storeKey, item)}><Trash2 size={15}/></button>
  </div>
}
