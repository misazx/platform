PE html>
<html lang="zh-CN">
<head>
<meta charset="UTF-8">
<title>LLM Reasoning Visualizer</title>
<script src="https://unpkg.com/vis-network/standalone/umd/vis-network.min.js"></script>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{background:#0d1117;color:#c9d1d9;font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;display:flex;height:100vh;overflow:hidden}
#graph{flex:1;position:relative;background:radial-gradient(ellipse at center,#161b22 0%,#0d1117 70%)}
#sidebar{width:360px;background:#161b22;border-left:1px solid #30363d;display:flex;flex-direction:column;overflow:hidden}
#header{padding:14px 16px;border-bottom:1px solid #30363d;background:#0d1117}
#header h2{font-size:14px;color:#58a6ff;margin-bottom:6px;display:flex;align-items:center;gap:8px}
#header .status{display:flex;gap:12px;font-size:11px;color:#8b949e;flex-wrap:wrap}
#header .status .dot{width:8px;height:8px;border-radius:50%;display:inline-block;margin-right:4px;vertical-align:middle}
#timeline-wrap{flex:1;overflow-y:auto;padding:8px}
#timeline{display:flex;flex-direction:column;gap:4px}
.tl-item{padding:8px 12px;border-radius:6px;background:#0d1117;border:1px solid #21262d;cursor:pointer;transition:all .15s;font-size:12px;line-height:1.5}
.tl-item:hover{border-color:#58a6ff;background:#161b22}
.tl-item.active{border-color:#58a6ff;background:#1a2332;box-shadow:0 0 12px rgba(88,166,255,.15)}
.tl-item .tl-head{display:flex;justify-content:space-between;align-items:center;margin-bottom:4px}
.tl-item .tl-icon{font-size:14px}
.tl-item .tl-tool{color:#58a6ff;font-weight:600;font-size:11px;text-transform:uppercase}
.tl-item .tl-time{color:#484f58;font-size:10px}
.tl-item .tl-query{color:#c9d1d9;font-weight:500;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.tl-item .tl-summary{color:#8b949e;font-size:11px;margin-top:2px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.tl-item .tl-nodes{margin-top:4px;display:flex;flex-wrap:wrap;gap:4px}
.tl-node-tag{background:#1a2332;color:#58a6ff;padding:1px 6px;border-radius:3px;font-size:10px;cursor:pointer;border:1px solid #21262d}
.tl-node-tag:hover{border-color:#58a6ff}
.tl-phase{display:inline-block;padding:1px 6px;border-radius:3px;font-size:10px;font-weight:600;margin-left:6px}
#controls{padding:10px 16px;border-top:1px solid #30363d;display:flex;gap:8px;align-items:center;flex-wrap:wrap}
#controls button{background:#21262d;color:#c9d1d9;border:1px solid #30363d;padding:4px 12px;border-radius:4px;font-size:11px;cursor:pointer}
#controls button:hover{background:#30363d}
#controls .speed-label{color:#8b949e;font-size:11px}
#graph-info{position:absolute;top:12px;left:12px;background:rgba(13,17,23,.88);border:1px solid #30363d;border-radius:8px;padding:10px 14px;font-size:11px;pointer-events:none;z-index:10}
#graph-info .gi-title{color:#58a6ff;font-weight:600;margin-bottom:4px}
#graph-info .gi-stats{color:#8b949e;display:flex;gap:12px}
#overlay{position:absolute;top:12px;left:12px;right:12px;background:rgba(13,17,23,.85);border:1px solid #30363d;border-radius:8px;padding:10px 14px;font-size:12px;z-index:20;max-width:none;display:none}
.overlay-visible{display:block!important}
#overlay .phase-indicator{display:flex;align-items:center;gap:8px;margin-bottom:4px}
#overlay .phase-dot{width:10px;height:10px;border-radius:50%;animation:pulse 1.5s infinite}
#overlay .phase-text{color:#58a6ff;font-weight:600;font-size:13px}
#overlay .current-action{color:#8b949e;font-size:11px}
@keyframes pulse{0%,100%{opacity:1;transform:scale(1)}50%{opacity:.5;transform:scale(1.3)}}
#empty-state{display:flex;flex-direction:column;align-items:center;justify-content:center;height:100%;color:#484f58;font-size:13px;text-align:center;padding:20px}
#empty-state .icon{font-size:40px;margin-bottom:12px;opacity:.5}
#loading-graph{position:absolute;top:50%;left:50%;transform:translate(-50%,-50%);color:#58a6ff;font-size:14px;z-index:5}
</style>
</head>
<body>
<div id="graph">
  <div id="loading-graph">Loading knowledge graph...</div>
  <div id="graph-info"></div>
  <div id="overlay">
    <div class="phase-indicator">
      <div class="phase-dot" id="phase-dot" style="background:#6b7280"></div>
      <span class="phase-text" id="phase-text">Waiting...</span>
    </div>
    <div class="current-action" id="current-action">LLM reasoning visualization</div>
  </div>
</div>
<div id="sidebar">
  <div id="header">
    <h2>🧠 LLM Reasoning Trace</h2>
    <div class="status">
      <span><span class="dot" style="background:#58a6ff"></span>Events: <b id="stat-events">0</b></span>
      <span><span class="dot" style="background:#3fb950"></span>Nodes: <b id="stat-nodes">0</b></span>
      <span><span class="dot" style="background:#d29922"></span>Edges: <b id="stat-edges">0</b></span>
      <span>⏱ <b id="stat-duration">0s</b></span>
    </div>
  </div>
  <div id="timeline-wrap">
    <div id="timeline">
      <div id="empty-state">
        <div class="icon">🧠</div>
        <div>Waiting for LLM activity...<br><small>Use MCP tools to see reasoning trace here</small></div>
      </div>
    </div>
  </div>
  <div id="controls">
    <button onclick="showAllNodes()">Show All</button>
    <button onclick="clearTimeline()">Clear</button>
    <button onclick="fitGraph()">Fit View</button>
    <button onclick="togglePhysics()">Physics</button>
    <span class="speed-label">Poll: <span id="poll-rate">500</span>ms</span>
  </div>
</div>
<script>
var COMMUNITY_COLORS = ['#ef4444','#f97316','#f59e0b','#84cc16','#22c55e','#06b6d4','#3b82f6','#8b5cf6','#d946ef','#ec4899'];
var PHASE_COLORS = {searching:'#3b82f6',analyzing:'#8b5cf6',traversing:'#f59e0b',synthesizing:'#10b981',debugging:'#ef4444',idle:'#6b7280'};
var EVENT_ICONS = {tool_call:'🔧',node_visit:'📍',edge_traverse:'🔗',community_explore:'🏘️',thinking:'💭',result:'✅',error:'❌',start:'🚀',end:'🏁'};

var network = null;
var nodesDS = null;
var edgesDS = null;
var lastEventIndex = 0;
var pollInterval = 500;
var visitedNodeIds = new Set();
var visitedEdgeKeys = new Set();
var physicsEnabled = false;
var allGraphNodeIds = new Set();
var timelineItems = [];
var nodeDegreeMap = {};
var hasActivity = false;

function esc(s) {
  return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function getCommColor(comm) {
  var idx = comm < 0 ? 0 : Math.abs(comm) % COMMUNITY_COLORS.length;
  return COMMUNITY_COLORS[idx];
}

async function initGraph() {
  try {
    var resp = await fetch('/graph_data');
    var data = await resp.json();
    var rawNodes = data.nodes || [];
    var rawLinks = data.links || [];

    nodeDegreeMap = {};
    for (var i = 0; i < rawLinks.length; i++) {
      var e = rawLinks[i];
      nodeDegreeMap[e.source] = (nodeDegreeMap[e.source] || 0) + 1;
      nodeDegreeMap[e.target] = (nodeDegreeMap[e.target] || 0) + 1;
    }
    var maxDeg = 1;
    var keys = Object.keys(nodeDegreeMap);
    for (var k = 0; k < keys.length; k++) {
      if (nodeDegreeMap[keys[k]] > maxDeg) maxDeg = nodeDegreeMap[keys[k]];
    }

    var visNodes = [];
    for (var ni = 0; ni < rawNodes.length; ni++) {
      var n = rawNodes[ni];
      var deg = nodeDegreeMap[n.id] || 0;
      var isHub = deg > maxDeg * 0.15;
      var cc = getCommColor(n.community);

      visNodes.push({
        id: n.id,
        label: n.label || n.id,
        color: {
          background: isHub ? cc : '#21262d',
          border: isHub ? cc : '#30363d',
          highlight: { background: '#fff', border: cc }
        },
        size: isHub ? Math.max(12, 4 + 30 * (deg / maxDeg)) : Math.max(3, 3 + 8 * (deg / maxDeg)),
        font: { size: isHub ? 11 : (deg > maxDeg * 0.05 ? 9 : 0), color: isHub ? '#fff' : '#8b949e', face: 'monospace' },
        title: esc((n.label || n.id) + '\nType: ' + (n.file_type || '?') + '\nFile: ' + (n.source_file || '') + '\nCommunity: ' + n.community),
        _source_file: n.source_file || '',
        _file_type: n.file_type || '',
        _community: n.community != null ? n.community : -1,
        _deg: deg
      });
    }

    var visEdges = [];
    for (var ei = 0; ei < rawLinks.length; ei++) {
      var le = rawLinks[ei];
      visEdges.push({
        id: ei,
        from: le.source,
        to: le.target,
        title: esc((le.relation || '') + ' [' + (le.confidence || '') + ']'),
        arrows: { to: { enabled: true, scaleFactor: 0.35 } },
        color: { opacity: 0.25, color: '#484f58' },
        width: 0.4,
        dashes: false,
        smooth: { type: 'continuous', roundness: 0.1 }
      });
    }

    for (var vi = 0; vi < visNodes.length; vi++) {
      allGraphNodeIds.add(visNodes[vi].id);
    }
    nodesDS = new vis.DataSet(visNodes);
    edgesDS = new vis.DataSet(visEdges);

    document.getElementById('loading-graph').style.display = 'none';
    document.getElementById('graph-info').innerHTML =
      '<div class="gi-title">📊 Knowledge Graph</div>' +
      '<div class="gi-stats"><span>' + visNodes.length + ' nodes</span><span>' + visEdges.length + ' edges</span><span>Click "Show All" to view</span></div>';

    var container = document.getElementById('graph');
    network = new vis.Network(container, { nodes: nodesDS, edges: edgesDS }, {
      physics: {
        enabled: true,
        solver: 'forceAtlas2Based',
        forceAtlas2Based: {
          gravitationalConstant: -80,
          centralGravity: 0.005,
          springLength: 150,
          springConstant: 0.04,
          damping: 0.3,
          avoidOverlap: 1
        },
        stabilization: { iterations: 300, fit: true, updateInterval: 25 }
      },
      interaction: {
        hover: true,
        tooltipDelay: 100,
        hideEdgesOnDrag: true,
        navigationButtons: false,
        keyboard: false,
        multiselect: false,
        zoomView: true,
        dragView: true
      },
      nodes: { shape: 'dot', borderWidth: 1, borderWidthSelected: 2, scaling: { min: 2, max: 30 } },
      edges: { selectionWidth: 1.5 }
    });

    network.on('stabilizationProgress', function(params) {
      document.getElementById('loading-graph').textContent = 'Layout: ' + Math.round(params.iterations) + '/' + params.total + ' (' + Math.round(params.perc) + '%)';
    });

    network.once('stabilizationIterationsDone', function() {
      network.setOptions({ physics: { enabled: false } });
      document.getElementById('loading-graph').style.display = 'none';
      showHubNodes();
    });

  } catch(err) {
    console.error('Graph load failed:', err);
    document.getElementById('graph').innerHTML =
      '<div style="display:flex;align-items:center;justify-content:center;height:100%;color:#484f58;text-align:center"><div><div style="font-size:40px;margin-bottom:12px">📊</div>No graph data<br><small>Run graphify or sync index first</small></div></div>';
  }
}

function showHubNodes() {
  if (!nodesDS) return;
  var allKeys = Object.keys(nodeDegreeMap);
  var maxD = 1;
  for (var k = 0; k < allKeys.length; k++) {
    if (nodeDegreeMap[allKeys[k]] > maxD) maxD = nodeDegreeMap[allKeys[k]];
  }
  var hubThreshold = maxD * 0.08;
  var updates = [];
  var allN = nodesDS.get();
  for (var i = 0; i < allN.length; i++) {
    var n = allN[i];
    var d = n._deg || 0;
    updates.push({ id: n.id, hidden: d >= hubThreshold ? false : true });
  }
  if (updates.length) nodesDS.update(updates);
  if (network) setTimeout(function() { network.fit({ animation: { duration: 400 } }); }, 100);
}

function showAllNodes() {
  if (!nodesDS) return;
  hasActivity = true;
  document.getElementById('overlay').className = 'overlay-visible';
  var allN = nodesDS.get();
  var updates = [];
  for (var i = 0; i < allN.length; i++) {
    updates.push({ id: allN[i].id, hidden: false });
  }
  if (updates.length) nodesDS.update(updates);
  if (network) setTimeout(function() { network.fit({ animation: { duration: 500 } }); }, 100);
}

function highlightNodes(nodeIds, phase) {
  if (!nodesDS) return;
  hasActivity = true;
  document.getElementById('overlay').className = 'overlay-visible';
  var color = PHASE_COLORS[phase] || '#58a6ff';

  var matchSet = new Set();
  for (var mi = 0; mi < nodeIds.length; mi++) {
    if (allGraphNodeIds.has(nodeIds[mi])) matchSet.add(nodeIds[mi]);
  }
  if (matchSet.size === 0) return;

  var allNodeIds = nodesDS.getIds();
  var updates = [];
  for (var ai = 0; ai < allNodeIds.length; ai++) {
    var nid = allNodeIds[ai];
    var n = nodesDS.get(nid);
    var isMatch = matchSet.has(nid);
    updates.push({
      id: nid,
      hidden: false,
      color: isMatch ?
        { background: color, border: color, highlight: { background: '#fff', border: color } } :
        { background: '#161b22', border: '#21262d', highlight: { background: '#30363d', border: '#484f58' } },
      size: isMatch ? 18 : ((n._deg || 0) > 3 ? 6 : 3),
      font: { size: isMatch ? 11 : ((n._deg || 0) > 3 ? 8 : 0), color: isMatch ? '#fff' : '#484f58' },
      borderWidth: isMatch ? 2 : 1
    });
  }
  if (updates.length) nodesDS.update(updates);

  // Highlight matching edges
  var msArr = Array.from(matchSet);
  for (var si = 0; si < msArr.length; si++) {
    var mid = msArr[si];
    var connected = network.getConnectedNodes(mid) || [];
    for (var ci = 0; ci < connected.length; ci++) {
      var cid = connected[ci];
      if (!matchSet.has(cid)) {
        var eid = findEdgeId(mid, cid);
        if (eid !== null) edgesDS.update({ id: eid, color: { opacity: 0.15, color: '#484f58' }, width: 0.3 });
      }
    }
  }
  for (var si2 = 0; si2 < msArr.length; si2++) {
    for (var ti = 0; ti < msArr.length; ti++) {
      if (si2 !== ti) {
        var eid2 = findEdgeId(msArr[si2], msArr[ti]);
        if (eid2 !== null) edgesDS.update({ id: eid2, color: { opacity: 0.9, color: color }, width: 2.5 });
      }
    }
  }

  visitedNodeIds = new Set([...visitedNodeIds, ...matchSet]);
  if (network && matchSet.size > 0) {
    var firstId = msArr[0];
    network.focus(firstId, { scale: 1.3, animation: { duration: 500, easingFunction: 'easeInOutQuad' } });
  }
}

function findEdgeId(fromId, toId) {
  if (!edgesDS) return null;
  var allE = edgesDS.get();
  for (var i = 0; i < allE.length; i++) {
    var e = allE[i];
    if ((e.from === fromId && e.to === toId) || (e.from === toId && e.to === fromId)) return e.id;
  }
  return null;
}

function highlightEdges(edgeList, phase) {
  if (!edgesDS) return;
  var color = PHASE_COLORS[phase] || '#58a6ff';
  for (var i = 0; i < edgeList.length; i++) {
    var e = edgeList[i];
    var fromId = e.from || e.source;
    var toId = e.to || e.target;
    if (!fromId || !toId) continue;
    visitedEdgeKeys.add(fromId + '->' + toId);
    var eid = findEdgeId(fromId, toId);
    if (eid !== null) edgesDS.update({ id: eid, color: { opacity: 0.9, color: color }, width: 2.5, dashes: false });
  }
}

function addTimelineItem(evt) {
  var timeline = document.getElementById('timeline');
  var emptyState = document.getElementById('empty-state');
  if (emptyState) emptyState.remove();

  var icon = EVENT_ICONS[evt.event_type] || '📌';
  var phaseColor = PHASE_COLORS[evt.phase] || '#6b7280';
  var timeStr = new Date(evt.timestamp * 1000).toLocaleTimeString('zh-CN', { hour12: false, hour: '2-digit', minute: '2-digit', second: '2-digit' });

  var item = document.createElement('div');
  item.className = 'tl-item';
  item.dataset.eventIndex = timelineItems.length;

  // Build node tags safely using DOM methods
  var nodesHtml = '';
  if (evt.nodes && evt.nodes.length > 0) {
    var nodeList = evt.nodes.slice(0, 8);
    for (var ni = 0; ni < nodeList.length; ni++) {
      var nodeId = esc(nodeList[ni]);
      var label = nodeList[ni].length > 20 ? esc(nodeList[ni].slice(0, 20)) + '…' : esc(nodeList[ni]);
      nodesHtml += '<span class="tl-node-tag" data-node="' + nodeId + '">' + label + '</span>';
    }
    if (evt.nodes.length > 8) {
      nodesHtml += '<span class="tl-node-tag">+' + (evt.nodes.length - 8) + '</span>';
    }
  }

  item.innerHTML =
    '<div class="tl-head">' +
      '<span><span class="tl-icon">' + icon + '</span> <span class="tl-tool">' + esc(evt.tool_name) + '</span><span class="tl-phase" style="background:' + phaseColor + '22;color:' + phaseColor + '">' + esc(evt.phase) + '</span></span>' +
      '<span class="tl-time">' + timeStr + '</span>' +
    '</div>' +
    '<div class="tl-query">' + esc(evt.query || '') + '</div>' +
    (evt.summary ? '<div class="tl-summary">' + esc(evt.summary) + '</div>' : '') +
    nodesHtml;

  // Delegate click on node tags
  item.addEventListener('click', function(ev) {
    var target = ev.target;
    if (target.classList.contains('tl-node-tag')) {
      focusNode(target.getAttribute('data-node'));
      return;
    }

    document.querySelectorAll('.tl-item.active').forEach(function(el) { el.classList.remove('active'); });
    item.classList.add('active');

    if (evt.nodes && evt.nodes.length > 0) highlightNodes(evt.nodes, evt.phase);
    if (evt.edges && evt.edges.length > 0) highlightEdges(evt.edges, evt.phase);
  });

  timeline.appendChild(item);
  timelineItems.push(evt);
  item.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

function updateOverlay(evt) {
  var dot = document.getElementById('phase-dot');
  var txt = document.getElementById('phase-text');
  var act = document.getElementById('current-action');
  var c = PHASE_COLORS[evt.phase] || '#6b7280';
  if (dot) dot.style.background = c;
  if (txt) { txt.textContent = evt.phase.toUpperCase(); txt.style.color = c; }
  if (act) act.textContent = evt.tool_name + ': ' + (evt.query || evt.summary || '');
}

function updateStats() {
  fetch('/stats').then(function(r) { return r.json(); }).then(function(s) {
    document.getElementById('stat-events').textContent = s.total_events;
    document.getElementById('stat-nodes').textContent = s.visited_nodes;
    document.getElementById('stat-edges').textContent = s.visited_edges;
    document.getElementById('stat-duration').textContent = s.session_duration + 's';
    var d = document.getElementById('phase-dot');
    if (d) d.style.background = PHASE_COLORS[s.current_phase] || '#6b7280';
  }).catch(function() {});
}

async function pollEvents() {
  try {
    var r = await fetch('/events?after=' + lastEventIndex);
    var evts = await r.json();
    if (evts.length > 0) {
      for (var i = 0; i < evts.length; i++) {
        var e = evts[i];
        addTimelineItem(e);
        updateOverlay(e);
        if (e.nodes && e.nodes.length > 0) highlightNodes(e.nodes, e.phase);
        if (e.edges && e.edges.length > 0) highlightEdges(e.edges, e.phase);
      }
      lastEventIndex += evts.length;
    }
  } catch(err) {}
  updateStats();
  setTimeout(pollEvents, pollInterval);
}

function focusNode(nodeId) {
  if (network && allGraphNodeIds.has(nodeId)) {
    network.focus(nodeId, { scale: 1.8, animation: true });
    network.selectNodes([nodeId]);
    highlightNodes([nodeId], 'analyzing');
  }
}

function clearTimeline() {
  document.getElementById('timeline').innerHTML =
    '<div id="empty-state"><div class="icon">🧠</div><div>Waiting for LLM activity...</div></div>';
  timelineItems = [];
  lastEventIndex = 0;
  visitedNodeIds.clear();
  visitedEdgeKeys.clear();
  hasActivity = false;
  document.getElementById('overlay').className = '';
  fetch('/clear').catch(function() {});

  if (nodesDS) {
    var allN = nodesDS.get();
    var maxD = 1;
    var ndKeys = Object.keys(nodeDegreeMap);
    for (var k = 0; k < ndKeys.length; k++) {
      if (nodeDegreeMap[ndKeys[k]] > maxD) maxD = nodeDegreeMap[ndKeys[k]];
    }
    var ht = maxD * 0.08;
    var u = [];
    for (var i = 0; i < allN.length; i++) {
      var n = allN[i];
      var cc = getCommColor(n._community);
      u.push({
        id: n.id,
        color: { background: (n._deg || 0) > 3 ? cc : '#21262d', border: (n._deg || 0) > 3 ? cc : '#30363d', highlight: { background: '#fff', border: cc } },
        size: (n._deg || 0) > 3 ? 6 : 3,
        font: { size: (n._deg || 0) > 3 ? 8 : 0, color: '#8b949e' },
        borderWidth: 1,
        hidden: (n._deg || 0) < ht
      });
    }
    if (u.length) nodesDS.update(u);
  }
  if (edgesDS) {
    var ae = edgesDS.get();
    var eu = [];
    for (var i = 0; i < ae.length; i++) {
      eu.push({ id: ae[i].id, color: { opacity: 0.25, color: '#484f58' }, width: 0.4, dashes: false });
    }
    if (eu.length) edgesDS.update(eu);
  }
  var pt = document.getElementById('phase-text');
  if (pt) pt.textContent = 'Waiting...';
  var pd = document.getElementById('phase-dot');
  if (pd) pd.style.background = '#6b7280';
  var ca = document.getElementById('current-action');
  if (ca) ca.textContent = 'LLM reasoning visualization';
  showHubNodes();
}

function fitGraph() { if (network) network.fit({ animation: true }); }

function togglePhysics() {
  physicsEnabled = !physicsEnabled;
  if (network) network.setOptions({ physics: { enabled: physicsEnabled } });
}

initGraph();
pollEvents();
</script>
</body>
</html>