const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const games = ['genshin', 'hsr', 'hi3', 'zzz', 'wuwa', 'nte'];
const root = path.resolve(__dirname, '..');

function worker(game) {
  const scope = `https://example.test/gacha-calculators/${game}/`;
  const events = {}, stores = new Map(), fetched = [];
  let offline = false;
  class Request {
    constructor(input, options = {}) {
      Object.assign(this, typeof input === 'object' && input.url ? input : { url: String(input) });
      this.method ||= 'GET';
      Object.assign(this, options);
    }
  }
  const key = input => new URL(input.url || input, scope).href;
  const caches = {
    keys: async () => [...stores.keys()],
    delete: async name => stores.delete(name),
    open: async name => {
      if (!stores.has(name)) stores.set(name, new Map());
      const store = stores.get(name);
      return {
        match: async req => store.get(key(req))?.clone(),
        put: async (req, response) => store.set(key(req), response.clone()),
        addAll: async requests => {
          assert.ok(requests.every(r => r.cache === 'reload'));
          for (const request of requests) store.set(key(request), new Response('installed shell'));
        }
      };
    }
  };
  const code = fs.readFileSync(path.join(root, game, 'sw.js'), 'utf8');
  const version = code.match(/const CACHE = '([^']+)'/)[1];
  vm.runInNewContext(code, {
    URL, Request, Response, caches,
    fetch: async req => {
      fetched.push(req);
      if (offline) throw new Error('offline');
      return new Response('fresh response');
    },
    self: {
      location: { href: scope + 'sw.js' },
      addEventListener: (name, callback) => { events[name] = callback; },
      skipWaiting: async () => {}, clients: { claim: async () => {} }
    }
  }, { filename: game + '/sw.js' });
  return {
    caches, stores, scope, version, fetched,
    setOffline: value => { offline = value; },
    lifecycle: async name => {
      let promise;
      events[name]({ waitUntil: p => { promise = p; } });
      await promise;
    },
    request: async (url, mode = 'cors') => {
      let promise;
      events.fetch({ request: new Request(url, { mode }), respondWith: p => { promise = p; }, waitUntil: () => {} });
      return promise ? await promise : undefined;
    }
  };
}

for (const game of games) {
  test(`${game}: activation preserves other games and current fonts`, async () => {
    const w = worker(game);
    const keep = games.filter(g => g !== game).map(g => g + '-v1');
    keep.push(w.version, w.version + '-fonts', 'unrelated-cache');
    for (const name of [...keep, game + '-v0', game + '-v0-fonts']) await w.caches.open(name);
    await w.lifecycle('install');
    await w.lifecycle('activate');
    assert.deepEqual((await w.caches.keys()).sort(), keep.sort());
  });
  test(`${game}: online navigation refreshes stale HTML; offline navigation uses own shell`, async () => {
    const w = worker(game), cache = await w.caches.open(w.version);
    await cache.put(w.scope + 'index.html', new Response('stale HTML'));
    const response = await w.request(w.scope, 'navigate');
    assert.equal(await response.text(), 'fresh response');
    assert.equal(w.fetched.at(-1).cache, 'no-cache');
    w.setOffline(true);
    assert.equal(await (await w.request(w.scope + '?refresh=1', 'navigate')).text(), 'fresh response');
  });
  test(`${game}: bypass other scopes and never substitute HTML for missing assets`, async () => {
    const w = worker(game), cache = await w.caches.open(w.version);
    await cache.put(w.scope + 'index.html', new Response('HTML'));
    await cache.put(w.scope + 'icon.jpg', new Response('own icon'));
    const other = await w.caches.open('another-game');
    await other.put(w.scope + 'icon.jpg', new Response('wrong icon'));
    w.setOffline(true);
    assert.equal(await (await w.request(w.scope + 'icon.jpg')).text(), 'own icon');
    assert.equal((await w.request(w.scope + 'missing.jpg')).type, 'error');
    assert.equal(await w.request('https://example.test/gacha-calculators/other/icon.jpg'), undefined);
    assert.equal(await w.request('https://elsewhere.test/script.js'), undefined);
  });
  test(`${game}: conditional defaults are opt-in and saved sources survive reload`, () => {
    const html = fs.readFileSync(path.join(root, game, 'index.html'), 'utf8');
    for (const match of html.matchAll(/<script(?:\s[^>]*)?>([\s\S]*?)<\/script>/g)) new vm.Script(match[1]);
    const declaration = html.match(/const defaultSources = \[[\s\S]*?\n  \];/)[0];
    const defaults = vm.runInNewContext(declaration + '; defaultSources;');
    assert.ok(defaults.filter(s => !/Nhiệm vụ hàng ngày/.test(s.name) && !s.shopKey).every(s => s.enabled === false));
    assert.equal(defaults.filter(s => s.shopKey).length, game === 'hi3' ? 0 : 1);
    const start = html.indexOf('  function applyState(state){');
    const end = html.indexOf("  document.getElementById('a-export')", start);
    const source = html.slice(start, end);
    for (const sources of [[], [{ name: 'Custom income', amount: 123, period: 'daily', enabled: false }]]) {
      const restored = [];
      const element = { value: '', style: {} };
      vm.runInNewContext(source + '; applyState(state);', {
        state: { sources }, defaultSources: defaults, srcList: { innerHTML: '' },
        addSourceRow: s => restored.push(s), setv: () => {}, renderNowPulls: () => {},
        calcAccum: () => {}, renderFons: () => {}, document: { getElementById: () => element }
      });
      assert.deepEqual(restored, sources);
    }
  });
}

