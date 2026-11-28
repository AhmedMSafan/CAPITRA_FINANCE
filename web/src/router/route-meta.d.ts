import 'vue-router'

declare module 'vue-router' {
  interface RouteMeta {
    /** Pages that do not need a signed-in user. Everything else does. */
    public?: boolean
    moduleName?: string
    functionName?: string
  }
}

export {}
