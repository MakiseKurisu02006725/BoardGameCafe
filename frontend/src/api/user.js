import request from './request'

export function getList(params) {
  return request({
    url: '/users',
    method: 'get',
    params
  })
}

export function getById(id) {
  return request({
    url: `/users/${id}`,
    method: 'get'
  })
}

export function create(data) {
  return request({
    url: '/users',
    method: 'post',
    data
  })
}

export function update(id, data) {
  return request({
    url: `/users/${id}`,
    method: 'put',
    data
  })
}

export function remove(id) {
  return request({
    url: `/users/${id}`,
    method: 'delete'
  })
}

export function changePassword(data) {
  return request({
    url: '/users/change-password',
    method: 'post',
    data
  })
}

export function resetPassword(id, data) {
  return request({
    url: `/users/${id}/reset-password`,
    method: 'post',
    data
  })
}
