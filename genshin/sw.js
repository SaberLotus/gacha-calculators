// Service worker — cache-first for the app shell so it runs offline.
const CACHE = 'genshin-v6';
const SHELL = ['./', './index.html', './manifest.json', './icon-192.png', './icon-512.png', './character-icon.jpg'];

self.addEventListener('install', (e) => {
  e.waitUntil(caches.open(CACHE).then(c => c.addAll(SHELL)).then(() => self.skipWaiting()));
});

self.addEventListener('activate', (e) => {
  e.waitUntil(
    caches.keys()
      .then(keys => Promise.all(keys.filter(k => k !== CACHE).map(k => caches.delete(k))))
      .then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (e) => {
  const req = e.request;
  if (req.method !== 'GET') return;

  // Google Fonts: serve from cache, refresh in the background.
  if (/fonts\.(googleapis|gstatic)\.com/.test(req.url)) {
    e.respondWith(caches.open(CACHE + '-fonts').then(async (c) => {
      const hit = await c.match(req);
      const net = fetch(req).then(r => { c.put(req, r.clone()); return r; }).catch(() => hit);
      return hit || net;
    }));
    return;
  }

  // App shell: cache first, network as a fallback.
  e.respondWith(
    caches.match(req).then(hit => hit || fetch(req).then((r) => {
      if (r.ok && new URL(req.url).origin === self.location.origin) {
        const copy = r.clone();
        caches.open(CACHE).then(c => c.put(req, copy));
      }
      return r;
    }).catch(() => caches.match('./index.html')))
  );
});
