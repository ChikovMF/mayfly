export interface ZipEntry {
  path: string
  file: File
}

export class ZipTooLargeError extends Error {}

const maxField = 0xffffffff
const maxEntries = 0xffff
const crcTable = buildCrcTable()

export async function createZip(
  name: string,
  entries: ZipEntry[],
  onProgress: (bytesRead: number) => void,
  signal?: AbortSignal,
): Promise<File> {
  if (entries.length > maxEntries) {
    throw new ZipTooLargeError('Too many files for a zip archive.')
  }

  const parts: BlobPart[] = []
  const directory: Uint8Array<ArrayBuffer>[] = []
  const encoder = new TextEncoder()
  let offset = 0
  let bytesRead = 0

  for (const { path, file } of entries) {
    if (file.size > maxField) {
      throw new ZipTooLargeError('A file is too large for a zip archive.')
    }

    const crc = await crc32OfFile(file, (bytes) => onProgress(bytesRead + bytes), signal)
    bytesRead += file.size

    const encodedPath = encoder.encode(path)
    const { time, date } = dosDateTime(new Date(file.lastModified))
    const header = localHeader(encodedPath, crc, file.size, time, date)
    directory.push(directoryHeader(encodedPath, crc, file.size, time, date, offset))
    parts.push(header, file)
    offset += header.byteLength + file.size
  }

  const directorySize = directory.reduce((sum, record) => sum + record.byteLength, 0)
  if (offset + directorySize > maxField) {
    throw new ZipTooLargeError('The zip archive is too large.')
  }

  parts.push(...directory, endRecord(entries.length, directorySize, offset))
  return new File(parts, name, { type: 'application/zip' })
}

async function crc32OfFile(file: File, onProgress: (bytesRead: number) => void, signal?: AbortSignal): Promise<number> {
  const reader = file.stream().getReader()
  let crc = 0xffffffff
  let bytesRead = 0

  try {
    for (;;) {
      signal?.throwIfAborted()
      const { done, value } = await reader.read()
      if (done) {
        break
      }

      crc = updateCrc(crc, value)
      bytesRead += value.byteLength
      onProgress(bytesRead)
    }
  } finally {
    await reader.cancel().catch(() => {})
  }

  if (bytesRead !== file.size) {
    throw new Error('The file changed while it was being read.')
  }

  return (crc ^ 0xffffffff) >>> 0
}

function updateCrc(crc: number, bytes: Uint8Array): number {
  for (let index = 0; index < bytes.length; index++) {
    crc = crcTable[(crc ^ bytes[index]) & 0xff] ^ (crc >>> 8)
  }

  return crc
}

function unicodePathField(name: Uint8Array): Uint8Array<ArrayBuffer> {
  const bytes = new Uint8Array(9 + name.length)
  const view = new DataView(bytes.buffer)
  view.setUint16(0, 0x7075, true)
  view.setUint16(2, 5 + name.length, true)
  view.setUint8(4, 1)
  view.setUint32(5, (updateCrc(0xffffffff, name) ^ 0xffffffff) >>> 0, true)
  bytes.set(name, 9)
  return bytes
}

function localHeader(
  name: Uint8Array,
  crc: number,
  size: number,
  time: number,
  date: number,
): Uint8Array<ArrayBuffer> {
  const extra = unicodePathField(name)
  const bytes = new Uint8Array(30 + name.length + extra.length)
  const view = new DataView(bytes.buffer)
  view.setUint32(0, 0x04034b50, true)
  view.setUint16(4, 20, true)
  view.setUint16(6, 0x0800, true)
  view.setUint16(10, time, true)
  view.setUint16(12, date, true)
  view.setUint32(14, crc, true)
  view.setUint32(18, size, true)
  view.setUint32(22, size, true)
  view.setUint16(26, name.length, true)
  view.setUint16(28, extra.length, true)
  bytes.set(name, 30)
  bytes.set(extra, 30 + name.length)
  return bytes
}

function directoryHeader(
  name: Uint8Array,
  crc: number,
  size: number,
  time: number,
  date: number,
  offset: number,
): Uint8Array<ArrayBuffer> {
  const extra = unicodePathField(name)
  const bytes = new Uint8Array(46 + name.length + extra.length)
  const view = new DataView(bytes.buffer)
  view.setUint32(0, 0x02014b50, true)
  view.setUint16(4, 20, true)
  view.setUint16(6, 20, true)
  view.setUint16(8, 0x0800, true)
  view.setUint16(12, time, true)
  view.setUint16(14, date, true)
  view.setUint32(16, crc, true)
  view.setUint32(20, size, true)
  view.setUint32(24, size, true)
  view.setUint16(28, name.length, true)
  view.setUint16(30, extra.length, true)
  view.setUint32(42, offset, true)
  bytes.set(name, 46)
  bytes.set(extra, 46 + name.length)
  return bytes
}

function endRecord(count: number, directorySize: number, directoryOffset: number): Uint8Array<ArrayBuffer> {
  const bytes = new Uint8Array(22)
  const view = new DataView(bytes.buffer)
  view.setUint32(0, 0x06054b50, true)
  view.setUint16(8, count, true)
  view.setUint16(10, count, true)
  view.setUint32(12, directorySize, true)
  view.setUint32(16, directoryOffset, true)
  return bytes
}

function dosDateTime(moment: Date): { time: number; date: number } {
  const year = Math.max(moment.getFullYear(), 1980)
  return {
    time: (moment.getHours() << 11) | (moment.getMinutes() << 5) | (moment.getSeconds() >> 1),
    date: ((year - 1980) << 9) | ((moment.getMonth() + 1) << 5) | moment.getDate(),
  }
}

function buildCrcTable(): Uint32Array {
  const table = new Uint32Array(256)
  for (let index = 0; index < 256; index++) {
    let value = index
    for (let bit = 0; bit < 8; bit++) {
      value = value & 1 ? 0xedb88320 ^ (value >>> 1) : value >>> 1
    }

    table[index] = value >>> 0
  }

  return table
}
