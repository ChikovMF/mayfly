import { UploadNetworkError, UploadRejectedError } from '../api/MayflyApi'
import { ZipTooLargeError } from './zip'

export interface UploadFailure {
  code: string
  title: string
  text: string
  again: boolean
  chooseAnother: boolean
}

export interface FailureContext {
  name: string
  sentPercent: number | null
}

export function describeFailure(exception: unknown, context: FailureContext): UploadFailure {
  if (exception instanceof UploadRejectedError) {
    return describeRejection(exception)
  }

  if (exception instanceof UploadNetworkError) {
    return {
      code: 'сеть',
      title: 'Нет связи с сервером',
      text:
        context.sentPercent === null
          ? 'Ответ от сервера не получен. Файл мог не сохраниться, повторите отправку.'
          : `Отправка ${context.name} прервалась на ${context.sentPercent}%. Файл остался выбран, можно повторить, когда появится связь.`,
      again: true,
      chooseAnother: false,
    }
  }

  if (exception instanceof ZipTooLargeError) {
    return {
      code: 'архив',
      title: 'Слишком большой архив',
      text: 'Эти файлы не помещаются в один архив. Отправьте их частями.',
      again: false,
      chooseAnother: true,
    }
  }

  return {
    code: 'файлы',
    title: 'Не удалось прочитать файлы',
    text: 'Файлы изменились или стали недоступны. Выберите их заново.',
    again: false,
    chooseAnother: true,
  }
}

function describeRejection({ status, fields }: UploadRejectedError): UploadFailure {
  const code = String(status)

  if (status === 413) {
    return {
      code,
      title: 'Файл слишком большой',
      text: 'Сервер не принимает файлы такого размера. Выберите файл поменьше.',
      again: false,
      chooseAnother: true,
    }
  }

  if (status === 400) {
    return describeBadRequest(fields)
  }

  if (status === 429) {
    return {
      code,
      title: 'Слишком много запросов',
      text: 'Сервер просит подождать. Повторите чуть позже.',
      again: true,
      chooseAnother: false,
    }
  }

  return {
    code,
    title: 'Сервер не смог принять файл',
    text: 'Попробуйте ещё раз чуть позже.',
    again: true,
    chooseAnother: false,
  }
}

function describeBadRequest(fields: string[]): UploadFailure {
  const settingsHint = 'Остальные настройки сохранены.'
  const base = { code: '400', again: false, chooseAnother: false }

  if (fields.includes('expiresAt')) {
    return { ...base, title: 'Нужен другой срок хранения', text: `Сервер не принимает выбранный срок. Выберите другой. ${settingsHint}` }
  }

  if (fields.includes('maxDownloads')) {
    return { ...base, title: 'Нужно другое число скачиваний', text: `Сервер не принимает выбранное число скачиваний. Выберите другое. ${settingsHint}` }
  }

  if (fields.includes('password')) {
    return { ...base, title: 'Пароль не подходит', text: `Сервер не принимает этот пароль. Задайте другой. ${settingsHint}` }
  }

  if (fields.includes('sha256')) {
    return { ...base, again: true, title: 'Файл повредился при передаче', text: 'Содержимое не совпало при проверке. Отправьте ещё раз.' }
  }

  return { ...base, title: 'Сервер отклонил запрос', text: `Проверьте настройки и попробуйте ещё раз.` }
}
