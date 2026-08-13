import request from './request'

export function getList(params) {
  return request({
    url: '/members',
    method: 'get',
    params
  })
}

export function getById(id) {
  return request({
    url: `/members/${id}`,
    method: 'get'
  })
}

export function create(data) {
  return request({
    url: '/members',
    method: 'post',
    data
  })
}

export function update(id, data) {
  return request({
    url: `/members/${id}`,
    method: 'put',
    data
  })
}

export function updateProfile(data) {
  return request({
    url: '/members/profile',
    method: 'put',
    data
  })
}

export function recharge(data) {
  return request({
    url: '/members/recharge',
    method: 'post',
    data
  })
}

export function getRecharges(id, params) {
  return request({
    url: `/members/${id}/recharges`,
    method: 'get',
    params
  })
}

export function getOrders(id, params) {
  return request({
    url: `/members/${id}/orders`,
    method: 'get',
    params
  })
}

export function getCoupons(id, params) {
  return request({
    url: `/members/${id}/coupons`,
    method: 'get',
    params
  })
}

export function getBalanceRecords(id, params) {
  return request({
    url: `/members/${id}/balance-records`,
    method: 'get',
    params
  })
}

export function remove(id) {
  return request({
    url: `/members/${id}`,
    method: 'delete'
  })
}
