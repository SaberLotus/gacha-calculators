// Each game owns its caches; other apps on this origin must remain available offline.
const CACHE_PREFIX = 'nte-';
const CACHE = 'nte-v5';
const FONT_CACHE = CACHE + '-fonts';
const SCOPE = new URL('./', self.location.href);
const INDEX_URL = new URL('index.html', SCOPE).href;
const SHELL = ['./', './index.html', './manifest.json', './icon.svg', './character-icon.jpg'];

self.addEventListener('install', (event) => {
  event.waitUntil(caches.open(CACHE)
    .then(cache => cache.addAll(SHELL.map(path => new Request(new URL(path, SCOPE), { cache: 'reload' }))))
    .then(() => self.skipWaiting()));
});

self.addEventListener('activate', (event) => {
  event.waitUntil(caches.keys()
    .then(keys => Promise.all(keys
      .filter(key => key.startsWith(CACHE_PREFIX) && key !== CACHE && key !== FONT_CACHE)
      .map(key => caches.delete(key))))
    .then(() => self.clients.claim()));
});

self.addEventListener('fetch', (event) => {
  const request = event.request;
  if (request.method !== 'GET') return;
  const url = new URL(request.url);
  const isFont = url.hostname === 'fonts.googleapis.com' || url.hostname === 'fonts.gstatic.com';
  if (isFont){
    const refresh = caches.open(FONT_CACHE).then(async cache => {
      try {
        const response = await fetch(request);
        if (response.ok || response.type === 'opaque'){
          await cache.put(request, response.clone()).catch(() => {});
        }
        return response;
      } catch (e){
        return (await cache.match(request)) || Response.error();
      }
    });
    event.waitUntil(refresh.then(() => {}));
    event.respondWith(caches.open(FONT_CACHE).then(async cache =>
      (await cache.match(request)) || refresh));
    return;
  }
  // Do not store another game's requests in this app's cache.
  if (url.origin !== SCOPE.origin || !url.pathname.startsWith(SCOPE.pathname)) return;
  if (request.mode === 'navigate'){
    event.respondWith(caches.open(CACHE).then(async cache => {
      try {
        const response = await fetch(new Request(request, { cache: 'no-cache' }));
        if (response.ok && (url.pathname === SCOPE.pathname ||
            url.pathname === new URL(INDEX_URL).pathname)){
          await cache.put(INDEX_URL, response.clone()).catch(() => {});
        }
        return response;
      } catch (e){
        return (await cache.match(INDEX_URL)) || Response.error();
      }
    }));
    return;
  }
  event.respondWith(caches.open(CACHE).then(async cache => {
    const cached = await cache.match(request);
    if (cached) return cached;
    try {
      const response = await fetch(request);
      if (response.status === 200){
        await cache.put(request, response.clone()).catch(() => {});
      }
      return response;
    } catch (e){
      // Never return HTML for a missing image, stylesheet, or script.
      return Response.error();
    }
  }));
});
