export type Route =
  | { page: 'home' }
  | { page: 'history' }
  | { page: 'file'; id: string }
  | { page: 'not-found' }

const fileIdPattern = /^\/([1-9A-HJ-NP-Za-km-z]{10})$/

let path = $state(location.pathname)

window.addEventListener('popstate', () => {
  path = location.pathname
})

document.addEventListener('click', (event) => {
  if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
    return
  }

  const link = (event.target as Element).closest('a')
  if (link === null || link.target !== '' || link.hasAttribute('download') || link.origin !== location.origin) {
    return
  }

  event.preventDefault()
  navigate(link.pathname)
})

export const router = {
  get route(): Route {
    return match(path)
  },
}

function navigate(to: string) {
  if (to === path) {
    return
  }

  history.pushState(null, '', to)
  path = to
  window.scrollTo(0, 0)
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
