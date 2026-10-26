import { useEffect, useState } from 'react'
import { confirmReport, getProducts, getReports, toImageUrl } from '../services/api'
import { formatDateTime, formatNumber, formatPercent } from '../utils/format'
import Alert from './Alert'
import { ConfirmationBadge, QualityBadge, SeverityBadge } from './Badges'
import EmptyState from './EmptyState'

const PAGE_SIZE = 10
const EMPTY_FILTERS = { productId: '', qualityStatus: '', isConfirmed: '' }
const LOADING_STATE = { status: 'loading', data: null, errorMessage: '' }

export default function InspectionHistory({ onNavigate, showToast }) {
  const [filters, setFilters] = useState(EMPTY_FILTERS)
  const [page, setPage] = useState(1)
  const [listState, setListState] = useState(LOADING_STATE)
  const [reloadKey, setReloadKey] = useState(0)
  const [products, setProducts] = useState([])
  const [productsError, setProductsError] = useState('')
  const [confirmingId, setConfirmingId] = useState(null)
  const [actionError, setActionError] = useState('')

  useEffect(() => {
    let isCancelled = false

    async function loadProducts() {
      try {
        const loadedProducts = await getProducts()
        if (!isCancelled) setProducts(loadedProducts)
      } catch (error) {
        if (!isCancelled) setProductsError(error.message)
      }
    }

    loadProducts()
    return () => {
      isCancelled = true
    }
  }, [])

  useEffect(() => {
    let isCancelled = false

    async function loadReports() {
      try {
        const data = await getReports({ ...filters, page, pageSize: PAGE_SIZE })
        if (!isCancelled) {
          setListState({ status: 'ready', data, errorMessage: '' })
        }
      } catch (error) {
        if (!isCancelled) {
          setListState({ status: 'error', data: null, errorMessage: error.message })
        }
      }
    }

    loadReports()
    return () => {
      isCancelled = true
    }
  }, [filters, page, reloadKey])

  const hasActiveFilter = Object.values(filters).some((value) => value !== '')

  function handleFilterChange(event) {
    const { name, value } = event.target
    setListState(LOADING_STATE)
    setPage(1)
    setFilters((current) => ({ ...current, [name]: value }))
  }

  function handleClearFilters() {
    setListState(LOADING_STATE)
    setPage(1)
    setFilters(EMPTY_FILTERS)
  }

  function handleChangePage(newPage) {
    setListState(LOADING_STATE)
    setPage(newPage)
  }

  function handleRetry() {
    setListState(LOADING_STATE)
    setReloadKey((key) => key + 1)
  }

  async function handleConfirmReport(reportId) {
    if (confirmingId !== null) return

    setConfirmingId(reportId)
    setActionError('')
    try {
      await confirmReport(reportId)
      showToast(`Đã xác nhận phiếu kiểm #${reportId}`)
      setReloadKey((key) => key + 1)
    } catch (error) {
      setActionError(error.message)
    } finally {
      setConfirmingId(null)
    }
  }

  return (
    <section aria-labelledby="history-title">
      <h1 id="history-title" className="page-title">Lịch sử kiểm tra</h1>

      <form className="filters" aria-label="Bộ lọc lịch sử kiểm tra" onSubmit={(event) => event.preventDefault()}>
        <div className="field">
          <label htmlFor="filter-productId">Sản phẩm</label>
          <select id="filter-productId" name="productId" value={filters.productId} onChange={handleFilterChange}>
            <option value="">Tất cả sản phẩm</option>
            {products.map((product) => (
              <option key={product.productId} value={product.productId}>{product.productName}</option>
            ))}
          </select>
          {productsError && <p className="field__error" role="alert">{productsError}</p>}
        </div>

        <div className="field">
          <label htmlFor="filter-qualityStatus">Kết luận</label>
          <select id="filter-qualityStatus" name="qualityStatus" value={filters.qualityStatus} onChange={handleFilterChange}>
            <option value="">Tất cả</option>
            <option value="Pass">Đạt</option>
            <option value="Fail">Không đạt</option>
          </select>
        </div>

        <div className="field">
          <label htmlFor="filter-isConfirmed">Trạng thái phiếu</label>
          <select id="filter-isConfirmed" name="isConfirmed" value={filters.isConfirmed} onChange={handleFilterChange}>
            <option value="">Tất cả</option>
            <option value="true">Đã xác nhận</option>
            <option value="false">Chưa xác nhận</option>
          </select>
        </div>

        <button type="button" className="button button--secondary" onClick={handleClearFilters} disabled={!hasActiveFilter}>
          Xóa bộ lọc
        </button>
      </form>

      {actionError && <Alert message={actionError} />}
      {listState.status === 'loading' && <HistorySkeleton />}
      {listState.status === 'error' && <Alert message={listState.errorMessage} onRetry={handleRetry} />}
      {listState.status === 'ready' && (
        <HistoryResult
          data={listState.data}
          hasActiveFilter={hasActiveFilter}
          confirmingId={confirmingId}
          onConfirmReport={handleConfirmReport}
          onChangePage={handleChangePage}
          onClearFilters={handleClearFilters}
          onNavigate={onNavigate}
        />
      )}
    </section>
  )
}

