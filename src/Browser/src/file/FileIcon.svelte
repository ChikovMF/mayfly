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
  import { fileExtension, fileKind, type FileKind } from './fileKind'

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
  const label = $derived(fileExtension(name).slice(0, 5).toUpperCase())
</script>

<div class="icon {kind}" aria-hidden="true">
  <Icon size={24} strokeWidth={1.7} />
  <span>{label}</span>
</div>

<style>
  .icon {
    --kind: var(--text-muted);
    flex: 0 0 auto;
    width: 48px;
    height: 56px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 3px;
    border-radius: 10px;
    background: color-mix(in srgb, var(--kind) 14%, transparent);
    color: var(--kind);
  }

  span {
    height: 11px;
    font: 700 9px/11px var(--font-mono);
  }

  .image {
    --kind: light-dark(#1d8a5f, #4cc38a);
  }

  .video {
    --kind: light-dark(#7c4dbd, #b794f4);
  }

  .audio {
    --kind: light-dark(#c2417c, #f08ab8);
  }

  .archive {
    --kind: light-dark(#a8660a, #f5b54a);
  }

  .pdf {
    --kind: light-dark(#c4372d, #ff8a7e);
  }

  .document {
    --kind: light-dark(#2f6fc2, #6ea8f0);
  }

  .table {
    --kind: light-dark(#2b8a3e, #6cc57c);
  }

  .presentation {
    --kind: light-dark(#c25b12, #f29a5c);
  }

  .code {
    --kind: light-dark(#4a5a78, #9db0d4);
  }
</style>
