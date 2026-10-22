<script lang="ts">
  import FilePage from './file/FilePage.svelte'
  import HistoryPage from './history/HistoryPage.svelte'
  import HomePage from './home/HomePage.svelte'
  import Footer from './layout/Footer.svelte'
  import Header from './layout/Header.svelte'
  import NotFoundPage from './routing/NotFoundPage.svelte'
  import { router } from './routing/router.svelte'

  const route = $derived(router.route)
</script>

<div class="layout">
  <Header />

  <main>
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

  <Footer />
</div>

<style>
  .layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
    max-width: 1120px;
    margin: 0 auto;
    padding: 0 clamp(16px, 4vw, 32px);
  }

  main {
    flex: 1;
  }
</style>
