<script lang="ts">
  import DownloadIcon from '@lucide/svelte/icons/download'
  import { repository } from '../repository'
  import { detectSystem, systems } from './platforms'
  import SystemIcon from './SystemIcon.svelte'

  const current = detectSystem()
  const downloadUrl = (archive: string) => `${repository}/releases/latest/download/${archive}`
</script>

<div class="page">
  <section class="card" aria-labelledby="console-title">
    <div class="intro">
      <h1 id="console-title">Консольный клиент</h1>
      <p>Отправляйте и скачивайте файлы из терминала. Скачайте последнюю версию для своей системы.</p>
    </div>

    <ul class="systems">
      {#each systems as system (system.id)}
        <li class:current={system.id === current}>
          <span class="logo"><SystemIcon system={system.id} /></span>
          <h2>{system.name}</h2>
          <div class="buttons">
            {#each system.builds as build (build.archive)}
              <a href={downloadUrl(build.archive)} aria-label="Скачать {system.name} {build.label}">
                <DownloadIcon size={18} aria-hidden="true" />
                {build.label}
              </a>
            {/each}
          </div>
        </li>
      {/each}
    </ul>

    <a class="docs" href="{repository}/blob/master/docs/terminal.md">Документация</a>
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
    max-width: 800px;
    display: flex;
    flex-direction: column;
    gap: 28px;
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

  .systems {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(190px, 1fr));
    gap: 14px;
    margin: 0;
    padding: 0;
    list-style: none;
  }

  li {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 14px;
    padding: 24px 16px 16px;
    border: 1px solid var(--border);
    border-radius: 18px;
    background: var(--bg);
    text-align: center;
  }

  li.current {
    border-color: var(--accent);
  }

  .logo {
    display: flex;
    color: var(--text);
  }

  h2 {
    margin: 0;
    font-size: 18px;
    font-weight: 700;
  }

  .buttons {
    display: flex;
    width: 100%;
    margin-top: auto;
    gap: 2px;
  }

  .buttons a {
    flex: 1;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    height: 44px;
    padding: 0 12px;
    background: var(--accent);
    color: var(--on-accent);
    font: 600 15px var(--font-sans);
    text-decoration: none;
    transition: background-color 120ms;
  }

  .buttons a:first-child {
    border-radius: 12px 0 0 12px;
  }

  .buttons a:last-child {
    border-radius: 0 12px 12px 0;
  }

  .buttons a:only-child {
    border-radius: 12px;
  }

  .buttons a:hover {
    background: var(--accent-hover);
  }

  .buttons a:focus-visible {
    outline: 2px solid var(--focus-ring);
    outline-offset: 2px;
  }

  .docs {
    align-self: flex-start;
    display: inline-flex;
    align-items: center;
    min-height: 44px;
    color: var(--text-muted);
  }
</style>
