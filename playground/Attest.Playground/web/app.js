(() => {
  const EXAMPLES = [
    { label: 'BE0403019261', value: 'BE0403019261', country: '' },
    { label: 'PL5260250274', value: 'PL5260250274', country: '' },
    { label: 'DE136695976', value: 'DE136695976', country: '' },
    { label: 'BR CPF 529.982.247-25', value: '529.982.247-25', country: 'BR' },
    { label: 'BE0403019262 (wrong)', value: 'BE0403019262', country: '' },
  ];
  const REASONS = {
    empty: 'The value is empty.',
    too_long: 'The value is too long.',
    invalid_characters: 'It contains a character that cannot occur in this identifier.',
    invalid_length: 'It has the wrong number of characters.',
    invalid_format: 'It is not in the shape this identifier has.',
    invalid_checksum: 'The check digit does not match.',
    invalid_date: 'The date inside the number does not exist.',
    invalid_component: 'A field is outside its published range.',
    invalid_prefix: 'The country prefix is wrong or missing.',
  };

  const $ = (id) => document.getElementById(id);
  const input = $('value');
  const select = $('country');
  const result = $('result');
  let timer = 0;
  let controller = null;

  const el = (tag, cls, text) => {
    const node = document.createElement(tag);
    if (cls) node.className = cls;
    if (text !== undefined) node.textContent = text;
    return node;
  };

  const names = typeof Intl.DisplayNames === 'function' ? new Intl.DisplayNames(['en'], { type: 'region' }) : null;
  const countryName = (code) => {
    try { return (names && names.of(code)) || code; } catch { return code; }
  };

  async function loadCountries() {
    try {
      const res = await fetch('/v1/countries');
      const data = await res.json();
      const items = data.countries
        .map((c) => ({ code: c.code, name: countryName(c.code) }))
        .sort((a, b) => a.name.localeCompare(b.name));
      for (const c of items) {
        const o = el('option', '', c.name + ' (' + c.code + ')');
        o.value = c.code;
        select.append(o);
      }
    } catch { /* auto-detect still works without the list */ }
  }

  function show(state, nodes) {
    result.dataset.state = state;
    result.replaceChildren(...nodes);
  }

  const mark = { valid: 'Valid', invalid: 'Not valid', not_covered: 'No rule' };
  const title = { valid: 'Valid', invalid: 'Not valid', not_covered: 'No rule' };

  function row(item) {
    const li = el('li');
    li.append(el('span', 'dot ' + item.verdict, mark[item.verdict]));
    const t = el('span', 'type', item.name);
    t.append(el('code', '', item.type));
    li.append(t);
    let why;
    if (item.valid) {
      why = el('span', 'why');
      why.append('Normalized ', el('code', '', item.normalized || ''));
      if (item.formatted && item.formatted !== item.normalized) why.append(', printed ', el('code', '', item.formatted));
    } else {
      why = el('span', 'why', item.verdict === 'not_covered'
        ? 'Attest has no rule for this type, so it has no opinion.'
        : (REASONS[item.reason] || item.detail || 'The value fails a check.'));
    }
    li.append(why);
    return li;
  }

  function render(data) {
    const state = data.valid ? 'valid' : (data.results.length === 1 && data.results[0].verdict === 'not_covered' ? 'not_covered' : 'invalid');
    const head = el('div', 'verdict');
    head.append(el('span', 'badge ' + state, title[state]), el('p', 'msg', data.message));
    const nodes = [head];
    if (data.results.length) {
      const list = el('ul', 'rows');
      data.results.forEach((r) => list.append(row(r)));
      nodes.push(list);
    }
    show(state, nodes);
  }

  async function check() {
    const value = input.value.trim();
    if (!value) {
      show('empty', [el('p', 'empty', 'Type or paste a number. The answer appears as you type.')]);
      return;
    }
    if (controller) controller.abort();
    controller = new AbortController();
    const body = { value };
    if (select.value) body.country = select.value;
    // POST, so the number never lands in a URL, an access log or the browser history.
    try {
      const res = await fetch('/v1/validate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'X-Attest-Source': 'playground' },
        body: JSON.stringify(body),
        signal: controller.signal,
      });
      const data = await res.json();
      if (res.status === 429) {
        show('error', [el('p', 'msg', 'Too many checks from your network. Wait a minute and try again.')]);
      } else if (!res.ok) {
        show('error', [el('p', 'msg', data.message || 'Something went wrong.')]);
      } else {
        render(data);
      }
    } catch (e) {
      if (e.name !== 'AbortError') show('error', [el('p', 'msg', 'The service did not answer. Try again in a moment.')]);
    }
  }

  const schedule = () => { clearTimeout(timer); timer = setTimeout(check, 250); };

  input.addEventListener('input', schedule);
  select.addEventListener('change', check);
  $('form').addEventListener('submit', (e) => { e.preventDefault(); check(); });

  const row$ = $('examples-row');
  for (const ex of EXAMPLES) {
    const b = el('button', '', ex.label);
    b.type = 'button';
    b.addEventListener('click', () => { input.value = ex.value; select.value = ex.country; check(); input.focus(); });
    row$.append(b);
  }

  loadCountries();
  const initial = new URLSearchParams(location.search);
  if (initial.get('value')) { input.value = initial.get('value').slice(0, 256); check(); }
})();
