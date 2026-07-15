export function BrandMark({ size = 34 }) {
  return (
    <svg aria-hidden="true" width={size} height={size} viewBox="0 0 36 36" fill="none">
      <path d="M12 3H7a4 4 0 0 0-4 4v5M24 3h5a4 4 0 0 1 4 4v5M12 33H7a4 4 0 0 1-4-4v-5m21 9h5a4 4 0 0 0 4-4v-5" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" />
      <circle cx="18" cy="18" r="6.5" stroke="currentColor" strokeWidth="2.6" />
    </svg>
  )
}

export function CaptureIcon({ size = 24 }) {
  return (
    <svg aria-hidden="true" width={size} height={size} viewBox="0 0 24 24" fill="none">
      <path d="M8.5 5.5 9.8 3.8h4.4l1.3 1.7H19A2.5 2.5 0 0 1 21.5 8v9A2.5 2.5 0 0 1 19 19.5H5A2.5 2.5 0 0 1 2.5 17V8A2.5 2.5 0 0 1 5 5.5h3.5Z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" />
      <circle cx="12" cy="12.5" r="3.5" stroke="currentColor" strokeWidth="1.8" />
    </svg>
  )
}

export function MonitorCaptureIcon() {
  return (
    <svg aria-hidden="true" className="monitor-icon" viewBox="0 0 160 130" fill="none">
      <rect x="19" y="11" width="122" height="82" rx="7" stroke="currentColor" strokeWidth="6" />
      <path d="M36 39V27h14M124 39V27h-14M36 65v13h14M124 65v13h-14M80 39v27M66.5 52.5h27M80 94v20M52 116h56" stroke="currentColor" strokeWidth="6" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  )
}

export function CopyIcon({ size = 20 }) {
  return (
    <svg aria-hidden="true" width={size} height={size} viewBox="0 0 24 24" fill="none">
      <rect x="8" y="8" width="11" height="12" rx="2" stroke="currentColor" strokeWidth="1.8" />
      <path d="M16 8V6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h2" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  )
}

export function LockIcon() {
  return (
    <svg aria-hidden="true" width="24" height="24" viewBox="0 0 24 24" fill="none">
      <rect x="5" y="10" width="14" height="10" rx="2" stroke="currentColor" strokeWidth="1.8" />
      <path d="M8 10V7a4 4 0 0 1 8 0v3M12 14v2" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  )
}
