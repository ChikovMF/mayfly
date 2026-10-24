<script lang="ts">
  import Check from '@lucide/svelte/icons/check'
  import { toast } from './toast.svelte'
</script>

<div class="host" role="status">
  {#if toast.message !== null}
    <div class="toast">
      <Check size={18} strokeWidth={2.6} color="var(--accent)" aria-hidden="true" />
      {toast.message}
    </div>
  {/if}
</div>

<style>
  .host {
    position: fixed;
    left: 50%;
    bottom: 24px;
    transform: translateX(-50%);
    z-index: 10;
    pointer-events: none;
  }

  .toast {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px 18px;
    border-radius: 14px;
    background: var(--text);
    color: var(--bg);
    box-shadow: 0 16px 40px rgb(0 0 0 / 25%);
    font-size: 14px;
    font-weight: 500;
    white-space: nowrap;
  }
</style>
