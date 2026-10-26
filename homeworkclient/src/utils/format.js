const numberFormatter = new Intl.NumberFormat('vi-VN')
const percentFormatter = new Intl.NumberFormat('vi-VN', { style: 'percent', maximumFractionDigits: 0 })
const dateTimeFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'short', timeStyle: 'short' })

const BYTES_PER_KB = 1024
const BYTES_PER_MB = 1024 * 1024

export const SEVERITY_LABELS = {
  None: 'Không lỗi',
  Minor: 'Lỗi nhẹ',
  Major: 'Lỗi nặng',
}

export function formatNumber(value) {
  return numberFormatter.format(value)
}

export function formatPercent(ratio) {
  return percentFormatter.format(ratio)
}

export function formatDateTime(isoText) {
  const utcText = isoText.endsWith('Z') ? isoText : `${isoText}Z`
  return dateTimeFormatter.format(new Date(utcText))
}

export function formatFileSize(bytes) {
  return bytes >= BYTES_PER_MB
    ? `${(bytes / BYTES_PER_MB).toFixed(1)} MB`
    : `${Math.max(1, Math.round(bytes / BYTES_PER_KB))} KB`
}
