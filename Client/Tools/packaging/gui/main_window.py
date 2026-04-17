import sys
import threading
from datetime import datetime
from pathlib import Path

from PyQt6.QtWidgets import (
    QApplication, QMainWindow, QWidget, QVBoxLayout, QHBoxLayout,
    QLabel, QPushButton, QCheckBox, QLineEdit, QTextEdit,
    QProgressBar, QGroupBox, QGridLayout, QFileDialog,
    QMessageBox, QSplitter, QFrame, QSizePolicy,
)
from PyQt6.QtCore import Qt, pyqtSignal, QObject
from PyQt6.QtGui import QFont, QColor, QPalette

sys.path.insert(0, str(Path(__file__).parent.parent))
from core.config import BuildConfig
from core.engine import BuildEngine
from core.environment import EnvironmentDetector


PLATFORMS = [
    ("windows", "Windows", "Desktop .exe"),
    ("macos", "macOS", ".app/.zip + signing"),
    ("android", "Android", ".apk"),
    ("ios", "iOS", "xcodebuild + .ipa"),
    ("web", "Web", "HTML5/WASM"),
    ("wechat", "WeChat", "Mini Game"),
]

STYLE_SHEET = """
QMainWindow { background-color: #1e1e2e; }
QWidget { background-color: #1e1e2e; color: #cdd6f4; font-family: 'Helvetica Neue', sans-serif; }
QGroupBox { border: 1px solid #45475a; border-radius: 8px; margin-top: 12px; padding-top: 16px; font-weight: bold; font-size: 13px; }
QGroupBox::title { subcontrol-origin: margin; left: 12px; padding: 0 6px; color: #89b4fa; }
QPushButton { background-color: #313244; border: 1px solid #45475a; border-radius: 6px; padding: 8px 16px; font-size: 13px; color: #cdd6f4; }
QPushButton:hover { background-color: #45475a; }
QPushButton:pressed { background-color: #585b70; }
QPushButton:disabled { background-color: #1e1e2e; color: #585b70; border-color: #313244; }
QPushButton#startBtn { background-color: #a6e3a1; color: #1e1e2e; font-weight: bold; font-size: 14px; border: none; }
QPushButton#startBtn:hover { background-color: #94e2d5; }
QPushButton#startBtn:disabled { background-color: #45475a; color: #585b70; }
QPushButton#stopBtn { background-color: #f38ba8; color: #1e1e2e; font-weight: bold; border: none; }
QCheckBox { spacing: 8px; font-size: 13px; }
QCheckBox::indicator { width: 18px; height: 18px; border-radius: 4px; border: 2px solid #45475a; }
QCheckBox::indicator:checked { background-color: #89b4fa; border-color: #89b4fa; }
QCheckBox::indicator:unchecked { background-color: #313244; }
QLineEdit { background-color: #313244; border: 1px solid #45475a; border-radius: 6px; padding: 6px 10px; font-size: 12px; color: #cdd6f4; }
QTextEdit { background-color: #11111b; border: 1px solid #313244; border-radius: 6px; font-family: 'SF Mono', 'Menlo', monospace; font-size: 11px; color: #a6e3a1; padding: 4px; }
QProgressBar { background-color: #313244; border: none; border-radius: 4px; height: 20px; text-align: center; color: #cdd6f4; font-size: 11px; }
QProgressBar::chunk { background-color: #89b4fa; border-radius: 4px; }
QLabel#envOk { color: #a6e3a1; }
QLabel#envWarn { color: #f9e2af; }
QLabel#envErr { color: #f38ba8; }
QLabel#sectionTitle { color: #89b4fa; font-size: 14px; font-weight: bold; }
"""


class SignalBridge(QObject):
    log_signal = pyqtSignal(str, str)
    progress_signal = pyqtSignal(int, str)
    build_done_signal = pyqtSignal(dict)
    env_done_signal = pyqtSignal(list)


