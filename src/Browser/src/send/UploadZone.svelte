<script lang="ts">
  import Upload from '@lucide/svelte/icons/upload'
  import FileIcon from '../file/FileIcon.svelte'
  import { formatSize } from '../format'
  import Button from '../ui/Button.svelte'
  import { selectedFile } from './selectedFile.svelte'

  let input: HTMLInputElement
  let dragging = $state(false)

  function choose(files: FileList | null | undefined) {
    if (files !== null && files !== undefined && files.length > 0) {
      selectedFile.select(files[0])
    }
  }

  function pick() {
    choose(input.files)
    input.value = ''
  }

  function dragOver(event: DragEvent) {
    event.preventDefault()
    dragging = true
  }

  function drop(event: DragEvent) {
    event.preventDefault()
    dragging = false
    choose(event.dataTransfer?.files)
  }

  function paste(event: ClipboardEvent) {
    if (event.clipboardData !== null && event.clipboardData.files.length > 0) {
      event.preventDefault()
      choose(event.clipboardData.files)
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
  <input bind:this={input} type="file" hidden onchange={pick} />

  {#if selectedFile.current === null}
    <span class="mark" aria-hidden="true">
      <Upload size={28} strokeWidth={2} />
    </span>
    <div class="text">
      <h2>Перетащите файл</h2>
      <p>или вставьте из буфера обмена</p>
    </div>
    <Button variant="primary" onclick={() => input.click()}>Выбрать файл</Button>
  {:else}
    <FileIcon name={selectedFile.current.name} contentType={selectedFile.current.type} />
    <div class="text">
      <h2>{selectedFile.current.name}</h2>
      <p>{formatSize(selectedFile.current.size)}</p>
    </div>
    <Button onclick={() => input.click()}>Выбрать другой</Button>
  {/if}
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
