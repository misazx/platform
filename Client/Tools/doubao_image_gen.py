#!/usr/bin/env python3
"""
豆包生图工具 - 基于火山引擎方舟 Seedream API 批量生成游戏美术资源

使用方式:
  python3 doubao_image_gen.py doc.yaml                    # 从YAML文档批量生成
  python3 doubao_image_gen.py doc.yaml --dry-run          # 仅预览不生成
  python3 doubao_image_gen.py doc.yaml --concurrency 10   # 指定并行数
  python3 doubao_image_gen.py doc.yaml --batch 0          # 只生成第0个批次
  python3 doubao_image_gen.py --single "prompt" -o out.png -s 64x64  # 单张生成

YAML文档格式:
  defaults:
    model: "doubao-seedream-5-0-260128"
    gen_size: "2K"
    output_format: "png"
    watermark: false
    concurrency: 5
    retry: 3
    style_prefix: ""
    style_suffix: ""

  batches:
    - name: "批次名称"
      size: "64x64"
      style: "风格描述前缀"
      output_dir: "相对输出目录"
      gen_size: "2K"
      resources:
        - filename: "xxx.png"
          prompt: "具体提示词"
          negative: "负面提示词"
"""

import json
import os
import sys
import ssl
import base64
import time
import argparse
import urllib.request
import urllib.error
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor, as_completed
from datetime import datetime

try:
    import yaml
    HAS_YAML = True
except ImportError:
    HAS_YAML = False

try:
    from PIL import Image
    import io
    HAS_PIL = True
except ImportError:
    HAS_PIL = False

PROJECT_ROOT = Path(__file__).parent.parent.parent
CLIENT_DIR = Path(__file__).parent.parent

API_URL = "https://ark.cn-beijing.volces.com/api/v3/images/generations"
DEFAULT_API_KEY = "dae84b47-92fc-4afa-bdcc-4e8bf99b486b"
DEFAULT_MODEL = "doubao-seedream-5-0-260128"

FALLBACK_MODELS = [
    "doubao-seedream-5-0-260128",
    "doubao-seedream-5-0-lite-260128",
    "doubao-seedream-4-5-251128",
    "doubao-seedream-4-0-250828",
    "doubao-seedream-3.0-t2i",
]

GEN_SIZE_MAP = {
    "1K": "1024x1024",
    "2K": "2048x2048",
    "3K": "3072x3072",
    "4K": "4096x4096",
}

MODEL_MIN_PIXELS = {
    "doubao-seedream-5-0-260128": 3686400,
    "doubao-seedream-5-0-lite-260128": 3686400,
    "doubao-seedream-4-5-251128": 3686400,
    "doubao-seedream-4-0-250828": 3686400,
    "doubao-seedream-3.0-t2i": 921600,
}

MODEL_COOLDOWN_SECONDS = 120

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE


class GenStats:
    def __init__(self):
        self.total = 0
        self.success = 0
        self.fail = 0
        self.skip = 0
        self.start_time = time.time()

    def elapsed(self):
        return time.time() - self.start_time

    def summary(self):
        return (
            f"\n{'='*60}\n"
            f"生成完成 | 耗时: {self.elapsed():.1f}s\n"
            f"  总计: {self.total}  成功: {self.success}  "
            f"失败: {self.fail}  跳过: {self.skip}\n"
            f"{'='*60}"
        )


