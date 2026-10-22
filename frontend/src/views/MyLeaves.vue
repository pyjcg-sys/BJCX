<template>
  <div>
    <el-card class="mb">
      <template #header>提交请假申请</template>
      <el-form :model="form" label-width="100px" class="submit-form">
        <el-form-item label="请假类型" required>
          <el-select v-model="form.leaveType" placeholder="请选择请假类型">
            <el-option v-for="opt in LEAVE_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="起止日期" required>
          <el-date-picker
            v-model="dateRange"
            type="daterange"
            value-format="YYYY-MM-DD"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
          />
        </el-form-item>
        <el-form-item label="请假事由">
          <el-input v-model="form.reason" type="textarea" :rows="3" placeholder="请假事由（类型为其他时必填）" />
        </el-form-item>
        <el-form-item label="附件">
          <el-upload v-model:file-list="fileList" :auto-upload="false" multiple>
            <el-button type="primary" plain>选择文件</el-button>
            <template #tip>
              <div class="el-upload__tip">支持多附件，单个不超过 50MB</div>
            </template>
          </el-upload>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :loading="submitting" @click="handleSubmit">提交申请</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card>
      <template #header>我的申请</template>
      <el-table v-loading="loading" :data="list" border>
        <el-table-column prop="requestNo" label="单据编号" width="200" />
        <el-table-column label="请假类型" width="100">
          <template #default="{ row }">{{ leaveTypeText(row.leaveType) }}</template>
        </el-table-column>
        <el-table-column label="起止日期" width="220">
          <template #default="{ row }">{{ formatDate(row.startDate) }} ~ {{ formatDate(row.endDate) }}</template>
        </el-table-column>
        <el-table-column prop="days" label="天数" width="80" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)">{{ leaveStatusText(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="reason" label="事由" show-overflow-tooltip />
      </el-table>

      <el-pagination
        v-model:current-page="query.pageIndex"
        v-model:page-size="query.pageSize"
        :total="total"
        :page-sizes="[5, 10, 20]"
        layout="total, sizes, prev, pager, next"
        class="pagination"
        @change="load"
      />
    </el-card>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { submitLeave, getMyLeaves } from '../api/leave'
import { LEAVE_TYPE_OPTIONS, LEAVE_TYPE_MAP, LEAVE_STATUS_MAP } from '../utils/enums'

const form = reactive({ leaveType: '', reason: '' })
const dateRange = ref([])
const fileList = ref([])
const submitting = ref(false)

const list = ref([])
const total = ref(0)
const loading = ref(false)
const query = reactive({ pageIndex: 1, pageSize: 10 })

async function handleSubmit() {
  if (!form.leaveType) {
    ElMessage.warning('请选择请假类型')
    return
  }
  if (!dateRange.value || dateRange.value.length !== 2) {
    ElMessage.warning('请选择起止日期')
    return
  }

  // 组装 multipart 表单：字段 + 多附件
  const formData = new FormData()
  formData.append('leaveType', form.leaveType)
  formData.append('startDate', dateRange.value[0])
  formData.append('endDate', dateRange.value[1])
  formData.append('reason', form.reason || '')
  fileList.value.forEach((f) => formData.append('attachments', f.raw))

  submitting.value = true
  try {
    await submitLeave(formData)
    ElMessage.success('提交成功')
    form.reason = ''
    dateRange.value = []
    fileList.value = []
    query.pageIndex = 1
    load()
  } finally {
    submitting.value = false
  }
}

async function load() {
  loading.value = true
  try {
    const data = await getMyLeaves({ pageIndex: query.pageIndex, pageSize: query.pageSize })
    list.value = data.items
    total.value = data.totalCount
  } finally {
    loading.value = false
  }
}

function leaveTypeText(t) {
  return LEAVE_TYPE_MAP[t] || t
}
function leaveStatusText(s) {
  return LEAVE_STATUS_MAP[s] || s
}
function statusType(s) {
  if (s === 'Approved') return 'success'
  if (s === 'Rejected') return 'danger'
  return 'warning'
}
function formatDate(d) {
  return d ? d.slice(0, 10) : ''
}

load()
</script>

<style scoped>
.mb {
  margin-bottom: 16px;
}
.submit-form {
  max-width: 560px;
}
.pagination {
  margin-top: 16px;
  justify-content: flex-end;
}
</style>
