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
.tl-item .tl-node-tag{background:#1a2332;color:#58a6ff;padding:1px 6px;border-radius:3px;font-size:10px;cursor:pointer;border:1px solid #21262d}
.tl-item .tl-node-tag:hover{border-color:#58a6ff}
.tl-item .tl-phase{display:inline-block;padding:1px 6px;border-radius:3px;font-size:10px;font-weight:600;margin-left:6px}
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
const COMMUNITY_COLORS = ['#ef4444','#f97316','#f59e0b','#84cc16','#22c55e','#06b6d4','#3b82f6','#8b5cf6','#d946ef','#ec4899'];
const PHASE_COLORS = {searching:'#3b82f6',analyzing:'#8b5cf6',traversing:'#f59e0b',synthesizing:'#10b981',debugging:'#ef4444',idle:'#6b7280'};
const EVENT_ICONS = {tool_call:'🔧',node_visit:'📍',edge_traverse:'🔗',community_explore:'🏘️',thinking:'💭',result:'✅',error:'❌',start:'🚀',end:'🏁'};
let network=null,nodesDS=null,edgesDS=null,lastEventIndex=0,pollInterval=500;
let visitedNodeIds=new Set(),visitedEdgeKeys=new Set(),physicsEnabled=false,allGraphNodeIds=new Set();
let timelineItems=[],nodeDegreeMap={},hasActivity=false;

function esc(s){return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;')}

function getCommColor(comm){return COMMUNITY_COLORS[(comm<0?0:comm)%COMMUNITY_COLORS.length]}

async function initGraph(){
  try{
    const resp=await fetch('/graph_data');
    const data=await resp.json();
    const rawNodes=(data.nodes||[]);
    const rawLinks=(data.links||[]);

    // Build degree map for smart sizing
    nodeDegreeMap={};
    rawLinks.forEach(e=>{
      nodeDegreeMap[e.source]=(nodeDegreeMap[e.source]||0)+1;
      nodeDegreeMap[e.target]=(nodeDegreeMap[e.target]||0)+1;
    });
    let maxDeg=Math.max(1,...Object.values(nodeDegreeMap));

    // Smart node rendering: size & label based on degree
    const visNodes=rawNodes.map(n=>{
      const deg=nodeDegreeMap[n.id]||0;
      const isHub=deg>maxDeg*0.15;
      const commColor=getCommColor(n.community);
      return {
        id:n.id,
        label:n.label||n.id,
        color:{background:isHub?commColor:'#21262d',border:isHub?commColor:'#30363d',highlight:{background:'#fff',border:commColor}},
        size:isHub?Math.max(12,4+30*(deg/maxDeg)):Math.max(3,3+8*(deg/maxDeg)),
        font:{size:isHub?11:(deg>maxDeg*0.05?9:0),color:isHub?'#fff':'#8b949e',face:'monospace'},
        title:esc((n.label||n.id)+'\nType: '+(n.file_type||'?')+'\nFile: '+(n.source_file||'')+'\nCommunity: '+n.community),
        _source_file:n.source_file||'',_file_type:n.file_type||'',_community:n.community??-1,_deg:deg
      };
    });

    const visEdges=rawLinks.map((e,i)=>({
      id:i,from:e.source,to:e.target,
      title:esc((e.relation||'')+' ['+(e.confidence||'')+']'),
      arrows:{to:{enabled:true,scaleFactor:0.35}},
      color:{opacity:0.25,color:'#484f58'},width:0.4,
      dashes:false,smooth:{type:'continuous',roundness:0.1}
    }));

    visNodes.forEach(n=>allGraphNodeIds.add(n.id));
    nodesDS=new vis.DataSet(visNodes);
    edgesDS=new vis.DataSet(visEdges);

    document.getElementById('loading-grid').style.display='none';
    document.getElementById('graph-info').innerHTML=
      '<div class="gi-title">📊 Knowledge Graph</div>'+
      '<div class="gi-stats"><span>'+visNodes.length+' nodes</span><span>'+visEdges.length+' edges</span><span>Click "Show All" to view</span></div>';

    const container=document.getElementById('graph');
    network=new vis.Network(container,{nodes:nodesDS,edges:edgesDS},{
      physics:{enabled:true,solver:'forceAtlas2Based',
        forceAtlas2Based:{gravitationalConstant:-80,centralGravity:0.005,springLength:150,springConstant:0.04,damping:0.3,avoidOverlap:1},
        stabilization:{iterations:300,fit:true,updateInterval:25}},
      interaction:{hover:true,tooltipDelay:100,hideEdgesOnDrag:true,navigationButtons:false,keyboard:false,multiselect:false,zoomView:true,dragView:true},
      nodes:{shape:'dot',borderWidth:1,borderWidthSelected:2,scaling:{min:2,max:30}},
      edges:{selectionWidth:1.5}
    });

    network.on('stabilizationProgress',(params)=>{
      document.getElementById('loading-graph').textContent='Layout: '+Math.round(params.iterations)+'/'+params.total+' ('+Math.round(params.perc)+'%)';
    });

    network.once('stabilizationIterationsDone',()=>{
      network.setOptions({physics:{enabled:false}});
      document.getElementById('loading-graph').style.display='none';
      showHubNodes();
    });

  }catch(err){
    console.error('Graph load failed:',err);
    document.getElementById('graph').innerHTML='<div style="display:flex;align-items:center;justify-content:center;height:100%;color:#484f58;text-align:center"><div><div style="font-size:40px;margin-bottom:12px">📊</div>No graph data<br><small>Run graphify or sync index first</small></div></div>';
  }
}

function showHubNodes(){
  if(!nodesDS) return;
  const hubThreshold=Math.max(3,...Object.values(nodeDegreeMap))*0.08;
  const updates=[];
  nodesDS.get().forEach(n=>{
    if((n._deg||0)>=hubThreshold){
      updates.push({id:n.id,hidden:false});
    }else{
      updates.push({id:n.id,hidden:true});
    }
  });
  if(updates.length) nodesDS.update(updates);
  if(network) setTimeout(()=>network.fit({animation:{duration:400}}),100);
}

function showAllNodes(){
  if(!nodesDS) return;
  hasActivity=true;
  document.getElementById('overlay').classList.add('overlay-visible');
  const updates=nodesDS.get().map(n=>({id:n.id,hidden:false}));
  if(updates.length) nodesDS.update(updates);
  if(network) setTimeout(()=>network.fit({animation:{duration:500}}),100);
}

function highlightNodes(nodeIds, phase){
  if(!nodesDS) return;
  hasActivity=true;
  document.getElementById('overlay').classList.add('overlay-visible');
  const color=PHASE_COLORS[phase]||'#58a6ff';
  const allNodeIds=nodesDS.getIds();
  const matchSet=new Set(nodeIds.filter(id=>allGraphNodeIds.has(id)));
  if(matchSet.size===0) return;

  const updates=allNodeIds.map(id=>{
    const n=nodesDS.get(id);
    const isMatch=matchSet.has(id);
    return {
      id,
      hidden:false,
      color:isMatch?
        {background:color,border:color,highlight:{background:'#fff',border:color}}:
        {background:'#161b22',border:'#21262d',highlight:{background:'#30363d',border:'#484f58'}},
      size:isMatch?18:((n._deg||0)>3?6:3),
      font:{size:isMatch?11:((n._deg||0)>3?8:0),color:isMatch?'#fff':'#484f58'},
      borderWidth:isMatch?2:1
    };
  });
  if(updates.length) nodesDS.update(updates);

  matchSet.forEach(mid=>{
    const connected=network.getConnectedNodes(mid)||[];
    connected.forEach(cid=>{
      if(!matchSet.has(cid)){
        const edgeId=findEdgeId(mid,cid);
        if(edgeId!==null) edgesDS.update({id:edgeId,color:{opacity:0.15,color:'#484f58'},width:0.3});
      }
    }
  });

  matchSet.forEach(src=>{
    matchSet.forEach(tgt=>{
      if(src!==tgt){
        const eid=findEdgeId(src,tgt);
        if(eid!==null) edgesDS.update({id:eid,color:{opacity:0.9,color},width:2.5});
      }
    });
  });

  visitedNodeIds=new Set([...visitedNodeIds,...matchSet]);
  if(network&&matchSet.size>0){
    network.focus(Array.from(matchSet)[0],{scale:1.3,animation:{duration:500,easingFunction:'easeInOutQuad'}});
  }
}

function findEdgeId(fromId,toId){
  if(!edgesDS) return null;
  const matches=edgesDS.get().filter(e=>(e.from===fromId&&e.to===toId)||(e.from===toId&&e.to===fromId));
  return matches.length>0?matches[0].id:null;
}

function highlightEdges(edgeList, phase){
  if(!edgesDS) return;
  const color=PHASE_COLORS[phase]||'#58a6ff';
  edgeList.forEach(e=>{
    const fromId=e.from||e.source;const toId=e.to||e.target;
    if(!fromId||!toId) return;
    visitedEdgeKeys.add(fromId+'->'+toId);
    const eid=findEdgeId(fromId,toId);
    if(eid!==null) edgesDS.update({id:eid,color:{opacity:0.9,color},width:2.5,dashes:false});
  });
}

function addTimelineItem(evt){
  const timeline=document.getElementById('timeline');
  const emptyState=document.getElementById('empty-state');
  if(emptyState) emptyState.remove();

  const icon=EVENT_ICONS[evt.event_type]||'📌';
  const phaseColor=PHASE_COLORS[evt.phase]||'#6b7280';
  const timeStr=new Date(evt.timestamp*1000).toLocaleTimeString('zh-CN',{hour12:false,hour:'2-digit',minute:'2-digit',second:'2-digit'});

  const item=document.createElement('div');
  item.className='tl-item';item.dataset.eventIndex=timelineItems.length;

  let nodesHtml='';
  if(evt.nodes&&evt.nodes.length>0){
    nodesHtml='<div class="tl-nodes">'+evt.nodes.slice(0,8).map(n=>
      `<span class="tl-node-tag" onclick="focusNode('${esc(n)}')">${esc(n.length>20?n.slice(0,20)+'…':n)}</span>`
    ).join('')+(evt.nodes.length>8?'<span class="tl-node-tag">+'+(evt.nodes.length-8)+'</span>':'')+'</div>';
  }

  item.innerHTML='<div class="tl-head"><span><span class="tl-icon">'+icon+'</span> <span class="tl-tool">'+esc(evt.tool_name)+'</span><span class="tl-phase" style="background:'+phaseColor+'22;color:'+phaseColor+'">'+esc(evt.phase)+'</span></span><span class="tl-time">'+timeStr+'</span></div><div class="tl-query">'+esc(evt.query||'')+'</div>'+(evt.summary?'<div class="tl-summary">'+esc(evt.summary)+'</div>':'')+nodesHtml;

  item.addEventListener('click',()=>{
    document.querySelectorAll('.tl-item.active').forEach(el=>el.classList.remove('active'));
    item.classList.add('active');
    if(evt.nodes&&evt.nodes.length>0) highlightNodes(evt.nodes,evt.phase);
    if(evt.edges&&evt.edges.length>0) highlightEdges(evt.edges,evt.phase);
  });

  timeline.appendChild(item);timelineItems.push(evt);
  item.scrollIntoView({behavior:'smooth',block:'nearest'});
}

function updateOverlay(evt){
  const dot=document.getElementById('phase-dot');const txt=document.getElementById('phase-text');const act=document.getElementById('current-action');
  const c=PHASE_COLORS[evt.phase]||'#6b7280';
  if(dot){dot.style.background=c}if(txt){txt.textContent=evt.phase.toUpperCase();txt.color=c}
  if(act) act.textContent=evt.tool_name+': '+(evt.query||evt.summary||'');
}

function updateStats(){
  fetch('/stats').then(r=>r.json()).then(s=>{
    document.getElementById('stat-events').textContent=s.total_events;
    document.getElementById('stat-nodes').textContent=s.visited_nodes;
    document.getElementById('stat-edges').textContent=s.visited_edges;
    document.getElementById('stat-duration').textContent=s.session_duration+'s';
    const d=document.getElementById('phase-dot');if(d)d.style.background=PHASE_COLORS[s.current_phase]||'#6b7280';
  }).catch(()=>{});
}

async function pollEvents(){
  try{
    const r=await fetch('/events?after='+lastEventIndex);
    const evts=await r.json();
    if(evts.length>0){evts.forEach(e=>{addTimelineItem(e);updateOverlay(e);if(e.nodes&&e.nodes.length>0) highlightNodes(e.nodes,e.phase);if(e.edges&&e.edges.length>0) highlightEdges(e.edges,e.phase)});lastEventIndex+=evts.length;}
  }catch(err){}
  updateStats();setTimeout(pollEvents,pollInterval);
}

function focusNode(nodeId){
  if(network&&allGraphNodeIds.has(nodeId)){network.focus(nodeId,{scale:1.8,animation:true});network.selectNodes([nodeId]);highlightNodes([nodeId],'analyzing');}
}

function clearTimeline(){
  document.getElementById('timeline').innerHTML='<div id="empty-state"><div class="icon">🧠</div><div>Waiting for LLM activity...</div></div>';
  timelineItems=[];lastEventIndex=0;visitedNodeIds.clear();visitedEdgeKeys.clear();hasActivity=false;
  document.getElementById('overlay').classList.remove('overlay-visible');
  fetch('/clear').catch(()=>{});
  if(nodesDS){const u=nodesDS.get().map(n=>({id:n.id,color:{background:getCommColor(n._community),border:getCommColor(n._community),highlight:{background:'#fff',border:getCommColor(n._community)}},size:(n._deg||0)>3?6:3,font:{size:(n._deg||0)>3?8:0,color:'#8b949e'},borderWidth:1,hidden:(n._deg||0)<(Math.max(1,...Object.values(nodeDegreeMap)))*0.08}));if(u.length)nodesDS.update(u);}
  if(edgesDS){const u=edgesDS.get().map(e=>({id:e.id,color:{opacity:0.25,color:'#484f58'},width:0.4,dashes:false}));if(u.length)edgesDS.update(u);}
  const pt=document.getElementById('phase-text');if(pt)pt.textContent='Waiting...';
  const pd=document.getElementById('phase-dot');if(pd)pd.style.background='#6b7280';
  const ca=document.getElementById('current-action');if(ca)ca.textContent='LLM reasoning visualization';
  showHubNodes();
}

function fitGraph(){if(network) network.fit({animation:true})}
function togglePhysics(){physicsEnabled=!physicsEnabled;if(network) network.setOptions({physics:{enabled:physicsEnabled}})}

initGraph();pollEvents();
</script>
</body>
</html