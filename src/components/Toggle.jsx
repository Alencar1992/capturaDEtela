export default function Toggle({ checked, onChange, children }) {
  return (
    <label className="option-toggle">
      <input type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)} />
      <span className="checkbox" aria-hidden="true">
        <svg viewBox="0 0 16 16" fill="none">
          <path d="m3.5 8.2 2.7 2.7 6.3-6.3" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      </span>
      <span>{children}</span>
    </label>
  )
}
