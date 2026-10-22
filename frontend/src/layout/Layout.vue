<template>
  <el-container class="layout">
    <el-aside width="200px" class="aside">
      <div class="logo">请假审批系统</div>
      <el-menu
        :default-active="$route.path"
        router
        background-color="#001529"
        text-color="#bfcbd9"
        active-text-color="#409eff"
      >
        <el-menu-item index="/pending">
          <el-icon><List /></el-icon>
          <span>待办审批</span>
        </el-menu-item>
        <el-menu-item index="/my-leaves">
          <el-icon><Document /></el-icon>
          <span>我的申请</span>
        </el-menu-item>
        <el-menu-item index="/workflows">
          <el-icon><Setting /></el-icon>
          <span>流程配置</span>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <el-container>
      <el-header class="header">
        <div class="user-info">
          <span>{{ displayName }}</span>
          <el-button link type="primary" @click="logout">退出登录</el-button>
        </div>
      </el-header>
      <el-main class="main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const displayName = computed(() => {
  const user = JSON.parse(localStorage.getItem('user') || '{}')
  return user.displayName || '未登录'
})

function logout() {
  localStorage.removeItem('token')
  localStorage.removeItem('user')
  router.push('/login')
}
</script>

<style scoped>
.layout {
  height: 100vh;
}
.aside {
  background-color: #001529;
}
.logo {
  color: #fff;
  font-size: 16px;
  font-weight: 600;
  padding: 20px 0;
  text-align: center;
}
.header {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  border-bottom: 1px solid #eee;
  background: #fff;
}
.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}
.main {
  background: #f5f7fa;
}
</style>