class ModelRotator:
    def __init__(self, primary_model=None, fallback_models=None):
        self.models = [primary_model or DEFAULT_MODEL]
        for m in (fallback_models or FALLBACK_MODELS):
            if m not in self.models:
                self.models.append(m)
        self.cooldowns = {}
        self.fail_counts = {}
        self._current_index = 0

    def get_next_model(self):
        now = time.time()
        for _ in range(len(self.models)):
            model = self.models[self._current_index % len(self.models)]
            cd_until = self.cooldowns.get(model, 0)
            if now >= cd_until:
                return model
            self._current_index += 1
        best = self.models[0]
        earliest = self.cooldowns.get(best, 0)
        for m in self.models[1:]:
            t = self.cooldowns.get(m, 0)
            if t < earliest:
                earliest = t
                best = m
        wait = max(0, earliest - now)
        if wait > 0:
            print(f"    [ROTATE] 所有模型冷却中，等待 {wait:.0f}s (最早恢复: {best})")
            time.sleep(wait)
        return best

    def mark_rate_limited(self, model):
        self.cooldowns[model] = time.time() + MODEL_COOLDOWN_SECONDS
        self.fail_counts[model] = self.fail_counts.get(model, 0) + 1
        self._current_index += 1
        next_model = self.get_next_model()
        print(f"    [ROTATE] {model} 额度耗尽(429)，切换到 {next_model}")
        return next_model

    def mark_success(self, model):
        self.fail_counts.pop(model, None)
        self.cooldowns.pop(model, None)

    def status_summary(self):
        now = time.time()
        lines = []
        for m in self.models:
            cd = self.cooldowns.get(m, 0)
            fails = self.fail_counts.get(m, 0)
            if cd > now:
                remaining = cd - now
                lines.append(f"  {m}: 冷却中({remaining:.0f}s) 失败{fails}次")
            else:
                lines.append(f"  {m}: 可用 失败{fails}次")
        return "\n".join(lines)


def resolve_api_key(config_key=""):
    if config_key:
        return config_key
    env_key = os.environ.get("ARK_API_KEY", "")
    if env_key:
        return env_key
    return DEFAULT_API_KEY


def resolve_gen_size(gen_size, target_size, model):
    if gen_size:
        if gen_size in GEN_SIZE_MAP:
            return GEN_SIZE_MAP[gen_size]
        return gen_size

    tw, th = map(int, target_size.split("x"))
    min_pixels = MODEL_MIN_PIXELS.get(model, 3686400)
    target_pixels = tw * th
    if target_pixels >= min_pixels:
        return f"{tw}x{th}"

    scale = (min_pixels / target_pixels) ** 0.5
    gw = int(tw * scale)
    gh = int(th * scale)
    gw = max(gw, 512)
    gh = max(gh, 512)
    while gw * gh < min_pixels:
        gw = int(gw * 1.1)
        gh = int(gh * 1.1)
    return f"{gw}x{gh}"


def call_seedream_api(api_key, model, prompt, gen_size, output_format="png",
                      response_format="url", watermark=False, negative="",
                      retry=1):
    payload_dict = {
        "model": model,
        "prompt": prompt,
        "size": gen_size,
        "output_format": output_format,
        "response_format": response_format,
        "watermark": watermark,
    }
    if negative:
        payload_dict["negative_prompt"] = negative

    payload = json.dumps(payload_dict).encode("utf-8")

    last_error = None
    for attempt in range(retry):
        try:
            req = urllib.request.Request(
                API_URL,
                data=payload,
                headers={
                    "Content-Type": "application/json",
                    "Authorization": f"Bearer {api_key}",
                },
            )
            with urllib.request.urlopen(req, timeout=120, context=ctx) as resp:
                result = json.loads(resp.read().decode("utf-8"))

            if "data" in result and len(result["data"]) > 0:
                item = result["data"][0]
                if response_format == "b64_json" and "b64_json" in item:
                    return {"type": "base64", "data": item["b64_json"]}
                elif "url" in item:
                    return {"type": "url", "data": item["url"]}

            error_msg = result.get("error", {}).get("message", "未知错误")
            last_error = f"API返回异常: {error_msg}"
        except urllib.error.HTTPError as e:
            body = ""
            try:
                body = e.read().decode("utf-8")
            except Exception:
                pass
            last_error = f"HTTP {e.code}: {body[:200]}"
            if e.code == 429:
                return {"type": "rate_limited", "data": last_error}
            if attempt < retry - 1:
                time.sleep(2 ** attempt)
        except urllib.error.URLError as e:
            last_error = f"网络错误: {e.reason}"
            if attempt < retry - 1:
                time.sleep(2 ** attempt)
        except Exception as e:
            last_error = f"未知错误: {e}"
            if attempt < retry - 1:
                time.sleep(2 ** attempt)

    return {"type": "error", "data": last_error or "所有重试均失败"}


def download_image(url, timeout=60):
    req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
    with urllib.request.urlopen(req, timeout=timeout, context=ctx) as resp:
        return resp.read()


