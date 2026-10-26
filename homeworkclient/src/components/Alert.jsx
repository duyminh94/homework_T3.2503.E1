export default function Alert({ message, onRetry }) {
  return (
    <div className="alert" role="alert">
      <svg className="alert__icon" viewBox="0 0 20 20" width="20" height="20" aria-hidden="true" focusable="false">
        <circle cx="10" cy="10" r="8.5" />
        <path d="M10 5.5v5.5M10 14v.5" />
      </svg>
      <p className="alert__message">{message}</p>
      {onRetry && (
        <button type="button" className="button button--secondary" onClick={onRetry}>
          Thử lại
        </button>
      )}
    </div>
  )
}
