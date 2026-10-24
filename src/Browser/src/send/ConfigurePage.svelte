<script lang="ts">
  import { getApi } from '../api/context'
  import ChevronLeft from '@lucide/svelte/icons/chevron-left'
  import X from '@lucide/svelte/icons/x'
  import FileIcon from '../file/FileIcon.svelte'
  import { formatDuration, formatSize } from '../format'
  import Button from '../ui/Button.svelte'
  import DownloadsSection from './DownloadsSection.svelte'
  import ExpirationSection from './ExpirationSection.svelte'
  import PasswordSection from './PasswordSection.svelte'
  import { selectedFile } from './selectedFile.svelte'
  import { settings } from './settings.svelte'
  import { upload } from './upload.svelte'

  let { file }: { file: File } = $props()

  const api = getApi()

  function send() {
    void upload.start(api, file, {
      expiresInMinutes: settings.expiresInMinutes,
      maxDownloads: settings.maxDownloads,
      password: settings.passwordValue,
    })
  }

  const expiration = $derived(
    settings.expiration === 'never'
      ? 'бессрочно'
      : settings.expiresInMinutes === null
        ? 'не указан'
        : formatDuration(settings.expiresInMinutes),
  )
  const downloads = $derived(
    settings.downloads === 'unlimited' ? 'без ограничений' : (settings.maxDownloads ?? 'не указан'),
  )
</script>

<div class="page">
  <button type="button" class="back" onclick={selectedFile.clear}>
    <ChevronLeft size={18} strokeWidth={1.9} aria-hidden="true" />
    Назад
  </button>

  <div class="layout">
    <div class="cards">
      <section class="file" aria-label="Выбранный файл">
        <FileIcon name={file.name} contentType={file.type} />
        <div class="info">
          <span class="name">{file.name}</span>
          <span class="meta">{formatSize(file.size)}{file.type === '' ? '' : ` · ${file.type}`}</span>
        </div>
        <Button onclick={selectedFile.pick}>Заменить</Button>
        <button type="button" class="remove" aria-label="Убрать файл" onclick={selectedFile.clear}>
          <X size={18} strokeWidth={2} aria-hidden="true" />
        </button>
      </section>

      <ExpirationSection />
      <DownloadsSection />
      <PasswordSection />
    </div>

    <aside aria-label="Настройки отправки">
      <h2>Настройки отправки</h2>
      <dl>
        <dt>Срок</dt>
        <dd>{expiration}</dd>
        <dt>Скачиваний</dt>
        <dd>{downloads}</dd>
        <dt>Пароль</dt>
        <dd>{settings.passwordEnabled ? 'да' : 'нет'}</dd>
      </dl>
      <Button variant="primary" disabled={!settings.isValid} onclick={send}>Загрузить и получить ссылку</Button>
      {#if upload.error !== null}
        <span class="error" role="alert">{upload.error}</span>
      {/if}
    </aside>
  </div>
</div>

<style>
  .page {
    display: flex;
    flex-direction: column;
    gap: 20px;
    padding: clamp(20px, 4vw, 36px) 0 44px;
  }

  .back {
    align-self: flex-start;
    display: inline-flex;
    align-items: center;
    gap: 4px;
    height: 44px;
    padding: 0 12px 0 6px;
    border: 0;
    border-radius: 10px;
    background: transparent;
    color: var(--text-muted);
    font: 500 15px var(--font-sans);
    cursor: pointer;
  }

  .back:hover {
    background: var(--surface-2);
    color: var(--text);
  }

  .layout {
    display: flex;
    flex-wrap: wrap;
    gap: 20px;
    align-items: flex-start;
  }

  .cards {
    flex: 2 1 520px;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .file {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 14px 16px;
    padding: 18px 20px;
    border: 1px solid var(--border);
    border-radius: 20px;
    background: var(--surface);
  }

  .info {
    flex: 1 1 200px;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .name {
    font-size: 17px;
    font-weight: 650;
    overflow-wrap: anywhere;
  }

  .meta {
    font-size: 14px;
    color: var(--text-muted);
  }

  .remove {
    width: 44px;
    height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 0;
    border-radius: 10px;
    background: transparent;
    color: var(--text-muted);
    cursor: pointer;
  }

  .remove:hover {
    background: var(--surface-2);
    color: var(--text);
  }

  aside {
    flex: 1 1 300px;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: 16px;
    padding: 24px;
    border: 1px solid var(--border);
    border-radius: 20px;
    background: var(--surface);
  }

  h2 {
    margin: 0;
    font-size: 18px;
    font-weight: 650;
    letter-spacing: -0.01em;
  }

  dl {
    display: grid;
    grid-template-columns: auto 1fr;
    gap: 10px 16px;
    margin: 0;
    font-size: 15px;
  }

  .error {
    font-size: 14px;
    font-weight: 500;
    color: var(--danger);
  }

  dt {
    color: var(--text-muted);
  }

  dd {
    margin: 0;
    text-align: right;
    font-weight: 600;
  }
</style>
