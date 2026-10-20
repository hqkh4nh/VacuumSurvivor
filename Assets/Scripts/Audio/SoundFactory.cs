using UnityEngine;

namespace VacuumSurvivor
{
    /// Bundles synthesized waveforms into AudioClips — SpriteFactory's counterpart for sound, so the
    /// project ships no imported audio. SFX are short tones; the music is a looping arpeggio.
    public static class SoundFactory
    {
        public const int Rate = 44100;

        public static AudioClip Clip(string name, float[] samples, int rate)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip Shoot() =>
            Clip("sfx_shoot", ToneSynth.Tone(640f, 0.09f, Rate, ToneSynth.Wave.Square, 0.005f, 0.07f), Rate);
        public static AudioClip Pickup() =>
            Clip("sfx_pickup", ToneSynth.Tone(1040f, 0.06f, Rate, ToneSynth.Wave.Sine, 0.002f, 0.05f), Rate);
        public static AudioClip Hit() =>
            Clip("sfx_hit", ToneSynth.Tone(150f, 0.12f, Rate, ToneSynth.Wave.Noise, 0.002f, 0.11f), Rate);
        public static AudioClip LevelUp() =>
            Clip("sfx_levelup", ToneSynth.Arp(new[] { 523f, 659f, 784f }, 0.08f, Rate, ToneSynth.Wave.Square), Rate);
        public static AudioClip Boss() =>
            Clip("sfx_boss", ToneSynth.Arp(new[] { 196f, 165f, 131f }, 0.16f, Rate, ToneSynth.Wave.Square), Rate);
        public static AudioClip Win() =>
            Clip("sfx_win", ToneSynth.Arp(new[] { 523f, 659f, 784f, 1047f }, 0.12f, Rate, ToneSynth.Wave.Square), Rate);
        public static AudioClip Lose() =>
            Clip("sfx_lose", ToneSynth.Arp(new[] { 330f, 247f, 196f }, 0.18f, Rate, ToneSynth.Wave.Square), Rate);
        public static AudioClip PowerUp() =>
            Clip("sfx_powerup", ToneSynth.Arp(new[] { 784f, 1047f }, 0.07f, Rate, ToneSynth.Wave.Sine), Rate);

        public static AudioClip MusicLoop()
        {
            float[] notes = { 220f, 262f, 330f, 262f, 294f, 349f, 294f, 220f };
            return Clip("music_loop", ToneSynth.Arp(notes, 0.28f, Rate, ToneSynth.Wave.Sine), Rate);
        }
    }
}
