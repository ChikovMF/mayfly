let file = $state<File | null>(null)

export const selectedFile = {
  get current(): File | null {
    return file
  },
  select(next: File) {
    file = next
  },
  clear() {
    file = null
  },
  pick() {
    const input = document.createElement('input')
    input.type = 'file'
    input.onchange = () => {
      if (input.files !== null && input.files.length > 0) {
        file = input.files[0]
      }
    }
    input.click()
  },
}
