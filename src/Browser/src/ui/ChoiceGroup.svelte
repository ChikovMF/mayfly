<script lang="ts" generics="T extends string">
  let {
    labelledby,
    name,
    options,
    value = $bindable(),
  }: { labelledby: string; name: string; options: { value: T; label: string }[]; value: T } = $props()
</script>

<div role="radiogroup" aria-labelledby={labelledby}>
  {#each options as option (option.value)}
    <label>
      <input type="radio" {name} value={option.value} bind:group={value} />
      <span>{option.label}</span>
    </label>
  {/each}
</div>

<style>
  div {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
  }

  label {
    position: relative;
  }

  input {
    position: absolute;
    inset: 0;
    opacity: 0;
    margin: 0;
    cursor: pointer;
  }

  span {
    display: inline-flex;
    align-items: center;
    height: 44px;
    padding: 0 18px;
    border: 1px solid var(--border-strong);
    border-radius: 999px;
    background: var(--bg);
    color: var(--text);
    font: 500 15px var(--font-sans);
    transition:
      background-color 120ms,
      color 120ms;
  }

  input:hover + span {
    background: var(--surface-2);
  }

  input:checked + span {
    border-color: var(--text);
    background: var(--text);
    color: var(--bg);
  }

  input:focus-visible + span {
    outline: 2px solid var(--focus-ring);
    outline-offset: 2px;
  }
</style>
