import request from './request'

export function getList(params) {
  return request({
    url: '/logs',
    method: 'get',
    params
  })
}

export function cleanup(days) {
  return request({
    url: '/operationlogs/cleanup',
    method: 'post',
    params: { days }
  })
}

export function getCleanupSettings() {
  return request({
    url: '/operationlogs/cleanup-settings',
    method: 'get'
  })
}

export function updateCleanupSettings(data) {
  return request({
    url: '/operationlogs/cleanup-settings',
    method: 'put',
    data
  })
}

export function clearAll() {
  return request({
    url: '/operationlogs/all',
    method: 'delete'
  })
}
