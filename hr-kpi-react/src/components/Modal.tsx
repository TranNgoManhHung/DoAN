import {CheckCircle2, X} from 'lucide-react'
import type {FormEvent} from 'react'
import {useApp} from '../context/AppContext'
import {Field} from './ui'
import type {FormValues, Kpi, ModalType} from '../types'

function fieldValue(v: FormValues[string]): string | number {
  if (typeof v === 'boolean' || v == null) return ''
  return v
}

function modalTitle(t: ModalType) {
  return ({
    employee:'Thông tin nhân viên',
    department:'Thông tin phòng ban',
    position:'Thông tin chức vụ',
    account:'Thông tin tài khoản',
    period:'Kỳ đánh giá KPI',
    criterion:'Tiêu chí KPI',
    kpi:'Thiết lập KPI',
    result:'Cập nhật kết quả KPI',
    evaluate:'Đánh giá KPI'
  })[t] || 'Thông tin'
}

export default function Modal() {
  const {modal, setModal, form, setForm, data, employee, criterion, period, updateKpiResult, evaluateKpi, setToast, saveModal} = useApp()
  if (!modal) return null
  const set = (k: string, v: string | number | boolean) => setForm(f => ({...f, [k]: v}))
  const t = modal.type
  const item = (modal.item ?? {}) as Partial<Kpi>
  const isEdit = !!modal.item
  const linkedEmployee = t === 'account'
    ? data.employees.find(employee => employee.id === Number(form.employeeId))
    : undefined

  async function submitResult(e: FormEvent) {
    e.preventDefault()
    const result = String(form.result ?? '').trim()
    if (result === '' || !Number.isFinite(Number(result)) || Number(result) < 0) { setToast('Kết quả phải là số không âm.'); return }
    const p = data.periods.find(x => x.id === item.periodId)
    if (p && p.end < new Date().toISOString().slice(0, 10)) { setToast('Kỳ đánh giá đã kết thúc, không thể cập nhật.'); return }
    if (await updateKpiResult(item.id!, Number(result), String(form.note || ''))) {
      setModal(null)
      setToast('Đã cập nhật kết quả KPI.')
    }
  }

  async function submitEvaluation(e: FormEvent) {
    e.preventDefault()
    const score = Number(form.score)
    if (!Number.isFinite(score) || score < 0 || score > 100) { setToast('Điểm đánh giá phải nằm trong khoảng 0 đến 100.'); return }
    if (await evaluateKpi(item.id!, score, !!form.published)) {
      setModal(null)
      setToast(form.published ? 'Đã lưu và công bố kết quả đánh giá.' : 'Đã lưu kết quả đánh giá.')
    }
  }

  const onSubmit = t === 'result' ? submitResult : t === 'evaluate' ? submitEvaluation : saveModal

  return <div className="modal-backdrop" onMouseDown={e => { if (e.target === e.currentTarget) setModal(null) }}>
    <form className="modal" onSubmit={onSubmit}>
      <div className="modal-head">
        <div><h2>{modalTitle(t)}</h2><p>{isEdit ? 'Cập nhật thông tin đã chọn.' : 'Nhập thông tin để tạo mới bản ghi.'}</p></div>
        <button type="button" className="icon-btn" onClick={() => setModal(null)}><X size={19}/></button>
      </div>
      <div className="modal-body">
        {t === 'employee' && <div className="form-grid">
          <Field label="Mã nhân viên *"><input value={String(form.code || '')} onChange={e => set('code', e.target.value)} required/></Field>
          <Field label="Họ và tên *"><input value={String(form.name || '')} onChange={e => set('name', e.target.value)} required/></Field>
          <Field label="Email *"><input
            type="email"
            autoComplete="email"
            maxLength={150}
            value={String(form.email || '')}
            onChange={e => {
              e.currentTarget.setCustomValidity('')
              set('email', e.target.value)
            }}
            onInvalid={e => e.currentTarget.setCustomValidity('Vui lòng nhập địa chỉ email hợp lệ.')}
            required
          /></Field>
          <Field label="Số điện thoại"><input value={String(form.phone || '')} onChange={e => set('phone', e.target.value)}/></Field>
          <Field label="Phòng ban"><select value={fieldValue(form.departmentId)} onChange={e => set('departmentId', e.target.value)} required><option value="">Chọn phòng ban</option>{data.departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}</select></Field>
          <Field label="Chức vụ"><select value={fieldValue(form.positionId)} onChange={e => set('positionId', e.target.value)} required><option value="">Chọn chức vụ</option>{data.positions.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}</select></Field>
          <Field label="Trạng thái"><select value={String(form.status || 'Đang làm việc')} onChange={e => set('status', e.target.value)}><option>Đang làm việc</option><option>Ngừng hoạt động</option></select></Field>
        </div>}
        {t === 'department' && <div className="form-grid">
          <Field label="Mã phòng ban *"><input value={String(form.code || '')} onChange={e => set('code', e.target.value)} required/></Field>
          <Field label="Tên phòng ban *"><input value={String(form.name || '')} onChange={e => set('name', e.target.value)} required/></Field>
          <Field label="Mô tả"><textarea value={String(form.description || '')} onChange={e => set('description', e.target.value)} rows={3}/></Field>
        </div>}
        {t === 'position' && <div className="form-grid">
          <Field label="Mã chức vụ *"><input value={String(form.code || '')} onChange={e => set('code', e.target.value)} required/></Field>
          <Field label="Tên chức vụ *"><input value={String(form.name || '')} onChange={e => set('name', e.target.value)} required/></Field>
        </div>}
        {t === 'account' && <div className="form-grid">
          <Field label="Nhân viên liên kết *"><select value={fieldValue(form.employeeId)} onChange={e => set('employeeId', e.target.value)} required><option value="">Chọn nhân viên</option>{data.employees.map(e => <option key={e.id} value={e.id}>{e.code} — {e.name}</option>)}</select></Field>
          <Field label="Tên đăng nhập *"><input value={String(form.username || '')} onChange={e => set('username', e.target.value)} required/></Field>
          <Field label="Vai trò"><select value={String(form.role || 'Nhân viên')} onChange={e => set('role', e.target.value)}><option>Quản trị viên</option><option>Người quản lý</option><option>Nhân viên</option></select></Field>
          <Field label="Trạng thái"><select value={String(form.status || 'Hoạt động')} onChange={e => set('status', e.target.value)}><option>Hoạt động</option><option>Bị khóa</option></select></Field>
          {!isEdit && <p className="form-hint full-span">
            {linkedEmployee
              ? `Mật khẩu tạm thời sẽ được gửi tới ${linkedEmployee.email} và buộc đổi sau lần đăng nhập đầu tiên.`
              : 'Mật khẩu tạm thời sẽ được gửi tới email hồ sơ nhân viên đã chọn và buộc đổi sau lần đăng nhập đầu tiên.'}
          </p>}
        </div>}
        {t === 'period' && <div className="form-grid">
          <Field label="Tên kỳ đánh giá *"><input placeholder="VD: Quý I/2027" value={String(form.name || '')} onChange={e => set('name', e.target.value)} required/></Field>
          <Field label="Trạng thái"><select value={String(form.status || 'Đang diễn ra')} onChange={e => set('status', e.target.value)}><option>Đang diễn ra</option><option>Đã kết thúc</option><option>Chưa bắt đầu</option></select></Field>
          <Field label="Ngày bắt đầu *"><input type="date" value={String(form.start || '')} onChange={e => set('start', e.target.value)} required/></Field>
          <Field label="Ngày kết thúc *"><input type="date" value={String(form.end || '')} onChange={e => set('end', e.target.value)} required/></Field>
        </div>}
        {t === 'criterion' && <div className="form-grid">
          <Field label="Tên tiêu chí *"><input value={String(form.name || '')} onChange={e => set('name', e.target.value)} required/></Field>
          <Field label="Trọng số mặc định (%)"><input type="number" min={0} max={100} value={fieldValue(form.defaultWeight) || 0} onChange={e => set('defaultWeight', e.target.value)}/></Field>
          <Field label="Mô tả"><textarea rows={3} value={String(form.description || '')} onChange={e => set('description', e.target.value)}/></Field>
        </div>}
        {t === 'kpi' && <div className="form-grid">
          <Field label="Kỳ đánh giá *"><select value={fieldValue(form.periodId)} onChange={e => set('periodId', e.target.value)} required><option value="">Chọn kỳ đánh giá</option>{data.periods.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}</select></Field>
          <Field label="Nhân viên nhận KPI *"><select value={fieldValue(form.employeeId)} onChange={e => set('employeeId', e.target.value)} required><option value="">Chọn nhân viên</option>{data.employees.filter(e => e.status === 'Đang làm việc').map(e => <option key={e.id} value={e.id}>{e.code} — {e.name}</option>)}</select></Field>
          <Field label="Tiêu chí KPI *"><select value={fieldValue(form.criterionId)} onChange={e => { const c = data.criteria.find(x => x.id === Number(e.target.value)); setForm(f => ({...f, criterionId: e.target.value, weight: c?.defaultWeight ?? f.weight ?? 0})) }} required><option value="">Chọn tiêu chí</option>{data.criteria.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}</select></Field>
          <Field label="Trọng số (%) *"><input type="number" min={1} max={100} value={fieldValue(form.weight)} onChange={e => set('weight', e.target.value)} required/></Field>
          <Field label="Mục tiêu cần đạt *"><textarea rows={3} value={String(form.target || '')} onChange={e => set('target', e.target.value)} required placeholder="Mô tả kết quả kỳ vọng"/></Field>
        </div>}
        {t === 'result' && <>
          <div className="detail-banner"><span>{criterion(item.criterionId)}</span><b>{item.target}</b><small>{period(item.periodId)} · Trọng số {item.weight}%</small></div>
          <div className="form-grid">
            <Field label="Kết quả thực hiện (số) *"><input type="number" min={0} step="any" value={fieldValue(form.result ?? item.result)} onChange={e => set('result', e.target.value)} required/></Field>
            <Field label="Ghi chú / minh chứng"><textarea rows={3} value={String(form.note ?? item.note ?? '')} onChange={e => set('note', e.target.value)} placeholder="Nhập ghi chú nếu có"/></Field>
          </div>
          <p className="form-hint">Nhập kết quả dạng số để quản lý có thể đánh giá theo thang điểm từ 0 đến 100.</p>
        </>}
        {t === 'evaluate' && <>
          <div className="detail-banner"><span>{employee(item.employeeId)?.name} · {period(item.periodId)}</span><b>{criterion(item.criterionId)}: {item.target}</b><small>Kết quả nhân viên: {item.result || 'Chưa cập nhật'} · Trọng số {item.weight}%</small></div>
          <div className="form-grid">
            <Field label="Điểm đánh giá (0–100) *"><input type="number" min={0} max={100} value={fieldValue(form.score ?? item.score)} onChange={e => set('score', e.target.value)} required/></Field>
            <Field label="Trạng thái công bố"><select value={(form.published ?? item.published) ? 'true' : 'false'} onChange={e => set('published', e.target.value === 'true')}><option value="false">Chưa công bố</option><option value="true">Công bố cho nhân viên</option></select></Field>
          </div>
          <p className="form-hint">Điểm KPI sau khi xác nhận được tổng hợp theo trọng số trong báo cáo. Chỉ công bố khi đã kiểm tra kết quả.</p>
        </>}
      </div>
      <div className="modal-foot">
        <button type="button" className="btn secondary" onClick={() => setModal(null)}>Hủy</button>
        <button className="btn primary"><CheckCircle2 size={16}/>{t === 'result' ? 'Cập nhật kết quả' : t === 'evaluate' ? 'Lưu đánh giá' : 'Lưu thông tin'}</button>
      </div>
    </form>
  </div>
}
