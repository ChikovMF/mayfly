<script lang="ts">
  import Upload from '@lucide/svelte/icons/upload'
  import Button from '../ui/Button.svelte'
  import { selection, selectionFromFiles } from './selection.svelte'

  let dragging = $state(false)

  function dragOver(event: DragEvent) {
    event.preventDefault()
    dragging = true
  }

  function drop(event: DragEvent) {
    event.preventDefault()
    dragging = false
    const files = [...(event.dataTransfer?.items ?? [])]
      .filter((item) => item.kind === 'file' && item.webkitGetAsEntry()?.isDirectory !== true)
      .map((item) => item.getAsFile())
      .filter((file) => file !== null)
    selection.select(selectionFromFiles(files))
  }

  function paste(event: ClipboardEvent) {
    if (event.clipboardData !== null && event.clipboardData.files.length > 0) {
      event.preventDefault()
      selection.select(selectionFromFiles([...event.clipboardData.files]))
    }
  }
</script>

<svelte:window onpaste={paste} />

<section
  aria-label="Отправить файл"
  class:dragging
  ondragover={dragOver}
  ondragleave={() => (dragging = false)}
  ondrop={drop}
>
  <span class="mark" aria-hidden="true">
    <Upload size={28} strokeWidth={2} />
  </span>
  <div class="text">
    <h2>Перетащите файлы</h2>
    <p>или вставьте из буфера обмена</p>
  </div>
  <Button variant="primary" onclick={selection.pickFiles}>Выбрать файлы</Button>
</section>

<style>
  section {
    flex: 2 1 520px;
    min-width: 0;
    min-height: 340px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 20px;
    padding: 40px 24px;
    border: 1.5px dashed var(--border-strong);
    border-radius: 22px;
    background: var(--surface);
    text-align: center;
    transition:
      border-color 120ms,
      background-color 120ms;
  }

  section.dragging {
    border-color: var(--accent);
    background: var(--accent-soft);
  }

  .mark {
    width: 64px;
    height: 64px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 18px;
    background: var(--accent);
    color: var(--on-accent);
  }

  .text {
    max-width: 100%;
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  h2 {
    margin: 0;
    font-size: 24px;
    font-weight: 650;
    letter-spacing: -0.02em;
    overflow-wrap: anywhere;
  }

  p {
    margin: 0;
    font-size: 15px;
    color: var(--text-muted);
  }
</style>
