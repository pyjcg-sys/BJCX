<template>
  <div class="login-page">
    <el-card class="login-card">
      <h2 class="title">请假审批系统</h2>
      <el-form @submit.prevent="handleLogin">
        <el-form-item>
          <el-input v-model="form.userName" placeholder="用户名" size="large">
            <template #prefix><el-icon><User /></el-icon></template>
          </el-input>
        </el-form-item>
        <el-form-item>
          <el-input
            v-model="form.password"
            type="password"
            placeholder="密码"
            size="large"
            show-password
            @keyup.enter="handleLogin"
          >
            <template #prefix><el-icon><Lock /></el-icon></template>
          </el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" size="large" class="login-btn" :loading="loading" @click="handleLogin">
            登录
          </el-button>
        </el-form-item>
      </el-form>

      <div class="tips">
        <div class="tips-title">演示账号（密码均为 123456，点击快速填入）：</div>
        <el-tag v-for="acc in accounts" :key="acc.userName" class="acc-tag" @click="fill(acc)">
          {{ acc.label }}
        </el-tag>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { login } from '../api/auth'

const router = useRouter()
const loading = ref(false)
const form = reactive({ userName: '', password: '' })

const accounts = [
  { label: '张三（申请人）', userName: 'zhangsan' },
  { label: '李四（直属领导）', userName: 'lisi' },
  { label: '王五（部门经理）', userName: 'wangwu' }
]

function fill(acc) {
  form.userName = acc.userName
  form.password = '123456'
}

async function handleLogin() {
  if (!form.userName || !form.password) {
    ElMessage.warning('请输入用户名和密码')
    return
  }
  loading.value = true
  try {
    const data = await login({ userName: form.userName, password: form.password })
    localStorage.setItem('token', data.token)
    localStorage.setItem('user', JSON.stringify(data))
    ElMessage.success(`欢迎，${data.displayName}`)
    router.push('/pending')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #1f3b64, #2d5f8b);
}
.login-card {
  width: 380px;
}
.title {
  text-align: center;
  margin-bottom: 24px;
}
.login-btn {
  width: 100%;
}
.tips {
  font-size: 12px;
  color: #999;
}
.tips-title {
  margin-bottom: 8px;
}
.acc-tag {
  margin: 0 6px 6px 0;
  cursor: pointer;
}
</style>
