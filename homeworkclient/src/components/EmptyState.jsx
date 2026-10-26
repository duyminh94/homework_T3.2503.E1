export default function EmptyState({ message, actionLabel, onAction }) {
  return (
    <div className="empty-state">
      <p>{message}</p>
      {actionLabel && (
        <button type="button" className="button button--secondary" onClick={onAction}>
          {actionLabel}
        </button>
      )}
    </div>
  )
}
