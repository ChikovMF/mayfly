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
  const download = prepared.get(token)
  prepared.delete(token)
  event.respondWith(download === undefined ? new Response(null, { status: 404 }) : forward(download.response, download.port))
})

async function prepare(token, url, password, port) {
  try {
    const response = await fetch(url, { headers: password === null ? {} : { 'Mayfly-Password': password } })
    if (response.ok) {
      prepared.set(token, { response, port })
    }
    port.postMessage({ type: 'status', status: response.status })
  } catch {
    port.postMessage({ type: 'status', status: 0 })
  }
}

function forward(response, port) {
  const headers = new Headers()
  for (const name of ['Content-Disposition', 'Content-Type', 'Content-Length']) {
    const value = response.headers.get(name)
    if (value !== null) {
      headers.set(name, value)
    }
  }

  return new Response(count(response.body, port), { headers })
}

function count(source, port) {
  const reader = source.getReader()
  let loaded = 0
  let reportedAt = 0

  return new ReadableStream({
    async pull(controller) {
      try {
        const { done, value } = await reader.read()
        if (done) {
          controller.close()
          port.postMessage({ type: 'done' })
          return
        }

        loaded += value.byteLength
        controller.enqueue(value)
        const now = Date.now()
        if (now - reportedAt >= 100) {
          reportedAt = now
          port.postMessage({ type: 'progress', loaded })
        }
      } catch (error) {
        controller.error(error)
        port.postMessage({ type: 'interrupted' })
      }
    },
    cancel(reason) {
      port.postMessage({ type: 'interrupted' })
      return reader.cancel(reason)
    },
  })
}
