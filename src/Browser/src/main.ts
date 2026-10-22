import { mount } from 'svelte'
import '@fontsource-variable/onest'
import '@fontsource-variable/jetbrains-mono'
import './app.css'
import { apiContext } from './api/context'
import { MayflyApi } from './api/MayflyApi'
import App from './App.svelte'
import { loadConfig } from './config'

const target = document.getElementById('app')!
const config = await loadConfig().catch((error: unknown) => {
  console.error('Mayfly could not load config.json:', error)
  return null
})

if (config === null) {
  target.textContent = 'Не удалось загрузить настройки'
} else {
  mount(App, { target, context: apiContext(new MayflyApi(config.server)) })
}
