<script lang="ts">
  import TerminalIcon from '@lucide/svelte/icons/terminal'
  import { repository } from '../repository'
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
    <a class="icon" href="/console" aria-label="Консольный клиент" title="Консольный клиент">
      <TerminalIcon size={20} aria-hidden="true" />
    </a>
    <a class="icon" href={repository} aria-label="Mayfly на GitHub" title="Mayfly на GitHub">
      <svg width="20" height="20" viewBox="0 0 16 16" aria-hidden="true">
        <path
          d="M8 0c4.42 0 8 3.58 8 8a8.013 8.013 0 0 1-5.45 7.59c-.4.08-.55-.17-.55-.38 0-.27.01-1.13.01-2.2 0-.75-.25-1.23-.54-1.48 1.78-.2 3.65-.88 3.65-3.95 0-.88-.31-1.59-.82-2.15.08-.2.36-1.02-.08-2.12 0 0-.67-.22-2.2.82-.64-.18-1.32-.27-2-.27-.68 0-1.36.09-2 .27-1.53-1.03-2.2-.82-2.2-.82-.44 1.1-.16 1.92-.08 2.12-.51.56-.82 1.28-.82 2.15 0 3.06 1.86 3.75 3.64 3.95-.23.2-.44.55-.51 1.07-.46.21-1.61.55-2.33-.66-.15-.24-.6-.83-1.23-.82-.67.01-.27.38.01.53.34.19.73.9.82 1.13.16.45.68 1.31 2.69.94 0 .67.01 1.3.01 1.49 0 .21-.15.45-.55.38A7.995 7.995 0 0 1 0 8c0-4.42 3.58-8 8-8Z"
        />
      </svg>
    </a>
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

  .icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 44px;
    height: 44px;
    border-radius: 10px;
    color: var(--text-muted);
    transition:
      background-color 120ms,
      color 120ms;
  }

  .icon:hover {
    background: var(--surface-2);
    color: var(--text);
  }

  .icon:focus-visible {
    outline: 2px solid var(--focus-ring);
    outline-offset: 2px;
  }

  .icon svg {
    fill: currentColor;
  }
</style>
