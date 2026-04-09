#!/usr/bin/env python3
"""
音频资源生成器 - 使用Python生成游戏音效和背景音乐
"""

import wave
import struct
import math
import os
import random

class AudioGenerator:
    def __init__(self, output_dir):
        self.output_dir = output_dir
        self.sample_rate = 44100
        os.makedirs(output_dir, exist_ok=True)

    def generate_sine_wave(self, frequency, duration, amplitude=0.5):
        """生成正弦波"""
        num_samples = int(self.sample_rate * duration)
        samples = []
        for i in range(num_samples):
            t = i / self.sample_rate
            envelope = math.exp(-t * 5)  # 衰减包络
            sample = amplitude * envelope * math.sin(2 * math.pi * frequency * t)
            samples.append(sample)
        return samples

    def generate_square_wave(self, frequency, duration, amplitude=0.3):
        """生成方波"""
        num_samples = int(self.sample_rate * duration)
        samples = []
        for i in range(num_samples):
            t = i / self.sample_rate
            envelope = math.exp(-t * 5)
            sample = amplitude * envelope * (1 if math.sin(2 * math.pi * frequency * t) > 0 else -1)
            samples.append(sample)
        return samples

    def generate_sawtooth_wave(self, frequency, duration, amplitude=0.3):
        """生成锯齿波"""
        num_samples = int(self.sample_rate * duration)
        samples = []
        for i in range(num_samples):
            t = i / self.sample_rate
            envelope = math.exp(-t * 5)
            period = 1.0 / frequency
            sample = amplitude * envelope * (2 * (t % period) / period - 1)
            samples.append(sample)
        return samples

    def generate_noise(self, duration, amplitude=0.3):
        """生成白噪声"""
        num_samples = int(self.sample_rate * duration)
        samples = []
        for i in range(num_samples):
            t = i / self.sample_rate
            envelope = math.exp(-t * 3)
            sample = amplitude * envelope * (random.random() * 2 - 1)
            samples.append(sample)
        return samples

    def generate_chord(self, frequencies, duration, amplitude=0.2):
        """生成和弦"""
        num_samples = int(self.sample_rate * duration)
        samples = []
        for i in range(num_samples):
            t = i / self.sample_rate
            envelope = math.exp(-t * 2)
            sample = 0
            for freq in frequencies:
                sample += amplitude * envelope * math.sin(2 * math.pi * freq * t)
            samples.append(sample)
        return samples

    def save_wav(self, samples, filename):
        """保存为WAV文件"""
        filepath = os.path.join(self.output_dir, filename)
        with wave.open(filepath, 'w') as wav_file:
            wav_file.setnchannels(1)  # 单声道
            wav_file.setsampwidth(2)  # 16位
            wav_file.setframerate(self.sample_rate)

            # 归一化并转换为16位整数
            max_val = max(abs(s) for s in samples) if samples else 1
            normalized = [int(32767 * s / max_val) for s in samples]

            for sample in normalized:
                wav_file.writeframes(struct.pack('<h', sample))

        print(f"已生成: {filepath}")

    def generate_all_sfx(self):
        """生成所有音效"""
        print("=== 生成音效资源 ===")

        sfx_configs = {
            'card_play': ('sine', 800, 0.1),
            'card_draw': ('sine', 600, 0.15),
            'attack': ('sawtooth', 200, 0.2),
            'block': ('square', 400, 0.15),
            'damage': ('noise', 0, 0.3),
            'enemy_hit': ('sawtooth', 300, 0.15),
            'enemy_death': ('noise', 0, 0.5),
            'potion_use': ('sine', 700, 0.3),
            'relic_activate': ('sine', 900, 0.4),
            'gold_pickup': ('sine', 1000, 0.1),
            'button_click': ('sine', 500, 0.05),
            'shop_buy': ('sine', 800, 0.2)
        }

        for name, (wave_type, freq, duration) in sfx_configs.items():
            if wave_type == 'sine':
                samples = self.generate_sine_wave(freq, duration)
            elif wave_type == 'square':
                samples = self.generate_square_wave(freq, duration)
            elif wave_type == 'sawtooth':
                samples = self.generate_sawtooth_wave(freq, duration)
            elif wave_type == 'noise':
                samples = self.generate_noise(duration)
            else:
                samples = self.generate_sine_wave(freq, duration)

            self.save_wav(samples, f'{name}.wav')

    def generate_all_bgm(self):
        """生成所有背景音乐"""
        print("=== 生成背景音乐资源 ===")

        # 定义和弦进行
        chord_progressions = {
            'main_menu': [
                [261.63, 329.63, 392.00],  # C major
                [293.66, 349.23, 440.00],  # D minor
                [329.63, 392.00, 493.88],  # E minor
                [349.23, 440.00, 523.25]   # F major
            ],
            'combat_normal': [
                [293.66, 349.23, 440.00],  # D minor
                [261.63, 329.63, 392.00],  # C major
                [293.66, 349.23, 440.00],  # D minor
                [329.63, 392.00, 493.88]   # E minor
            ],
            'combat_elite': [
                [220.00, 277.18, 329.63],  # A minor
                [196.00, 246.94, 293.66],  # G major
                [220.00, 277.18, 329.63],  # A minor
                [174.61, 220.00, 261.63]   # F major
            ],
            'combat_boss': [
                [164.81, 207.65, 246.94],  # E minor (low)
                [146.83, 185.00, 220.00],  # D minor (low)
                [164.81, 207.65, 246.94],  # E minor (low)
                [130.81, 164.81, 196.00]   # C major (low)
            ],
            'shop': [
                [349.23, 440.00, 523.25],  # F major
                [392.00, 493.88, 587.33],  # G major
                [440.00, 523.25, 659.25],  # A minor
                [349.23, 440.00, 523.25]   # F major
            ],
            'rest': [
                [392.00, 493.88, 587.33],  # G major
                [440.00, 523.25, 659.25],  # A minor
                [392.00, 493.88, 587.33],  # G major
                [329.63, 392.00, 493.88]   # E minor
            ],
            'map': [
                [261.63, 329.63, 392.00],  # C major
                [293.66, 349.23, 440.00],  # D minor
                [349.23, 440.00, 523.25],  # F major
                [392.00, 493.88, 587.33]   # G major
            ],
            'victory': [
                [261.63, 329.63, 392.00],  # C major
                [349.23, 440.00, 523.25],  # F major
                [392.00, 493.88, 587.33],  # G major
                [261.63, 329.63, 392.00]   # C major
            ],
            'game_over': [
                [220.00, 277.18, 329.63],  # A minor
                [196.00, 246.94, 293.66],  # G major
                [174.61, 220.00, 261.63],  # F major
                [164.81, 207.65, 246.94]   # E minor
            ]
        }

        for name, chords in chord_progressions.items():
            samples = []
            chord_duration = 2.0  # 每个和弦持续2秒

            # 生成4个小节的音乐（循环4次）
            for _ in range(4):
                for chord in chords:
                    chord_samples = self.generate_chord(chord, chord_duration)
                    samples.extend(chord_samples)

            self.save_wav(samples, f'{name}.wav')

def main():
    print("========================================")
    print("   音频资源生成器 - 杀戮尖塔2")
    print("========================================")

    base_dir = os.path.dirname(os.path.abspath(__file__))

    # 生成音效
    sfx_dir = os.path.join(base_dir, 'Audio', 'SFX')
    sfx_gen = AudioGenerator(sfx_dir)
    sfx_gen.generate_all_sfx()

    # 生成背景音乐
    bgm_dir = os.path.join(base_dir, 'Audio', 'BGM')
    bgm_gen = AudioGenerator(bgm_dir)
    bgm_gen.generate_all_bgm()

    print("========================================")
    print("   所有音频资源生成完成！")
    print("========================================")

if __name__ == '__main__':
    main()
