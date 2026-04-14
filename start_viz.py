#!/usr/bin/env python3
import sys, os, time
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '.trany', 'mcps', 'tranycode-core'))

os.environ.setdefault('MCP_PROJECT_ROOT', os.path.dirname(os.path.abspath(__file__)))

from modules.engine_graph import knowledge_graph
from modules.reasoning_viz import viz_server, _VizHandler, emit_reasoning_event

_VizHandler._html_cache = None
if viz_server.is_running():
    viz_server.stop()
    time.sleep(0.5)

result = viz_server.start()
print(result)
url = viz_server.get_url()
print(f"URL: {url}")
print(f"Graph: {knowledge_graph.graph.number_of_nodes()} nodes, {knowledge_graph.graph.number_of_edges()} edges")

emit_reasoning_event(event_type="start", tool_name="system", query="Server started", phase="idle", summary=f"Running at {url}")

print(f"\nServer running. Press Ctrl+C to stop.")
try:
    while True:
        time.sleep(1)
except KeyboardInterrupt:
    viz_server.stop()
    print("Stopped.")
