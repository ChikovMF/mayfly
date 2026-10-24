import type { MayflyApi } from '../api/MayflyApi'
import type { Selection } from './selection.svelte'
import { settings } from './settings.svelte'
import { sha256OfFile } from './sha256'
import { describeFailure, type UploadFailure } from './uploadFailure'
import { createZip } from './zip'

export type UploadPhase = 'idle' | 'archiving' | 'hashing' | 'sending' | 'saving' | 'done' | 'failed'

export interface SentFile {
  id: string
  name: string
  size: number
  expiresAt: Date | null
  maxDownloads: number | null
  password: string | null
}

export interface UploadRequest {
  expiresInMinutes: number | null
  maxDownloads: number | null
  password: string | null
}

const speedWindowMilliseconds = 3000

class Upload {
  phase = $state<UploadPhase>('idle')
  name = $state('')
  totalBytes = $state(0)
  archived = $state(false)
  sentBytes = $state(0)
  bytesPerSecond = $state(0)
  sent = $state<SentFile | null>(null)
  failure = $state<UploadFailure | null>(null)

  private controller: AbortController | null = null
  private last: { api: MayflyApi; selection: Selection; request: UploadRequest } | null = null
  private samples: { time: number; bytes: number }[] = []

  get active(): boolean {
    return this.phase === 'archiving' || this.phase === 'hashing' || this.phase === 'sending' || this.phase === 'saving'
  }

  async start(api: MayflyApi, selection: Selection, request: UploadRequest) {
    const controller = new AbortController()
    this.controller = controller
    this.last = { api, selection, request }
    this.sent = null
    this.failure = null
    this.archived = selection.kind === 'archive'
    this.name = selection.kind === 'file' ? selection.file.name : selection.name
    this.totalBytes = selection.kind === 'file' ? selection.file.size : selection.size
    this.restartCount()

    try {
      let file: File
      if (selection.kind === 'file') {
        file = selection.file
      } else {
        this.phase = 'archiving'
        file = await createZip(selection.name, selection.entries, (bytes) => this.count(bytes), controller.signal)
        this.totalBytes = file.size
        this.restartCount()
      }

      this.phase = 'hashing'
      const sha256 = await sha256OfFile(file, (bytes) => this.count(bytes), controller.signal)

      this.phase = 'sending'
      this.restartCount()
      const uploaded = await api.upload(
        file,
        {
          expiresAt: request.expiresInMinutes === null ? null : new Date(Date.now() + request.expiresInMinutes * 60_000),
          maxDownloads: request.maxDownloads,
          password: request.password,
          sha256,
        },
        (bytes) => {
          this.count(bytes)
          if (bytes >= file.size) {
            this.phase = 'saving'
          }
        },
        controller.signal,
      )

      this.sent = {
        id: uploaded.id,
        name: file.name,
        size: file.size,
        expiresAt: uploaded.expiresAt,
        maxDownloads: uploaded.maxDownloads,
        password: request.password,
      }
      this.phase = 'done'
    } catch (exception) {
      if (controller.signal.aborted) {
        this.phase = 'idle'
        return
      }

      this.failure = describeFailure(exception, {
        name: this.name,
        sentPercent: this.phase === 'sending' && this.totalBytes > 0 ? Math.floor((this.sentBytes / this.totalBytes) * 100) : null,
      })
      this.phase = 'failed'
    }
  }

  retry() {
    if (this.last !== null) {
      void this.start(this.last.api, this.last.selection, this.last.request)
    }
  }

  dismissFailure() {
    this.phase = 'idle'
    this.failure = null
  }

  cancel() {
    this.controller?.abort()
  }

  reset() {
    this.phase = 'idle'
    this.sent = null
    this.failure = null
    settings.clearPassword()
  }

  private restartCount() {
    this.sentBytes = 0
    this.bytesPerSecond = 0
    this.samples = []
  }

  private count(bytes: number) {
    const now = performance.now()
    this.sentBytes = bytes
    this.samples.push({ time: now, bytes })
    while (this.samples.length > 2 && now - this.samples[0].time > speedWindowMilliseconds) {
      this.samples.shift()
    }

    const first = this.samples[0]
    const seconds = (now - first.time) / 1000
    this.bytesPerSecond = seconds > 0 ? (bytes - first.bytes) / seconds : 0
  }
}

export const upload = new Upload()
