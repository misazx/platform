import json
from datetime import datetime
from pathlib import Path
from typing import Any, Optional


class BuildReporter:
    def __init__(self, build_dir: Path):
        self.build_dir = build_dir
        self.report_data: dict[str, Any] = {
            "timestamp": datetime.now().isoformat(),
            "duration_seconds": 0,
            "platforms": {},
        }
        self._start_time: Optional[datetime] = None

    def start(self) -> None:
        self._start_time = datetime.now()

    def finish(self) -> None:
        if self._start_time:
            delta = datetime.now() - self._start_time
            self.report_data["duration_seconds"] = round(delta.total_seconds(), 1)
        self._save()

    def add_platform_result(
        self,
        platform: str,
        status: str,
        output: str = "",
        size_mb: float = 0.0,
        duration_seconds: float = 0.0,
        error: str = "",
        extra: Optional[dict] = None,
    ) -> None:
        entry: dict[str, Any] = {
            "status": status,
            "output": output,
            "size_mb": size_mb,
            "duration_seconds": round(duration_seconds, 1),
        }
        if error:
            entry["error"] = error
        if extra:
            entry.update(extra)
        self.report_data["platforms"][platform] = entry

    def _save(self) -> None:
        self.build_dir.mkdir(parents=True, exist_ok=True)
        report_path = self.build_dir / "build_report.json"
        with open(report_path, "w", encoding="utf-8") as f:
            json.dump(self.report_data, f, indent=2, ensure_ascii=False)

    def get_summary(self) -> str:
        lines = []
        lines.append("=" * 60)
        lines.append("BUILD REPORT")
        lines.append("=" * 60)
        lines.append(f"Duration: {self.report_data['duration_seconds']}s")
        lines.append("")

        platforms = self.report_data.get("platforms", {})
        success_count = sum(1 for p in platforms.values() if p["status"] == "success")
        total_count = len(platforms)

        for plat, info in platforms.items():
            status_icon = "OK" if info["status"] == "success" else "FAIL"
            size_str = f" ({info['size_mb']}MB)" if info.get("size_mb") else ""
            lines.append(f"  [{status_icon}] {plat}{size_str}")
            if info.get("error"):
                lines.append(f"       Error: {info['error']}")

        lines.append("")
        lines.append(f"Result: {success_count}/{total_count} succeeded")
        lines.append("=" * 60)

        return "\n".join(lines)
