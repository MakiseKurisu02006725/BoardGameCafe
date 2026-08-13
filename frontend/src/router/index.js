import { createRouter, createWebHistory } from 'vue-router'
import NProgress from 'nprogress'
import 'nprogress/nprogress.css'
import { useUserStore } from '@/store/modules/user'
import { ElMessage } from 'element-plus'

NProgress.configure({ showSpinner: false })

const Layout = () => import('@/layout/index.vue')

export const constantRoutes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/LoginView.vue'),
    meta: { title: '登录', public: true }
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/RegisterView.vue'),
    meta: { title: '注册', public: true }
  },
  {
    path: '/404',
    name: 'NotFound',
    component: () => import('@/views/NotFoundView.vue'),
    meta: { title: '404', public: true }
  },
  {
    path: '/401',
    name: 'Forbidden',
    component: () => import('@/views/ForbiddenView.vue'),
    meta: { title: '401', public: true }
  },
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'CatchAll',
    redirect: '/404',
    meta: { title: '404', public: true }
  }
]

export const asyncRoutes = [
  {
    path: '',
    component: Layout,
    redirect: '/dashboard',
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/statistics/Dashboard.vue'),
        meta: {
          title: '经营大屏',
          icon: 'DataAnalysis',
          roles: ['Admin', 'Manager']
        }
      },
      {
        path: 'tables',
        name: 'TableList',
        component: () => import('@/views/tables/TableList.vue'),
        meta: {
          title: '桌位管理',
          icon: 'Table',
          roles: ['Admin', 'Manager', 'Cashier']
        }
      },
      {
        path: 'tables/board',
        name: 'TableBoard',
        component: () => import('@/views/tables/TableBoard.vue'),
        meta: {
          title: '实时看板',
          icon: 'Monitor',
          roles: ['Admin', 'Manager', 'Cashier']
        }
      },
      {
        path: 'boardgames',
        name: 'BoardGameList',
        component: () => import('@/views/boardgames/BoardGameList.vue'),
        meta: {
          title: '桌游管理',
          icon: 'Goods',
          roles: ['Admin', 'Manager', 'Cashier']
        }
      },
      {
        path: 'members',
        name: 'MemberList',
        component: () => import('@/views/members/MemberList.vue'),
        meta: {
          title: '会员管理',
          icon: 'User',
          roles: ['Admin', 'Manager', 'Cashier']
        }
      },
      {
        path: 'members/profile',
        name: 'MemberProfile',
        component: () => import('@/views/members/MemberProfile.vue'),
        meta: {
          title: '个人中心',
          icon: 'UserFilled',
          roles: ['Member']
        }
      },
      {
        path: 'members/:id',
        name: 'MemberDetail',
        component: () => import('@/views/members/MemberDetail.vue'),
        meta: {
          title: '会员详情',
          icon: 'User',
          hidden: true, // 仅通过会员列表行进入，不在侧边栏显示
          roles: ['Admin', 'Manager', 'Cashier']
        }
      },
      {
        path: 'reservations',
        name: 'ReservationList',
        component: () => import('@/views/reservations/ReservationList.vue'),
        meta: {
          title: '预约管理',
          icon: 'Calendar',
          roles: ['Admin', 'Manager', 'Cashier', 'Member']
        }
      },
      {
        path: 'orders',
        name: 'OrderList',
        component: () => import('@/views/orders/OrderList.vue'),
        meta: {
          title: '订单管理',
          icon: 'List',
          roles: ['Admin', 'Manager', 'Cashier']
        }
      },
      {
        path: 'orders/checkout/:id',
        name: 'Checkout',
        component: () => import('@/views/orders/CheckoutView.vue'),
        meta: {
          title: '收银结账',
          icon: 'Money',
          hidden: true, // 仅通过订单列表行进入，不在侧边栏显示
          roles: ['Admin', 'Manager', 'Cashier']
        }
      },
      {
        path: 'statistics',
        name: 'Statistics',
        component: () => import('@/views/statistics/Statistics.vue'),
        meta: {
          title: '数据分析',
          icon: 'TrendCharts',
          roles: ['Admin', 'Manager']
        }
      },
      {
        path: 'users',
        name: 'UserList',
        component: () => import('@/views/users/UserList.vue'),
        meta: {
          title: '员工管理',
          icon: 'UserFilled',
          roles: ['Admin', 'Manager']
        }
      },
      {
        path: 'logs',
        name: 'OperationLog',
        component: () => import('@/views/logs/OperationLog.vue'),
        meta: {
          title: '操作日志',
          icon: 'Document',
          roles: ['Admin', 'Manager']
        }
      }
    ]
  }
]

