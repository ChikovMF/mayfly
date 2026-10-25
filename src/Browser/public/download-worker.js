const downloadPrefix = '/download/'
const prepared = new Map()

self.addEventListener('install', () => self.skipWaiting())
self.addEventListener('activate', (event) => event.waitUntil(self.clients.claim()))

self.addEventListener('message', (event) => {
  const { token, url, password } = event.data
  event.waitUntil(prepare(token, url, password, event.ports[0]))
})

self.addEventListener('fetch', (event) => {
  const { pathname } = new URL(event.request.url)
  if (!pathname.startsWith(downloadPrefix)) {
    return
  }

  const token = pathname.slice(downloadPrefix.length)
  const response = prepared.get(token)
  prepared.delete(token)
  event.respondWith(response === undefined ? new Response(null, { status: 404 }) : forward(response))
})

async function prepare(token, url, password, port) {
  try {
    const response = await fetch(url, { headers: password === null ? {} : { 'Mayfly-Password': password } })
    if (response.ok) {
      prepared.set(token, response)
    }
    port.postMessage(response.status)
  } catch {
    port.postMessage(0)
  }
}

function forward(response) {
  const headers = new Headers()
  for (const name of ['Content-Disposition', 'Content-Type', 'Content-Length']) {
    const value = response.headers.get(name)
    if (value !== null) {
      headers.set(name, value)
    }
  }

  return new Response(response.body, { headers })
}
