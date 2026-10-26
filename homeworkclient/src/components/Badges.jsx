import { SEVERITY_LABELS } from '../utils/format'

export function QualityBadge({ qualityStatus }) {
  const isPass = qualityStatus === 'Pass'

  return (
    <span className={isPass ? 'badge badge--pass' : 'badge badge--fail'}>
      <svg className="badge__icon" viewBox="0 0 16 16" width="16" height="16" aria-hidden="true" focusable="false">
        {isPass ? <path d="M3 8.5l3.5 3.5L13 5" /> : <path d="M4 4l8 8M12 4l-8 8" />}
      </svg>
      {isPass ? 'Đạt' : 'Không đạt'}
    </span>
  )
}

export function SeverityBadge({ severityLevel }) {
  return <span className="badge badge--neutral">{SEVERITY_LABELS[severityLevel] ?? severityLevel}</span>
}

export function ConfirmationBadge({ isUserConfirmed }) {
  return (
    <span className={isUserConfirmed ? 'badge badge--confirmed' : 'badge badge--pending'}>
      {isUserConfirmed ? 'Đã xác nhận' : 'Chưa xác nhận'}
    </span>
  )
}
