<script lang="ts">
  import { getApi } from '../api/context'
  import { ApiError, type FileInfo } from '../api/MayflyApi'
  import Button from '../ui/Button.svelte'

  let { id, onunlock }: { id: string; onunlock: (info: FileInfo) => void } = $props()

  const api = getApi()

  let password = $state('')
  let visible = $state(false)
  let checking = $state(false)
  let error = $state<string | null>(null)
  let retryAt = $state<number | null>(null)
  let now = $state(Date.now())

  const secondsLeft = $derived(retryAt === null ? 0 : Math.max(0, Math.ceil((retryAt - now) / 1000)))
  const message = $derived(retryAt === null ? error : `Слишком много попыток. Повторим через ${secondsLeft} с`)

  $effect(() => {
    if (retryAt === null) {
      return
    }

    const timer = setInterval(() => {
      now = Date.now()
      if (retryAt !== null && now >= retryAt) {
        retryAt = null
      }
    }, 250)

    return () => clearInterval(timer)
  })

  async function submit(event: SubmitEvent) {
    event.preventDefault()
    if (password === '') {
      error = 'Введите пароль'
      return
    }

    checking = true
    try {
      const result = await api.getFileInfo(id, password)
      if (result.status === 'found') {
        onunlock(result.info)
      } else if (result.status === 'access-denied') {
        error = 'Неверный пароль'
      } else if (result.status === 'rate-limited') {
        if (result.retryAfterSeconds === null) {
          error = 'Слишком много попыток. Повторите позже'
        } else {
          now = Date.now()
          retryAt = now + result.retryAfterSeconds * 1000
        }
      } else {
        error = 'Файл не найден'
      }
    } catch (exception) {
      error = exception instanceof ApiError ? 'Не удалось проверить пароль' : 'Нет связи с сервером'
    } finally {
      checking = false
    }
  }
</script>

<section aria-labelledby="password-title">
  <span class="icon" aria-hidden="true">
    <svg width="26" height="26" viewBox="0 0 24 24">
      <rect x="5" y="11" width="14" height="9" rx="2" />
      <path d="M8 11V8a4 4 0 0 1 8 0v3" />
    </svg>
  </span>

  <div class="intro">
    <h1 id="password-title">Файл защищён паролем</h1>
    <p>Введите пароль, чтобы открыть файл.</p>
  </div>

  <form onsubmit={submit} novalidate>
    <div class="field">
      <label for="file-password">Пароль</label>
      <div class="input" class:invalid={message !== null}>
        <input
          id="file-password"
          type={visible ? 'text' : 'password'}
          autocomplete="off"
          bind:value={password}
          oninput={() => (error = null)}
          aria-invalid={message !== null}
          aria-describedby={message === null ? undefined : 'file-password-error'}
        />
        <button
          type="button"
          class="toggle"
          aria-label={visible ? 'Скрыть пароль' : 'Показать пароль'}
          aria-pressed={visible}
          onclick={() => (visible = !visible)}
        >
          <svg width="20" height="20" viewBox="0 0 24 24" aria-hidden="true">
            <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" />
            <circle cx="12" cy="12" r="3" />
            {#if visible}
              <path d="M4 4l16 16" />
            {/if}
          </svg>
        </button>
      </div>
      {#if message !== null}
        <span id="file-password-error" class="error" role={retryAt === null ? 'alert' : undefined}>{message}</span>
      {/if}
    </div>

    <Button type="submit" variant="primary" disabled={checking || retryAt !== null}>Открыть файл</Button>
  </form>
</section>

<style>
  section {
    width: 100%;
    max-width: 520px;
    display: flex;
    flex-direction: column;
    gap: 22px;
    padding: clamp(22px, 4vw, 36px);
    border: 1px solid var(--border);
    border-radius: 24px;
    background: var(--surface);
  }

  .icon {
    width: 56px;
    height: 56px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 16px;
    background: var(--accent-soft);
    color: var(--accent-text);
  }

  svg {
    fill: none;
    stroke: currentColor;
    stroke-width: 1.9;
    stroke-linecap: round;
    stroke-linejoin: round;
  }

  .intro {
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
    color: var(--text-muted);
  }

  form {
    display: flex;
    flex-direction: column;
    gap: 22px;
  }

  .field {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  label {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-muted);
  }

  .input {
    display: flex;
    align-items: center;
    gap: 4px;
    height: 52px;
    padding-right: 4px;
    border: 1px solid var(--border-strong);
    border-radius: 12px;
    background: var(--input-bg);
  }

  .input:focus-within {
    outline: 2px solid var(--focus-ring);
    outline-offset: 2px;
  }

  .input.invalid {
    border-color: var(--danger);
  }

  input {
    flex: 1 1 auto;
    min-width: 0;
    height: 100%;
    padding: 0 14px;
    border: 0;
    background: transparent;
    color: var(--text);
    font: 500 17px var(--font-mono);
    outline: none;
  }

  .toggle {
    flex: 0 0 auto;
    width: 44px;
    height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 0;
    border-radius: 9px;
    background: transparent;
    color: var(--text-muted);
    cursor: pointer;
  }

  .error {
    font-size: 14px;
    font-weight: 500;
    color: var(--danger);
  }
</style>
