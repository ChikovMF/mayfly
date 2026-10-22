import { getContext } from 'svelte'
import type { MayflyApi } from './MayflyApi'

const key = Symbol('api')

export function apiContext(api: MayflyApi): Map<symbol, MayflyApi> {
  return new Map([[key, api]])
}

export function getApi(): MayflyApi {
  return getContext(key)
}
