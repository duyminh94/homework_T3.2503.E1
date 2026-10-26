import { useEffect, useState } from 'react'
import { getDashboard, getProducts, getReports, toImageUrl } from '../services/api'
import { formatDateTime, formatNumber, formatPercent } from '../utils/format'
import Alert from './Alert'
import { ConfirmationBadge, QualityBadge, SeverityBadge } from './Badges'
import EmptyState from './EmptyState'

const LOADING_STATE = { status: 'loading', data: null, errorMessage: '' }
const RECENT_REPORT_COUNT = 5

export default function Dashboard({ onNavigate }) {
  const [state, setState] = useState(LOADING_STATE)
  const [reloadKey, setReloadKey] = useState(0)

  useEffect(() => {
    let isCancelled = false

    async function loadDashboard() {
      try {
        const [summary, products, recentPage] = await Promise.all([
          getDashboard(),
          getProducts(),
          getReports({ page: 1, pageSize: RECENT_REPORT_COUNT }),
        ])
        if (!isCancelled) {
          setState({ status: 'ready', data: { summary, products, recentReports: recentPage.items }, errorMessage: '' })
        }
      } catch (error) {
        if (!isCancelled) {
          setState({ status: 'error', data: null, errorMessage: error.message })
        }
      }
    }

    loadDashboard()
    return () => {
      isCancelled = true
    }
  }, [reloadKey])

  function handleRetry() {
    setState(LOADING_STATE)
    setReloadKey((key) => key + 1)
  }

  return (
    <section aria-labelledby="dashboard-title">
      <h1 id="dashboard-title" className="page-title">Bảng điều khiển QC</h1>

      {state.status === 'loading' && <DashboardSkeleton />}
      {state.status === 'error' && <Alert message={state.errorMessage} onRetry={handleRetry} />}
      {state.status === 'ready' && <DashboardContent data={state.data} onNavigate={onNavigate} />}
    </section>
  )
}

function DashboardSkeleton() {
  return (
    <div className="skeleton" aria-busy="true">
      <p className="visually-hidden" role="status">Đang tải dữ liệu bảng điều khiển...</p>
      <div className="skeleton__row">
        <div className="skeleton__block" />
        <div className="skeleton__block" />
        <div className="skeleton__block" />
      </div>
      <div className="skeleton__block skeleton__block--tall" />
    </div>
  )
}

function DashboardContent({ data, onNavigate }) {
  const { summary, products, recentReports } = data

  return (
    <>
      <StatTiles summary={summary} />

      <h2 className="section-title">Tiến độ kiểm tra theo sản phẩm</h2>
      <ProductProgress products={products} />

      <h2 className="section-title">Loại lỗi đã ghi nhận</h2>
      <DefectBreakdown items={summary.defectBreakdown} onNavigate={onNavigate} />

      <div className="section-header">
        <h2 className="section-title">Phiếu kiểm gần đây</h2>
        {recentReports.length > 0 && (
          <button type="button" className="button button--secondary" onClick={() => onNavigate('history')}>
            Xem tất cả
          </button>
        )}
      </div>
      <RecentReports reports={recentReports} onNavigate={onNavigate} />
    </>
  )
}

function StatTiles({ summary }) {
  const checkedCount = summary.passedStock + summary.failedStock
  const passRateNote = checkedCount === 0
    ? 'Chưa có sản phẩm nào được kiểm'
    : `${formatPercent(summary.passedStock / checkedCount)} số đã kiểm`

  const tiles = [
    { label: 'Sản phẩm', value: summary.totalProducts },
    { label: 'Tổng tồn kho', value: summary.totalStock },
    { label: 'Đã kiểm đạt', value: summary.passedStock, note: passRateNote },
    { label: 'Đã kiểm không đạt', value: summary.failedStock },
    { label: 'Phiếu chờ xác nhận', value: summary.pendingReports },
  ]

  return (
    <ul className="stat-tiles">
      {tiles.map((tile) => (
        <li key={tile.label} className="stat-tile">
          <span className="stat-tile__label">{tile.label}</span>
          <span className="stat-tile__value">{formatNumber(tile.value)}</span>
          {tile.note && <span className="stat-tile__note">{tile.note}</span>}
        </li>
      ))}
    </ul>
  )
}

