import { MediaQuery } from 'svelte/reactivity'

type Theme = 'light' | 'dark'

const storageKey = 'theme'
const systemDark = new MediaQuery('prefers-color-scheme: dark')

let chosen = $state<Theme | null>(readChosen())
applyColorScheme()

export const theme = {
  get current(): Theme {
    return chosen ?? (systemDark.current ? 'dark' : 'light')
  },
  toggle() {
    chosen = theme.current === 'dark' ? 'light' : 'dark'
    applyColorScheme()
    try {
      localStorage.setItem(storageKey, chosen)
    } catch {
      // The choice then lasts until the page is closed.
    }
  },
}

function readChosen(): Theme | null {
  try {
    const value = localStorage.getItem(storageKey)
    return value === 'light' || value === 'dark' ? value : null
  } catch {
    return null
  }
}

function applyColorScheme() {
  document.documentElement.style.colorScheme = chosen ?? 'light dark'
}
