<script lang="ts">
  import { isFileId } from '../file/fileId'
  import Button from '../ui/Button.svelte'

  let value = $state('')
  let error = $state<string | null>(null)

  function submit(event: SubmitEvent) {
    event.preventDefault()

    const id = extractId(value)
    error = validate(id)
    if (error === null) {
      location.assign(`/${id}`)
    }
  }

  function extractId(input: string): string {
    const path = input.trim().split(/[?#]/)[0]
    return path.split('/').filter((segment) => segment !== '').at(-1) ?? ''
  }

  function validate(id: string): string | null {
    if (id === '') {
      return 'Введите ID или ссылку'
    }

    if (!isFileId(id)) {
      return 'Неверный формат ID или ссылки'
    }

    return null
  }
</script>

<section id="get" aria-labelledby="get-title">
  <h2 id="get-title">Получить файл</h2>
  <p class="lead">Вставьте ID или ссылку на файл.</p>

  <form onsubmit={submit} novalidate>
    <div class="field">
      <label for="file-id">ID или ссылка</label>
      <input
        id="file-id"
        type="text"
        placeholder="7KpQ2mXa9B"
        spellcheck="false"
        autocomplete="off"
        bind:value
        oninput={() => (error = null)}
        aria-invalid={error !== null}
        aria-describedby={error === null ? undefined : 'file-id-error'}
      />
      {#if error !== null}
        <span id="file-id-error" class="error" role="alert">{error}</span>
      {/if}
    </div>

    <Button type="submit">Открыть</Button>
  </form>
</section>

<style>
  section {
    flex: 1 1 300px;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: 14px;
    padding: 24px;
    border: 1px solid var(--border);
    border-radius: 22px;
    background: var(--surface);
  }

  h2 {
    margin: 0;
    font-size: 19px;
    font-weight: 650;
    letter-spacing: -0.01em;
  }

  .lead {
    margin: 0;
    font-size: 15px;
    color: var(--text-muted);
  }

  form {
    display: flex;
    flex-direction: column;
    gap: 14px;
  }

  .field {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  label {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-muted);
  }

  input {
    width: 100%;
    height: 50px;
    padding: 0 14px;
    border: 1px solid var(--border-strong);
    border-radius: 12px;
    background: var(--input-bg);
    color: var(--text);
    font: 500 17px var(--font-mono);
    letter-spacing: 0.06em;
  }

  input[aria-invalid='true'] {
    border-color: var(--danger);
  }

  .error {
    font-size: 13px;
    line-height: 1.45;
    color: var(--danger);
  }
</style>
