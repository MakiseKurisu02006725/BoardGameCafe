<template>
  <div class="file-upload-wrapper">
    <el-upload
      class="avatar-uploader"
      :show-file-list="false"
      :accept="accept"
      :before-upload="handleBeforeUpload"
      :http-request="handleUpload"
    >
      <el-image
        v-if="imageUrl"
        :src="imageUrl"
        class="avatar-image"
        fit="cover"
        :preview-src-list="[imageUrl]"
      />
      <div v-else class="avatar-placeholder">
        <el-icon class="avatar-icon"><Plus /></el-icon>
        <div class="avatar-text">上传图片</div>
      </div>
    </el-upload>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { ElMessage } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { uploadImage } from '@/api/upload'

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  maxSize: {
    type: Number,
    default: 1
  },
  accept: {
    type: String,
    default: 'image/*'
  }
})

const emit = defineEmits(['update:modelValue', 'success', 'error'])

const UPLOAD_PREFIX = import.meta.env.VITE_UPLOAD_PREFIX || ''

const imageUrl = computed(() => {
  if (!props.modelValue) return ''
  if (props.modelValue.startsWith('http')) return props.modelValue
  return UPLOAD_PREFIX + props.modelValue
})

function handleBeforeUpload(file) {
  const isImage = file.type.startsWith('image/')
  const isLtMax = file.size / 1024 / 1024 < props.maxSize

  if (!isImage) {
    ElMessage.error('只能上传图片格式!')
    return false
  }
  if (!isLtMax) {
    ElMessage.error(`图片大小不能超过 ${props.maxSize}MB!`)
    return false
  }
  return true
}

async function handleUpload(options) {
  try {
    const url = await uploadImage(options.file)
    const relativeUrl = url.startsWith(UPLOAD_PREFIX) ? url.slice(UPLOAD_PREFIX.length) : url
    emit('update:modelValue', relativeUrl)
    emit('success', relativeUrl)
    ElMessage.success('上传成功')
  } catch (error) {
    emit('error', error)
  }
}
</script>

<style lang="scss" scoped>
.avatar-uploader {
  :deep(.el-upload) {
    border: 1px dashed #d9d9d9;
    border-radius: 6px;
    cursor: pointer;
    position: relative;
    overflow: hidden;
    transition: border-color 0.3s;
    width: 178px;
    height: 178px;
    display: flex;
    align-items: center;
    justify-content: center;

    &:hover {
      border-color: #409eff;
    }
  }
}

.avatar-image {
  width: 178px;
  height: 178px;
  display: block;
}

.avatar-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #8c939d;
}

.avatar-icon {
  font-size: 28px;
  margin-bottom: 8px;
}

.avatar-text {
  font-size: 14px;
}
</style>
