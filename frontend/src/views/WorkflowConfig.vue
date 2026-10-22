<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>审批流程列表</span>
          <el-button type="primary" @click="openCreate">新建流程</el-button>
        </div>
      </template>

      <el-table v-loading="loading" :data="list" border>
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="name" label="流程名称" />
        <el-table-column prop="description" label="描述" />
        <el-table-column prop="nodeCount" label="节点数" width="100" />
        <el-table-column label="启用" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'info'">{{ row.isEnabled ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link @click="openDetail(row.id)">查看</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新建流程弹窗 -->
    <el-dialog v-model="createVisible" title="新建审批流程" width="620px">
      <el-form :model="createForm" label-width="100px">
        <el-form-item label="流程名称" required>
          <el-input v-model="createForm.name" placeholder="如：请假审批流程" />
        </el-form-item>
        <el-form-item label="流程描述">
          <el-input v-model="createForm.description" placeholder="流程描述" />
        </el-form-item>
        <el-form-item label="是否启用">
          <el-switch v-model="createForm.isEnabled" />
        </el-form-item>
        <el-form-item label="审批节点" required>
          <div v-for="(node, index) in createForm.nodes" :key="index" class="node-row">
            <span class="node-order">{{ index + 1 }}</span>
            <el-input v-model="node.name" placeholder="节点名称（如：直属领导审批）" class="node-name" />
            <el-select v-model="node.approverId" placeholder="审批人" class="node-approver">
              <el-option v-for="u in users" :key="u.id" :label="`${u.displayName}（${u.department}）`" :value="u.id" />
            </el-select>
            <el-button type="danger" link @click="removeNode(index)">删除</el-button>
          </div>
          <el-button type="primary" plain @click="addNode">添加节点</el-button>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" :loading="creating" @click="handleCreate">创建</el-button>
      </template>
    </el-dialog>

    <!-- 流程详情弹窗 -->
    <el-dialog v-model="detailVisible" title="流程详情" width="520px">
      <el-descriptions v-if="detail" :column="1" border>
        <el-descriptions-item label="流程名称">{{ detail.name }}</el-descriptions-item>
        <el-descriptions-item label="描述">{{ detail.description || '-' }}</el-descriptions-item>
        <el-descriptions-item label="启用">{{ detail.isEnabled ? '是' : '否' }}</el-descriptions-item>
      </el-descriptions>
      <el-table v-if="detail" :data="detail.nodes" border class="mt">
        <el-table-column prop="order" label="顺序" width="80" />
        <el-table-column prop="name" label="节点名称" />
        <el-table-column prop="approverName" label="审批人" />
      </el-table>
    </el-dialog>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { getWorkflows, createWorkflow, getWorkflowDetail } from '../api/workflow'
import { getUsers } from '../api/user'

const list = ref([])
const loading = ref(false)
const users = ref([])

const createVisible = ref(false)
const creating = ref(false)
const createForm = reactive({ name: '', description: '', isEnabled: true, nodes: [] })

const detailVisible = ref(false)
const detail = ref(null)

async function load() {
  loading.value = true
  try {
    list.value = await getWorkflows()
  } finally {
    loading.value = false
  }
}

async function loadUsers() {
  users.value = await getUsers()
}

function openCreate() {
  createForm.name = ''
  createForm.description = ''
  createForm.isEnabled = true
  createForm.nodes = [{ name: '', approverId: null }]
  createVisible.value = true
}

function addNode() {
  createForm.nodes.push({ name: '', approverId: null })
}

function removeNode(index) {
  createForm.nodes.splice(index, 1)
}

async function handleCreate() {
  if (!createForm.name.trim()) {
    ElMessage.warning('请输入流程名称')
    return
  }
  if (!createForm.nodes.length) {
    ElMessage.warning('至少配置一个审批节点')
    return
  }
  for (const node of createForm.nodes) {
    if (!node.name.trim() || !node.approverId) {
      ElMessage.warning('请完整填写节点名称与审批人')
      return
    }
  }

  // 节点顺序按数组下标生成
  const data = {
    name: createForm.name,
    description: createForm.description,
    isEnabled: createForm.isEnabled,
    nodes: createForm.nodes.map((n, i) => ({ order: i, name: n.name, approverId: n.approverId }))
  }

  creating.value = true
  try {
    await createWorkflow(data)
    ElMessage.success('创建成功')
    createVisible.value = false
    load()
  } finally {
    creating.value = false
  }
}

async function openDetail(id) {
  detail.value = await getWorkflowDetail(id)
  detailVisible.value = true
}

load()
loadUsers()
</script>

<style scoped>
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.node-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}
.node-order {
  width: 20px;
  text-align: center;
  color: #666;
}
.node-name {
  flex: 1;
}
.node-approver {
  width: 200px;
}
.mt {
  margin-top: 16px;
}
</style>
