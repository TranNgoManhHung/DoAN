import {Plus, RefreshCw} from 'lucide-react'
import type {ReactNode} from 'react'
import {useApp} from '../context/AppContext'
import {ActionButtons, SearchBox, SectionTitle, Table} from './ui'
import type {DataKey, Entity, ModalType} from '../types'

interface SimpleCrudProps<T extends Entity> {
  kind: ModalType
  keyName: DataKey
  items: T[]
  headers: string[]
  renderRow: (item: T) => ReactNode
  desc: string
  addLabel: string
  searchPlaceholder?: string
  title: string
}

export default function SimpleCrud<T extends Entity>({kind, keyName, items, headers, renderRow, desc, addLabel, searchPlaceholder, title}: SimpleCrudProps<T>) {
  const {openModal, search, setSearch} = useApp()
  return <>
    <SectionTitle title={title} desc={desc} action={<button className="btn primary" onClick={() => openModal(kind)}><Plus size={17}/> {addLabel}</button>}/>
    <div className="panel">
      <div className="toolbar">
        <SearchBox placeholder={searchPlaceholder || 'Tìm kiếm...'} />
        <button className="btn secondary" onClick={() => setSearch('')}><RefreshCw size={15}/> Làm mới</button>
      </div>
      <Table headers={headers}>{items.filter(x => JSON.stringify(x).toLowerCase().includes(search.toLowerCase())).map(x => <tr key={x.id}>{renderRow(x)}<td><ActionButtons storeKey={keyName} item={x}/></td></tr>)}</Table>
      <div className="table-foot">Tổng số: {items.length} bản ghi</div>
    </div>
  </>
}
