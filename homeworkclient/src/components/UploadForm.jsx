import { useEffect, useState } from 'react'
import { analyzeImage, confirmReport, getInspectors, getProducts } from '../services/api'
import { formatFileSize, formatNumber } from '../utils/format'
import Alert from './Alert'
import PreviewForm from './PreviewForm'

const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_IMAGE_SIZE_IN_BYTES = 10 * 1024 * 1024
const LOADING_LOOKUPS = { status: 'loading', products: [], inspectors: [], errorMessage: '' }
const EMPTY_FORM_VALUES = { productName: '', inspectorId: '' }

const REQUIRED_MESSAGES = {
  productName: 'Nhập tên sản phẩm cần kiểm tra.',
  inspectorId: 'Chọn nhân viên thực hiện kiểm tra.',
  image: 'Chọn một ảnh sản phẩm (JPG, PNG hoặc WEBP, tối đa 10 MB).',
}

function getImageFileError(file) {
  if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
    return 'Chỉ nhận ảnh JPG, PNG hoặc WEBP. Hãy chọn lại file khác.'
  }
  if (file.size > MAX_IMAGE_SIZE_IN_BYTES) {
    return `Ảnh nặng ${formatFileSize(file.size)}, vượt giới hạn 10 MB. Hãy chọn ảnh nhỏ hơn.`
  }
  return ''
}

function findProductByName(products, productName) {
  const normalizedName = productName.trim().toLowerCase()
  return products.find((product) => product.productName.toLowerCase() === normalizedName)
}

function getProductNameError(products, productName) {
  if (!productName.trim()) return REQUIRED_MESSAGES.productName

  const product = findProductByName(products, productName)
  return product && product.remainingStock <= 0 ? `${product.productName} đã kiểm đủ số lượng, không thể kiểm thêm.` : ''
}

function getFormErrors(formValues, selectedImage, products) {
  return {
    productName: getProductNameError(products, formValues.productName),
    inspectorId: formValues.inspectorId ? '' : REQUIRED_MESSAGES.inspectorId,
    image: selectedImage ? '' : REQUIRED_MESSAGES.image,
  }
}

