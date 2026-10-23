import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/login',
    component: () => import('../views/Login.vue')
  },
  {
    path: '/',
    component: () => import('../layout/Layout.vue'),
    redirect: '/pending',
    children: [
      {
        path: 'pending',
        name: 'Pending',
        component: () => import('../views/PendingApproval.vue'),
        meta: { title: '待办审批' }
      },
      {
        path: 'my-leaves',
        name: 'MyLeaves',
        component: () => import('../views/MyLeaves.vue'),
        meta: { title: '我的申请' }
      },
      {
        path: 'workflows',
        name: 'Workflows',
        component: () => import('../views/WorkflowConfig.vue'),
        meta: { title: '流程配置' }
      },
      {
        path: 'emails',
        name: 'Emails',
        component: () => import('../views/Emails.vue'),
        meta: { title: '邮件记录' }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 登录守卫：未登录跳登录页，已登录访问登录页则跳待办
router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('token')
  if (to.path !== '/login' && !token) {
    next('/login')
  } else if (to.path === '/login' && token) {
    next('/pending')
  } else {
    next()
  }
})

export default router