function ProductProgress({ products }) {
  if (products.length === 0) {
    return <p className="empty-text">Chưa có sản phẩm nào trong hệ thống.</p>
  }

  return (
    <>
      <ul className="legend" aria-label="Chú giải thanh tiến độ">
        <li><span className="legend__swatch legend__swatch--pass" aria-hidden="true" />Đạt</li>
        <li><span className="legend__swatch legend__swatch--fail" aria-hidden="true" />Không đạt</li>
        <li><span className="legend__swatch legend__swatch--remaining" aria-hidden="true" />Chưa kiểm</li>
      </ul>
      <div className="table-scroll">
        <table className="data-table">
          <caption className="visually-hidden">Tiến độ kiểm tra theo sản phẩm</caption>
          <thead>
            <tr>
              <th scope="col">Sản phẩm</th>
              <th scope="col">Tiến độ</th>
              <th scope="col" className="numeric">Đạt</th>
              <th scope="col" className="numeric">Không đạt</th>
              <th scope="col" className="numeric">Chưa kiểm</th>
              <th scope="col" className="numeric">Tồn kho</th>
            </tr>
          </thead>
          <tbody>
            {products.map((product) => (
              <tr key={product.productId}>
                <th scope="row">
                  {product.productName}
                  <span className="cell-note">{product.category}</span>
                </th>
                <td className="cell-bar"><StockBar product={product} /></td>
                <td className="numeric">{formatNumber(product.passedStock)}</td>
                <td className="numeric">{formatNumber(product.failedStock)}</td>
                <td className="numeric">{formatNumber(product.remainingStock)}</td>
                <td className="numeric">{formatNumber(product.totalStock)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  )
}

function StockBar({ product }) {
  const segments = [
    { key: 'pass', value: product.passedStock },
    { key: 'fail', value: product.failedStock },
    { key: 'remaining', value: Math.max(product.remainingStock, 0) },
  ].filter((segment) => segment.value > 0)

  const description = `Đạt ${product.passedStock}, không đạt ${product.failedStock}, chưa kiểm ${product.remainingStock} trên tổng ${product.totalStock}`

  return (
    <div className="stock-bar" role="img" aria-label={description}>
      {segments.map((segment) => (
        <span
          key={segment.key}
          className={`stock-bar__segment stock-bar__segment--${segment.key}`}
          style={{ flexGrow: segment.value }}
        />
      ))}
    </div>
  )
}

function DefectBreakdown({ items, onNavigate }) {
  if (items.length === 0) {
    return (
      <EmptyState
        message="Chưa có phiếu nào được xác nhận nên chưa có thống kê lỗi."
        actionLabel="Tải ảnh lên để kiểm tra"
        onAction={() => onNavigate('upload')}
      />
    )
  }

  const maxCount = Math.max(...items.map((item) => item.count))

  return (
    <div className="table-scroll">
      <table className="data-table">
        <caption className="visually-hidden">Số phiếu đã xác nhận theo loại lỗi</caption>
        <thead>
          <tr>
            <th scope="col">Loại lỗi</th>
            <th scope="col">Biểu đồ</th>
            <th scope="col" className="numeric">Số phiếu</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.categoryName}>
              <th scope="row">
                {item.categoryName}
                <SeverityBadge severityLevel={item.severityLevel} />
              </th>
              <td className="cell-bar">
                <div className="metric-bar" aria-hidden="true">
                  <span className="metric-bar__fill" style={{ width: `${(item.count / maxCount) * 100}%` }} />
                </div>
              </td>
              <td className="numeric">{formatNumber(item.count)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function RecentReports({ reports, onNavigate }) {
  if (reports.length === 0) {
    return (
      <EmptyState
        message="Chưa có phiếu kiểm nào."
        actionLabel="Tải ảnh lên để kiểm tra"
        onAction={() => onNavigate('upload')}
      />
    )
  }

  return (
    <ul className="recent-list">
      {reports.map((report) => (
        <li key={report.reportId} className="recent-item">
          <img
            className="recent-item__image"
            src={toImageUrl(report.imageUrl)}
            alt={`Ảnh phiếu kiểm số ${report.reportId}`}
            width="48"
            height="48"
            loading="lazy"
          />
          <div className="recent-item__body">
            <span className="recent-item__title">{report.productName}</span>
            <span className="cell-note">{report.defectCategoryName} — {formatDateTime(report.createdAt)}</span>
          </div>
          <div className="recent-item__badges">
            <QualityBadge qualityStatus={report.qualityStatus} />
            <ConfirmationBadge isUserConfirmed={report.isUserConfirmed} />
          </div>
        </li>
      ))}
    </ul>
  )
}
