<script lang="ts">
  import ConsolePage from './console/ConsolePage.svelte'
  import FilePage from './file/FilePage.svelte'
  import HomePage from './home/HomePage.svelte'
  import Footer from './layout/Footer.svelte'
  import Header from './layout/Header.svelte'
  import NotFoundPage from './routing/NotFoundPage.svelte'
  import { router } from './routing/router.svelte'
  import ToastHost from './ui/ToastHost.svelte'

  const route = $derived(router.route)
</script>

<div class="layout">
  <Header />

  <main>
    {#if route.page === 'home'}
      <HomePage />
    {:else if route.page === 'console'}
      <ConsolePage />
    {:else if route.page === 'file'}
      {#key route.id}
        <FilePage id={route.id} />
      {/key}
    {:else}
      <NotFoundPage />
    {/if}
  </main>

  <Footer />
</div>

<ToastHost />

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
