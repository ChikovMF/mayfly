import { tick } from 'svelte'
import { isFileId } from '../file/fileId'

export type Route =
  | { page: 'home' }
  | { page: 'file'; id: string }
  | { page: 'not-found' }

let path = $state(location.pathname)

if ('navigation' in window) {
  navigation.addEventListener('navigate', (event) => {
    if (!event.canIntercept || event.navigationType === 'reload' || event.hashChange || event.downloadRequest !== null) {
      return
    }

    const destination = new URL(event.destination.url)
    event.intercept({
      async handler() {
        path = destination.pathname
        await tick()
      },
    })
  })
}

export const router = {
  get route(): Route {
    return match(path)
  },
}

function match(path: string): Route {
  if (path === '/') {
    return { page: 'home' }
  }

  const id = path.slice(1)
  if (isFileId(id)) {
    return { page: 'file', id }
  }

  return { page: 'not-found' }
}
