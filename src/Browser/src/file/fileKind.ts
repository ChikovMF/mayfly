export type FileKind =
  | 'image'
  | 'video'
  | 'audio'
  | 'archive'
  | 'pdf'
  | 'document'
  | 'table'
  | 'presentation'
  | 'code'
  | 'text'
  | 'other'

const extensions: Record<Exclude<FileKind, 'other'>, string[]> = {
  image: ['jpg', 'jpeg', 'png', 'gif', 'webp', 'svg', 'bmp', 'ico', 'avif', 'heic', 'tif', 'tiff'],
  video: ['mp4', 'mov', 'mkv', 'avi', 'webm', 'm4v', 'wmv'],
  audio: ['mp3', 'wav', 'flac', 'ogg', 'm4a', 'aac', 'opus'],
  archive: ['zip', 'rar', '7z', 'tar', 'gz', 'tgz', 'bz2', 'xz'],
  pdf: ['pdf'],
  document: ['doc', 'docx', 'odt', 'rtf'],
  table: ['xls', 'xlsx', 'ods', 'csv'],
  presentation: ['ppt', 'pptx', 'odp', 'key'],
  code: ['js', 'ts', 'json', 'html', 'css', 'xml', 'yml', 'yaml', 'py', 'cs', 'java', 'go', 'rs', 'c', 'cpp', 'h', 'sh', 'sql', 'svelte'],
  text: ['txt', 'md', 'log'],
}

export function fileExtension(name: string): string {
  return /\.([^.]+)$/.exec(name)?.[1].toLowerCase() ?? ''
}

export function fileKind(name: string, contentType: string): FileKind {
  const extension = fileExtension(name)
  for (const [kind, known] of Object.entries(extensions)) {
    if (known.includes(extension)) {
      return kind as FileKind
    }
  }

  if (contentType.startsWith('image/')) return 'image'
  if (contentType.startsWith('video/')) return 'video'
  if (contentType.startsWith('audio/')) return 'audio'
  if (contentType.startsWith('text/')) return 'text'
  return 'other'
}