def resize_image(image_data, target_w, target_h, output_format="png"):
    if not HAS_PIL:
        return image_data

    img = Image.open(io.BytesIO(image_data))
    if img.mode == "RGBA" and output_format.lower() == "jpeg":
        img = img.convert("RGB")
    elif output_format.lower() == "png" and img.mode != "RGBA":
        img = img.convert("RGBA")

    img = img.resize((target_w, target_h), Image.LANCZOS)

    buf = io.BytesIO()
    fmt = "PNG" if output_format.lower() == "png" else "JPEG"
    save_kwargs = {"format": fmt}
    if fmt == "PNG":
        save_kwargs["optimize"] = True
    elif fmt == "JPEG":
        save_kwargs["quality"] = 95
    img.save(buf, **save_kwargs)
    return buf.getvalue()


def generate_single_resource(api_key, model_rotator, gen_size, output_format, watermark,
                             retry, resource, batch_style, global_style_prefix,
                             global_style_suffix, output_dir, target_size, stats):
    filename = resource.get("filename", "")
    prompt = resource.get("prompt", "")
    negative = resource.get("negative", "")

    if not filename or not prompt:
        print(f"  [SKIP] 缺少filename或prompt")
        stats.skip += 1
        return False

    full_prompt = f"{global_style_prefix}{batch_style}{prompt}{global_style_suffix}"

    tw, th = map(int, target_size.split("x"))

    max_model_attempts = len(model_rotator.models)
    tried_models = set()

    for model_attempt in range(max_model_attempts):
        model = model_rotator.get_next_model()
        resolved_size = resolve_gen_size(gen_size, target_size, model)

        model_tag = f"[{model.split('-')[-1]}]" if len(model.split('-')) > 3 else ""
        print(f"  [GEN] {filename} | {target_size} (API: {resolved_size}) {model_tag} | {full_prompt[:60]}...")

        result = call_seedream_api(
            api_key=api_key,
            model=model,
            prompt=full_prompt,
            gen_size=resolved_size,
            output_format=output_format,
            response_format="url",
            watermark=watermark,
            negative=negative,
            retry=1,
        )

        if result.get("type") == "rate_limited":
            tried_models.add(model)
            model_rotator.mark_rate_limited(model)
            if len(tried_models) >= max_model_attempts:
                print(f"  [FAIL] {filename}: 所有模型额度耗尽(429)")
                stats.fail += 1
                return False
            continue

        if result.get("type") == "error":
            print(f"  [FAIL] {filename}: {result['data']}")
            stats.fail += 1
            return False

        model_rotator.mark_success(model)
        break
    else:
        stats.fail += 1
        return False

    try:
        if result["type"] == "url":
            image_data = download_image(result["data"])
        else:
            image_data = base64.b64decode(result["data"])

        if tw != 0 and th != 0:
            image_data = resize_image(image_data, tw, th, output_format)

        out_path = Path(output_dir) / filename
        out_path.parent.mkdir(parents=True, exist_ok=True)
        with open(str(out_path), "wb") as f:
            f.write(image_data)

        file_size_kb = len(image_data) / 1024
        print(f"  [OK] {filename} ({file_size_kb:.1f}KB)")
        stats.success += 1
        return True

    except Exception as e:
        print(f"  [FAIL] {filename} 保存失败: {e}")
        stats.fail += 1
        return False


def load_document(doc_path):
    if not HAS_YAML:
        with open(doc_path, "r", encoding="utf-8") as f:
            content = f.read()
        try:
            import re
            json_match = re.search(r'```json\s*(.*?)\s*```', content, re.DOTALL)
            if json_match:
                return json.loads(json_match.group(1))
            return json.loads(content)
        except json.JSONDecodeError:
            print("[ERROR] 需要安装PyYAML: pip install pyyaml")
            sys.exit(1)

    with open(doc_path, "r", encoding="utf-8") as f:
        return yaml.safe_load(f)


