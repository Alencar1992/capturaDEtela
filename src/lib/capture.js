export const CAPTURE_ERROR_CODES = {
  unsupported: 'unsupported',
  insecure: 'insecure',
  denied: 'denied',
  clipboard: 'clipboard',
  unknown: 'unknown',
}

export function getCaptureSupport() {
  return {
    secureContext: window.isSecureContext,
    displayMedia: Boolean(navigator.mediaDevices?.getDisplayMedia),
    clipboardImage: Boolean(navigator.clipboard?.write && window.ClipboardItem),
  }
}

export async function requestScreenFrame() {
  const support = getCaptureSupport()

  if (!support.secureContext) {
    throw createCaptureError(
      CAPTURE_ERROR_CODES.insecure,
      'Abra o aplicativo por HTTPS ou em localhost para capturar a tela.',
    )
  }

  if (!support.displayMedia) {
    throw createCaptureError(
      CAPTURE_ERROR_CODES.unsupported,
      'Este navegador não oferece captura de tela. Use o Microsoft Edge ou o Google Chrome.',
    )
  }

  let stream

  try {
    stream = await navigator.mediaDevices.getDisplayMedia({
      video: true,
      audio: false,
    })

    const video = document.createElement('video')
    video.muted = true
    video.playsInline = true
    video.srcObject = stream

    await waitForVideo(video)
    const blob = await videoFrameToPng(video)

    return {
      blob,
      width: video.videoWidth,
      height: video.videoHeight,
      surface: stream.getVideoTracks()[0]?.getSettings?.().displaySurface ?? 'unknown',
    }
  } catch (error) {
    if (error?.name === 'NotAllowedError' || error?.name === 'AbortError') {
      throw createCaptureError(
        CAPTURE_ERROR_CODES.denied,
        'A captura foi cancelada. Clique no botão e escolha uma tela, janela ou guia.',
      )
    }

    if (error?.code) throw error

    throw createCaptureError(
      CAPTURE_ERROR_CODES.unknown,
      'Não foi possível capturar a imagem. Tente novamente.',
      error,
    )
  } finally {
    stream?.getTracks().forEach((track) => track.stop())
  }
}

export async function copyPngToClipboard(blob) {
  const support = getCaptureSupport()

  if (!support.clipboardImage) {
    throw createCaptureError(
      CAPTURE_ERROR_CODES.clipboard,
      'A cópia de imagem não está disponível neste navegador.',
    )
  }

  try {
    await navigator.clipboard.write([
      new ClipboardItem({
        'image/png': blob,
      }),
    ])
  } catch (error) {
    throw createCaptureError(
      CAPTURE_ERROR_CODES.clipboard,
      'A imagem foi capturada, mas o navegador bloqueou a cópia automática.',
      error,
    )
  }
}

export function downloadPng(blob, filename = createFilename()) {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  link.click()
  setTimeout(() => URL.revokeObjectURL(url), 1_000)
}

export function createFilename(date = new Date()) {
  const parts = new Intl.DateTimeFormat('pt-BR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false,
  }).formatToParts(date)

  const values = Object.fromEntries(parts.map(({ type, value }) => [type, value]))
  return `captura-${values.year}-${values.month}-${values.day}_${values.hour}-${values.minute}-${values.second}.png`
}

function waitForVideo(video) {
  return new Promise((resolve, reject) => {
    const timeout = setTimeout(() => reject(new Error('Tempo limite da captura excedido.')), 8_000)

    const finish = async () => {
      try {
        await video.play()
        await waitForRenderedFrame(video)
        clearTimeout(timeout)
        resolve()
      } catch (error) {
        clearTimeout(timeout)
        reject(error)
      }
    }

    if (video.readyState >= HTMLMediaElement.HAVE_METADATA) {
      finish()
      return
    }

    video.addEventListener('loadedmetadata', finish, { once: true })
    video.addEventListener(
      'error',
      () => {
        clearTimeout(timeout)
        reject(new Error('O navegador não conseguiu preparar a prévia.'))
      },
      { once: true },
    )
  })
}

function waitForRenderedFrame(video) {
  if ('requestVideoFrameCallback' in video) {
    return new Promise((resolve) => {
      video.requestVideoFrameCallback(() => resolve())
    })
  }

  return new Promise((resolve) => {
    requestAnimationFrame(() => requestAnimationFrame(resolve))
  })
}

function videoFrameToPng(video) {
  const canvas = document.createElement('canvas')
  canvas.width = video.videoWidth
  canvas.height = video.videoHeight

  const context = canvas.getContext('2d', { alpha: false })
  context.drawImage(video, 0, 0, canvas.width, canvas.height)

  return new Promise((resolve, reject) => {
    canvas.toBlob((blob) => {
      if (blob) {
        resolve(blob)
      } else {
        reject(new Error('Não foi possível converter a captura em PNG.'))
      }
    }, 'image/png')
  })
}

function createCaptureError(code, message, cause) {
  const error = new Error(message, cause ? { cause } : undefined)
  error.code = code
  return error
}
