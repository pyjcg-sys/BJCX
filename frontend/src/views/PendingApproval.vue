<template>
  <div>
    <el-card>
      <template #header>待办审批</template>

      <el-table v-loading="loading" :data="list" border>
        <el-table-column prop="requestNo" label="单据编号" width="200" />
        <el-table-column prop="applicantName" label="申请人" width="120" />
        <el-table-column label="请假类型" width="100">
          <template #default="{ row }">{{ leaveTypeText(row.leaveType) }}</template>
        </el-table-column>
        <el-table-column label="起止日期" width="220">
          <template #default="{ row }">{{ formatDate(row.startDate) }} ~ {{ formatDate(row.endDate) }}</template>
        </el-table-column>
        <el-table-column prop="days" label="天数" width="80" />
        <el-table-column prop="reason" label="事由" show-overflow-tooltip />
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link @click="openDetail(row.id)">审批</el-button>
          </template>
        </el-table-column>
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

    <!-- 审批详情抽屉 -->
    <el-drawer v-model="drawerVisible" title="单据详情" size="50%">
      <el-descriptions v-if="detail" :column="2" border>
        <el-descriptions-item label="单据编号">{{ detail.requestNo }}</el-descriptions-item>
        <el-descriptions-item label="申请人">{{ detail.applicantName }}</el-descriptions-item>
        <el-descriptions-item label="请假类型">{{ leaveTypeText(detail.leaveType) }}</el-descriptions-item>
        <el-descriptions-item label="状态">{{ leaveStatusText(detail.status) }}</el-descriptions-item>
        <el-descriptions-item label="起止日期">{{ formatDate(detail.startDate) }} ~ {{ formatDate(detail.endDate) }}</el-descriptions-item>
        <el-descriptions-item label="天数">{{ detail.days }}</el-descriptions-item>
        <el-descriptions-item label="事由" :span="2">{{ detail.reason || '-' }}</el-descriptions-item>
      </el-descriptions>

      <h4 class="section-title">附件</h4>
      <div v-if="detail && detail.attachments.length">
        <div v-for="att in detail.attachments" :key="att.id" class="att-item">
          <el-icon class="att-icon"><Document /></el-icon>
          <el-link type="primary" @click="previewAttachment(att)">{{ att.fileName }}</el-link>
          <span class="att-size">{{ formatSize(att.fileSize) }}</span>
        </div>
      </div>
      <div v-else class="empty-text">无附件</div>

      <h4 class="section-title">审批记录</h4>
      <el-timeline v-if="detail">
        <el-timeline-item
          v-for="record in detail.approvalRecords"
          :key="record.id"
          :timestamp="formatTime(record.actionTime)"
          :type="recordActionType(record.action)"
        >
          {{ record.nodeName }}（{{ record.approverName }}）— {{ actionText(record.action) }}
          <div v-if="record.comment" class="comment">意见：{{ record.comment }}</div>
        </el-timeline-item>
      </el-timeline>

      <template v-if="canApprove">
        <el-divider />
        <el-input
          v-model="comment"
          type="textarea"
          :rows="3"
          placeholder="审批意见（驳回时必填）"
        />
        <div class="actions">
          <el-button type="success" @click="handleApprove">同意</el-button>
          <el-button type="danger" @click="handleReject">驳回</el-button>
        </div>
      </template>
    </el-drawer>
  </div>
</template>

<script setup>
import { computed, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { getPendingLeaves, getLeaveDetail, approveLeave, rejectLeave, downloadAttachment } from '../api/leave'
import { LEAVE_TYPE_MAP, LEAVE_STATUS_MAP, APPROVAL_ACTION_MAP } from '../utils/enums'

const list = ref([])
const total = ref(0)
const loading = ref(false)
const query = reactive({ pageIndex: 1, pageSize: 10 })

const drawerVisible = ref(false)
const detail = ref(null)
const comment = ref('')

const canApprove = computed(() => detail.value && detail.value.status === 'Pending')

async function load() {
  loading.value = true
  try {
    const data = await getPendingLeaves({ pageIndex: query.pageIndex, pageSize: query.pageSize })
    list.value = data.items
    total.value = data.totalCount
  } finally {
    loading.value = false
  }
}

async function openDetail(id) {
  detail.value = await getLeaveDetail(id)
  comment.value = ''
  drawerVisible.value = true
}

async function handleApprove() {
  await approveLeave(detail.value.id, comment.value)
  ElMessage.success('审批通过')
  drawerVisible.value = false
  load()
}

async function handleReject() {
  if (!comment.value.trim()) {
    ElMessage.warning('驳回时必须填写意见')
    return
  }
  await rejectLeave(detail.value.id, comment.value)
  ElMessage.success('已驳回')
  drawerVisible.value = false
  load()
}

function leaveTypeText(t) {
  return LEAVE_TYPE_MAP[t] || t
}
function leaveStatusText(s) {
  return LEAVE_STATUS_MAP[s] || s
}
function actionText(a) {
  return APPROVAL_ACTION_MAP[a] || a
}
function formatDate(d) {
  return d ? d.slice(0, 10) : ''
}
function formatTime(t) {
  return t ? t.replace('T', ' ').slice(0, 19) : ''
}
function recordActionType(a) {
  if (a === 'Approved') return 'success'
  if (a === 'Rejected') return 'danger'
  return 'primary'
}

async function previewAttachment(att) {
  const blob = await downloadAttachment(att.id)
  const url = URL.createObjectURL(blob)
  window.open(url, '_blank')
  setTimeout(() => URL.revokeObjectURL(url), 60000)
}

function formatSize(size) {
  if (size < 1024) return `${size} B`
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`
  return `${(size / 1024 / 1024).toFixed(1)} MB`
}

load()
</script>

<style scoped>
.pagination {
  margin-top: 16px;
  justify-content: flex-end;
}
.section-title {
  margin: 20px 0 12px;
}
.att-item {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}
.att-icon {
  color: #909399;
}
.att-size {
  color: #999;
  font-size: 12px;
}
.empty-text {
  color: #999;
  font-size: 13px;
}
.comment {
  color: #666;
  font-size: 12px;
  margin-top: 4px;
}
.actions {
  margin-top: 16px;
}
</style>
