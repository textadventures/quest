import { type DevProxyEnv, proxyGameResource } from "../../lib/dev-proxy";

export const onRequest: PagesFunction<DevProxyEnv> = async ({ request, env, next }) =>
    env.DEV_PROXY ? proxyGameResource(request) : next();
