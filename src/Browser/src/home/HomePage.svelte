<script lang="ts">
  import ConfigurePage from '../send/ConfigurePage.svelte'
  import DonePage from '../send/DonePage.svelte'
  import { selection } from '../send/selection.svelte'
  import { upload } from '../send/upload.svelte'
  import UploadingPage from '../send/UploadingPage.svelte'
  import UploadZone from '../send/UploadZone.svelte'
  import GetFileForm from './GetFileForm.svelte'
</script>

{#if upload.phase === 'done' && upload.sent !== null}
  <DonePage sent={upload.sent} />
{:else if upload.active}
  <UploadingPage />
{:else if selection.current !== null}
  <ConfigurePage selection={selection.current} />
{:else}
  <div class="page">
    <section class="intro">
      <h1>Обмен файлами по ссылке</h1>
      <p>По желанию можно ограничить срок хранения и число скачиваний или защитить файл паролем.</p>
    </section>

    <div class="actions">
      <UploadZone />
      <GetFileForm />
    </div>
  </div>
{/if}

<style>
  .page {
    display: flex;
    flex-direction: column;
    gap: 28px;
    padding: clamp(28px, 6vw, 60px) 0 44px;
  }

  .intro {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  h1 {
    margin: 0;
    font-size: 28px;
    line-height: 1.2;
    letter-spacing: -0.03em;
    font-weight: 700;
  }

  .intro p {
    margin: 0;
    max-width: 58ch;
    font-size: 15px;
    color: var(--text-muted);
    text-wrap: pretty;
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: 20px;
    align-items: stretch;
  }
</style>
