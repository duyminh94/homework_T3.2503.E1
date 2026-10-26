import { toImageUrl } from '../services/api'
import { formatPercent } from '../utils/format'
import Alert from './Alert'
import { ConfirmationBadge, QualityBadge, SeverityBadge } from './Badges'

export default function PreviewForm({ report, productName, inspectorName, isConfirming, errorMessage, onConfirm, onBack }) {
  const isPass = report.qualityStatus === 'Pass'

  return (
    <article className="preview" aria-labelledby="preview-title">
      <h2 id="preview-title" className="section-title">Kết quả AI: xem lại trước khi xác nhận</h2>

      <div className="preview__layout">
        <img className="preview__image" src={toImageUrl(report.imageUrl)} alt="Ảnh sản phẩm đã tải lên" />

        <dl className="detail-list">
          <div className="detail-list__item">
            <dt>Sản phẩm</dt>
            <dd>{productName}</dd>
          </div>
          <div className="detail-list__item">
            <dt>Nhân viên kiểm tra</dt>
            <dd>{inspectorName}</dd>
          </div>
          <div className="detail-list__item">
            <dt>Loại lỗi</dt>
            <dd>
              {report.defectCategoryName} <SeverityBadge severityLevel={report.severityLevel} />
            </dd>
          </div>
          <div className="detail-list__item">
            <dt>Mô tả</dt>
            <dd>{report.defectDescription}</dd>
          </div>
          <div className="detail-list__item">
            <dt>Độ tin cậy của AI</dt>
            <dd>{formatPercent(report.confidenceScore)}</dd>
          </div>
          <div className="detail-list__item">
            <dt>Kết luận</dt>
            <dd><QualityBadge qualityStatus={report.qualityStatus} /></dd>
          </div>
          <div className="detail-list__item">
            <dt>Trạng thái phiếu #{report.reportId}</dt>
            <dd><ConfirmationBadge isUserConfirmed={report.isUserConfirmed} /></dd>
          </div>
        </dl>
      </div>

      <p className="form__hint">
        Xác nhận sẽ cộng 1 vào tồn kho {isPass ? 'đạt' : 'không đạt'} của sản phẩm. Nếu quay lại mà không xác nhận,
        phiếu vẫn nằm trong Lịch sử với trạng thái Chưa xác nhận.
      </p>

      {errorMessage && <Alert message={errorMessage} />}

      <div className="button-row">
        <button type="button" className="button button--primary" onClick={onConfirm} disabled={isConfirming}>
          {isConfirming ? (
            <>
              <span className="spinner" aria-hidden="true" />
              Đang xác nhận...
            </>
          ) : (
            'Xác nhận kết quả'
          )}
        </button>
        <button type="button" className="button button--secondary" onClick={onBack} disabled={isConfirming}>
          Quay lại, chọn ảnh khác
        </button>
      </div>
    </article>
  )
}
