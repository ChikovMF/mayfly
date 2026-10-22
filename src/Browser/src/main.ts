import { mount } from 'svelte'
import '@fontsource-variable/onest'
import '@fontsource-variable/jetbrains-mono'
import './app.css'
import App from './App.svelte'

mount(App, {
  target: document.getElementById('app')!,
})