class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.config = BuildConfig()
        self.engine = BuildEngine(self.config)
        self.signals = SignalBridge()
        self.platform_checks: dict[str, QCheckBox] = {}

        self.signals.log_signal.connect(self._on_log)
        self.signals.progress_signal.connect(self._on_progress)
        self.signals.build_done_signal.connect(self._on_build_done)
        self.signals.env_done_signal.connect(self._on_env_done)

        self.engine.set_log_callback(self._log_handler)
        self.engine.set_progress_callback(self._progress_handler)

        self._init_ui()
        self._detect_environment()

    def _init_ui(self) -> None:
        self.setWindowTitle(f"Build Tool v2.0 - {self.config.project_name}")
        self.setMinimumSize(1100, 720)
        self.setStyleSheet(STYLE_SHEET)

        central = QWidget()
        self.setCentralWidget(central)
        main_layout = QHBoxLayout(central)
        main_layout.setSpacing(12)
        main_layout.setContentsMargins(12, 12, 12, 12)

        left_panel = self._create_left_panel()
        center_panel = self._create_center_panel()
        right_panel = self._create_right_panel()

        splitter = QSplitter(Qt.Orientation.Horizontal)
        splitter.addWidget(left_panel)
        splitter.addWidget(center_panel)
        splitter.addWidget(right_panel)
        splitter.setSizes([260, 340, 440])
        splitter.setHandleWidth(2)

        main_layout.addWidget(splitter)

    def _create_left_panel(self) -> QWidget:
        panel = QWidget()
        layout = QVBoxLayout(panel)
        layout.setSpacing(8)

        title = QLabel("Platforms")
        title.setObjectName("sectionTitle")
        layout.addWidget(title)

        for pid, name, desc in PLATFORMS:
            cb = QCheckBox(f"  {name}")
            cb.setToolTip(desc)
            cb.setChecked(self.config.is_platform_enabled(pid))
            cb.stateChanged.connect(lambda state, p=pid: self._on_platform_toggle(p, state))
            self.platform_checks[pid] = cb
            layout.addWidget(cb)

        btn_row = QHBoxLayout()
        select_all_btn = QPushButton("Select All")
        select_all_btn.clicked.connect(self._select_all)
        clear_all_btn = QPushButton("Clear All")
        clear_all_btn.clicked.connect(self._clear_all)
        btn_row.addWidget(select_all_btn)
        btn_row.addWidget(clear_all_btn)
        layout.addLayout(btn_row)

        layout.addSpacing(16)

        env_title = QLabel("Environment")
        env_title.setObjectName("sectionTitle")
        layout.addWidget(env_title)

        self.env_container = QVBoxLayout()
        layout.addLayout(self.env_container)

        layout.addStretch()
        return panel

    def _create_center_panel(self) -> QWidget:
        panel = QWidget()
        layout = QVBoxLayout(panel)
        layout.setSpacing(8)

        config_group = QGroupBox("Configuration")
        config_layout = QGridLayout(config_group)

        config_layout.addWidget(QLabel("Version:"), 0, 0)
        self.version_input = QLineEdit(self.config.version)
        self.version_input.textChanged.connect(lambda t: setattr(self.config, 'version', t))
        config_layout.addWidget(self.version_input, 0, 1)

        config_layout.addWidget(QLabel("Build #:"), 1, 0)
        self.buildnum_input = QLineEdit(str(self.config.build_number))
        self.buildnum_input.setMaximumWidth(80)
        config_layout.addWidget(self.buildnum_input, 1, 1)

        config_layout.addWidget(QLabel("Godot:"), 2, 0)
        godot_row = QHBoxLayout()
        self.godot_path_input = QLineEdit(self.config.godot_path)
        self.godot_path_input.setReadOnly(True)
        godot_row.addWidget(self.godot_path_input)
        find_btn = QPushButton("Find")
        find_btn.clicked.connect(self._find_godot)
        godot_row.addWidget(find_btn)
        browse_btn = QPushButton("Browse")
        browse_btn.clicked.connect(self._browse_godot)
        godot_row.addWidget(browse_btn)
        config_layout.addLayout(godot_row, 2, 1)

        self.debug_check = QCheckBox("Debug Mode")
        config_layout.addWidget(self.debug_check, 3, 0, 1, 2)

        self.sign_check = QCheckBox("Code Sign & Notarize (macOS/iOS)")
        config_layout.addWidget(self.sign_check, 4, 0, 1, 2)

        layout.addWidget(config_group)

        steps_group = QGroupBox("Build Steps Preview")
        steps_layout = QVBoxLayout(steps_group)
        self.steps_label = QLabel(self._get_steps_text())
        self.steps_label.setWordWrap(True)
        self.steps_label.setFont(QFont("SF Mono", 10))
        steps_layout.addWidget(self.steps_label)
        layout.addWidget(steps_group)

        action_layout = QVBoxLayout()
        self.start_btn = QPushButton("START BUILD")
        self.start_btn.setObjectName("startBtn")
        self.start_btn.setMinimumHeight(48)
        self.start_btn.clicked.connect(self._start_build)
        action_layout.addWidget(self.start_btn)

        action_row = QHBoxLayout()
        self.stop_btn = QPushButton("STOP")
        self.stop_btn.setObjectName("stopBtn")
        self.stop_btn.setEnabled(False)
        self.stop_btn.clicked.connect(self._stop_build)
        action_row.addWidget(self.stop_btn)

        clean_btn = QPushButton("Clean")
        clean_btn.clicked.connect(self._clean_build)
        action_row.addWidget(clean_btn)

        open_btn = QPushButton("Open Output")
        open_btn.clicked.connect(self._open_output)
        action_row.addWidget(open_btn)
        action_layout.addLayout(action_row)

        layout.addLayout(action_layout)
        return panel

    def _create_right_panel(self) -> QWidget:
        panel = QWidget()
        layout = QVBoxLayout(panel)
        layout.setSpacing(8)

        log_title = QLabel("Build Log")
        log_title.setObjectName("sectionTitle")
        layout.addWidget(log_title)

        self.log_text = QTextEdit()
        self.log_text.setReadOnly(True)
        layout.addWidget(self.log_text)

        self.progress_bar = QProgressBar()
        self.progress_bar.setValue(0)
        layout.addWidget(self.progress_bar)

        self.status_label = QLabel("Ready")
        self.status_label.setAlignment(Qt.AlignmentFlag.AlignCenter)
        layout.addWidget(self.status_label)

        return panel

    def _get_steps_text(self) -> str:
        enabled = [p for p, cb in self.platform_checks.items() if cb.isChecked()]
        lines = ["1. dotnet build (C# compilation)"]
        if enabled:
            lines.append(f"2. Godot export ({len(enabled)} platform(s))")
            if "ios" in enabled:
                lines.append("   iOS: xcodebuild archive + export IPA")
            if "macos" in enabled and self.sign_check.isChecked():
                lines.append("   macOS: codesign + notarize + staple")
            if "android" in enabled and self.sign_check.isChecked():
                lines.append("   Android: zipalign + apksigner")
            if "wechat" in enabled:
                lines.append("   WeChat: web export + adapter conversion")
        lines.append("3. Validate output files")
        lines.append("4. Generate build report")
        return "\n".join(lines)

    def _detect_environment(self) -> None:
        self.status_label.setText("Detecting environment...")
        threading.Thread(target=self._do_detect_env, daemon=True).start()

    def _do_detect_env(self) -> None:
        detector = EnvironmentDetector()
        results = detector.detect_all()
        self.signals.env_done_signal.emit(results)

    def _on_env_done(self, results: list) -> None:
        for i in reversed(range(self.env_container.count())):
            widget = self.env_container.itemAt(i).widget()
            if widget:
                widget.setParent(None)

        for r in results:
            label = QLabel()
            if r.installed:
                icon = "OK"
                obj_name = "envOk"
                ver = f" {r.version}" if r.version else ""
                label.setText(f"  {icon}  {r.name}{ver}")
            else:
                icon = "!!"
                obj_name = "envWarn"
                label.setText(f"  {icon}  {r.name} - {r.message}")
            label.setObjectName(obj_name)
            label.setStyleSheet(label.styleSheet())
            label.setToolTip(", ".join(r.required_for) if r.required_for else "")
            self.env_container.addWidget(label)

        self.status_label.setText("Environment detected")

    def _on_platform_toggle(self, platform: str, state: int) -> None:
        enabled = state == Qt.CheckState.Checked.value
        self.config.set_platform_enabled(platform, enabled)
        self.config.save()
        self.steps_label.setText(self._get_steps_text())

    def _select_all(self) -> None:
        for cb in self.platform_checks.values():
            cb.setChecked(True)

    def _clear_all(self) -> None:
        for cb in self.platform_checks.values():
            cb.setChecked(False)

    def _find_godot(self) -> None:
        detector = EnvironmentDetector()
        results = detector.detect_all()
        for r in results:
            if r.name == "Godot Engine" and r.installed:
                self.godot_path_input.setText(r.path)
                self.config.godot_path = r.path
                self.config.save()
                self._log(f"Found Godot: {r.path} {r.version}", "ok")
                return
        self._log("Godot not found. Use Browse to set manually.", "warn")

    def _browse_godot(self) -> None:
        path, _ = QFileDialog.getOpenFileName(
            self, "Select Godot Executable",
            "/Applications",
            "App Bundle (*.app);;All Files (*)",
        )
        if path:
            if path.endswith(".app"):
                import os
                exe = os.path.join(path, "Contents", "MacOS", "Godot")
                if os.path.exists(exe):
                    path = exe
            self.godot_path_input.setText(path)
            self.config.godot_path = path
            self.config.save()
            self._log(f"Godot path set: {path}", "ok")

    def _start_build(self) -> None:
        selected = [p for p, cb in self.platform_checks.items() if cb.isChecked()]
        if not selected:
            QMessageBox.warning(self, "Warning", "Select at least 1 platform!")
            return
        if not self.config.godot_path:
            QMessageBox.critical(self, "Error", "Set Godot path first!")
            return

        self.start_btn.setEnabled(False)
        self.start_btn.setText("BUILDING...")
        self.stop_btn.setEnabled(True)
        self.log_text.clear()
        self.progress_bar.setValue(0)

        debug = self.debug_check.isChecked()
        if self.sign_check.isChecked():
            self.config.set("platforms.macos.sign", True)

        threading.Thread(
            target=self._do_build,
            args=(selected, debug),
            daemon=True,
        ).start()

    def _do_build(self, platforms: list, debug: bool) -> None:
        results = self.engine.build_platforms(platforms, debug)
        self.signals.build_done_signal.emit(results)

    def _stop_build(self) -> None:
        self.engine.stop()
        self._log("Build stopped by user", "warn")

    def _on_build_done(self, results: dict) -> None:
        self.start_btn.setEnabled(True)
        self.start_btn.setText("START BUILD")
        self.stop_btn.setEnabled(False)

        success = sum(1 for r in results.values() if r.get("status") == "success")
        total = len(results)

        if success == total:
            self.status_label.setText(f"All {total} platforms succeeded!")
            QMessageBox.information(self, "Done", f"All {total} platforms built successfully!")
        else:
            self.status_label.setText(f"Partial: {success}/{total} succeeded")
            QMessageBox.warning(self, "Done", f"Success: {success}/{total}")

    def _clean_build(self) -> None:
        reply = QMessageBox.question(
            self, "Confirm", f"Delete build directory?\n{self.config.build_dir}",
            QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No,
        )
        if reply == QMessageBox.StandardButton.Yes:
            import shutil
            if self.config.build_dir.exists():
                shutil.rmtree(self.config.build_dir)
            self._log("Build directory cleaned", "ok")

    def _open_output(self) -> None:
        import subprocess
        self.config.build_dir.mkdir(parents=True, exist_ok=True)
        subprocess.run(["open", str(self.config.build_dir)])

    def _log_handler(self, msg: str, level: str) -> None:
        self.signals.log_signal.emit(msg, level)

    def _progress_handler(self, pct: int, status: str) -> None:
        self.signals.progress_signal.emit(pct, status)

    def _on_log(self, msg: str, level: str) -> None:
        ts = datetime.now().strftime("[%H:%M:%S]")
        color_map = {"ok": "#a6e3a1", "err": "#f38ba8", "warn": "#f9e2af", "hdr": "#89b4fa"}
        color = color_map.get(level, "#cdd6f4")
        self.log_text.append(f'<span style="color:{color}">{ts} {msg}</span>')

    def _on_progress(self, pct: int, status: str) -> None:
        self.progress_bar.setValue(pct)
        self.status_label.setText(status)

    def closeEvent(self, event) -> None:
        if self.engine.is_building:
            reply = QMessageBox.question(
                self, "Confirm", "Build in progress. Quit anyway?",
                QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No,
            )
            if reply == QMessageBox.StandardButton.No:
                event.ignore()
                return
            self.engine.stop()
        event.accept()
