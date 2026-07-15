import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import HelpPanel from './components/HelpPanel.jsx'
import { BrandMark, CaptureIcon, CopyIcon, MonitorCaptureIcon } from './components/Icons.jsx'
import Toggle from './components/Toggle.jsx'
import { copyPngToClipboard, downloadPng, getCaptureSupport, requestScreenFrame } from './lib/capture.js'

const initialStatus = {
  tone: 'idle',
  message: 'Aguardando captura',
}

export default function App() {
  const [autoCopy, setAutoCopy] = useState(true)
  const [autoDownload, setAutoDownload] = useState(false)
  const [capture, setCapture] = useState(null)
  const [status, setStatus] = useState(initialStatus)
  const [busy, setBusy] = useState(false)
  const previewUrlRef = useRef(null)
  const support = useMemo(() => getCaptureSupport(), [])

  const updatePreview = useCallback((blob, metadata) => {
    if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current)
    const url = URL.createObjectURL(blob)
    previewUrlRef.current = url
    setCapture({ blob, url, ...metadata })
  }, [])

  const copyCapture = useCallback(async (blob = capture?.blob) => {
    if (!blob) return false

    try {
      await copyPngToClipboard(blob)
      setStatus({ tone: 'success', message: 'Imagem copiada — use Ctrl + V para colar' })
      return true
    } catch (error) {
      setStatus({ tone: 'warning', message: `${error.message} Use o botão “Copiar imagem”.` })
      return false
    }
  }, [capture])

  const startCapture = useCallback(async () => {
    if (busy) return

    setBusy(true)
    setStatus({ tone: 'working', message: 'Escolha uma tela, janela ou guia…' })

    try {
      const result = await requestScreenFrame()
      updatePreview(result.blob, result)

      if (autoDownload) downloadPng(result.blob)

      if (autoCopy) {
        const copied = await copyCapture(result.blob)
        if (!copied) return
      } else {
        setStatus({ tone: 'success', message: `Captura pronta — ${result.width} × ${result.height} px` })
      }
    } catch (error) {
      setStatus({ tone: error.code === 'denied' ? 'idle' : 'error', message: error.message })
    } finally {
      setBusy(false)
    }
  }, [autoCopy, autoDownload, busy, copyCapture, updatePreview])

  useEffect(() => {
    const onShortcut = (event) => {
      if (event.altKey && event.shiftKey && event.code === 'KeyS') {
        event.preventDefault()
        startCapture()
      }
    }

    window.addEventListener('keydown', onShortcut)
    return () => window.removeEventListener('keydown', onShortcut)
  }, [startCapture])

  useEffect(() => () => {
    if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current)
  }, [])

  const canCapture = support.secureContext && support.displayMedia

  return (
    <div className="app-shell">
      <header className="topbar">
        <a className="brand" href="./" aria-label="Captura Rápida — início">
          <BrandMark />
          <span>Captura Rápida</span>
        </a>
        <a className="help-link" href="#ajuda">Ajuda</a>
      </header>

      <main className="app-layout">
        <section className="capture-workspace" aria-label="Área de captura">
          <div className={`capture-stage ${capture ? 'has-preview' : ''}`} aria-live="polite">
            {capture ? (
              <>
                <img src={capture.url} alt="Prévia da última captura" />
                <div className="preview-actions">
                  <button className="secondary-button" type="button" onClick={() => copyCapture()}>
                    <CopyIcon />
                    Copiar imagem
                  </button>
                  <button className="primary-button compact" type="button" onClick={startCapture} disabled={busy}>
                    <CaptureIcon />
                    Capturar novamente
                  </button>
                </div>
              </>
            ) : (
              <div className="empty-state">
                <MonitorCaptureIcon />
                <h1>Pronto para capturar</h1>
                <p>Escolha uma tela, janela ou guia para começar.</p>
                <button className="primary-button" type="button" onClick={startCapture} disabled={busy || !canCapture}>
                  <CaptureIcon size={25} />
                  {busy ? 'Abrindo seletor…' : 'Capturar tela'}
                </button>
              </div>
            )}
          </div>

          <div className="options-row" aria-label="Opções da captura">
            <Toggle checked={autoCopy} onChange={setAutoCopy}>Copiar automaticamente</Toggle>
            <Toggle checked={autoDownload} onChange={setAutoDownload}>Baixar PNG</Toggle>
          </div>

          <div className="status-row">
            <p className={`status status-${status.tone}`} role="status">
              <span className="status-dot" />
              <span>{status.message}</span>
            </p>
            <div className="shortcut" aria-label="Atalho Alt mais Shift mais S">
              <kbd>Alt</kbd><span>+</span><kbd>Shift</kbd><span>+</span><kbd>S</kbd>
            </div>
          </div>

          {!canCapture && (
            <p className="support-warning" role="alert">
              Abra por HTTPS no Microsoft Edge ou Google Chrome para usar a captura de tela.
            </p>
          )}
        </section>

        <HelpPanel />
      </main>
    </div>
  )
}
