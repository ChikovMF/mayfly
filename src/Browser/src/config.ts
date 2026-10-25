export interface Config {
  server: string
}

export async function loadConfig(): Promise<Config> {
  const response = await fetch('/config.json')
  if (!response.ok) {
    throw new Error(`config.json answered ${response.status}`)
  }

  const config: unknown = await response.json()
  if (typeof config !== 'object' || config === null || !('server' in config) || typeof config.server !== 'string') {
    throw new Error('config.json has no server address')
  }

  return { server: config.server }
}
