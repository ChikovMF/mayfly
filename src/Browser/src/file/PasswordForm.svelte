<script lang="ts">
  import { getApi } from '../api/context'
  import { ApiError, type FileInfo } from '../api/MayflyApi'
  import Button from '../ui/Button.svelte'
  import PasswordInput from '../ui/PasswordInput.svelte'

  let { id, onunlock }: { id: string; onunlock: (info: FileInfo, password: string) => void } = $props()

  const api = getApi()

  let password = $state('')
  let checking = $state(false)
  let error = $state<string | null>(null)
  let retryAt = $state<number | null>(null)
  let now = $state(Date.now())

  const secondsLeft = $derived(retryAt === null ? 0 : Math.max(0, Math.ceil((retryAt - now) / 1000)))
  const message = $derived(retryAt === null ? error : `Слишком много попыток. Можно повторить через ${secondsLeft} с`)

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
        onunlock(result.info, password)
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
      <PasswordInput
        id="file-password"
        bind:value={password}
        invalid={message !== null}
        oninput={() => (error = null)}
        aria-describedby={message === null ? undefined : 'file-password-error'}
      />
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

  .error {
    font-size: 14px;
    font-weight: 500;
    color: var(--danger);
  }
</style>
