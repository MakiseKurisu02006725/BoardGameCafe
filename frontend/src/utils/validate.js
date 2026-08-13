/**
 * 校验手机号码
 * @param {string} phone - 手机号码
 * @returns {boolean} 是否为有效手机号
 */
export function isPhone(phone) {
  const reg = /^1[3-9]\d{9}$/
  return reg.test(phone)
}

/**
 * 校验邮箱地址
 * @param {string} email - 邮箱地址
 * @returns {boolean} 是否为有效邮箱
 */
export function isEmail(email) {
  const reg = /^[\w.-]+@[\w.-]+\.\w+$/
  return reg.test(email)
}

/**
 * 校验是否为数字（含整数和小数）
 * @param {string|number} num - 待校验的值
 * @returns {boolean} 是否为有效数字
 */
export function isNumber(num) {
  if (typeof num === 'number') {
    return !isNaN(num) && isFinite(num)
  }
  if (typeof num === 'string' && num.trim() !== '') {
    const reg = /^-?\d+(\.\d+)?$/
    return reg.test(num.trim())
  }
  return false
}

/**
 * 格式化金额显示
 * @param {number|string} money - 金额数值
 * @param {number} decimals - 保留小数位数，默认2位
 * @returns {string} 格式化后的金额字符串
 */
export function formatMoney(money, decimals = 2) {
  if (!isNumber(money)) {
    return '0.00'
  }
  const num = parseFloat(money)
  return num.toLocaleString('zh-CN', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals
  })
}

/**
 * 校验身份证号码
 * @param {string} idCard - 身份证号码
 * @returns {boolean} 是否为有效身份证
 */
export function isIdCard(idCard) {
  const reg = /(^\d{15}$)|(^\d{17}(\d|X|x)$)/
  return reg.test(idCard)
}

/**
 * 校验是否为URL地址
 * @param {string} url - URL地址
 * @returns {boolean} 是否为有效URL
 */
export function isUrl(url) {
  const reg = /^(https?:\/\/|ftp:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$/
  return reg.test(url)
}

/**
 * 校验是否为空（空字符串、null、undefined、空数组、空对象）
 * @param {any} value - 待校验的值
 * @returns {boolean} 是否为空
 */
export function isEmpty(value) {
  if (value === null || value === undefined) {
    return true
  }
  if (typeof value === 'string') {
    return value.trim() === ''
  }
  if (Array.isArray(value)) {
    return value.length === 0
  }
  if (typeof value === 'object') {
    return Object.keys(value).length === 0
  }
  return false
}
