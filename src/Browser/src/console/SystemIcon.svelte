<script lang="ts">
  import { siApple, siLinux } from 'simple-icons'
  import type { System } from './platforms'

  let { system }: { system: System } = $props()
</script>

<svg width="40" height="40" viewBox="0 0 24 24" aria-hidden="true">
  {#if system === 'linux'}
    <path d={siLinux.path} />
  {:else if system === 'macos'}
    <path d={siApple.path} />
  {:else}
    <path d="M2 3.6 10.5 2.4v8.1H2zM11.5 2.3 22 .8v9.7H11.5zM2 11.5h8.5v8.1L2 18.4zM11.5 11.5H22v9.7l-10.5-1.5z" />
  {/if}
</svg>

<style>
  svg {
    fill: currentColor;
  }
</style>
