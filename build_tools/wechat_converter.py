#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
微信小游戏转换器
将 Godot Web 导出转换为微信小游戏格式

功能：
1. 生成微信小游戏适配层（weapp-adapter.js）
2. 转换 HTML 为 game.js 入口文件
3. 适配 Canvas 和 Audio API
4. 处理资源加载和文件系统
5. 生成 project.config.json 配置

使用方法：
  python wechat_converter.py --input <web_export_dir> --output <wechat_output_dir>
"""

import os
import sys
import json
import shutil
import argparse
from pathlib import Path


class WeChatConverter:
    def __init__(self):
        self.adapter_code = self._generate_adapter_code()

    def convert(self, input_dir, output_dir):
        """
        将 Godot Web 导出转换为微信小游戏

        Args:
            input_dir: Godot Web 导出的目录路径
            output_dir: 微信小游戏输出目录路径
        """
        input_path = Path(input_dir).resolve()
        output_path = Path(output_dir).resolve()

        print(f"🔄 开始转换...")
        print(f"   输入: {input_path}")
        print(f"   输出: {output_path}")

        # 验证输入目录
        if not input_path.exists():
            raise FileNotFoundError(f"输入目录不存在: {input_path}")

        # 清理并创建输出目录
        if output_path.exists():
            shutil.rmtree(output_path)
        output_path.mkdir(parents=True)

        # 执行转换步骤
        self._copy_godot_files(input_path, output_path)
        self._create_adapter(output_path)
        self._create_game_js(output_path)
        self._modify_html_for_wechat(input_path, output_path)
        self._create_project_config(output_path)
        self._process_assets(input_path, output_path)

        print(f"\n✅ 转换完成!")
        print(f"   输出位置: {output_path}")
        print(f"\n📝 使用说明:")
        print(f"   1. 打开微信开发者工具")
        print(f"   2. 导入项目: {output_path}")
        print(f"   3. 填写 AppID (或使用测试号)")
        print(f"   4. 点击编译预览")

    def _copy_godot_files(self, input_path, output_path):
        """复制 Godot 引擎核心文件"""
        print("📦 复制 Godot 引擎文件...")

        # 需要复制的文件类型
        extensions = ['.wasm', '.js', '.pck', '.png', '.json', '.html']

        for item in input_path.iterdir():
            if item.is_file() and item.suffix.lower() in extensions:
                dest = output_path / item.name
                shutil.copy2(item, dest)
                print(f"   ✓ {item.name}")

    def _create_adapter(self, output_path):
        """创建微信小游戏适配层"""
        print("🔧 生成适配层 (weapp-adapter.js)...")

        adapter_file = output_path / "weapp-adapter.js"
        with open(adapter_file, 'w', encoding='utf-8') as f:
            f.write(self.adapter_code)

    def _generate_adapter_code(self):
        """生成微信小游戏适配层代码"""
        return '''/**
 * 微信小游戏适配层
 * 提供 Window、Document、Canvas 等浏览器 API 的兼容实现
 * 基于 Godot 官方和社区方案优化
 */

// 全局对象模拟
if (typeof window === 'undefined') {
    var window = global || {};
}

if (typeof document === 'undefined') {
    // Document 对象模拟
    var document = {
        createElement: function(tagName) {
            if (tagName.toLowerCase() === 'canvas') {
                return wx.createCanvas();
            }
            return {};
        },
        getElementById: function(id) {
            if (id === 'gameCanvas' || id === 'canvas') {
                return wx.createCanvas();
            }
            return null;
        },
        addEventListener: function(event, handler) {
            wx[event] && wx[event](handler);
        },
        body: {
            appendChild: function(element) {},
            removeChild: function(element) {}
        },
        cookie: '',
        title: '',
        location: {
            href: '',
            protocol: 'https:',
            host: '',
            pathname: ''
        },
        readyState: 'complete',
        hidden: false,
        visibilityState: 'visible',
        fullscreenElement: null,
        fullscreenEnabled: true,
        documentElement: {
            requestFullscreen: function() {}
        },
        exitFullscreen: function() {},
        addEventListener: function() {},
        removeEventListener: function() {},
        createEvent: function(type) {
            return {
                initEvent: function() {}
            };
        },
        querySelector: function(selector) {
            return null;
        },
        querySelectorAll: function(selector) {
            return [];
        }
    };
}

// Navigator 模拟
if (typeof navigator === 'undefined') {
    var navigator = {
        userAgent: 'wxgame',
        platform: 'wxgame',
        language: 'zh-CN',
        languages: ['zh-CN'],
        onLine: true,
        cookieEnabled: true,
        hardwareConcurrency: 4,
        deviceMemory: 4,
        maxTouchPoints: 5,
        vendor: '',
        vendorSub: '',
        productSub: '',
        webdriver: false,
        appName: 'Netscape',
        appVersion: '5.0 (Windows NT 10.0; Win64; x64)',
        appCodeName: 'Mozilla',
        product: 'Gecko',
        javaEnabled: function() { return false; },
        sendBeacon: function() { return false; },
        getBattery: function() { return Promise.resolve({}); },
        vibrate: function(pattern) {
            if (wx.vibrateShort) {
                wx.vibrateShort({ type: 'medium' });
            }
        },
        getGamepads: function() { return []; },
        requestMIDIAccess: function() { return Promise.reject(); },
        mediaDevices: {
            getUserMedia: function() { return Promise.reject(); }
        }
    };
}

// Location 模拟
if (typeof window.location === 'undefined') {
    window.location = {
        href: '',
        protocol: 'https:',
        host: '',
        hostname: '',
        port: '',
        pathname: '/',
        search: '',
        hash: '',
        origin: '',
        assign: function(url) {},
        replace: function(url) {},
        reload: function(forcedReload) {},
        toString: function() { return this.href; }
    };
}

// Screen 模拟
if (typeof screen === 'undefined') {
    var screen = {
        width: wx.getSystemInfoSync().screenWidth || 375,
        height: wx.getSystemInfoSync().screenHeight || 667,
        availWidth: wx.getSystemInfoSync().screenWidth || 375,
        availHeight: wx.getSystemInfoSync().screenHeight || 667,
        colorDepth: 24,
        pixelDepth: 24,
        orientation: {
            type: 'landscape-primary',
            angle: 90,
            onchange: null,
            lock: function() { return Promise.resolve(); },
            unlock: function() {}
        }
    };
}

// Canvas 增强
var originalCreateCanvas = typeof wx !== 'undefined' ? wx.createCanvas : null;

if (originalCreateCanvas) {
    var enhancedCanvas = originalCreateCanvas.call(wx);

    // 增强 Canvas 方法
    if (!enhancedCanvas.toDataURL) {
        enhancedCanvas.toDataURL = function(type, quality) {
            console.warn('toDataURL is not fully supported in WeChat Mini Game');
            return '';
        };
    }

    if (!enhancedCanvas.toBlob) {
        enhancedCanvas.toBlob = function(callback, type, quality) {
            console.warn('toBlob is not fully supported in WeChat Mini Game');
            callback(null);
        };
    }

    if (!enhancedCanvas.captureStream) {
        enhancedCanvas.captureStream = function(frameRate) {
            console.warn('captureStream is not supported in WeChat Mini Game');
            return null;
        };
    }

    // 覆盖 createCanvas 返回增强版
    wx.createCanvas = function() {
        return enhancedCanvas;
    };
}

// AudioContext 适配
if (typeof AudioContext === 'undefined' && typeof wx !== 'undefined') {
    if (wx.createInnerAudioContext) {
        window.AudioContext = function() {
            return {
                createBufferSource: function() {
                    return {
                        buffer: null,
                        loop: false,
                        start: function(when, offset, duration) {},
                        stop: function(when) {},
                        connect: function(destination) {},
                        disconnect: function() {},
                        onended: null
                    };
                },
                createGain: function() {
                    return {
                        gain: { value: 1.0 },
                        connect: function(dest) {},
                        disconnect: function() {}
                    };
                },
                createAnalyser: function() {
                    return {
                        fftSize: 2048,
                        frequencyBinCount: 1024,
                        getFloatFrequencyData: function(array) {},
                        getByteFrequencyData: function(array) {},
                        getByteTimeDomainData: function(array) {},
                        connect: function(dest) {},
                        disconnect: function() {}
                    };
                },
                decodeAudioData: function(audioData, successCallback, errorCallback) {
                    if (errorCallback) {
                        errorCallback(new Error('decodeAudioData not fully supported'));
                    }
                },
                destination: {},
                sampleRate: 44100,
                state: 'running',
                close: function() { return Promise.resolve(); },
                suspend: function() { return Promise.resolve(); },
                resume: function() { return Promise.resolve(); },
                onstatechange: null,
                currentTime: 0,
                listener: {
                    positionX: { value: 0 },
                    positionY: { value: 0 },
                    positionZ: { value: 0 },
                    forwardX: { value: 0 },
                    forwardY: { value: 0 },
                    forwardZ: { value: -1 },
                    upX: { value: 0 },
                    upY: { value: -1 },
                    upZ: { value: 0 }
                },
                createOscillator: function() {
                    return {
                        type: 'sine',
                        frequency: { value: 440 },
                        detune: { value: 0 },
                        connect: function(dest) {},
                        disconnect: function() {},
                        start: function(when) {},
                        stop: function(when) {},
                        onended: null
                    };
                },
                createBiquadFilter: function() {
                    return {
                        type: 'lowpass',
                        frequency: { value: 350 },
                        Q: { value: 1 },
                        gain: { value: 0 },
                        detune: { value: 0 },
                        connect: function(dest) {},
                        disconnect: function() {},
                        getFrequencyResponse: function(freqHz, magResp, phaseResp) {}
                    };
                },
                createPanner: function() {
                    return {
                        panningModel: 'equalpower',
                        distanceModel: 'inverse',
                        refDistance: 1,
                        maxDistance: 10000,
                        rolloffFactor: 1,
                        coneInnerAngle: 360,
                        coneOuterAngle: 360,
                        coneOuterGain: 0,
                        setPosition: function(x, y, z) {},
                        setOrientation: function(x, y, z, xUp, yUp, zUp) {},
                        connect: function(dest) {},
                        disconnect: function() {}
                    };
                },
                createDelay: function() {
                    return {
                        delayTime: { value: 0 },
                        connect: function(dest) {},
                        disconnect: function() {}
                    };
                },
                createDynamicsCompressor: function() {
                    return {
                        threshold: { value: -24 },
                        knee: { value: 30 },
                        ratio: { value: 12 },
                        reduction: { value: 0 },
                        attack: { value: 0.003 },
                        release: { value: 0.25 },
                        connect: function(dest) {},
                        disconnect: function() {}
                    };
                },
                createWaveShaper: function() {
                    return {
                        oversample: 'none',
                        curve: null,
                        connect: function(dest) {},
                        disconnect: function() {}
                    };
                },
                createScriptProcessor: function(bufferSize, numberOfInputChannels, numberOfOutputChannels) {
                    return {
                        bufferSize: bufferSize,
                        onaudioprocess: null,
                        connect: function(dest) {},
                        disconnect: function() {}
                    };
                }
            };
        };

        // 兼容 webkitAudioContext
        window.webkitAudioContext = window.AudioContext;
    }
}

// XMLHttpRequest 适配
if (typeof XMLHttpRequest === 'undefined' && typeof wx !== 'undefined') {
    window.XMLHttpRequest = function() {
        this.readyState = 0;
        this.status = 0;
        this.responseText = '';
        this.responseType = '';
        this.response = null;
        this.timeout = 0;
        this.withCredentials = false;
        this.upload = {};
        this.onreadystatechange = null;
        this.onload = null;
       .onerror = null;
        this.ontimeout = null;
        this.onprogress = null;
        this._method = 'GET';
        this._url = '';
        this._async = true;
        this._headers = {};

        this.open = function(method, url, async) {
            this._method = method;
            this._url = url;
            this._async = async !== false;
            this.readyState = 1;
            if (this.onreadystatechange) {
                this.onreadystatechange();
            }
        };

        this.setRequestHeader = function(name, value) {
            this._headers[name] = value;
        };

        this.getResponseHeader = function(name) {
            return this._responseHeaders ? this._responseHeaders[name] : null;
        };

        this.getAllResponseHeaders = function() {
            return this._responseHeaders || '';
        };

        this.send = function(data) {
            var self = this;
            this.readyState = 2;
            if (this.onreadystatechange) {
                this.onreadystatechange();
            }

            try {
                wx.request({
                    url: this._url,
                    method: this._method,
                    data: data,
                    header: this._headers,
                    dataType: this.responseType === 'json' ? 'json' : '其他',
                    responseType: this.responseType || 'text',
                    timeout: this.timeout || 60000,
                    success: function(res) {
                        self.readyState = 4;
                        self.status = res.statusCode;
                        self.responseText = typeof res.data === 'string' ? res.data : JSON.stringify(res.data);
                        self.response = res.data;
                        self._responseHeaders = res.header;
                        if (self.onload) {
                            self.onload();
                        }
                        if (self.onreadystatechange) {
                            self.onreadystatechange();
                        }
                    },
                    fail: function(err) {
                        self.readyState = 4;
                        self.status = 0;
                        if (self.onerror) {
                            self.onerror(err);
                        }
                        if (self.onreadystatechange) {
                            self.onreadystatechange();
                        }
                    }
                });
            } catch (e) {
                self.readyState = 4;
                self.status = 0;
                if (self.onerror) {
                    self.onerror(e);
                }
            }
        };

        this.abort = function() {
            this.readyState = 0;
            if (this.onabort) {
                this.onabort();
            }
        };
    };
}

// localStorage/sessionStorage 适配
if (typeof localStorage === 'undefined' && typeof wx !== 'undefined') {
    window.localStorage = {
        getItem: function(key) {
            try {
                return wx.getStorageSync(key) || null;
            } catch (e) {
                return null;
            }
        },
        setItem: function(key, value) {
            try {
                wx.setStorageSync(key, value);
            } catch (e) {
                console.warn('localStorage.setItem failed:', e);
            }
        },
        removeItem: function(key) {
            try {
                wx.removeStorageSync(key);
            } catch (e) {
                console.warn('localStorage.removeItem failed:', e);
            }
        },
        clear: function() {
            try {
                wx.clearStorageSync();
            } catch (e) {
                console.warn('localStorage.clear failed:', e);
            }
        },
        key: function(index) {
            try {
                var info = wx.getStorageInfoSync();
                return info.keys[index] || null;
            } catch (e) {
                return null;
            }
        },
        get length() {
            try {
                var info = wx.getStorageInfoSync();
                return info.keys.length;
            } catch (e) {
                return 0;
            }
        }
    };

    window.sessionStorage = window.localStorage;
}

// Image 适配
if (typeof Image === 'undefined' && typeof wx !== 'undefined') {
    window.Image = function() {
        var img = wx.createImage();
        img.__proto__ = {
            onload: null,
            onerror: null,
            onabort: null,
            crossOrigin: '',
            src: '',
            naturalWidth: 0,
            naturalHeight: 0,
            complete: false,
            width: 0,
            height: 0,
            x: 0,
            y: 0,
            alt: '',
            name: '',
            align: '',
            useMap: '',
            isMap: false,
            longDesc: '',
            lowsrc: '',
            border: '',
            hspace: 0,
            vspace: 0,
            decoding: 'auto',
            loading: 'auto'
        };

        Object.defineProperty(img, 'src', {
            get: function() {
                return this._src || '';
            },
            set: function(value) {
                this._src = value;
                var self = this;
                if (value) {
                    img.onload = function() {
                        if (self.onload) {
                            self.onload();
                        }
                    };
                    img.src = value;
                }
            }
        });

        return img;
    };
}

// WebSocket 适配（如果需要）
if (typeof WebSocket === 'undefined' && typeof wx !== 'undefined' && wx.connectSocket) {
    window.WebSocket = function(url, protocols) {
        var socket = {
            url: url,
            readyState: 0,
            bufferedAmount: 0,
            extensions: '',
            protocol: '',
            binaryType: 'blob',
            onopen: null,
            onmessage: null,
            onerror: null,
            onclose: null,

            CONNECTING: 0,
            OPEN: 1,
            CLOSING: 2,
            CLOSED: 3,

            send: function(data) {
                wx.sendSocketMessage({
                    data: data,
                    fail: function(err) {
                        if (socket.onerror) {
                            socket.onerror(err);
                        }
                    }
                });
            },

            close: function(code, reason) {
                wx.closeSocket({
                    code: code,
                    reason: reason
                });
            },

            get readyState() {
                return this._readyState || 0;
            },

            set readyState(state) {
                this._readyState = state;
            }
        };

        // 连接
        wx.connectSocket({
            url: url,
            protocols: protocols,
            success: function() {
                socket.readyState = 0; // CONNECTING
                wx.onSocketOpen(function(res) {
                    socket.readyState = 1; // OPEN
                    if (socket.onopen) {
                        socket.onopen(res);
                    }
                });
                wx.onSocketMessage(function(res) {
                    if (socket.onmessage) {
                        socket.onmessage({
                            data: res.data,
                            type: 'message'
                        });
                    }
                });
                wx.onSocketError(function(res) {
                    if (socket.onerror) {
                        socket.onerror(res);
                    }
                });
                wx.onSocketClose(function(res) {
                    socket.readyState = 3; // CLOSED
                    if (socket.onclose) {
                        socket.onclose({
                            code: res.code,
                            reason: res.reason,
                            wasClean: true
                        });
                    }
                });
            },
            fail: function(err) {
                if (socket.onerror) {
                    socket.onerror(err);
                }
            }
        });

        return socket;
    };
}

// Performance API 适配
if (typeof performance === 'undefined') {
    window.performance = {
        now: function() {
            return Date.now();
        },
        timing: {
            navigationStart: Date.now(),
            unloadEventStart: 0,
            unloadEventEnd: 0,
            redirectStart: 0,
            redirectEnd: 0,
            fetchStart: 0,
            domainLookupStart: 0,
            domainLookupEnd: 0,
            connectStart: 0,
            connectEnd: 0,
            secureConnectionStart: 0,
            requestStart: 0,
            responseStart: 0,
            responseEnd: 0,
            domLoading: 0,
            domInteractive: 0,
            domContentLoadedEventStart: 0,
            domContentLoadedEventEnd: 0,
            domComplete: 0,
            loadEventStart: 0,
            loadEventEnd: 0
        },
        navigation: {
            type: 0,
            redirectCount: 0
        },
        mark: function(name) {},
        measure: function(name, startMark, endMark) {},
        getEntries: function() { return []; },
        getEntriesByName: function(name) { return []; },
        getEntriesByType: function(type) { return []; },
        clearMarks: function(name) {},
        clearMeasures: function(name) {},
        memory: {
            jsHeapSizeLimit: 0,
            totalJSHeapSize: 0,
            usedJSHeapSize: 0
        }
    };
}

// URL/URLSearchParams 适配
if (typeof URL === 'undefined') {
    window.URL = function(url, base) {
        this.href = url;
        this.origin = '';
        this.protocol = 'https:';
        this.host = '';
        this.hostname = '';
        this.port = '';
        this.pathname = '/';
        this.search = '';
        this.hash = '';
        this.username = '';
        this.password = '';

        if (url) {
            try {
                var parser = document.createElement('a');
                parser.href = url;
                this.protocol = parser.protocol || 'https:';
                this.host = parser.host || '';
                this.hostname = parser.hostname || '';
                this.port = parser.port || '';
                this.pathname = parser.pathname || '/';
                this.search = parser.search || '';
                this.hash = parser.hash || '';
            } catch (e) {
                console.warn('URL parsing failed:', e);
            }
        }

        this.toString = function() { return this.href; };
        this.toJSON = function() { return this.href; };
    };

    window.URLSearchParams = function(init) {
        this._params = [];

        if (init) {
            if (typeof init === 'string') {
                var pairs = init.split('&');
                pairs.forEach(function(pair) {
                    var idx = pair.indexOf('=');
                    if (idx > -1) {
                        this._params.push([
                            decodeURIComponent(pair.substring(0, idx)),
                            decodeURIComponent(pair.substring(idx + 1))
                        ]);
                    } else if (pair) {
                        this._params.push([decodeURIComponent(pair), '']);
                    }
                }.bind(this));
            } else if (init instanceof Array) {
                this._params = init;
            } else if (typeof init === 'object') {
                Object.keys(init).forEach(function(key) {
                    this._params.push([key, init[key]]);
                }.bind(this));
            }
        }

        this.append = function(name, value) {
            this._params.push([name, value]);
        };

        this.delete = function(name) {
            this._params = this._params.filter(function(p) { return p[0] !== name; });
        };

        this.get = function(name) {
            var found = this._params.filter(function(p) { return p[0] === name; });
            return found.length > 0 ? found[0][1] : null;
        };

        this.getAll = function(name) {
            return this._params.filter(function(p) { return p[0] === name; }).map(function(p) { return p[1]; });
        };

        this.has = function(name) {
            return this._params.some(function(p) { return p[0] === name; });
        };

        this.set = function(name, value) {
            this.delete(name);
            this.append(name, value);
        };

        this.forEach = function(callback, thisArg) {
            this._params.forEach(function(pair) {
                callback.call(thisArg, pair[1], pair[0], this);
            }.bind(this));
        };

        this.toString = function() {
            return this._params.map(function(p) {
                return encodeURIComponent(p[0]) + '=' + encodeURIComponent(p[1]);
            }).join('&');
        };
    };
}

// Console 增强
if (typeof console !== 'undefined') {
    var originalLog = console.log;
    console.log = function() {
        var args = Array.prototype.slice.call(arguments);
        originalLog.apply(console, ['[Godot]'].concat(args));
        if (typeof wx !== 'undefined' && wx.getLogManager) {
            try {
                var logger = wx.getLogManager();
                logger.log.apply(logger, args);
            } catch (e) {}
        }
    };

    console.error = function() {
        var args = Array.prototype.slice.call(arguments);
        originalError.apply(console, ['[Godot Error]'].concat(args));
    };

    console.warn = function() {
        var args = Array.prototype.slice.call(arguments);
        originalWarn.apply(console, ['[Godot Warning]'].concat(args));
    };
}

// Atob/Btoa 适配
if (typeof atob === 'undefined') {
    window.atob = function(str) {
        try {
            return Buffer.from(str, 'base64').toString('binary');
        } catch (e) {
            return '';
        }
    };
}

if (typeof btoa === 'undefined') {
    window.btoa = function(str) {
        try {
            return Buffer.from(str, 'binary').toString('base64');
        } catch (e) {
            return '';
        }
    };
}

// TextEncoder/TextDecoder 适配
if (typeof TextEncoder === 'undefined') {
    window.TextEncoder = function(encoding) {
        this.encoding = encoding || 'utf-8';
        this.encode = function(str) {
            var buf = Buffer.from(str, this.encoding);
            var uint8 = new Uint8Array(buf.length);
            for (var i = 0; i < buf.length; i++) {
                uint8[i] = buf[i];
            }
            return uint8;
        };
    };
}

if (typeof TextDecoder === 'undefined') {
    window.TextDecoder = function(encoding) {
        this.encoding = encoding || 'utf-8';
        this.fatal = false;
        this.ignoreBOM = false;
        this.decode = function(uint8array) {
            var buf = Buffer.from(uint8array);
            return buf.toString(this.encoding);
        };
    };
}

// Blob/File 适配
if (typeof Blob === 'undefined') {
    window.Blob = function(parts, options) {
        this.parts = parts || [];
        this.type = (options && options.type) || '';
        this.size = 0;

        for (var i = 0; i < this.parts.length; i++) {
            if (typeof this.parts[i] === 'string') {
                this.size += this.parts[i].length;
            } else if (this.parts[i] instanceof ArrayBuffer) {
                this.size += this.parts[i].byteLength;
            }
        }

        this.slice = function(start, end, contentType) {
            return new Blob([], { type: contentType || this.type });
        };

        this.text = function() {
            return Promise.resolve('');
        };

        this.arrayBuffer = function() {
            return Promise.resolve(new ArrayBuffer(0));
        };
    };
}

if (typeof File === 'undefined') {
    window.File = function(bits, name, options) {
        Blob.call(this, bits, options);
        this.name = name;
        this.lastModified = (options && options.lastModified) || Date.now();
    };
    File.prototype = Blob.prototype;
}

// FileReader 适配
if (typeof FileReader === 'undefined') {
    window.FileReader = function() {
        this.readyState = 0;
        this.result = null;
        this.error = null;
        this.onload = null;
        this.onerror = null;
        this.onabort = null;
        this.onprogress = null;
        this.onloadstart = null;
        this.onloadend = null;

        this.EMPTY = 0;
        this.LOADING = 1;
        this.DONE = 2;

        this.readAsArrayBuffer = function(blob) {
            var self = this;
            setTimeout(function() {
                self.readyState = self.DONE;
                self.result = new ArrayBuffer(0);
                if (self.onload) {
                    self.onload();
                }
                if (self.onloadend) {
                    self.onloadend();
                }
            }, 0);
        };

        this.readAsDataURL = function(blob) {
            var self = this;
            setTimeout(function() {
                self.readyState = self.DONE;
                self.result = 'data:' + blob.type + ';base64,';
                if (self.onload) {
                    self.onload();
                }
                if (self.onloadend) {
                    self.onloadend();
                }
            }, 0);
        };

        this.readAsText = function(blob, encoding) {
            var self = this;
            setTimeout(function() {
                self.readyState = self.DONE;
                self.result = '';
                if (self.onload) {
                    self.onload();
                }
                if (self.onloadend) {
                    self.onloadend();
                }
            }, 0);
        };

        this.abort = function() {
            this.readyState = self.DONE;
            this.error = new DOMException('Aborted', 'AbortError');
            if (this.onabort) {
                this.onabort();
            }
            if (this.onloadend) {
                this.onloadend();
            }
        };
    };
}

// DOMException 适配
if (typeof DOMException === 'undefined') {
    window.DOMException = function(message, name) {
        this.message = message || '';
        this.name = name || 'Error';
        this.code = 0;
    };
    DOMException.prototype = Error.prototype;

    DOMException.ABORT_ERR = 20;
    DOMException.NETWORK_ERR = 19;
    DOMException.URL_MISMATCH_ERR = 21;
    DOMException.QUOTA_EXCEEDED_ERR = 22;
    DOMException.SECURITY_ERR = 18;
    DOMException.INVALID_STATE_ERR = 11;
    DOMException.SYNTAX_ERR = 12;
}

// IntersectionObserver 适配（空实现）
if (typeof IntersectionObserver === 'undefined') {
    window.IntersectionObserver = function(callback, options) {
        this.callback = callback;
        this.options = options || {};
        this._observations = [];

        this.observe = function(target) {
            this._observations.push(target);
        };

        this.unobserve = function(target) {
            this._observations = this._observations.filter(function(el) {
                return el !== target;
            });
        };

        this.disconnect = function() {
            this._observations = [];
        };

        this.takeRecords = function() {
            return [];
        };
    };
}

// ResizeObserver 适配（空实现）
if (typeof ResizeObserver === 'undefined') {
    window.ResizeObserver = function(callback) {
        this.callback = callback;
        this._observations = [];

        this.observe = function(target) {
            this._observations.push(target);
        };

        this.unobserve = function(target) {
            this._observations = this._observations.filter(function(el) {
                return el !== target;
            });
        };

        this.disconnect = function() {
            this._observations = [];
        };
    };
}

// MutationObserver 适配（空实现）
if (typeof MutationObserver === 'undefined') {
    window.MutationObserver = function(callback) {
        this.callback = callback;

        this.observe = function(target, options) {};

        this.disconnect = function() {};

        this.takeRecords = function() {
            return [];
        };
    };
}

// requestAnimationFrame/cancelAnimationFrame 适配
if (typeof requestAnimationFrame === 'undefined') {
    window.requestAnimationFrame = function(callback) {
        return setTimeout(function() {
            callback(Date.now());
        }, 1000 / 60);
    };
}

if (typeof cancelAnimationFrame === 'undefined') {
    window.cancelAnimationFrame = function(id) {
        clearTimeout(id);
    };
}

console.log('[WeChat Adapter] 微信小游戏适配层加载完成');
'''

    def _create_game_js(self, output_path):
        """创建游戏入口 JS 文件"""
        print("🎮 生成入口文件 (game.js)...")

        game_js_content = '''/**
 * 杀戮尖塔2 - 微信小游戏版本
 */

require('./weapp-adapter.js');

const canvas = wx.createCanvas();

// 设置 canvas 尺寸为全屏
const systemInfo = wx.getSystemInfoSync();
canvas.width = systemInfo.screenWidth;
canvas.height = systemInfo.screenHeight;

// 监听屏幕旋转
wx.onWindowResize && wx.onWindowResize(function(res) {
    canvas.width = res.size.windowWidth;
    canvas.height = res.size.windowHeight;
});

// 启动 Godot 引擎
function startGodot() {
    // 动态加载 Godot 引擎脚本
    const godotScripts = [
        './godot.js'
    ];

    let loadIndex = 0;

    function loadNextScript() {
        if (loadIndex >= godotScripts.length) {
            initializeEngine();
            return;
        }

        const script = document.createElement('script');
        script.src = godotScripts[loadIndex];
        script.onload = function() {
            loadIndex++;
            loadNextScript();
        };
        script.onerror = function(e) {
            console.error('加载 Godot 脚本失败:', godotScripts[loadIndex], e);
        };

        document.body.appendChild(script);
    }

    loadNextScript();
}

function initializeEngine() {
    if (typeof Godot === 'undefined') {
        console.error('Godot 引擎未加载');
        return;
    }

    console.log('初始化 Godot 引擎...');

    const engine = new Engine({
        canvas: canvas,
        onProgress: function(current, total) {
            if (total > 0) {
                const percent = Math.round((current / total) * 100);
                console.log(`加载进度: ${percent}%`);
            }
        }
    });

    engine.startGame().then(function() {
        console.log('✅ 游戏启动成功!');

        // 隐藏启动画面
        if (wx.hideLoading) {
            wx.hideLoading();
        }
    }).catch(function(error) {
        console.error('❌ 游戏启动失败:', error);
    });
}

// 显示加载提示
if (wx.showLoading) {
    wx.showLoading({
        title: '加载中...',
        mask: true
    });
}

// 启动
startGodot();
'''

        game_js_file = output_path / "game.js"
        with open(game_js_file, 'w', encoding='utf-8') as f:
            f.write(game_js_content)

    def _modify_html_for_wechat(self, input_path, output_path):
        """修改 HTML 文件以适配微信小游戏"""
        print("📄 适配 HTML 文件...")

        html_file = input_path / "index.html"
        if not html_file.exists():
            print("⚠️  未找到 index.html，跳过")
            return

        with open(html_file, 'r', encoding='utf-8') as f:
            html_content = f.read()

        # 注释掉自动执行代码，改为手动调用
        modified_html = html_content.replace(
            '<script>',
            '<!-- Godot 自动初始化已禁用 -->\n<script>'
        )

        wechat_html = output_path / "index.html"
        with open(wechat_html, 'w', encoding='utf-8') as f:
            f.write(modified_html)

    def _create_project_config(self, output_path):
        """创建微信项目配置文件"""
        print("⚙️  生成项目配置...")

        config = {
            "description": "杀戮尖塔2 - 微信小游戏",
            "packOptions": {
                "ignore": [],
                "include": []
            },
            "setting": {
                "bundle": False,
                "userConfirmedBundleSwitch": False,
                "urlCheck": True,
                "scopeDataCheck": False,
                "coverView": True,
                "es6": True,
                "postcss": True,
                "compileHotReLoad": False,
                "lazyloadPlaceholderEnable": False,
                "preloadBackgroundData": False,
                "minified": True,
                "autoAudits": False,
                "newFeature": False,
                "uglifyFileName": False,
                "uploadWithSourceMap": True,
                "useIsolateContext": True,
            },
            "compileType": "game",
            "libVersion": "2.25.0",
            "appid": "",
            "projectname": "roguelikegame2",
            "condition": {},
            "deviceOrientation": "landscape",
            "showStatusBar": False,
            "networkTimeout": {
                "request": 10000,
                "connectSocket": 10000,
                "uploadFile": 10000,
                "downloadFile": 10000
            },
            "debugOptions": {
                "hidedInDevtools": []
            },
            "isGameTourist": False
        }

        config_file = output_path / "project.config.json"
        with open(config_file, 'w', encoding='utf-8') as f:
            json.dump(config, f, indent=2, ensure_ascii=False)

    def _process_assets(self, input_path, output_path):
        """处理游戏资源文件"""
        print("📦 处理资源文件...")

        asset_dirs = [
            'textures',
            'sounds',
            'models',
            'fonts'
        ]

        for dir_name in asset_dirs:
            src_dir = input_path / dir_name
            if src_dir.exists() and src_dir.is_dir():
                dst_dir = output_path / dir_name
                shutil.copytree(src_dir, dst_dir)
                file_count = sum(1 for _ in dst_dir.rglob('*'))
                print(f"   ✓ {dir_name}/ ({file_count} 个文件)")


def main():
    parser = argparse.ArgumentParser(
        description='微信小游戏转换器 - 将 Godot Web 导出转为微信小游戏',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
示例:
  %(prog)s --input ./build/web --output ./build/wechat
  %(prog)s --input ../build/web --output ../wechat_minigame
        '''
    )

    parser.add_argument('--input', '-i', required=True, help='Godot Web 导出目录')
    parser.add_argument('--output', '-o', required=True, help='微信小游戏输出目录')
    parser.add_argument('--verbose', '-v', action='store_true', help='显示详细日志')

    args = parser.parse_args()

    try:
        converter = WeChatConverter()
        converter.convert(args.input, args.output)
        return 0
    except Exception as e:
        print(f"❌ 转换失败: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        return 1


if __name__ == "__main__":
    sys.exit(main())
