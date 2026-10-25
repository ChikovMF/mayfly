const visibleMilliseconds = 2500

let message = $state<string | null>(null)
let timer: ReturnType<typeof setTimeout> | undefined

export const toast = {
  get message(): string | null {
    return message
  },
  show(text: string) {
    message = text
    clearTimeout(timer)
    timer = setTimeout(() => (message = null), visibleMilliseconds)
  },
}

export async function copyToClipboard(text: string, copied: string) {
  if (await copyText(text)) {
    toast.show(copied)
  } else {
    toast.show('Не удалось скопировать')
  }
}

/** Copies the text; `navigator.clipboard` exists only in a secure context, so over plain http it falls back to `execCommand`. */
export async function copyText(text: string): Promise<boolean> {
  try {
    await navigator.clipboard.writeText(text)
    return true
  } catch {
    const field = document.createElement('textarea')
    field.value = text
    field.style.position = 'fixed'
    field.style.opacity = '0'
    document.body.append(field)
    field.select()
    try {
      return document.execCommand('copy')
    } catch {
      return false
    } finally {
      field.remove()
    }
  }
}
