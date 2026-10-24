<script lang="ts">
  import Button from '../ui/Button.svelte'
  import { selection } from './selection.svelte'
  import { upload } from './upload.svelte'
  import type { UploadFailure } from './uploadFailure'

  let { failure }: { failure: UploadFailure } = $props()

  function chooseAnother() {
    selection.clear()
    upload.dismissFailure()
  }
</script>

<div class="page">
  <section aria-labelledby="failure-title">
    <span class="code">{failure.code}</span>
    <div class="text">
      <h1 id="failure-title">{failure.title}</h1>
      <p>{failure.text}</p>
    </div>
    <div class="buttons">
      {#if failure.again}
        <Button variant="primary" onclick={() => upload.retry()}>Повторить</Button>
      {/if}
      {#if failure.chooseAnother}
        <Button variant="primary" onclick={chooseAnother}>Выбрать другой</Button>
      {:else}
        <Button variant={failure.again ? 'secondary' : 'primary'} onclick={() => upload.dismissFailure()}>К настройкам</Button>
      {/if}
    </div>
  </section>
</div>

<style>
  .page {
    display: flex;
    justify-content: center;
    padding: clamp(28px, 6vw, 64px) 0 48px;
  }

  section {
    width: 100%;
    max-width: 560px;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 20px;
    padding: clamp(22px, 4vw, 36px);
    border: 1px solid var(--border);
    border-radius: 24px;
    background: var(--surface);
  }

  .code {
    display: inline-flex;
    align-items: center;
    height: 28px;
    padding: 0 12px;
    border-radius: 999px;
    background: color-mix(in srgb, var(--danger) 14%, transparent);
    color: var(--danger);
    font: 600 13px var(--font-mono);
  }

  .text {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  h1 {
    margin: 0;
    font-size: clamp(24px, 3.4vw, 30px);
    font-weight: 700;
    letter-spacing: -0.03em;
  }

  p {
    margin: 0;
    font-size: 15px;
    line-height: 1.55;
    color: var(--text-muted);
  }

  .buttons {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  }
</style>
