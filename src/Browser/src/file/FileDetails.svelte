<script lang="ts">
  import { getApi } from '../api/context'
  import type { FileInfo } from '../api/MayflyApi'
  import FileIcon from './FileIcon.svelte'
  import { formatDate, formatSize, formatTimeLeft } from '../format'

  let { id, info, password }: { id: string; info: FileInfo; password?: string } = $props()

  const api = getApi()
  let copyState = $state<'idle' | 'copied' | 'failed'>('idle')
  let downloading = $state(false)
  let downloadError = $state<string | null>(null)

  async function copyChecksum() {
    try {
      await navigator.clipboard.writeText(info.sha256)
      copyState = 'copied'
    } catch {
      copyState = 'failed'
    }

    setTimeout(() => (copyState = 'idle'), 2000)
  }

  async function download(password: string) {
    downloadError = null
    downloading = true
    try {
      await api.downloadProtected(id, password)
    } catch {
      downloadError = 'Не удалось начать скачивание'
    } finally {
      downloading = false
    }
  }

</script>

<article class="card" aria-labelledby="file-name">
  <p class="address">{location.host}/<span>{id}</span></p>

  <div class="title">
    <FileIcon name={info.name} contentType={info.contentType} />
    <div class="name">
      <h1 id="file-name">{info.name}</h1>
      <span>{formatSize(info.size)} · {info.contentType}</span>
    </div>
  </div>

  {#if info.expiresAt !== null || info.downloadsLeft !== null}
    <div class="limits">
      {#if info.expiresAt !== null}
        <div class="limit">
          <span class="label">Доступен ещё</span>
          <span class="value">{formatTimeLeft(info.expiresAt)}</span>
          <span class="note">до {formatDate(info.expiresAt)}</span>
        </div>
      {/if}
      {#if info.downloadsLeft !== null}
        <div class="limit">
          <span class="label">Осталось скачиваний</span>
          <span class="value">{info.downloadsLeft}</span>
        </div>
      {/if}
    </div>
  {/if}

  {#if password === undefined}
    <a class="download" href={api.fileUrl(id)}>Скачать</a>
  {:else}
    <button type="button" class="download" disabled={downloading} onclick={() => download(password)}>
      {downloading ? 'Готовим файл…' : 'Скачать'}
    </button>
  {/if}
  {#if downloadError !== null}
    <span class="error" role="alert">{downloadError}</span>
  {/if}

  <div class="checksum">
    <span class="label">Контрольная сумма SHA-256</span>
    <button type="button" class="hash" class:failed={copyState === 'failed'} aria-label="Копировать SHA-256" onclick={copyChecksum}>
      <code>{info.sha256}</code>
      <span class="state">
        <svg width="18" height="18" viewBox="0 0 24 24" aria-hidden="true">
          {#if copyState === 'copied'}
            <path d="M5 12.5l4.5 4.5L19 7.5" />
          {:else if copyState === 'failed'}
            <path d="M6 6l12 12M18 6L6 18" />
          {:else}
            <rect x="9" y="9" width="11" height="11" rx="2" />
            <path d="M5 15V6a2 2 0 0 1 2-2h9" />
          {/if}
        </svg>
        {#if copyState === 'copied'}
          Скопировано
        {:else if copyState === 'failed'}
          Не удалось скопировать
        {/if}
      </span>
    </button>
  </div>
</article>

<style>
  .card {
    width: 100%;
    max-width: 760px;
    display: flex;
    flex-direction: column;
    gap: 26px;
    padding: clamp(20px, 4vw, 36px);
    border: 1px solid var(--border);
    border-radius: 24px;
    background: var(--surface);
  }

  .address {
    margin: 0;
    font: 500 13px var(--font-mono);
    color: var(--text-faint);
    overflow-wrap: anywhere;
  }

  .address span {
    color: var(--text);
  }

  .title {
    display: flex;
    align-items: center;
    gap: 16px;
  }

  .name {
    flex: 1 1 0;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: 6px;
    font-size: 15px;
    color: var(--text-muted);
  }

  h1 {
    margin: 0;
    font-size: clamp(24px, 3.6vw, 34px);
    font-weight: 700;
    letter-spacing: -0.03em;
    color: var(--text);
    overflow-wrap: anywhere;
  }

  .limits {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(240px, 100%), 1fr));
    gap: 16px;
  }

  .limit {
    display: flex;
    flex-direction: column;
    gap: 6px;
    padding: 16px 18px;
    border-radius: 16px;
    background: var(--surface-2);
  }

  .label {
    font-size: 13px;
    color: var(--text-muted);
  }

  .value {
    font-size: 20px;
    font-weight: 650;
    letter-spacing: -0.01em;
  }

  .note {
    font-size: 13px;
    color: var(--text-faint);
  }

  .download {
    align-self: flex-start;
    display: inline-flex;
    align-items: center;
    height: 48px;
    padding: 0 28px;
    border-radius: 12px;
    background: var(--accent);
    color: var(--on-accent);
    border: 0;
    font: 600 15px var(--font-sans);
    text-decoration: none;
    cursor: pointer;
    transition: background-color 120ms;
  }

  .error {
    margin-top: -14px;
    font-size: 14px;
    font-weight: 500;
    color: var(--danger);
  }

  .download:hover:not(:disabled) {
    background: var(--accent-hover);
  }

  .download:disabled {
    background: var(--surface-2);
    color: var(--text-faint);
    cursor: default;
  }

  .checksum {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .hash {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-between;
    gap: 6px 14px;
    width: 100%;
    padding: 12px 14px 12px 16px;
    border: 1px solid transparent;
    border-radius: 14px;
    background: var(--surface-2);
    color: var(--text-muted);
    text-align: left;
    cursor: pointer;
    transition: border-color 120ms;
  }

  .hash:hover {
    border-color: var(--border-strong);
    color: var(--text);
  }

  .hash.failed {
    color: var(--danger);
  }

  .state {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    margin-left: auto;
    font: 500 13px var(--font-sans);
  }

  .state svg {
    fill: none;
    stroke: currentColor;
    stroke-width: 1.9;
    stroke-linecap: round;
    stroke-linejoin: round;
  }

  code {
    flex: 1 1 280px;
    min-width: 0;
    font-size: 13px;
    line-height: 1.55;
    color: inherit;
    overflow-wrap: anywhere;
  }
</style>
