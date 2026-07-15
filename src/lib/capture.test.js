import { afterEach, describe, expect, it, vi } from 'vitest'
import { copyPngToClipboard, createFilename, getCaptureSupport } from './capture.js'

describe('createFilename', () => {
  it('cria um nome de arquivo PNG ordenável', () => {
    const date = new Date(2026, 6, 15, 14, 5, 9)
    expect(createFilename(date)).toBe('captura-2026-07-15_14-05-09.png')
  })
})

describe('clipboard', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('detecta suporte e escreve o PNG na área de transferência', async () => {
    const write = vi.fn().mockResolvedValue(undefined)
    class ClipboardItemMock {
      constructor(data) {
        this.data = data
      }
    }

    vi.stubGlobal('window', { isSecureContext: true, ClipboardItem: ClipboardItemMock })
    vi.stubGlobal('ClipboardItem', ClipboardItemMock)
    vi.stubGlobal('navigator', {
      mediaDevices: { getDisplayMedia: vi.fn() },
      clipboard: { write },
    })

    expect(getCaptureSupport()).toEqual({
      secureContext: true,
      displayMedia: true,
      clipboardImage: true,
    })

    const blob = new Blob(['png'], { type: 'image/png' })
    await copyPngToClipboard(blob)

    expect(write).toHaveBeenCalledOnce()
    expect(write.mock.calls[0][0][0].data['image/png']).toBe(blob)
  })
})
