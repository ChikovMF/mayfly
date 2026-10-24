<script lang="ts">
  import { encodeQR } from 'qr'

  let { text }: { text: string } = $props()

  const image = $derived(encodeQR(text, 'data-url', { scale: 8, border: 2 }))
</script>

<img src={image} alt="QR-код ссылки" />

<style>
  img {
    display: block;
    width: 100%;
    height: auto;
    image-rendering: pixelated;
  }
</style>
