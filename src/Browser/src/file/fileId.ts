const fileIdLength = 10

const fileIdAlphabet = /^[1-9A-HJ-NP-Za-km-z]*$/

export function isFileId(value: string): boolean {
  return value.length === fileIdLength && fileIdAlphabet.test(value)
}
