<script lang="ts">
  import Check from '@lucide/svelte/icons/check'
  import { formatDownloads, formatDuration, formatSecondsLeft, formatSize, formatSpeed } from '../format'
  import Button from '../ui/Button.svelte'
  import { settings } from './settings.svelte'
  import { upload } from './upload.svelte'

  let { file }: { file: File } = $props()

  const stages = [
    { phase: 'hashing', label: 'Подсчёт контрольной суммы' },
    { phase: 'sending', label: 'Отправка' },
    { phase: 'saving', label: 'Сохранение на сервере' },
  ]

  const current = $derived(stages.findIndex((stage) => stage.phase === upload.phase))
  const saving = $derived(upload.phase === 'saving')
  const percent = $derived(file.size === 0 || saving ? 100 : Math.floor((upload.sentBytes / file.size) * 100))
  const secondsLeft = $derived(
    upload.bytesPerSecond > 0 ? (file.size - upload.sentBytes) / upload.bytesPerSecond : null,
  )
  const chips = $derived([
    settings.expiresInMinutes === null ? null : formatDuration(settings.expiresInMinutes),
    settings.maxDownloads === null ? null : `${settings.maxDownloads} ${formatDownloads(settings.maxDownloads)}`,
    settings.passwordValue === null ? null : 'пароль',
  ].filter((chip) => chip !== null))
</script>

<svelte:window onbeforeunload={(event) => event.preventDefault()} />

<div class="page">
  <section aria-labelledby="uploading-title">
    <div class="head">
      <h1 id="uploading-title">Отправляем {file.name}</h1>
      <p>{formatSize(file.size)}</p>
      {#if chips.length > 0}
        <div class="chips">
          {#each chips as chip (chip)}
            <span>{chip}</span>
          {/each}
        </div>
      {/if}
    </div>

    <div class="progress">
      <div class="numbers">
        <span class="percent">{percent}<span>%</span></span>
        {#if saving}
          <span class="speed">Сохраняем на сервере</span>
        {:else if secondsLeft !== null}
          <span class="speed">{formatSpeed(upload.bytesPerSecond)} · ещё {formatSecondsLeft(secondsLeft)}</span>
        {/if}
      </div>
      <div class="bar" role="progressbar" aria-label="Отправка" aria-valuemin="0" aria-valuemax="100" aria-valuenow={percent}>
        <div class="fill" style:width="{percent}%"></div>
      </div>
      <span class="amount">{formatSize(saving ? file.size : upload.sentBytes)} из {formatSize(file.size)}</span>
    </div>

    <ol aria-label="Этапы">
      {#each stages as stage, index (stage.phase)}
        <li aria-current={index === current ? 'step' : undefined} class:pending={index > current}>
          {#if index < current}
            <span class="mark done"><Check size={14} strokeWidth={3} aria-hidden="true" /></span>
          {:else if index === current}
            <span class="mark spinner"></span>
          {:else}
            <span class="mark waiting"></span>
          {/if}
          <span class="label">{stage.label}</span>
          {#if index === current && !saving}
            <span class="value">{percent}%</span>
          {/if}
        </li>
      {/each}
    </ol>

    <div class="footer">
      <Button onclick={() => upload.cancel()}>Отменить</Button>
      <span>Не закрывайте вкладку, пока идёт отправка. Ссылка появится сразу после сохранения.</span>
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
    max-width: 720px;
    display: flex;
    flex-direction: column;
    gap: 26px;
    padding: clamp(20px, 4vw, 36px);
    border: 1px solid var(--border);
    border-radius: 22px;
    background: var(--surface);
  }

  .head {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  h1 {
    margin: 0;
    font-size: clamp(24px, 3.4vw, 32px);
    font-weight: 700;
    letter-spacing: -0.03em;
    overflow-wrap: anywhere;
  }

  .head p {
    margin: 0;
    font-size: 15px;
    color: var(--text-muted);
  }

  .chips {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    margin-top: 4px;
  }

  .chips span {
    display: inline-flex;
    align-items: center;
    height: 28px;
    padding: 0 10px;
    border-radius: 999px;
    background: var(--surface-2);
    font-size: 13px;
    color: var(--text-muted);
  }

  .progress {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .numbers {
    display: flex;
    flex-wrap: wrap;
    align-items: baseline;
    justify-content: space-between;
    gap: 12px;
  }

  .percent {
    font: 600 clamp(44px, 7vw, 64px) / 1 var(--font-mono);
    letter-spacing: -0.04em;
  }

  .percent span {
    font-size: 0.5em;
    color: var(--text-muted);
  }

  .speed {
    font: 500 14px var(--font-mono);
    color: var(--text-muted);
  }

  .bar {
    height: 10px;
    border-radius: 999px;
    background: var(--border);
    overflow: hidden;
  }

  .fill {
    height: 100%;
    border-radius: 999px;
    background: var(--accent);
    transition: width 150ms linear;
  }

  .amount {
    font-size: 14px;
    color: var(--text-muted);
  }

  ol {
    list-style: none;
    margin: 0;
    padding: 0;
    border-top: 1px solid var(--border);
  }

  li {
    display: flex;
    align-items: center;
    gap: 14px;
    min-height: 52px;
    border-bottom: 1px solid var(--border);
  }

  li.pending {
    color: var(--text-muted);
  }

  .label {
    flex: 1 1 auto;
    font-size: 15px;
  }

  li[aria-current='step'] .label {
    font-weight: 650;
  }

  .value {
    font: 500 13px var(--font-mono);
    color: var(--accent-text);
  }

  .mark {
    flex: 0 0 auto;
    width: 26px;
    height: 26px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 999px;
  }

  .done {
    background: color-mix(in srgb, var(--ok) 16%, transparent);
    color: var(--ok);
  }

  .spinner {
    border: 3px solid var(--border);
    border-top-color: var(--accent);
    border-right-color: var(--accent);
    animation: spin 0.9s linear infinite;
  }

  .waiting {
    border: 2px dashed var(--border-strong);
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }

  .footer {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 12px 16px;
  }

  .footer span {
    flex: 1 1 260px;
    font-size: 14px;
    line-height: 1.5;
    color: var(--text-faint);
  }
</style>
