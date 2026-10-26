const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5195'
const NETWORK_ERROR_MESSAGE = 'Không kết nối được máy chủ. Hãy kiểm tra API đã chạy chưa rồi thử lại.'
const DEFAULT_ERROR_MESSAGE = 'Có lỗi xảy ra, vui lòng thử lại.'

async function request(path, options) {
  let response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, options)
  } catch {
    throw new Error(NETWORK_ERROR_MESSAGE)
  }

  const data = await response.json().catch(() => ({}))
  if (!response.ok) {
    throw new Error(data.message ?? data.title ?? DEFAULT_ERROR_MESSAGE)
  }
  return data
}

export function toImageUrl(imageUrl) {
  return `${API_BASE_URL}${imageUrl}`
}

export function getProducts() {
  return request('/api/products')
}

export function getInspectors() {
  return request('/api/inspectors')
}

export function getDashboard() {
  return request('/api/dashboard')
}

export function getReports(filters) {
  const params = new URLSearchParams()
  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== '') {
      params.set(key, value)
    }
  })
  return request(`/api/reports?${params.toString()}`)
}

export function analyzeImage({ productName, inspectorId, imageFile }) {
  const formData = new FormData()
  formData.append('productName', productName)
  formData.append('inspectorId', inspectorId)
  formData.append('image', imageFile)
  return request('/api/analyze', { method: 'POST', body: formData })
}

export function confirmReport(reportId) {
  return request('/api/confirm-report', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ reportId }),
  })
}
