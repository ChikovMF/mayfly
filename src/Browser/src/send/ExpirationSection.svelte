<script lang="ts">
  import ChoiceGroup from '../ui/ChoiceGroup.svelte'
  import { formatDuration } from '../format'
  import SettingsSection from './SettingsSection.svelte'
  import { settings, type ExpirationChoice } from './settings.svelte'

  const options: { value: ExpirationChoice; label: string }[] = [
    { value: '1h', label: '1 час' },
    { value: '1d', label: '1 день' },
    { value: '7d', label: '7 дней' },
    { value: '30d', label: '30 дней' },
    { value: 'never', label: 'Бессрочно' },
    { value: 'custom', label: 'Свой срок…' },
  ]

  const minutes = $derived(settings.expiresInMinutes)
  const invalid = $derived(
    settings.expiration === 'custom' && settings.customExpiration.trim() !== '' && settings.customExpirationMinutes === null,
  )
</script>

<SettingsSection id="expiration" title="Срок хранения">
  <ChoiceGroup labelledby="expiration-title" name="expiration" {options} bind:value={settings.expiration} />

  {#if settings.expiration === 'custom'}
    <div class="custom">
      <label for="expiration-custom">Свой срок</label>
      <input
        id="expiration-custom"
        type="text"
        placeholder="12h"
        spellcheck="false"
        autocomplete="off"
        bind:value={settings.customExpiration}
        aria-invalid={invalid}
        aria-describedby="expiration-custom-hint"
      />
      <span id="expiration-custom-hint" class="hint" class:error={invalid}>
        Число и единица: m (минуты), h (часы), d (дни). Например, 30m, 12h, 7d.
      </span>
    </div>
  {/if}

  <p>
    {#if settings.expiration === 'never'}
      Срок хранения не ограничен.
    {:else if minutes !== null}
      Файл исчезнет через {formatDuration(minutes)}.
    {/if}
  </p>
</SettingsSection>

<style>
  .custom {
    display: flex;
    flex-direction: column;
    gap: 6px;
    max-width: 320px;
  }

  label {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-muted);
  }

  input {
    height: 50px;
    padding: 0 14px;
    border: 1px solid var(--border-strong);
    border-radius: 12px;
    background: var(--input-bg);
    color: var(--text);
    font: 500 17px var(--font-mono);
  }

  input[aria-invalid='true'] {
    border-color: var(--danger);
  }

  .hint {
    font-size: 13px;
    line-height: 1.45;
    color: var(--text-faint);
  }

  .hint.error {
    color: var(--danger);
  }

  p {
    min-height: 1.5em;
    margin: 0;
    font-size: 15px;
    color: var(--text-muted);
  }
</style>