export default function UploadForm({ onNavigate, showToast }) {
  const [lookups, setLookups] = useState(LOADING_LOOKUPS)
  const [reloadKey, setReloadKey] = useState(0)
  const [formValues, setFormValues] = useState(EMPTY_FORM_VALUES)
  const [selectedImage, setSelectedImage] = useState(null)
  const [fieldErrors, setFieldErrors] = useState({})
  const [step, setStep] = useState('form')
  const [isAnalyzing, setIsAnalyzing] = useState(false)
  const [analyzeError, setAnalyzeError] = useState('')
  const [analysis, setAnalysis] = useState(null)
  const [isConfirming, setIsConfirming] = useState(false)
  const [confirmError, setConfirmError] = useState('')
  const [confirmedResult, setConfirmedResult] = useState(null)

  const previewUrl = selectedImage?.previewUrl

  useEffect(() => {
    let isCancelled = false

    async function loadLookups() {
      try {
        const [products, inspectors] = await Promise.all([getProducts(), getInspectors()])
        if (!isCancelled) {
          setLookups({ status: 'ready', products, inspectors, errorMessage: '' })
        }
      } catch (error) {
        if (!isCancelled) {
          setLookups({ status: 'error', products: [], inspectors: [], errorMessage: error.message })
        }
      }
    }

    loadLookups()
    return () => {
      isCancelled = true
    }
  }, [reloadKey])

  useEffect(() => {
    return () => {
      if (previewUrl) URL.revokeObjectURL(previewUrl)
    }
  }, [previewUrl])

  const selectedInspector = lookups.inspectors.find((inspector) => String(inspector.inspectorId) === formValues.inspectorId)

  function handleRetryLookups() {
    setLookups(LOADING_LOOKUPS)
    setReloadKey((key) => key + 1)
  }

  function handleFieldChange(event) {
    const { name, value } = event.target
    setFormValues((current) => ({ ...current, [name]: value }))
    setFieldErrors((current) => ({ ...current, [name]: '' }))
  }

  function handleFieldBlur(event) {
    const { name, value } = event.target
    const message = name === 'productName' ? getProductNameError(lookups.products, value) : value ? '' : REQUIRED_MESSAGES[name]
    setFieldErrors((current) => ({ ...current, [name]: message }))
  }

  function handleImageChange(event) {
    const file = event.target.files[0]
    if (!file) {
      setSelectedImage(null)
      return
    }

    const imageError = getImageFileError(file)
    if (imageError) {
      event.target.value = ''
      setSelectedImage(null)
      setFieldErrors((current) => ({ ...current, image: imageError }))
      return
    }

    setSelectedImage({ file, previewUrl: URL.createObjectURL(file) })
    setFieldErrors((current) => ({ ...current, image: '' }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    if (isAnalyzing) return

    const errors = getFormErrors(formValues, selectedImage, lookups.products)
    const firstErrorField = Object.keys(errors).find((fieldName) => errors[fieldName])
    setFieldErrors(errors)
    if (firstErrorField) {
      document.getElementById(`field-${firstErrorField}`)?.focus()
      return
    }

    setIsAnalyzing(true)
    setAnalyzeError('')
    try {
      const result = await analyzeImage({
        productName: formValues.productName.trim(),
        inspectorId: formValues.inspectorId,
        imageFile: selectedImage.file,
      })
      setAnalysis(result)
      setConfirmError('')
      setStep('preview')
    } catch (error) {
      setAnalyzeError(error.message)
    } finally {
      setIsAnalyzing(false)
    }
  }

  async function handleConfirm() {
    if (isConfirming) return

    setIsConfirming(true)
    setConfirmError('')
    try {
      const result = await confirmReport(analysis.reportId)
      setConfirmedResult(result)
      setStep('done')
      showToast(`Đã xác nhận phiếu kiểm #${result.reportId}`)
    } catch (error) {
      setConfirmError(error.message)
    } finally {
      setIsConfirming(false)
    }
  }

  function handleBackToForm() {
    setSelectedImage(null)
    setAnalysis(null)
    setStep('form')
  }

  function handleStartOver() {
    setSelectedImage(null)
    setAnalysis(null)
    setConfirmedResult(null)
    setReloadKey((key) => key + 1)
    setStep('form')
  }

  return (
    <section aria-labelledby="upload-title">
      <h1 id="upload-title" className="page-title">Tải ảnh lên để kiểm tra</h1>

      {lookups.status === 'loading' && <p role="status">Đang tải danh sách sản phẩm và nhân viên...</p>}
      {lookups.status === 'error' && <Alert message={lookups.errorMessage} onRetry={handleRetryLookups} />}

      {lookups.status === 'ready' && step === 'form' && (
        <form className="form" onSubmit={handleSubmit} noValidate aria-busy={isAnalyzing}>
          <fieldset className="form__fieldset" disabled={isAnalyzing}>
            <legend className="visually-hidden">Thông tin phiếu kiểm</legend>

            <div className="field">
              <label htmlFor="field-productName">Tên sản phẩm *</label>
              <input
                id="field-productName"
                name="productName"
                type="text"
                list="product-suggestions"
                autoComplete="off"
                maxLength={200}
                placeholder="Gõ tên sản phẩm bất kỳ (tên mới sẽ được tạo tự động)"
                value={formValues.productName}
                onChange={handleFieldChange}
                onBlur={handleFieldBlur}
                aria-invalid={Boolean(fieldErrors.productName)}
                aria-describedby={fieldErrors.productName ? 'error-productName' : undefined}
              />
              <datalist id="product-suggestions">
                {lookups.products.map((product) => (
                  <option key={product.productId} value={product.productName}>
                    còn {formatNumber(product.remainingStock)}/{formatNumber(product.totalStock)} chưa kiểm
                    {product.remainingStock <= 0 ? ' (đã kiểm đủ)' : ''}
                  </option>
                ))}
              </datalist>
              {fieldErrors.productName && <p id="error-productName" className="field__error" role="alert">{fieldErrors.productName}</p>}
            </div>

            <div className="field">
              <label htmlFor="field-inspectorId">Nhân viên kiểm tra *</label>
              <select
                id="field-inspectorId"
                name="inspectorId"
                value={formValues.inspectorId}
                onChange={handleFieldChange}
                onBlur={handleFieldBlur}
                aria-invalid={Boolean(fieldErrors.inspectorId)}
                aria-describedby={fieldErrors.inspectorId ? 'error-inspectorId' : undefined}
              >
                <option value="">Chọn nhân viên</option>
                {lookups.inspectors.map((inspector) => (
                  <option key={inspector.inspectorId} value={inspector.inspectorId}>
                    {inspector.inspectorName} ({inspector.department})
                  </option>
                ))}
              </select>
              {fieldErrors.inspectorId && <p id="error-inspectorId" className="field__error" role="alert">{fieldErrors.inspectorId}</p>}
            </div>

            <div className="field">
              <label htmlFor="field-image">Ảnh sản phẩm *</label>
              <input
                id="field-image"
                name="image"
                type="file"
                accept="image/jpeg,image/png,image/webp"
                onChange={handleImageChange}
                aria-invalid={Boolean(fieldErrors.image)}
                aria-describedby={fieldErrors.image ? 'image-hint error-image' : 'image-hint'}
              />
              <p id="image-hint" className="form__hint">JPG, PNG hoặc WEBP, tối đa 10 MB. Chụp rõ, đủ sáng, thấy toàn bộ sản phẩm.</p>
              {fieldErrors.image && <p id="error-image" className="field__error" role="alert">{fieldErrors.image}</p>}

              {selectedImage && (
                <figure className="image-preview">
                  <img src={selectedImage.previewUrl} alt={`Ảnh xem trước: ${selectedImage.file.name}`} />
                  <figcaption>{selectedImage.file.name} ({formatFileSize(selectedImage.file.size)})</figcaption>
                </figure>
              )}
            </div>
          </fieldset>

          {analyzeError && <Alert message={analyzeError} />}

          <button type="submit" className="button button--primary" disabled={isAnalyzing}>
            {isAnalyzing ? (
              <>
                <span className="spinner" aria-hidden="true" />
                AI đang phân tích ảnh...
              </>
            ) : (
              'Phân tích ảnh bằng AI'
            )}
          </button>
          {isAnalyzing && <p className="form__hint" role="status">Quá trình này có thể mất vài giây, vui lòng không đóng trang.</p>}
        </form>
      )}

      {step === 'preview' && analysis && (
        <PreviewForm
          report={analysis}
          productName={analysis.productName}
          inspectorName={selectedInspector?.inspectorName ?? ''}
          isConfirming={isConfirming}
          errorMessage={confirmError}
          onConfirm={handleConfirm}
          onBack={handleBackToForm}
        />
      )}

      {step === 'done' && confirmedResult && (
        <div className="result-panel" role="status">
          <h2 className="section-title">Đã ghi nhận phiếu kiểm #{confirmedResult.reportId}</h2>
          <p>
            Tồn kho hiện tại của <strong>{confirmedResult.productName}</strong>: đạt {formatNumber(confirmedResult.passedStock)},
            không đạt {formatNumber(confirmedResult.failedStock)}, chưa kiểm{' '}
            {formatNumber(confirmedResult.totalStock - confirmedResult.passedStock - confirmedResult.failedStock)} trên tổng{' '}
            {formatNumber(confirmedResult.totalStock)}.
          </p>
          <div className="button-row">
            <button type="button" className="button button--primary" onClick={handleStartOver}>
              Kiểm tra ảnh khác
            </button>
            <button type="button" className="button button--secondary" onClick={() => onNavigate('dashboard')}>
              Xem bảng điều khiển
            </button>
          </div>
        </div>
      )}
    </section>
  )
}
