#!/bin/bash
cd "$(dirname "$0")"
exec /usr/bin/python3 Client/Tools/build_gui.py "$@"
