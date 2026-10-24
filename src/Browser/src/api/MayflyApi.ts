export interface FileInfo {
  name: string
  size: number
  contentType: string
  sha256: string
  expiresAt: Date | null
  downloadsLeft: number | null
}

export type FileInfoResult =
  | { status: 'found'; info: FileInfo }
  | { status: 'not-found' }
  | { status: 'access-denied' }
  | { status: 'rate-limited'; retryAfterSeconds: number | null }

export class ApiError extends Error {}

let downloadFrame: HTMLIFrameElement | undefined

export class MayflyApi {
  private readonly server: string

  constructor(server: string) {
    this.server = server
  }

  fileUrl(id: string): string {
    return new URL(`/api/files/${id}`, this.server).href
  }

  async downloadProtected(id: string, password: string): Promise<void> {
    if (!('serviceWorker' in navigator)) {
      throw new Error('Service workers are not available.')
    }

    await navigator.serviceWorker.register('/download-worker.js')
    const { active } = await navigator.serviceWorker.ready
    if (active === null) {
      throw new Error('The download worker is not active.')
    }

    const token = crypto.randomUUID()
    const channel = new MessageChannel()
    const answered = new Promise<number>((resolve) => {
      channel.port1.onmessage = (event: MessageEvent<number>) => resolve(event.data)
    })
    active.postMessage({ token, url: this.fileUrl(id), password: encodePassword(password) }, [channel.port2])
    const status = await answered
    if (status !== 200) {
      throw new ApiError(`The server answered ${status}.`)
    }

    downloadFrame?.remove()
    downloadFrame = document.createElement('iframe')
    downloadFrame.hidden = true
    downloadFrame.src = `/download/${token}`
    document.body.append(downloadFrame)
  }

  async getFileInfo(id: string, password?: string): Promise<FileInfoResult> {
    const headers = password === undefined ? undefined : { 'Mayfly-Password': encodePassword(password) }
    const response = await fetch(this.fileUrl(id), { method: 'HEAD', headers })

    switch (response.status) {
      case 200:
        return { status: 'found', info: readFileInfo(response.headers) }
      case 403:
        return { status: 'access-denied' }
      case 404:
        return { status: 'not-found' }
      case 429:
        return { status: 'rate-limited', retryAfterSeconds: readRetryAfter(response.headers.get('Retry-After')) }
      default:
        throw new ApiError(`The server answered ${response.status}.`)
    }
  }
}

function encodePassword(password: string): string {
  return btoa(String.fromCharCode(...new TextEncoder().encode(password)))
}

function readRetryAfter(value: string | null): number | null {
  const seconds = Number(value)
  return value !== null && Number.isSafeInteger(seconds) && seconds >= 0 ? seconds : null
}

function readFileInfo(headers: Headers): FileInfo {
  const name = readFileName(headers.get('Content-Disposition'))
  const size = Number(headers.get('Content-Length'))
  const contentType = headers.get('Content-Type')
  const sha256 = readSha256(headers.get('Repr-Digest'))
  if (name === null || !Number.isSafeInteger(size) || contentType === null || sha256 === null) {
    throw new ApiError('The server answered without the file headers.')
  }

  return {
    name,
    size,
    contentType,
    sha256,
    expiresAt: readExpiresAt(headers.get('Mayfly-Expires-At')),
    downloadsLeft: readDownloadsLeft(headers.get('Mayfly-Downloads-Left')),
  }
}

function readFileName(contentDisposition: string | null): string | null {
  if (contentDisposition === null) {
    return null
  }

  const encoded = /filename\*\s*=\s*UTF-8''([^;]+)/i.exec(contentDisposition)
  if (encoded !== null) {
    return decodeURIComponent(encoded[1].trim())
  }

  const quoted = /filename\s*=\s*"([^"]*)"/i.exec(contentDisposition)
  return quoted === null ? null : quoted[1]
}

function readSha256(reprDigest: string | null): string | null {
  const entry = reprDigest === null ? null : /sha-256=:([A-Za-z0-9+/]+=*):/.exec(reprDigest)
  if (entry === null) {
    return null
  }

  const bytes = Uint8Array.from(atob(entry[1]), (char) => char.charCodeAt(0))
  return Array.from(bytes, (byte) => byte.toString(16).padStart(2, '0')).join('')
}

function readExpiresAt(value: string | null): Date | null {
  if (value === null) {
    return null
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    throw new ApiError('The server answered with an invalid Mayfly-Expires-At.')
  }

  return date
}

function readDownloadsLeft(value: string | null): number | null {
  if (value === null) {
    return null
  }

  const count = Number(value)
  if (!Number.isSafeInteger(count) || count < 0) {
    throw new ApiError('The server answered with an invalid Mayfly-Downloads-Left.')
  }

  return count
}
