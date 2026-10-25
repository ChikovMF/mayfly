<script lang="ts">
  import ThemeToggle from '../theme/ThemeToggle.svelte'
</script>

<header>
  <a class="logo" href="/" aria-label="Mayfly, на главную">
    <svg width="30" height="30" viewBox="0 0 64 64" aria-hidden="true">
      <g transform="translate(0 -1.75)">
        <g>
          <polygon class="wing" points="27,15 3.5,10 8,28 14,34 27,28" />
          <path d="M30.2 44.5 L32.4 46.5 L21 61 Z" />
        </g>
        <g transform="matrix(-1 0 0 1 64 0)">
          <polygon class="wing" points="27,15 3.5,10 8,28 14,34 27,28" />
          <path d="M30.2 44.5 L32.4 46.5 L21 61 Z" />
        </g>
        <circle cx="29.6" cy="9.6" r="3.4" />
        <circle cx="34.4" cy="9.6" r="3.4" />
        <ellipse cx="32" cy="19.5" rx="4.4" ry="6.2" />
        <path d="M29.5 24 H34.5 L33.3 47 H30.7 Z" />
        <path d="M30.6 45 H33.4 L32 62.5 Z" />
      </g>
    </svg>
    <span>mayfly</span>
  </a>

  <div class="actions">
    <ThemeToggle />
  </div>
</header>

<style>
  header {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 10px 20px;
    padding: 14px 0;
    border-bottom: 1px solid var(--border);
  }

  .logo {
    display: inline-flex;
    align-items: center;
    gap: 10px;
    min-height: 44px;
    color: var(--text);
    text-decoration: none;
    font-size: 20px;
    font-weight: 700;
    letter-spacing: -0.03em;
  }

  .logo svg {
    fill: currentColor;
  }

  .wing {
    fill: var(--accent);
    stroke: var(--accent);
    stroke-width: 3;
    stroke-linejoin: round;
  }

  .actions {
    margin-left: auto;
    display: flex;
    align-items: center;
    gap: 8px;
  }
</style>
