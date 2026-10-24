<script lang="ts">
  import { getApi } from '../api/context'
  import type { FileInfo } from '../api/MayflyApi'
  import { formatDate, formatSize, formatTimeLeft } from '../format'

  let { id, info, password }: { id: string; info: FileInfo; password?: string } = $props()

  const api = getApi()
  let downloading = $state(false)
  let downloadError = $state<string | null>(null)

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

  const extension = $derived(/\.([^.]{1,5})$/.exec(info.name)?.[1].toUpperCase() ?? '')
</script>

<article class="card" aria-labelledby="file-name">
  <p class="address">{location.host}/<span>{id}</span></p>

  <div class="title">
    <div class="badge" aria-hidden="true">{extension}</div>
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
    <span class="label">SHA-256</span>
    <code>{info.sha256}</code>
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
    flex-wrap: wrap;
    align-items: center;
    gap: 16px 20px;
  }

  .badge {
    flex: 0 0 auto;
    width: 64px;
    height: 76px;
    display: flex;
    align-items: flex-end;
    justify-content: center;
    padding-bottom: 10px;
    border-radius: 12px;
    background: var(--surface-2);
    font: 600 12px var(--font-mono);
    color: var(--text-muted);
  }

  .name {
    flex: 1 1 260px;
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
    gap: 6px;
    padding-top: 20px;
    border-top: 1px solid var(--border);
  }

  code {
    font-size: 13px;
    line-height: 1.55;
    color: var(--text-muted);
    overflow-wrap: anywhere;
  }
</style>
