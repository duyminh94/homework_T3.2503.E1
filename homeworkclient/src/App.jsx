import { useEffect, useRef, useState } from 'react'
import './App.css'
import Dashboard from './components/Dashboard'
import InspectionHistory from './components/InspectionHistory'
import Toast from './components/Toast'
import UploadForm from './components/UploadForm'

const TABS = [
  { key: 'dashboard', label: 'Bảng điều khiển' },
  { key: 'upload', label: 'Tải ảnh kiểm tra' },
  { key: 'history', label: 'Lịch sử kiểm tra' },
]
const TOAST_DURATION_IN_MS = 4000

export default function App() {
  const [activeTab, setActiveTab] = useState('dashboard')
  const [toastMessage, setToastMessage] = useState('')
  const toastTimerRef = useRef(null)

  useEffect(() => {
    return () => clearTimeout(toastTimerRef.current)
  }, [])

  function showToast(message) {
    clearTimeout(toastTimerRef.current)
    setToastMessage(message)
    toastTimerRef.current = setTimeout(() => setToastMessage(''), TOAST_DURATION_IN_MS)
  }

  return (
    <div className="app">
      <header className="app__header">
        <div className="app__container app__header-inner">
          <p className="app__brand">Kiểm tra chất lượng sản phẩm bằng AI</p>
          <nav aria-label="Điều hướng chính">
            <ul className="nav">
              {TABS.map((tab) => (
                <li key={tab.key}>
                  <button
                    type="button"
                    className="nav__button"
                    aria-current={activeTab === tab.key ? 'page' : undefined}
                    onClick={() => setActiveTab(tab.key)}
                  >
                    {tab.label}
                  </button>
                </li>
              ))}
            </ul>
          </nav>
        </div>
      </header>

      <main className="app__container app__main">
        {activeTab === 'dashboard' && <Dashboard onNavigate={setActiveTab} />}
        <div hidden={activeTab !== 'upload'}>
          <UploadForm onNavigate={setActiveTab} showToast={showToast} />
        </div>
        {activeTab === 'history' && <InspectionHistory onNavigate={setActiveTab} showToast={showToast} />}
      </main>

      <Toast message={toastMessage} />
    </div>
  )
}
