<script lang="ts">
  import type { Snippet } from 'svelte'
  import type { HTMLButtonAttributes } from 'svelte/elements'

  let {
    variant = 'secondary',
    children,
    ...attributes
  }: HTMLButtonAttributes & { variant?: 'primary' | 'secondary'; children: Snippet } = $props()
</script>

<button type="button" class={variant} {...attributes}>
  {@render children()}
</button>

<style>
  button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    height: 48px;
    padding: 0 20px;
    border: 1px solid transparent;
    border-radius: 12px;
    font: 600 15px var(--font-sans);
    cursor: pointer;
    transition:
      background-color 120ms,
      border-color 120ms,
      transform 80ms;
  }

  button:active:not(:disabled) {
    transform: translateY(1px);
  }

  button:disabled {
    border-color: var(--border);
    background: var(--surface-2);
    color: var(--text-faint);
    cursor: default;
  }

  .primary {
    background: var(--accent);
    color: var(--on-accent);
  }

  .primary:hover:not(:disabled) {
    background: var(--accent-hover);
  }

  .secondary {
    border-color: var(--border-strong);
    background: var(--bg);
    color: var(--text);
  }

  .secondary:hover:not(:disabled) {
    background: var(--surface-2);
  }
</style>
