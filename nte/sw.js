// Service worker — cache-first for the app shell so it runs offline.
const CACHE = 'nte-v2';
const SHELL = ['./', './index.html', './manifest.json', './icon.svg'];
self.addEventListener('install', e => e.waitUntil(caches.open(CACHE).then(c => c.addAll(SHELL)).then(() => self.skipWaiting())));
self.addEventListener('activate', e => e.waitUntil(caches.keys().then(keys => Promise.all(keys.filter(k => k.startsWith('nte-') && k !== CACHE && !k.endsWith('-fonts')).map(k => caches.delete(k)))).then(() => self.clients.claim())));
self.addEventListener('fetch', e => {
  const req = e.request;
  if (req.method !== 'GET') return;
  if (/fonts\.(googleapis|gstatic)\.com/.test(req.url)) {
    e.respondWith(caches.open(CACHE + '-fonts').then(async c => {
      const hit = await c.match(req);
      const net = fetch(req).then(r => { c.put(req, r.clone()); return r; }).catch(() => hit);
      return hit || net;
    }));
    return;
  }
  e.respondWith(caches.match(req).then(hit => hit || fetch(req).then(r => {
    if (r.ok && new URL(req.url).origin === self.location.origin) caches.open(CACHE).then(c => c.put(req, r.clone()));
    return r;
  }).catch(() => caches.match('./index.html'))));
});
