<script lang="ts">
  import { getApi } from '../api/context'
  import { ApiError } from '../api/MayflyApi'
  import FileDetails from './FileDetails.svelte'

  let { id }: { id: string } = $props()

  const api = getApi()
  const result = $derived(api.getFileInfo(id))
</script>

<div class="page">
  {#await result}
    <section class="card" aria-busy="true" aria-label="Загружаем сведения о файле">
      <div class="skeleton title"></div>
      <div class="skeleton line"></div>
      <div class="skeleton block"></div>
    </section>
  {:then result}
    {#if result.status === 'found'}
      <FileDetails {id} info={result.info} />
    {:else if result.status === 'not-found'}
      <section class="card">
        <h1>Файл не найден</h1>
        <a href="/#get">Ввести другой ID</a>
      </section>
    {:else}
      <section class="card">
        <h1>Файл защищён паролем</h1>
      </section>
    {/if}
  {:catch error}
    <section class="card">
      <h1>{error instanceof ApiError ? 'Не удалось получить сведения о файле' : 'Нет связи с сервером'}</h1>
    </section>
  {/await}
</div>

<style>
  .page {
    display: flex;
    justify-content: center;
    padding: clamp(28px, 6vw, 60px) 0 48px;
  }

  .card {
    width: 100%;
    max-width: 760px;
    display: flex;
    flex-direction: column;
    gap: 18px;
    padding: clamp(20px, 4vw, 36px);
    border: 1px solid var(--border);
    border-radius: 24px;
    background: var(--surface);
  }

  h1 {
    margin: 0;
    font-size: clamp(24px, 3.6vw, 34px);
    font-weight: 700;
    letter-spacing: -0.03em;
  }

  a {
    color: var(--text-muted);
  }

  .skeleton {
    border-radius: 12px;
    background: var(--surface-2);
    animation: pulse 1.2s ease-in-out infinite alternate;
  }

  .title {
    width: 60%;
    height: 34px;
  }

  .line {
    width: 35%;
    height: 18px;
  }

  .block {
    height: 96px;
  }

  @keyframes pulse {
    to {
      opacity: 0.5;
    }
  }
</style>
