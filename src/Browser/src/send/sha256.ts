import { sha256 } from '@noble/hashes/sha2.js'

export async function sha256OfFile(
  file: File,
  onProgress: (bytesRead: number) => void,
  signal?: AbortSignal,
): Promise<string> {
  const hash = sha256.create()
  const reader = file.stream().getReader()
  let bytesRead = 0

  try {
    for (;;) {
      signal?.throwIfAborted()
      const { done, value } = await reader.read()
      if (done) {
        break
      }

      hash.update(value)
      bytesRead += value.byteLength
      onProgress(bytesRead)
    }
  } finally {
    await reader.cancel().catch(() => {})
  }

  return btoa(String.fromCharCode(...hash.digest()))
}
