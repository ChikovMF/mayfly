const downloadPrefix = '/download/'
const pending = new Map()

self.addEventListener('install', () => self.skipWaiting())
self.addEventListener('activate', (event) => event.waitUntil(self.clients.claim()))

self.addEventListener('message', (event) => {
  const { token, url, password } = event.data
  pending.set(token, { url, password })
  event.ports[0].postMessage(null)
})

self.addEventListener('fetch', (event) => {
  const { pathname } = new URL(event.request.url)
  if (!pathname.startsWith(downloadPrefix)) {
    return
  }

  const token = pathname.slice(downloadPrefix.length)
  const entry = pending.get(token)
  pending.delete(token)
  event.respondWith(entry === undefined ? new Response(null, { status: 404 }) : download(entry))
})

async function download({ url, password }) {
  const response = await fetch(url, { headers: { 'Mayfly-Password': password } })

  const headers = new Headers()
  for (const name of ['Content-Disposition', 'Content-Type', 'Content-Length']) {
    const value = response.headers.get(name)
    if (value !== null) {
      headers.set(name, value)
    }
  }

  return new Response(response.ok ? response.body : null, { status: response.status, headers })
}
