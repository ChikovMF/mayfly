import type { ZipEntry } from './zip'

export type Selection =
  | { kind: 'file'; file: File }
  | { kind: 'archive'; name: string; entries: ZipEntry[]; size: number }

const archiveName = 'файлы.zip'

let current = $state<Selection | null>(null)

export function selectionFromFiles(files: File[]): Selection | null {
  if (files.length === 0) {
    return null
  }

  if (files.length === 1) {
    return { kind: 'file', file: files[0] }
  }

  return {
    kind: 'archive',
    name: archiveName,
    entries: files.map((file) => ({ path: file.name, file })),
    size: files.reduce((sum, file) => sum + file.size, 0),
  }
}

export const selection = {
  get current(): Selection | null {
    return current
  },
  select(next: Selection | null) {
    if (next !== null) {
      current = next
    }
  },
  clear() {
    current = null
  },
  pickFiles() {
    const input = document.createElement('input')
    input.type = 'file'
    input.multiple = true
    input.onchange = () => selection.select(selectionFromFiles([...(input.files ?? [])]))
    input.click()
  },
}
