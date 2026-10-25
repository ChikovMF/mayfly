<script lang="ts">
  import Check from '@lucide/svelte/icons/check'
  import Copy from '@lucide/svelte/icons/copy'
  import Lock from '@lucide/svelte/icons/lock'
  import { formatDate, formatSize, formatTimeLeft } from '../format'
  import Button from '../ui/Button.svelte'
  import { copyToClipboard } from '../ui/toast.svelte'
  import { selection } from './selection.svelte'
  import QrCode from './QrCode.svelte'
  import { upload, type SentFile } from './upload.svelte'

  let { sent }: { sent: SentFile } = $props()

  const link = $derived(`${location.origin}/${sent.id}`)

  function sendAnother() {
    selection.clear()
    upload.reset()
  }
</script>

<div class="page">
  <div class="title">
    <span class="mark" aria-hidden="true"><Check size={22} strokeWidth={2.6} /></span>
    <div>
      <h1>Ссылка готова</h1>
      <span class="file">{sent.name} · {formatSize(sent.size)}</span>
    </div>
  </div>

  <div class="row">
    <section aria-label="Ссылка">
      <div class="field">
        <label for="share-link">Ссылка на файл</label>
        <input id="share-link" type="text" readonly value={link} onfocus={(event) => event.currentTarget.select()} />
      </div>

      <div class="buttons">
        <Button variant="primary" onclick={() => copyToClipboard(link, 'Ссылка скопирована')}>
          <Copy size={18} strokeWidth={2} aria-hidden="true" />
          Скопировать ссылку
        </Button>
        <Button onclick={() => copyToClipboard(sent.id, 'ID скопирован')}>Только ID</Button>
      </div>

      {#if sent.password !== null}
        {@const password = sent.password}
        <div class="password">
          <Lock size={20} strokeWidth={2} aria-hidden="true" />
          <span>Файл защищён паролем. Отправьте пароль отдельно от ссылки: другим сообщением или каналом.</span>
          <Button onclick={() => copyToClipboard(password, 'Пароль скопирован')}>Скопировать пароль</Button>
        </div>
      {/if}

      {#if sent.expiresAt !== null || sent.maxDownloads !== null}
        <dl>
          {#if sent.expiresAt !== null}
            <div>
              <dt>Истекает</dt>
              <dd>{formatDate(sent.expiresAt)}</dd>
              <span>через {formatTimeLeft(sent.expiresAt)}</span>
            </div>
          {/if}
          {#if sent.maxDownloads !== null}
            <div>
              <dt>Скачиваний</dt>
              <dd>{sent.maxDownloads}</dd>
            </div>
          {/if}
        </dl>
      {/if}
    </section>

    <aside aria-labelledby="qr-title">
      <h2 id="qr-title">QR-код</h2>
      <div class="qr"><QrCode text={link} /></div>
      <span>Отсканируйте телефоном, чтобы открыть ссылку</span>
    </aside>
  </div>

  <div class="more">
    <Button onclick={sendAnother}>Отправить ещё файл</Button>
  </div>
</div>

<style>
  .page {
    display: flex;
    flex-direction: column;
    gap: 24px;
    padding: clamp(28px, 5vw, 52px) 0 60px;
  }

  .title {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 14px;
  }

  .mark {
    width: 44px;
    height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 14px;
    background: color-mix(in srgb, var(--ok) 16%, transparent);
    color: var(--ok);
  }

  h1 {
    margin: 0;
    font-size: clamp(26px, 3.6vw, 36px);
    font-weight: 700;
    letter-spacing: -0.03em;
  }

  .file {
    font-size: 15px;
    color: var(--text-muted);
    overflow-wrap: anywhere;
  }

  .row {
    display: flex;
    flex-wrap: wrap;
    gap: 20px;
    align-items: stretch;
  }

  section {
    flex: 3 1 520px;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: 20px;
    padding: clamp(20px, 3vw, 28px);
    border: 1px solid var(--border);
    border-radius: 22px;
    background: var(--surface);
  }

  .field {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  label {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-muted);
  }

  input {
    width: 100%;
    height: 64px;
    padding: 0 18px;
    border: 1px solid var(--border-strong);
    border-radius: 14px;
    background: var(--input-bg);
    color: var(--text);
    font: 500 clamp(15px, 2.2vw, 20px) var(--font-mono);
  }

  .buttons {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  }

  .password {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 12px 16px;
    padding: 14px 16px;
    border-radius: 14px;
    background: var(--accent-soft);
    color: var(--accent-text);
  }

  .password span {
    flex: 1 1 260px;
    font-size: 14px;
    line-height: 1.5;
    color: var(--text);
  }

  dl {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(200px, 100%), 1fr));
    gap: 18px 24px;
    margin: 0;
    padding-top: 18px;
    border-top: 1px solid var(--border);
  }

  dl div {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  dt {
    font-size: 13px;
    color: var(--text-muted);
  }

  dd {
    margin: 0;
    font-size: 16px;
    font-weight: 600;
  }

  dl span {
    font-size: 13px;
    color: var(--text-faint);
  }

  aside {
    flex: 1 1 260px;
    min-width: 0;
    max-width: 320px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 14px;
    padding: 24px;
    border: 1px solid var(--border);
    border-radius: 22px;
    background: var(--surface);
  }

  @media (max-width: 700px) {
    aside {
      max-width: none;
    }
  }

  h2 {
    align-self: flex-start;
    margin: 0;
    font-size: 16px;
    font-weight: 650;
  }

  .qr {
    width: 200px;
    max-width: 100%;
    overflow: hidden;
    border: 1px solid var(--border);
    border-radius: 16px;
    background: #fff;
  }

  aside span {
    font-size: 13px;
    line-height: 1.5;
    text-align: center;
    color: var(--text-faint);
  }

  .more {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  }
</style>
