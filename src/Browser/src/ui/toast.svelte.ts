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
  try {
    await navigator.clipboard.writeText(text)
    toast.show(copied)
  } catch {
    toast.show('Не удалось скопировать')
  }
}