function HistorySkeleton() {
  return (
    <div className="skeleton" aria-busy="true">
      <p className="visually-hidden" role="status">Đang tải lịch sử kiểm tra...</p>
      <div className="skeleton__block skeleton__block--tall" />
    </div>
  )
}

function HistoryResult({ data, hasActiveFilter, confirmingId, onConfirmReport, onChangePage, onClearFilters, onNavigate }) {
  if (data.totalCount === 0) {
    return hasActiveFilter ? (
      <EmptyState message="Không có phiếu kiểm nào khớp bộ lọc." actionLabel="Xóa bộ lọc" onAction={onClearFilters} />
    ) : (
      <EmptyState message="Chưa có phiếu kiểm nào." actionLabel="Tải ảnh lên để kiểm tra" onAction={() => onNavigate('upload')} />
    )
  }

  const totalPages = Math.max(1, Math.ceil(data.totalCount / data.pageSize))

  return (
    <>
      <div className="table-scroll">
        <table className="data-table data-table--history">
          <caption className="visually-hidden">Danh sách phiếu kiểm tra</caption>
          <thead>
            <tr>
              <th scope="col">Ảnh</th>
              <th scope="col">Thời gian</th>
              <th scope="col">Sản phẩm</th>
              <th scope="col">Kết quả AI</th>
              <th scope="col">Kết luận</th>
              <th scope="col" className="numeric">Độ tin cậy</th>
              <th scope="col">Trạng thái phiếu</th>
              <th scope="col">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {data.items.map((report) => (
              <tr key={report.reportId}>
                <td>
                  <img
                    className="thumb"
                    src={toImageUrl(report.imageUrl)}
                    alt={`Ảnh phiếu kiểm số ${report.reportId}`}
                    width="64"
                    height="64"
                    loading="lazy"
                  />
                </td>
                <td>{formatDateTime(report.createdAt)}</td>
                <td>
                  {report.productName}
                  <span className="cell-note">Kiểm bởi {report.inspectorName}</span>
                </td>
                <td className="cell-wide">
                  {report.defectCategoryName} <SeverityBadge severityLevel={report.severityLevel} />
                  <span className="cell-note">{report.defectDescription}</span>
                </td>
                <td><QualityBadge qualityStatus={report.qualityStatus} /></td>
                <td className="numeric">{formatPercent(report.confidenceScore)}</td>
                <td><ConfirmationBadge isUserConfirmed={report.isUserConfirmed} /></td>
                <td>
                  {!report.isUserConfirmed && (
                    <button
                      type="button"
                      className="button button--secondary"
                      onClick={() => onConfirmReport(report.reportId)}
                      disabled={confirmingId !== null}
                    >
                      {confirmingId === report.reportId ? (
                        <>
                          <span className="spinner spinner--dark" aria-hidden="true" />
                          Đang xác nhận...
                        </>
                      ) : (
                        'Xác nhận'
                      )}
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <nav className="pagination" aria-label="Phân trang lịch sử">
        <button type="button" className="button button--secondary" onClick={() => onChangePage(data.page - 1)} disabled={data.page <= 1}>
          Trang trước
        </button>
        <span>Trang {data.page} / {totalPages} ({formatNumber(data.totalCount)} phiếu)</span>
        <button type="button" className="button button--secondary" onClick={() => onChangePage(data.page + 1)} disabled={data.page >= totalPages}>
          Trang sau
        </button>
      </nav>
    </>
  )
}
