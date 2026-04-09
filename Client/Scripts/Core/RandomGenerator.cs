using Godot;
using System;
using System.Collections.Generic;

namespace RoguelikeGame.Core
{
    public class RandomGenerator
    {
        private RandomNumberGenerator _rng;

        public uint Seed { get; private set; }

        public RandomGenerator(uint seed)
        {
            Seed = seed;
            _rng = new RandomNumberGenerator();
            _rng.Seed = seed;
        }

        public void SetSeed(uint seed)
        {
            Seed = seed;
            _rng.Seed = seed;
        }

        public int Next()
        {
            return (int)_rng.Randi();
        }

        public int Next(int max)
        {
            return (int)(_rng.Randi() % max);
        }

        public int Next(int min, int max)
        {
            return min + (int)(_rng.Randi() % (max - min));
        }

        public float NextFloat()
        {
            return _rng.Randf();
        }

        public float NextFloat(float min, float max)
        {
            return _rng.RandfRange(min, max);
        }

        public bool NextBool(float probability = 0.5f)
        {
            return _rng.Randf() < probability;
        }

        public T Choose<T>(params T[] items)
        {
            if (items == null || items.Length == 0)
                return default;
            
            return items[Next(items.Length)];
        }

        public T Choose<T>(IList<T> items)
        {
            if (items == null || items.Count == 0)
                return default;
            
            return items[Next(items.Count)];
        }

        public void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public List<T> GetShuffled<T>(IList<T> list)
        {
            var result = new List<T>(list);
            Shuffle(result);
            return result;
        }

        public float NextGaussian(float mean = 0f, float stdDev = 1f)
        {
            float u1 = NextFloat();
            float u2 = NextFloat();
            float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.Pi * u2);
            return mean + stdDev * randStdNormal;
        }

        public Vector2I NextVector2I(int minX, int maxX, int minY, int maxY)
        {
            return new Vector2I(Next(minX, maxX), Next(minY, maxY));
        }

        public Vector2 NextVector2(float minX, float maxX, float minY, float maxY)
        {
            return new Vector2(NextFloat(minX, maxX), NextFloat(minY, maxY));
        }
    }
}
