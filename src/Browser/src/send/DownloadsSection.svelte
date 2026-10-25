<script lang="ts">
  import ChoiceGroup from '../ui/ChoiceGroup.svelte'
  import SettingsSection from './SettingsSection.svelte'
  import { settings, type DownloadsChoice } from './settings.svelte'

  const options: { value: DownloadsChoice; label: string }[] = [
    { value: 'unlimited', label: 'Без ограничений' },
    { value: '1', label: '1' },
    { value: '3', label: '3' },
    { value: '10', label: '10' },
    { value: 'custom', label: 'Своё…' },
  ]

  const count = $derived(settings.maxDownloads)
  const invalid = $derived(
    settings.downloads === 'custom' && settings.customDownloads.trim() !== '' && settings.customMaxDownloads === null,
  )
</script>

<SettingsSection id="downloads" title="Сколько раз можно скачать">
  <ChoiceGroup labelledby="downloads-title" name="downloads" {options} bind:value={settings.downloads} />

  {#if settings.downloads === 'custom'}
    <div class="custom">
      <label for="downloads-custom">Своё число</label>
      <input
        id="downloads-custom"
        type="text"
        inputmode="numeric"
        placeholder="5"
        autocomplete="off"
        bind:value={settings.customDownloads}
        aria-invalid={invalid}
        aria-describedby={invalid ? 'downloads-custom-error' : undefined}
      />
      {#if invalid}
        <span id="downloads-custom-error" class="error">Введите целое число, не меньше 1</span>
      {/if}
    </div>
  {/if}

  <p>
    {#if settings.downloads === 'unlimited'}
      Число скачиваний не ограничено.
    {:else if count !== null}
      После {count}-го скачивания файл станет недоступен.
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

  .error {
    font-size: 13px;
    line-height: 1.45;
    color: var(--danger);
  }

  p {
    min-height: 1.5em;
    margin: 0;
    font-size: 15px;
    color: var(--text-muted);
  }
</style>
