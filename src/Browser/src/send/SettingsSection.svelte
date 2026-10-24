<script lang="ts">
  import type { Snippet } from 'svelte'

  let { id, title, action, children }: { id: string; title: string; action?: Snippet; children: Snippet } = $props()
</script>

<section aria-labelledby="{id}-title">
  <div class="head">
    <h2 id="{id}-title">{title}</h2>
    {@render action?.()}
  </div>
  {@render children()}
</section>

<style>
  section {
    display: flex;
    flex-direction: column;
    gap: 16px;
    padding: 24px;
    border: 1px solid var(--border);
    border-radius: 20px;
    background: var(--surface);
  }

  .head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
  }

  h2 {
    margin: 0;
    font-size: 18px;
    font-weight: 650;
    letter-spacing: -0.01em;
  }
</style>
