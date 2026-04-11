#!/usr/bin/env python3
"""Quick test to verify TypeError fix"""

import sys
import os
import time
import urllib.request
import json

sys.path.insert(0, os.path.join(os.path.dirname(__file__), '.trany', 'mcps', 'tranycode-core'))

from modules.reasoning_viz import viz_server, emit_reasoning_event

print("🔧 Testing TypeError fix...")
print("=" * 50)

# Start server
result = viz_server.start()
print(f"✅ {result}")
url = viz_server.get_url()

# Emit events rapidly to trigger stabilization
print("\n⚡ Emitting events to trigger graph stabilization...")
for i in range(10):
    emit_reasoning_event(
        event_type="node_visit",
        tool_name="test",
        query=f"Test event {i}",
        nodes=[f"Node{i}", f"Node{i+1}"],
        phase="searching",
        summary=f"Testing stabilization {i}"
    )
    time.sleep(0.1)

# Wait for stabilization to complete
print("⏳ Waiting for graph stabilization (5s)...")
time.sleep(5)

# Test endpoints
print("\n📡 Testing API endpoints...")
try:
    req = urllib.request.Request(f"{url}/stats")
    with urllib.request.urlopen(req, timeout=5) as response:
        stats = json.loads(response.read().decode())
        print(f"✅ Stats: {stats['total_events']} events")
except Exception as e:
    print(f"❌ Error: {e}")

print("\n" + "=" * 50)
print("✅ Test completed!")
print(f"\n🌐 Open {url} and check browser console for errors")
print("   There should be NO TypeError messages")

# Stop after 15 seconds
print("\n⏳ Server will stop in 15 seconds...")
time.sleep(15)
viz_server.stop()
print("✅ Server stopped")
