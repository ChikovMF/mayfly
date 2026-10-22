export type Route =
  | { page: 'home' }
  | { page: 'history' }
  | { page: 'file'; id: string }
  | { page: 'not-found' }

const fileIdPattern = /^\/([1-9A-HJ-NP-Za-km-z]{10})$/

let path = $state(location.pathname)

if ('navigation' in window) {
  navigation.addEventListener('navigate', (event) => {
    if (!event.canIntercept || event.navigationType === 'reload' || event.hashChange || event.downloadRequest !== null) {
      return
    }

    const destination = new URL(event.destination.url)
    event.intercept({
      handler() {
        path = destination.pathname
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

  if (path === '/history') {
    return { page: 'history' }
  }

  const fileId = fileIdPattern.exec(path)
  if (fileId !== null) {
    return { page: 'file', id: fileId[1] }
  }

  return { page: 'not-found' }
}
