export interface FileInfo {
  name: string
  size: number
  contentType: string
  sha256: string
  expiresAt: Date | null
  downloadsLeft: number | null
}

export type FileInfoResult = { status: 'found'; info: FileInfo } | { status: 'not-found' } | { status: 'access-denied' }

export class ApiError extends Error {}

export class MayflyApi {
  private readonly server: string

  constructor(server: string) {
    this.server = server
  }

  async getFileInfo(id: string): Promise<FileInfoResult> {
    const response = await fetch(new URL(`/api/files/${id}`, this.server), { method: 'HEAD' })

    switch (response.status) {
      case 200:
        return { status: 'found', info: readFileInfo(response.headers) }
      case 403:
        return { status: 'access-denied' }
      case 404:
        return { status: 'not-found' }
      default:
        throw new ApiError(`The server answered ${response.status}.`)
    }
  }
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
