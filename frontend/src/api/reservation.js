import request from './request'

export function getList(params) {
  return request({
    url: '/reservations',
    method: 'get',
    params
  })
}

export function getById(id) {
  return request({
    url: `/reservations/${id}`,
    method: 'get'
  })
}

export function create(data) {
  return request({
    url: '/reservations',
    method: 'post',
    data
  })
}

export function confirm(id) {
  return request({
    url: `/reservations/${id}/confirm`,
    method: 'put'
  })
}

export function cancel(id) {
  return request({
    url: `/reservations/${id}/cancel`,
    method: 'put'
  })
}

export function complete(id) {
  return request({
    url: `/reservations/${id}/complete`,
    method: 'put'
  })
}

export function convertToOrder(id, tableId) {
  return request({
    url: `/reservations/${id}/convert`,
    method: 'post',
    params: tableId ? { tableId } : undefined
  })
}
