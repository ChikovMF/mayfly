<script lang="ts">
  import Eye from '@lucide/svelte/icons/eye'
  import EyeOff from '@lucide/svelte/icons/eye-off'
  import type { HTMLInputAttributes } from 'svelte/elements'

  let {
    value = $bindable(),
    invalid = false,
    ...attributes
  }: Omit<HTMLInputAttributes, 'value' | 'type'> & { value: string; invalid?: boolean } = $props()

  let visible = $state(false)
</script>

<div class="input" class:invalid>
  <input
    type="text"
    class:masked={!visible}
    autocomplete="off"
    autocapitalize="off"
    spellcheck="false"
    data-1p-ignore
    data-lpignore="true"
    aria-invalid={invalid}
    bind:value
    {...attributes}
  />
  <button
    type="button"
    class="toggle"
    aria-label={visible ? 'Скрыть пароль' : 'Показать пароль'}
    aria-pressed={visible}
    onclick={() => (visible = !visible)}
  >
    {#if visible}
      <EyeOff size={20} strokeWidth={1.9} aria-hidden="true" />
    {:else}
      <Eye size={20} strokeWidth={1.9} aria-hidden="true" />
    {/if}
  </button>
</div>

<style>
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

  input.masked {
    -webkit-text-security: disc;
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
</style>
