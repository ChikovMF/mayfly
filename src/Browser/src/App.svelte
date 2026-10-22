<script lang="ts">
  import FilePage from './file/FilePage.svelte'
  import HistoryPage from './history/HistoryPage.svelte'
  import HomePage from './home/HomePage.svelte'
  import NotFoundPage from './routing/NotFoundPage.svelte'
  import { router } from './routing/router.svelte'
  import ThemeToggle from './theme/ThemeToggle.svelte'

  const route = $derived(router.route)
</script>

<main>
  <ThemeToggle />

  {#if route.page === 'home'}
    <HomePage />
  {:else if route.page === 'history'}
    <HistoryPage />
  {:else if route.page === 'file'}
    <FilePage id={route.id} />
  {:else}
    <NotFoundPage />
  {/if}
</main>
