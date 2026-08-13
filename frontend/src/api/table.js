import request from './request'

export function getList(params) {
  return request({
    url: '/tables',
    method: 'get',
    params
  })
}

export function getById(id) {
  return request({
    url: `/tables/${id}`,
    method: 'get'
  })
}

export function create(data) {
  return request({
    url: '/tables',
    method: 'post',
    data
  })
}

export function update(id, data) {
  return request({
    url: `/tables/${id}`,
    method: 'put',
    data
  })
}

export function remove(id) {
  return request({
    url: `/tables/${id}`,
    method: 'delete'
  })
}

export function maintain(id, status) {
  return request({
    url: `/tables/${id}/maintain`,
    method: 'put',
    data: { status }
  })
}

export function openTable(data) {
  return request({
    url: '/tables/open',
    method: 'post',
    data
  })
}

export function changeTable(data) {
  return request({
    url: '/tables/change',
    method: 'post',
    data
  })
}

export function closeTable(data) {
  return request({
    url: '/tables/close',
    method: 'post',
    data
  })
}
