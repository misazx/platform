#!/usr/bin/env python3
import sys, os, time
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '.trany', 'mcps', 'tranycode-core'))
from modules.reasoning_viz import emit_reasoning_event
from modules.engine_graph import knowledge_graph

names = list(knowledge_graph._name_index.keys())
hubs = [(n, knowledge_graph._name_index[n]) for n in names if len(knowledge_graph._name_index[n]) >= 3][:5]
print(f'Hubs: {[h[0] for h in hubs]}')

name, ids = hubs[0]
emit_reasoning_event(event_type='tool_call', tool_name='code_search', query=name, phase='searching', summary=f'Searching for {name}')
time.sleep(0.3)
emit_reasoning_event(event_type='node_visit', tool_name='code_search', query=name, nodes=ids[:5], phase='searching', summary=f'Found {len(ids)} nodes for {name}')
time.sleep(0.3)

name2, ids2 = hubs[1]
emit_reasoning_event(event_type='edge_traverse', tool_name='trace_chain', query=f'{name} -> {name2}', nodes=ids[:3]+ids2[:3], edges=[{'from': ids[0], 'to': ids2[0]}], phase='traversing', summary=f'Tracing: {name} -> {name2}')
time.sleep(0.3)

emit_reasoning_event(event_type='result', tool_name='code_search', query=name, nodes=ids[:5], phase='synthesizing', summary=f'Complete: {len(ids)} matches')
print('Events emitted! Check http://localhost:18765')
