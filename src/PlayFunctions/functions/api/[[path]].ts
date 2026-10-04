import { type DevProxyEnv, proxyApi } from "../../lib/dev-proxy";

export const onRequest: PagesFunction<DevProxyEnv> = async ({ request, env, next }) =>
    env.DEV_PROXY ? proxyApi(request) : next();
