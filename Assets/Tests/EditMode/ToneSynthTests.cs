using NUnit.Framework;
using UnityEngine;
using VacuumSurvivor;

public class ToneSynthTests
{
    const int Rate = 44100;

    static float AvgAbs(float[] s, float from01, float to01)
    {
        int a = Mathf.RoundToInt(s.Length * from01), b = Mathf.RoundToInt(s.Length * to01);
        float sum = 0f;
        for (int i = a; i < b; i++) sum += Mathf.Abs(s[i]);
        return sum / Mathf.Max(1, b - a);
    }

    [Test]
    public void Tone_length_equals_round_seconds_times_rate()
    {
        var t = ToneSynth.Tone(440f, 0.1f, Rate, ToneSynth.Wave.Sine, 0.01f, 0.05f);
        Assert.AreEqual(Mathf.RoundToInt(0.1f * Rate), t.Length);
    }

    [Test]
    public void Tone_samples_stay_within_minus_one_and_one()
    {
        var t = ToneSynth.Tone(440f, 0.1f, Rate, ToneSynth.Wave.Square, 0.01f, 0.05f);
        foreach (var s in t) Assert.LessOrEqual(Mathf.Abs(s), 1f);
    }

    [Test]
    public void Tone_is_not_silent()
    {
        var t = ToneSynth.Tone(440f, 0.1f, Rate, ToneSynth.Wave.Sine, 0.01f, 0.05f);
        bool any = false;
        foreach (var s in t) if (Mathf.Abs(s) > 0.01f) { any = true; break; }
        Assert.IsTrue(any);
    }

    [Test]
    public void Tone_envelope_decays_a_late_sample_is_quieter_than_a_mid_sample()
    {
        var t = ToneSynth.Tone(200f, 0.2f, Rate, ToneSynth.Wave.Square, 0f, 0.1f);
        Assert.Less(AvgAbs(t, 0.92f, 0.99f), AvgAbs(t, 0.45f, 0.55f));
    }

    [Test]
    public void Tone_with_zero_seconds_returns_an_empty_array()
    {
        Assert.AreEqual(0, ToneSynth.Tone(440f, 0f, Rate, ToneSynth.Wave.Sine, 0f, 0f).Length);
    }

    [Test]
    public void Arp_concatenates_one_note_per_frequency()
    {
        float note = 0.05f;
        var a = ToneSynth.Arp(new[] { 200f, 300f, 400f }, note, Rate, ToneSynth.Wave.Sine);
        Assert.AreEqual(3 * Mathf.RoundToInt(note * Rate), a.Length);
    }
}