function filterAsyncRoutes(routes, roles) {
  const res = []
  routes.forEach(route => {
    const tmp = { ...route }
    if (hasPermission(roles, tmp)) {
      if (tmp.children) {
        tmp.children = filterAsyncRoutes(tmp.children, roles)
      }
      res.push(tmp)
    }
  })
  return res
}

function hasPermission(roles, route) {
  if (route.meta && route.meta.roles) {
    return roles.some(role => route.meta.roles.includes(role))
  }
  return true
}

const whiteList = ['/login', '/register', '/404', '/401']

const router = createRouter({
  history: createWebHistory(),
  routes: constantRoutes
})

let routesAdded = false
// Vue Router 4：addRoute() 返回一个「移除该路由的函数」，用它来实现真正的路由回滚
const addedRouteRemovers = []

export function generateAndAddRoutes(role) {
  if (!role) return []
  const accessRoutes = filterAsyncRoutes(asyncRoutes, [role])
  accessRoutes.forEach(route => {
    try {
      const removeFn = router.addRoute(route)
      addedRouteRemovers.push(removeFn)
    } catch (e) {
      console.error('addRoute failed:', route, e)
    }
  })
  routesAdded = true
  return accessRoutes
}

router.beforeEach(async (to, from, next) => {
  NProgress.start()
  const userStore = useUserStore()

  if (to.meta && to.meta.title) {
    document.title = `${to.meta.title} - 桌游馆管理系统`
  }

  // 刷新场景：动态路由尚未注册时，CatchAll 会把任意业务URL（如 /members/123）
  // 在守卫执行前就重定向到 /404，导致 to.path === '/404' 而真实浏览器URL是业务页面。
  // 此时必须先加载动态路由，再跳回真实URL，否则会一直停在404页。
  const realPathname = window.location.pathname
  const realFullPath = realPathname + window.location.search
  const isCatchAll404OnRefresh = to.path === '/404' && realPathname !== '/404' && !routesAdded

  if (isCatchAll404OnRefresh) {
    if (!userStore.token) {
      next(`/login?redirect=${encodeURIComponent(realFullPath)}`)
      return
    }
    if (!userStore.rolesLoaded) {
      try {
        await userStore.getUserInfo()
      } catch (error) {
        userStore.token = ''
        userStore.refreshToken = ''
        userStore.rolesLoaded = false
        userStore.routes = []
        localStorage.removeItem('token')
        localStorage.removeItem('refreshToken')
        resetRouter()
        next(`/login?redirect=${encodeURIComponent(realFullPath)}`)
        return
      }
    }
    const accessRoutes = generateAndAddRoutes(userStore.role)
    userStore.routes = accessRoutes
    // 跳回真实URL（保留 query），动态路由已注册，再次匹配将命中真实页面而非 CatchAll
    next({ path: realPathname, query: Object.fromEntries(new URLSearchParams(window.location.search)), replace: true })
    return
  }

  if (whiteList.includes(to.path)) {
    next()
    return
  }

  if (!userStore.token) {
    next(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
    return
  }

  if (!userStore.rolesLoaded) {
    try {
      await userStore.getUserInfo()
    } catch (error) {
      // getUserInfo 失败（token真的失效）：直接紧急清场（不要调userStore.logout避免递归）
      userStore.token = ''
      userStore.refreshToken = ''
      userStore.rolesLoaded = false
      userStore.routes = []
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      resetRouter()
      next(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
      return
    }
  }

  if (!routesAdded) {
    const accessRoutes = generateAndAddRoutes(userStore.role)
    userStore.routes = accessRoutes
    // 首次 addRoute 后必须重新匹配（否则 to.path 此时还不在路由表里）
    next({ ...to, replace: true })
    return
  }

  if (to.meta && to.meta.roles && to.meta.roles.length > 0) {
    if (!to.meta.roles.includes(userStore.role)) {
      ElMessage.error('权限不足')
      next('/401')
      return
    }
  }

  next()
})

router.afterEach((to) => {
  NProgress.done()
  if (to.meta && to.meta.title) {
    document.title = `${to.meta.title} - 桌游馆管理系统`
  }
})

export function resetRouter() {
  // Vue Router 4 官方推荐：逐个调用 addRoute 返回的 remover 函数
  while (addedRouteRemovers.length > 0) {
    const remove = addedRouteRemovers.pop()
    try { remove() } catch (e) { /* ignore already removed */ }
  }
  routesAdded = false
}

export default router
