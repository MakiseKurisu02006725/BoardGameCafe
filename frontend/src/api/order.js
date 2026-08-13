import request from './request'

export function getList(params) {
  return request({
    url: '/orders',
    method: 'get',
    params
  })
}

export function getById(id) {
  return request({
    url: `/orders/${id}`,
    method: 'get'
  })
}

export function pay(data) {
  return request({
    url: '/orders/pay',
    method: 'post',
    data
  })
}

export function refund(id) {
  return request({
    url: `/orders/${id}/refund`,
    method: 'post'
  })
}
