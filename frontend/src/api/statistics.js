import request from './request'

export function getDashboard() {
  return request({
    url: '/statistics/dashboard',
    method: 'get'
  })
}

export function getRevenueTrend(params) {
  return request({
    url: '/statistics/revenue-trend',
    method: 'get',
    params
  })
}

export function getPopularGames() {
  return request({
    url: '/statistics/popular-games',
    method: 'get'
  })
}

export function getMemberRfm() {
  return request({
    url: '/statistics/member-rfm',
    method: 'get'
  })
}

export function getFlow(params) {
  return request({
    url: '/statistics/flow',
    method: 'get',
    params
  })
}

export function exportExcel() {
  return request({
    url: '/statistics/export',
    method: 'get',
    responseType: 'blob'
  })
}
