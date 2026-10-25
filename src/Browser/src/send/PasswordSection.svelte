<script lang="ts">
  import PasswordInput from '../ui/PasswordInput.svelte'
  import SettingsSection from './SettingsSection.svelte'
  import { settings } from './settings.svelte'
</script>

<SettingsSection id="password" title="Пароль">
  {#snippet action()}
    <button
      type="button"
      role="switch"
      class="switch"
      aria-checked={settings.passwordEnabled}
      aria-labelledby="password-title"
      onclick={() => settings.togglePassword()}
    >
      <span class="knob"></span>
    </button>
  {/snippet}

  {#if settings.passwordEnabled}
    <div class="field">
      <label for="password-new">Пароль</label>
      <PasswordInput id="password-new" maxlength={128} bind:value={settings.password} />
    </div>
    <p>Не больше 128 символов. Передавайте пароль отдельно от ссылки.</p>
  {:else}
    <p>Без пароля файл доступен всем, у кого есть ссылка.</p>
  {/if}
</SettingsSection>

<style>
  .switch {
    position: relative;
    flex: 0 0 auto;
    width: 52px;
    height: 30px;
    padding: 0;
    border: 0;
    border-radius: 999px;
    background: var(--border-strong);
    cursor: pointer;
    transition: background-color 120ms;
  }

  .switch[aria-checked='true'] {
    background: var(--accent);
  }

  .knob {
    position: absolute;
    top: 3px;
    left: 3px;
    width: 24px;
    height: 24px;
    border-radius: 50%;
    background: #fff;
    transition:
      transform 120ms,
      background-color 120ms;
  }

  .switch[aria-checked='true'] .knob {
    transform: translateX(22px);
    background: var(--on-accent);
  }

  .field {
    max-width: 360px;
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  label {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-muted);
  }

  p {
    margin: 0;
    font-size: 15px;
    color: var(--text-muted);
  }
</style>
