<template>
  <el-form :model="formModel" class="search-bar-wrapper" inline @submit.prevent>
    <slot name="default" :form="formModel">
    </slot>
    <div class="search-bar-footer">
      <slot name="footer">
        <el-button type="primary" :icon="Search" @click="handleSearch">搜索</el-button>
        <el-button :icon="Refresh" @click="handleReset">重置</el-button>
      </slot>
    </div>
  </el-form>
</template>

<script setup>
import { reactive, watch, ref } from 'vue'
import { Search, Refresh } from '@element-plus/icons-vue'

const props = defineProps({
  initialValues: {
    type: Object,
    default: () => ({})
  }
})

const emit = defineEmits(['search', 'reset'])

// 深拷贝：避免与父组件响应式对象共享引用，导致重置时被已输入值污染
function deepClone(obj) {
  return JSON.parse(JSON.stringify(obj || {}))
}

const initialSnapshot = ref(deepClone(props.initialValues))
const formModel = reactive(deepClone(props.initialValues))

watch(
  () => props.initialValues,
  (val) => {
    const cloned = deepClone(val)
    initialSnapshot.value = cloned
    Object.assign(formModel, cloned)
  },
  { deep: true }
)

function handleSearch() {
  emit('search', { ...formModel })
}

function handleReset() {
  Object.assign(formModel, deepClone(initialSnapshot.value))
  emit('reset', { ...formModel })
}

defineExpose({
  formModel,
  handleSearch,
  handleReset
})
</script>

<style lang="scss" scoped>
.search-bar-wrapper {
  background-color: #fff;
  padding: 20px 20px 0;
  border-radius: 4px;
  margin-bottom: 20px;

  :deep(.el-form-item) {
    margin-bottom: 20px;
  }
}

.search-bar-footer {
  display: inline-flex;
  align-items: center;
  gap: 10px;
}
</style>
