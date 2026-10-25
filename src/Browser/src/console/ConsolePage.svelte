<script lang="ts">
  import { repository } from '../repository'
  import ChoiceGroup from '../ui/ChoiceGroup.svelte'
  import { archiveName, detectPlatform, platforms, type Platform } from './platforms'

  let platform = $state<Platform>(detectPlatform())

  const archive = $derived(archiveName(platform))
  const downloadUrl = $derived(`${repository}/releases/latest/download/${archive}`)
</script>

<div class="page">
  <section class="card" aria-labelledby="console-title">
    <div class="intro">
      <h1 id="console-title">Консольный клиент</h1>
      <p>Отправляйте и скачивайте файлы из терминала. Скачайте последнюю версию для своей системы.</p>
    </div>

    <div class="field">
      <span id="platform-label">Платформа</span>
      <ChoiceGroup labelledby="platform-label" name="platform" options={platforms} bind:value={platform} />
    </div>

    <div class="actions">
      <a class="download" href={downloadUrl}>Скачать {archive}</a>
      <a class="docs" href="{repository}/blob/master/docs/terminal.md">Документация</a>
    </div>
  </section>
</div>

<style>
  .page {
    display: flex;
    justify-content: center;
    padding: clamp(28px, 6vw, 60px) 0 48px;
  }

  .card {
    width: 100%;
    max-width: 640px;
    display: flex;
    flex-direction: column;
    gap: 24px;
    padding: clamp(22px, 4vw, 36px);
    border: 1px solid var(--border);
    border-radius: 24px;
    background: var(--surface);
  }

  .intro {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  h1 {
    margin: 0;
    font-size: clamp(24px, 3.6vw, 34px);
    font-weight: 700;
    letter-spacing: -0.03em;
  }

  p {
    margin: 0;
    font-size: 15px;
    color: var(--text-muted);
  }

  .field {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  .field > span {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-muted);
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 12px 24px;
  }

  .download {
    display: inline-flex;
    align-items: center;
    height: 48px;
    padding: 0 20px;
    border-radius: 12px;
    background: var(--accent);
    color: var(--on-accent);
    font: 600 15px var(--font-sans);
    text-decoration: none;
  }

  .download:hover {
    background: var(--accent-hover);
  }

  .docs {
    display: inline-flex;
    align-items: center;
    min-height: 44px;
    color: var(--text-muted);
  }
</style>
