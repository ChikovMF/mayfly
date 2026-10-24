export type ExpirationChoice = '1h' | '1d' | '7d' | '30d' | 'never' | 'custom'
export type DownloadsChoice = 'unlimited' | '1' | '3' | '10' | 'custom'

const presetMinutes = { '1h': 60, '1d': 1440, '7d': 10_080, '30d': 43_200 }
const unitMinutes = { m: 1, h: 60, d: 1440 }

class SendSettings {
  expiration = $state<ExpirationChoice>('never')
  customExpiration = $state('')
  downloads = $state<DownloadsChoice>('unlimited')
  customDownloads = $state('')
  passwordEnabled = $state(false)
  password = $state('')

  get customExpirationMinutes(): number | null {
    const match = /^(\d+)([mhd])$/.exec(this.customExpiration.trim().toLowerCase())
    if (match === null) {
      return null
    }

    const minutes = Number(match[1]) * unitMinutes[match[2] as keyof typeof unitMinutes]
    return minutes > 0 ? minutes : null
  }

  get customMaxDownloads(): number | null {
    const text = this.customDownloads.trim()
    return /^\d+$/.test(text) && Number(text) > 0 ? Number(text) : null
  }

  get expiresInMinutes(): number | null {
    if (this.expiration === 'never') {
      return null
    }

    return this.expiration === 'custom' ? this.customExpirationMinutes : presetMinutes[this.expiration]
  }

  get maxDownloads(): number | null {
    if (this.downloads === 'unlimited') {
      return null
    }

    return this.downloads === 'custom' ? this.customMaxDownloads : Number(this.downloads)
  }

  get passwordValue(): string | null {
    return this.passwordEnabled && this.password !== '' ? this.password : null
  }

  togglePassword() {
    this.passwordEnabled = !this.passwordEnabled
    if (!this.passwordEnabled) {
      this.password = ''
    }
  }

  get isValid(): boolean {
    return (
      (!this.passwordEnabled || this.password !== '') &&
      (this.expiration !== 'custom' || this.customExpirationMinutes !== null) &&
      (this.downloads !== 'custom' || this.customMaxDownloads !== null)
    )
  }
}

export const settings = new SendSettings()
