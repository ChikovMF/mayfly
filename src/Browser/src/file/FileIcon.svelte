<script lang="ts">
  import File from '@lucide/svelte/icons/file'
  import FileArchive from '@lucide/svelte/icons/file-archive'
  import FileCode from '@lucide/svelte/icons/file-code'
  import FileImage from '@lucide/svelte/icons/file-image'
  import FileMusic from '@lucide/svelte/icons/file-music'
  import FileSpreadsheet from '@lucide/svelte/icons/file-spreadsheet'
  import FileText from '@lucide/svelte/icons/file-text'
  import FileVideoCamera from '@lucide/svelte/icons/file-video-camera'
  import Presentation from '@lucide/svelte/icons/presentation'
  import { fileKind, type FileKind } from './fileKind'

  let { name, contentType }: { name: string; contentType: string } = $props()

  const icons: Record<FileKind, typeof File> = {
    image: FileImage,
    video: FileVideoCamera,
    audio: FileMusic,
    archive: FileArchive,
    pdf: FileText,
    document: FileText,
    table: FileSpreadsheet,
    presentation: Presentation,
    code: FileCode,
    text: FileText,
    other: File,
  }

  const kind = $derived(fileKind(name, contentType))
  const Icon = $derived(icons[kind])
</script>

<Icon size={32} strokeWidth={1.6} color="var(--text-muted)" aria-hidden="true" />
