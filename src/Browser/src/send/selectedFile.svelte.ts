let file = $state<File | null>(null)

export const selectedFile = {
  get current(): File | null {
    return file
  },
  select(next: File) {
    file = next
  },
}
