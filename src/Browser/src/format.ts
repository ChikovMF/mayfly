const sizeUnits = ['Б', 'КБ', 'МБ', 'ГБ', 'ТБ']
const number = new Intl.NumberFormat('ru', { maximumFractionDigits: 1 })
const plural = new Intl.PluralRules('ru')

export function formatSize(bytes: number): string {
  let value = bytes
  let unit = 0
  while (value >= 1024 && unit < sizeUnits.length - 1) {
    value /= 1024
    unit++
  }

  return `${number.format(value)} ${sizeUnits[unit]}`
}

export function formatTimeLeft(until: Date): string {
  const minutes = Math.floor((until.getTime() - Date.now()) / 60_000)
  if (minutes < 1) {
    return 'меньше минуты'
  }

  const parts = [
    count(Math.floor(minutes / 1440), ['день', 'дня', 'дней']),
    count(Math.floor(minutes / 60) % 24, ['час', 'часа', 'часов']),
    count(minutes % 60, ['минута', 'минуты', 'минут']),
  ].filter((part) => part !== null)

  return parts.slice(0, 2).join(' ')
}

export function formatDate(date: Date): string {
  return date.toLocaleString('ru', {
    day: 'numeric',
    month: 'long',
    year: date.getFullYear() === new Date().getFullYear() ? undefined : 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function count(value: number, forms: [one: string, few: string, many: string]): string | null {
  if (value === 0) {
    return null
  }

  const form = plural.select(value)
  return `${value} ${form === 'one' ? forms[0] : form === 'few' ? forms[1] : forms[2]}`
}
