<template>
  <div>
    <el-card>
      <template #header>我的邮件</template>

      <el-table v-loading="loading" :data="list" border>
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="toUserName" label="收件人" width="120" />
        <el-table-column prop="subject" label="主题" min-width="200" show-overflow-tooltip />
        <el-table-column label="正文" min-width="320" show-overflow-tooltip>
          <template #default="{ row }">{{ row.body }}</template>
        </el-table-column>
        <el-table-column label="发送时间" width="180">
          <template #default="{ row }">{{ formatTime(row.sentAt) }}</template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-model:current-page="query.pageIndex"
        v-model:page-size="query.pageSize"
        :total="total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        class="pagination"
        @change="load"
      />
    </el-card>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { getEmails } from '../api/email'

const list = ref([])
const total = ref(0)
const loading = ref(false)
const query = reactive({ pageIndex: 1, pageSize: 20 })

async function load() {
  loading.value = true
  try {
    const data = await getEmails({ pageIndex: query.pageIndex, pageSize: query.pageSize })
    list.value = data.items
    total.value = data.totalCount
  } finally {
    loading.value = false
  }
}

function formatTime(t) {
  return t ? t.replace('T', ' ').slice(0, 19) : ''
}

load()
</script>

<style scoped>
.pagination {
  margin-top: 16px;
  justify-content: flex-end;
}
</style>
