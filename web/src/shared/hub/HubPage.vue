<script setup lang="ts">
// A page of tiles that open the screens of one area: the Vue version of an APEX hub page.
import type { HubTile } from './types'

defineProps<{
  title: string
  description: string
  icon: string
  tiles: HubTile[]
  apexPage: string
}>()
</script>

<template>
  <div class="hub-page">
    <header class="hub-header">
      <div class="hub-header-icon"><i :class="icon"></i></div>
      <div>
        <h2>{{ title }}</h2>
        <p>{{ description }}</p>
      </div>
      <span class="spacer"></span>
      <slot name="actions"></slot>
    </header>

    <div class="hub-grid">
      <RouterLink v-for="tile in tiles" :key="tile.to" :to="tile.to" class="hub-tile">
        <div class="hub-tile-icon"><i :class="tile.icon"></i></div>
        <div class="hub-tile-text">
          <strong>{{ tile.title }}</strong>
          <span>{{ tile.description }}</span>
        </div>
        <i class="pi pi-angle-right hub-tile-go"></i>
      </RouterLink>
    </div>
    <p class="apex-ref">Replaces APEX App 105 page {{ apexPage }}</p>
  </div>
</template>

<style scoped>
.hub-page { padding: 24px 30px 36px; font-family: Inter, "Segoe UI", Arial, sans-serif; }
.hub-header { display: flex; align-items: center; gap: 15px; margin-bottom: 20px; padding: 20px 28px; border: 1px solid #dce6e8; border-radius: 16px; background: #fff; box-shadow: 0 8px 24px rgba(1, 75, 85, .06); }
.hub-header-icon { width: 54px; height: 54px; flex: 0 0 54px; display: grid; place-items: center; border-radius: 14px; background: #fff0df; color: #c47b2c; font-size: 22px; }
.hub-header h2 { margin: 0; color: #0a3573; font-size: 20px; font-weight: 600; }
.hub-header p { margin: 5px 0 0; color: #718579; font-size: 13px; }
.spacer { flex: 1; }
.hub-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 14px; }
.hub-tile { display: flex; align-items: center; gap: 14px; padding: 16px 18px; border: 1px solid #dce6e8; border-radius: 12px; background: #fff; color: inherit; text-decoration: none; transition: border-color .15s, box-shadow .15s; }
.hub-tile:hover { border-color: #9cc7b4; box-shadow: 0 6px 18px rgba(1, 75, 85, .08); }
.hub-tile-icon { width: 42px; height: 42px; flex: 0 0 42px; display: grid; place-items: center; border-radius: 10px; background: #e7f0fa; color: #0a3573; font-size: 17px; }
.hub-tile-text { min-width: 0; flex: 1; display: flex; flex-direction: column; gap: 4px; }
.hub-tile-text strong { color: #183550; font-size: 14px; font-weight: 600; }
.hub-tile-text span { color: #718579; font-size: 12px; line-height: 1.4; }
.hub-tile-go { color: #9aa9b4; }
.apex-ref { margin: 12px 2px 0; color: #9aa9b4; font-size: 10px; }
@media (max-width: 760px) { .hub-page { padding: 16px; } .hub-header { padding: 18px; } }
</style>
