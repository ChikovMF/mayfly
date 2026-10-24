<script lang="ts">
  import Button from '../ui/Button.svelte'
  import { selectedFile } from './selectedFile.svelte'
  import { upload } from './upload.svelte'

  let { id }: { id: string } = $props()

  function sendAnother() {
    selectedFile.clear()
    upload.reset()
  }
</script>

<div class="page">
  <h1>Файл отправлен</h1>
  <a href="/{id}">{location.host}/{id}</a>
  <Button onclick={sendAnother}>Отправить ещё</Button>
</div>

<style>
  .page {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
    padding: clamp(28px, 6vw, 64px) 0 48px;
  }

  h1 {
    margin: 0;
    font-size: 28px;
  }

  a {
    font: 500 17px var(--font-mono);
    color: var(--text);
  }
</style>
