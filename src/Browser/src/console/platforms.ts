export type System = 'linux' | 'windows' | 'macos'

export interface Build {
  archive: string
  label: string
}

export interface SystemInfo {
  id: System
  name: string
  builds: Build[]
}

export const systems: SystemInfo[] = [
  {
    id: 'linux',
    name: 'Linux',
    builds: [
      { archive: 'mayfly-linux-x64.tar.gz', label: 'x64' },
      { archive: 'mayfly-linux-arm64.tar.gz', label: 'ARM64' },
    ],
  },
  {
    id: 'windows',
    name: 'Windows',
    builds: [{ archive: 'mayfly-win-x64.zip', label: 'x64' }],
  },
  {
    id: 'macos',
    name: 'macOS',
    builds: [{ archive: 'mayfly-osx-arm64.tar.gz', label: 'ARM64' }],
  },
]

export function detectSystem(): System {
  const agent = navigator.userAgent
  if (/windows/i.test(agent)) {
    return 'windows'
  }

  if (/macintosh|mac os x/i.test(agent)) {
    return 'macos'
  }

  return 'linux'
}
