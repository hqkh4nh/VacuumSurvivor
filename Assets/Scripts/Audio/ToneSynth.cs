using System.Collections.Generic;
using UnityEngine;

namespace VacuumSurvivor
{
    /// Pure waveform synthesis — the audio counterpart to SpriteFactory, so the game ships no
    /// imported sound files. Produces raw PCM samples; SoundFactory wraps them into AudioClips.
    public static class ToneSynth
    {
        public enum Wave { Sine, Square, Noise }

        /// One note: the chosen waveform at `freq`, shaped by a linear attack-then-decay envelope so
        /// it fades in and out instead of clicking. Samples stay in [-1, 1].
        public static float[] Tone(float freq, float seconds, int rate, Wave wave, float attack, float decay)
        {
            int n = Mathf.RoundToInt(seconds * rate);
            if (n <= 0) return new float[0];

            var buf = new float[n];
            var noise = new System.Random(unchecked(Mathf.RoundToInt(freq) * 7919 + n));
            int attackSamples = Mathf.Clamp(Mathf.RoundToInt(attack * rate), 0, n);
            int decaySamples = Mathf.Clamp(Mathf.RoundToInt(decay * rate), 0, n);

            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float raw;
                switch (wave)
                {
                    case Wave.Square: raw = Mathf.Sin(2f * Mathf.PI * freq * t) >= 0f ? 1f : -1f; break;
                    case Wave.Noise: raw = (float)(noise.NextDouble() * 2.0 - 1.0); break;
                    default: raw = Mathf.Sin(2f * Mathf.PI * freq * t); break;
                }

                float env = 1f;
                if (attackSamples > 0 && i < attackSamples) env = (float)i / attackSamples;
                int fromEnd = n - 1 - i;
                if (decaySamples > 0 && fromEnd < decaySamples) env = Mathf.Min(env, (float)fromEnd / decaySamples);

                buf[i] = Mathf.Clamp(raw * env, -1f, 1f);
            }
            return buf;
        }

        /// A short melodic run: one note per frequency, concatenated. Used to build the music loop.
        public static float[] Arp(float[] freqs, float noteSeconds, int rate, Wave wave)
        {
            if (freqs == null || freqs.Length == 0) return new float[0];
            var parts = new List<float>();
            foreach (var f in freqs)
                parts.AddRange(Tone(f, noteSeconds, rate, wave, 0.01f, noteSeconds * 0.5f));
            return parts.ToArray();
        }
    }
}