def validate_document(doc):
    if not isinstance(doc, dict):
        print("[ERROR] 文档格式错误: 根节点必须是字典")
        return False

    batches = doc.get("batches", [])
    if not batches:
        print("[ERROR] 文档中没有 batches 定义")
        return False

    for i, batch in enumerate(batches):
        if "resources" not in batch or not batch["resources"]:
            print(f"[ERROR] 批次 {i} ({batch.get('name', '未命名')}) 没有 resources")
            return False
        if "output_dir" not in batch:
            print(f"[ERROR] 批次 {i} ({batch.get('name', '未命名')}) 缺少 output_dir")
            return False
        if "size" not in batch:
            print(f"[ERROR] 批次 {i} ({batch.get('name', '未命名')}) 缺少 size")
            return False

    return True


def preview_document(doc):
    defaults = doc.get("defaults", {})
    batches = doc.get("batches", [])

    print(f"\n{'='*60}")
    print(f"豆包生图 - 文档预览")
    print(f"{'='*60}")
    print(f"全局配置:")
    print(f"  模型: {defaults.get('model', DEFAULT_MODEL)}")
    print(f"  生成尺寸: {defaults.get('gen_size', '自动')}")
    print(f"  输出格式: {defaults.get('output_format', 'png')}")
    print(f"  水印: {defaults.get('watermark', False)}")
    print(f"  并行数: {defaults.get('concurrency', 5)}")
    print(f"  重试: {defaults.get('retry', 3)}")

    total_resources = 0
    for i, batch in enumerate(batches):
        resources = batch.get("resources", [])
        total_resources += len(resources)
        print(f"\n  批次 {i}: {batch.get('name', '未命名')}")
        print(f"    输出尺寸: {batch.get('size', 'N/A')}")
        print(f"    风格前缀: {batch.get('style', '')}")
        print(f"    输出目录: {batch.get('output_dir', 'N/A')}")
        print(f"    资源数量: {len(resources)}")
        for res in resources[:3]:
            print(f"      - {res.get('filename', '?')}: {res.get('prompt', '')[:50]}...")
        if len(resources) > 3:
            print(f"      ... 还有 {len(resources) - 3} 个资源")

    print(f"\n  总计: {len(batches)} 个批次, {total_resources} 个资源")
    print(f"{'='*60}")


def run_batch(api_key, defaults, batch, batch_index, stats):
    name = batch.get("name", f"批次{batch_index}")
    target_size = batch.get("size", "64x64")
    style = batch.get("style", "")
    output_dir = batch.get("output_dir", "")
    resources = batch.get("resources", [])

    model = batch.get("model", defaults.get("model", DEFAULT_MODEL))
    gen_size = batch.get("gen_size", defaults.get("gen_size", ""))
    output_format = batch.get("output_format", defaults.get("output_format", "png"))
    watermark = batch.get("watermark", defaults.get("watermark", False))
    retry = batch.get("retry", defaults.get("retry", 3))
    concurrency = batch.get("concurrency", defaults.get("concurrency", 5))
    global_style_prefix = defaults.get("style_prefix", "")
    global_style_suffix = defaults.get("style_suffix", "")

    if not output_dir.startswith("/"):
        output_dir = str(PROJECT_ROOT / output_dir)

    print(f"\n{'─'*50}")
    print(f"批次 {batch_index}: {name} | {target_size} | {len(resources)}个资源")
    print(f"输出: {output_dir}")
    print(f"{'─'*50}")

    stats.total += len(resources)

    with ThreadPoolExecutor(max_workers=concurrency) as executor:
        futures = {}
        for res in resources:
            future = executor.submit(
                generate_single_resource,
                api_key, model, gen_size, output_format, watermark, retry,
                res, style, global_style_prefix, global_style_suffix,
                output_dir, target_size, stats,
            )
            futures[future] = res.get("filename", "?")

        for future in as_completed(futures):
            fname = futures[future]
            try:
                future.result()
            except Exception as e:
                print(f"  [FAIL] {fname}: {e}")
                stats.fail += 1


