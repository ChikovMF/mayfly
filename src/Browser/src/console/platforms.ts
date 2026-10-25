export type Platform = 'linux-x64' | 'linux-arm64' | 'win-x64' | 'osx-arm64'

export const platforms: { value: Platform; label: string }[] = [
  { value: 'linux-x64', label: 'Linux x64' },
  { value: 'linux-arm64', label: 'Linux ARM64' },
  { value: 'win-x64', label: 'Windows x64' },
  { value: 'osx-arm64', label: 'macOS ARM64' },
]

export function archiveName(platform: Platform): string {
  return `mayfly-${platform}.${platform === 'win-x64' ? 'zip' : 'tar.gz'}`
}

export function detectPlatform(): Platform {
  const agent = navigator.userAgent
  if (/windows/i.test(agent)) {
    return 'win-x64'
  }

  if (/macintosh|mac os x/i.test(agent)) {
    return 'osx-arm64'
  }

  return /aarch64|arm64/i.test(agent) ? 'linux-arm64' : 'linux-x64'
}
