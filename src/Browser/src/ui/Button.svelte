<script lang="ts">
  import type { Snippet } from 'svelte'
  import type { HTMLButtonAttributes } from 'svelte/elements'

  let {
    variant = 'secondary',
    loading = false,
    children,
    ...attributes
  }: HTMLButtonAttributes & { variant?: 'primary' | 'secondary'; loading?: boolean; children: Snippet } = $props()
</script>

<button type="button" class={variant} class:loading {...attributes} disabled={attributes.disabled || loading} aria-busy={loading}>
  {#if loading}
    <span class="spinner" aria-hidden="true"></span>
  {/if}
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

  button.loading:disabled {
    border-color: transparent;
    background: var(--accent);
    color: var(--on-accent);
    cursor: progress;
    opacity: 0.85;
  }

  button.secondary.loading:disabled {
    border-color: var(--border-strong);
    background: var(--bg);
    color: var(--text);
  }

  .spinner {
    width: 16px;
    height: 16px;
    border: 2px solid currentColor;
    border-right-color: transparent;
    border-radius: 50%;
    animation: spin 700ms linear infinite;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .spinner {
      animation-duration: 2s;
    }
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
