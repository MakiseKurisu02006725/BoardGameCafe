import request from './request'

export function getList(params) {
  return request({
    url: '/boardgames',
    method: 'get',
    params
  })
}

export function getById(id) {
  return request({
    url: `/boardgames/${id}`,
    method: 'get'
  })
}

export function getBorrowed() {
  return request({
    url: '/boardgames/borrowed',
    method: 'get'
  })
}

export function create(data) {
  return request({
    url: '/boardgames',
    method: 'post',
    data
  })
}

export function update(id, data) {
  return request({
    url: `/boardgames/${id}`,
    method: 'put',
    data
  })
}

export function remove(id) {
  return request({
    url: `/boardgames/${id}`,
    method: 'delete'
  })
}

export function borrow(id) {
  return request({
    url: `/boardgames/${id}/borrow`,
    method: 'post'
  })
}

export function returnItem(id) {
  return request({
    url: `/boardgames/${id}/return`,
    method: 'post'
  })
}

export function restock(id, data) {
  return request({
    url: `/boardgames/${id}/restock`,
    method: 'post',
    data
  })
}

export function recordDamage(data) {
  return request({
    url: '/boardgames/damage',
    method: 'post',
    data
  })
}
