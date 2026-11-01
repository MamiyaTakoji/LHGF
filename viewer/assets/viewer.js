/* 角色立绘 sizeY 搭配舞台 - 逻辑 */
(function () {
  'use strict';

  var DATA = window.PORTRAIT_DATA;
  if (!DATA) { return; }

  var $ = function (id) { return document.getElementById(id); };
  var stageScaler = $('stageScaler');
  var stageOuter = $('stageOuter');
  var stageActors = $('stageActors');
  var stageMarks = $('stageMarks');

  /* ---------- 画布 ---------- */
  var cvW = 1920, cvH = 1080;

  /* ---------- 舞台角色状态 ---------- */
  var actors = []; // { id, cid, face, sizeY, x(中心px), imgW, imgH }
  var uid = 1;
  var selectedId = null;

  function getChar(cid) {
    for (var i = 0; i < DATA.chars.length; i++) if (DATA.chars[i].id === cid) return DATA.chars[i];
    return null;
  }
  function getImg(c, face) {
    for (var i = 0; i < c.imgs.length; i++) if (c.imgs[i].file === face) return c.imgs[i];
    return c.imgs[0];
  }
  function defaultFace(c) {
    for (var i = 0; i < c.imgs.length; i++) {
      var f = c.imgs[i].file.toLowerCase();
      if (f.indexOf('normal') >= 0 || f.indexOf('portrait') >= 0) return c.imgs[i].file;
    }
    return c.imgs[0].file;
  }
  function mainSizey(c) {
    return c.sizey && c.sizey.main ? parseInt(c.sizey.main) : 800;
  }

  /* ---------- 舞台缩放 ---------- */
  function fitStage() {
    var ow = stageOuter.clientWidth;
    var scale = ow / cvW;
    stageScaler.style.width = cvW + 'px';
    stageScaler.style.height = cvH + 'px';
    stageScaler.style.transform = 'scale(' + scale + ')';
    stageOuter.style.height = Math.round(cvH * scale) + 'px';
    $('infoScale').textContent = scale.toFixed(3);
    $('infoCv').textContent = cvW + '×' + cvH;
    renderMarks();
  }

  function renderMarks() {
    var html = '';
    var marks = [0.25, 0.5, 0.75];
    marks.forEach(function (m) {
      html += '<div class="stage-mark" style="top:' + Math.round(cvH * m) + 'px">' +
        'height ' + Math.round(cvH * m) + 'px (' + Math.round(m * 100) + '%)</div>';
    });
    stageMarks.innerHTML = html;
  }

  /* ---------- 渲染舞台 ---------- */
  function renderStage() {
    stageActors.innerHTML = '';
    actors.forEach(function (a) {
      var c = getChar(a.cid);
      var im = getImg(c, a.face);
      var el = document.createElement('div');
      el.className = 'actor' + (a.id === selectedId ? ' selected' : '');
      el.dataset.aid = a.id;
      el.style.left = a.x + 'px';
      el.style.height = a.sizeY + 'px';
      el.style.transform = 'translateX(-50%)';

      var img = document.createElement('img');
      img.src = im.url;
      img.alt = c.name;
      img.style.height = a.sizeY + 'px';
      img.style.width = 'auto';
      el.appendChild(img);

      var tag = document.createElement('div');
      tag.className = 'tag';
      tag.innerHTML = '<span class="nm">' + c.name + '</span>' +
        '<span class="sy">' + a.sizeY + '</span>' +
        '<span class="px">' + a.w + '×' + a.sizeY + '</span>';
      el.appendChild(tag);

      var guide = document.createElement('div');
      guide.className = 'guide';
      el.appendChild(guide);

      stageActors.appendChild(el);
      bindActorDrag(el, a);
    });
    $('infoCnt').textContent = actors.length;
  }

  /* ---------- 控制条渲染 ---------- */
  function renderRows() {
    var box = $('actorRows');
    if (!actors.length) {
      box.innerHTML = '';
      $('emptyRows').style.display = 'block';
      return;
    }
    $('emptyRows').style.display = 'none';
    var html = '';
    actors.forEach(function (a, idx) {
      var c = getChar(a.cid);
      var opts = '';
      c.imgs.forEach(function (im) {
        opts += '<option value="' + im.file + '"' + (im.file === a.face ? ' selected' : '') + '>' + im.file + '</option>';
      });
      html += '<div class="actor-row" data-aid="' + a.id + '">' +
        '<button class="btn-sm ar-up" title="左移">◀</button>' +
        '<span class="ar-name">' + c.name + '</span>' +
        '<span class="ar-dir">' + c.id + '</span>' +
        '<select class="face">' + opts + '</select>' +
        '<div class="ar-slider"><span style="font-size:11px;color:var(--muted)">sizeY</span>' +
        '<input type="range" min="300" max="1400" step="10" value="' + a.sizeY + '">' +
        '</div>' +
        '<span class="ar-val">' + a.sizeY + 'px</span>' +
        '<span class="ar-cal">显示 ' + a.w + '×' + a.sizeY + ' · 占高 ' + Math.round(100 * a.sizeY / cvH) + '%</span>' +
        '<button class="btn-sm ar-down" title="右移">▶</button>' +
        '<button class="btn-sm ar-del" title="移下舞台">移除</button>' +
        '</div>';
    });
    box.innerHTML = html;

    box.querySelectorAll('.actor-row').forEach(function (row) {
      var a = getActor(parseInt(row.dataset.aid));
      if (!a) return;
      var slider = row.querySelector('input[type="range"]');
      var val = row.querySelector('.ar-val');
      var cal = row.querySelector('.ar-cal');
      slider.addEventListener('input', function () {
        a.sizeY = parseInt(slider.value);
        val.textContent = a.sizeY + 'px';
        var c = getChar(a.cid);
        var im = getImg(c, a.face);
        a.w = Math.round(im.w * a.sizeY / im.h);
        cal.textContent = '显示 ' + a.w + '×' + a.sizeY + ' · 占高 ' + Math.round(100 * a.sizeY / cvH) + '%';
        renderStage();
      });
      row.querySelector('select.face').addEventListener('change', function (e) {
        var c = getChar(a.cid);
        var im = getImg(c, e.target.value);
        a.face = e.target.value;
        a.w = Math.round(im.w * a.sizeY / im.h);
        renderStage();
        renderRows();
      });
      row.querySelector('.ar-up').addEventListener('click', function () {
        a.x = Math.max(0, a.x - 60);
        renderStage();
      });
      row.querySelector('.ar-down').addEventListener('click', function () {
        a.x = Math.min(cvW, a.x + 60);
        renderStage();
      });
      row.querySelector('.ar-del').addEventListener('click', function () {
        removeActor(a.id);
      });
    });
  }

  function getActor(id) {
    for (var i = 0; i < actors.length; i++) if (actors[i].id === id) return actors[i];
    return null;
  }

  /* ---------- 添加/移除角色 ---------- */
  function addActor(cid) {
    if (getActorByCid(cid)) return; // 已在台上则选中
    var c = getChar(cid);
    var sy = mainSizey(c);
    var im = getImg(c, defaultFace(c));
    var a = {
      id: uid++, cid: cid, face: im.file,
      sizeY: sy,
      w: Math.round(im.w * sy / im.h),
      x: cvW / 2
    };
    actors.push(a);
    selectedId = a.id;
    autoArrange();
    renderStage();
    renderRows();
    renderRoleList();
  }
  function getActorByCid(cid) {
    for (var i = 0; i < actors.length; i++) if (actors[i].cid === cid) return actors[i];
    return null;
  }
  function removeActor(id) {
    actors = actors.filter(function (a) { return a.id !== id; });
    if (selectedId === id) selectedId = null;
    renderStage();
    renderRows();
    renderRoleList();
  }
  function clearAll() {
    actors = [];
    selectedId = null;
    renderStage();
    renderRows();
    renderRoleList();
  }

  /* ---------- 自动均分排列 ---------- */
  function autoArrange() {
    var total = actors.length;
    if (!total) return;
    var gap = 130; // 角色间最小中心距
    var used = total * gap;
    var start = (cvW - used) / 2;
    if (start < 40) { start = 40; gap = (cvW - 80) / total; }
    actors.forEach(function (a, i) {
      a.x = start + gap * i + gap / 2;
    });
  }

  /* ---------- 拖拽移动角色 ---------- */
  function bindActorDrag(el, a) {
    var dragging = null;
    el.addEventListener('mousedown', function (e) {
      e.preventDefault();
      selectedId = a.id;
      el.classList.add('selected');
      renderRows();
      renderRoleList();
      dragging = { sx: e.clientX, ox: a.x };
      el.classList.add('dragging');
      document.body.style.cursor = 'grabbing';
    });
    window.addEventListener('mousemove', function (e) {
      if (!dragging) return;
      var dx = e.clientX - dragging.sx;
      var scale = stageOuter.clientWidth / cvW;
      a.x = Math.round(dragging.ox + dx / scale);
      a.x = Math.max(20, Math.min(cvW - 20, a.x));
      el.style.left = a.x + 'px';
    });
    window.addEventListener('mouseup', function () {
      if (!dragging) return;
      dragging = null;
      el.classList.remove('dragging');
      document.body.style.cursor = '';
    });
    el.addEventListener('dblclick', function () {
      autoArrange();
      renderStage();
    });
  }

  /* ---------- 角色列表 ---------- */
  function renderRoleList() {
    var html = '';
    DATA.chars.forEach(function (c) {
      var im = getImg(c, defaultFace(c));
      var on = actors.some(function (a) { return a.cid === c.id; });
      var syInfo = '';
      if (c.sizey && c.sizey.total > 0) {
        var keys = Object.keys(c.sizey.sizes).map(Number).sort(function (a, b) { return a - b; });
        syInfo = keys.map(function (k) { return k; }).join('/') + 'px';
      } else {
        syInfo = '未设置';
      }
      html += '<div class="role-item' + (on ? ' on' : '') + '" data-c="' + c.id + '">' +
        '<img class="av" src="' + im.url + '" alt="">' +
        '<div><div class="ri-name">' + c.name + '</div><div class="ri-meta">' + c.id + ' · ' + c.imgs.length + '张</div></div>' +
        '<div class="ri-sy">' + syInfo + '</div>' +
        '</div>';
    });
    $('roleList').innerHTML = html;
    $('roleList').querySelectorAll('.role-item').forEach(function (item) {
      item.addEventListener('click', function () {
        addActor(item.dataset.c);
      });
    });
  }

  /* ---------- 载入脚本组合 ---------- */
  function loadPreset() {
    clearAll();
    DATA.chars.forEach(function (c) {
      if (c.sizey && c.sizey.total > 0) {
        var sy = mainSizey(c);
        var im = getImg(c, defaultFace(c));
        actors.push({
          id: uid++, cid: c.id, face: im.file,
          sizeY: sy,
          w: Math.round(im.w * sy / im.h),
          x: cvW / 2
        });
      }
    });
    // 按 sizeY 降序排（主次分明）
    actors.sort(function (a, b) { return b.sizeY - a.sizeY; });
    autoArrange();
    selectedId = actors.length ? actors[0].id : null;
    renderStage();
    renderRows();
    renderRoleList();
  }

  /* ---------- 初始演示组合：脚本中常见的同屏核心角色 ---------- */
  function loadDemo() {
    var demo = ['Z', 'WD', 'YM', 'JJ', 'A', 'LD'];
    clearAll();
    demo.forEach(function (cid) {
      var c = getChar(cid);
      if (!c) return;
      var sy = mainSizey(c);
      var im = getImg(c, defaultFace(c));
      actors.push({
        id: uid++, cid: cid, face: im.file,
        sizeY: sy,
        w: Math.round(im.w * sy / im.h),
        x: cvW / 2
      });
    });
    autoArrange();
    selectedId = actors.length ? actors[0].id : null;
    renderStage();
    renderRows();
    renderRoleList();
  }

  /* ---------- 事件绑定 ---------- */
  $('btnPreset').addEventListener('click', loadPreset);
  $('btnClear').addEventListener('click', function () {
    if (actors.length && !confirm('确定清空舞台？')) return;
    clearAll();
  });
  $('cvW').addEventListener('change', function () {
    var v = parseInt(this.value);
    if (v && v >= 800 && v <= 3840) { cvW = v; fitStage(); renderStage(); renderRows(); }
  });
  $('cvH').addEventListener('change', function () {
    var v = parseInt(this.value);
    if (v && v >= 600 && v <= 2160) { cvH = v; fitStage(); renderStage(); renderRows(); }
  });
  window.addEventListener('resize', function () {
    fitStage();
  });

  /* ---------- 初始化 ---------- */
  fitStage();
  renderRoleList();
  // 默认载入核心演示组合（象山/舞冬/姆伊/季季/彩夏/里岛）
  loadDemo();
})();