def run_from_document(doc_path, dry_run=False, batch_filter=None):
    doc = load_document(doc_path)
    if not validate_document(doc):
        sys.exit(1)

    preview_document(doc)

    if dry_run:
        print("\n[DRY-RUN] 预览模式，不执行生成")
        return

    defaults = doc.get("defaults", {})
    api_key = resolve_api_key(defaults.get("api_key", ""))
    batches = doc.get("batches", [])

    stats = GenStats()

    for i, batch in enumerate(batches):
        if batch_filter is not None and i != batch_filter:
            continue
        run_batch(api_key, defaults, batch, i, stats)

    print(stats.summary())

    report_path = PROJECT_ROOT / "Client" / "Assets_Library" / "image_gen_report.json"
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report = {
        "timestamp": datetime.now().isoformat(),
        "document": str(doc_path),
        "stats": {
            "total": stats.total,
            "success": stats.success,
            "fail": stats.fail,
            "skip": stats.skip,
            "elapsed_seconds": round(stats.elapsed(), 1),
        },
    }
    with open(str(report_path), "w", encoding="utf-8") as f:
        json.dump(report, f, ensure_ascii=False, indent=2)
    print(f"报告已保存: {report_path}")


def run_single(prompt, output, size, model, gen_size, output_format, negative):
    api_key = resolve_api_key()
    stats = GenStats()
    stats.total = 1

    resolved_size = resolve_gen_size(gen_size, size, model)

    print(f"单张生成: {output} | {size} (API: {resolved_size})")
    print(f"提示词: {prompt}")

    result = call_seedream_api(
        api_key=api_key,
        model=model,
        prompt=prompt,
        gen_size=resolved_size,
        output_format=output_format,
        response_format="url",
        watermark=False,
        negative=negative,
        retry=3,
    )

    if result.get("type") == "error":
        print(f"[FAIL] {result['data']}")
        sys.exit(1)

    try:
        if result["type"] == "url":
            image_data = download_image(result["data"])
        else:
            image_data = base64.b64decode(result["data"])

        tw, th = map(int, size.split("x"))
        image_data = resize_image(image_data, tw, th, output_format)

        out_path = Path(output)
        out_path.parent.mkdir(parents=True, exist_ok=True)
        with open(str(out_path), "wb") as f:
            f.write(image_data)

        file_size_kb = len(image_data) / 1024
        print(f"[OK] {output} ({file_size_kb:.1f}KB)")
        stats.success += 1
    except Exception as e:
        print(f"[FAIL] 保存失败: {e}")
        stats.fail += 1
        sys.exit(1)


def main():
    parser = argparse.ArgumentParser(
        description="豆包生图工具 - 基于火山引擎方舟 Seedream API 批量生成游戏美术资源",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument("document", nargs="?", help="YAML/JSON 文档路径")
    parser.add_argument("--dry-run", action="store_true", help="仅预览不生成")
    parser.add_argument("--batch", type=int, help="只生成指定批次(索引从0开始)")
    parser.add_argument("--concurrency", type=int, default=None, help="并行数")
    parser.add_argument("--model", default=DEFAULT_MODEL, help="模型ID")
    parser.add_argument("--api-key", default="", help="API Key(优先级: 参数>环境变量>默认)")

    parser.add_argument("--single", type=str, help="单张模式: 直接指定提示词")
    parser.add_argument("-o", "--output", type=str, help="单张模式输出路径")
    parser.add_argument("-s", "--size", type=str, default="512x512", help="单张模式输出尺寸")
    parser.add_argument("--gen-size", type=str, default="", help="API生成尺寸(如2K/2048x2048)")
    parser.add_argument("--format", type=str, default="png", help="输出格式(png/jpeg)")
    parser.add_argument("--negative", type=str, default="", help="负面提示词")

    args = parser.parse_args()

    if args.single:
        if not args.output:
            print("[ERROR] 单张模式需要指定 -o 输出路径")
            sys.exit(1)
        run_single(
            prompt=args.single,
            output=args.output,
            size=args.size,
            model=args.model,
            gen_size=args.gen_size,
            output_format=args.format,
            negative=args.negative,
        )
        return

    if not args.document:
        parser.print_help()
        sys.exit(1)

    doc_path = Path(args.document)
    if not doc_path.exists():
        print(f"[ERROR] 文档不存在: {doc_path}")
        sys.exit(1)

    run_from_document(
        doc_path=str(doc_path),
        dry_run=args.dry_run,
        batch_filter=args.batch,
    )


if __name__ == "__main__":
    main()
