import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  base: './',
  server: {
    port: 5173,
    // 开发环境代理：把 /api 转发到后端，规避跨域
    proxy: {
      '/api': {
        target: 'http://localhost:5107',
        changeOrigin: true
      }
    }
  }
})
