using NUnit.Framework;
using UnityEngine;
using VacuumSurvivor;

public class WeaponMotionTests
{
    static void DestroyEffect(GameObject owner)
    {
        // EditMode fixtures invoke lifecycle callbacks explicitly because these effects run in Play Mode.
        foreach (var component in owner.GetComponents<MonoBehaviour>())
            component.GetType().GetMethod("OnDestroy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(component, null);
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void Suction_has_curved_streams_and_dust_that_travels_inward()
    {
        var root = new GameObject("Suction motion test");
        Material material = null;
        try
        {
            var type = System.Type.GetType("VacuumSurvivor.SuctionFlowEffect, VacuumSurvivor");
            Assert.That(type, Is.Not.Null);
            var effect = root.AddComponent(type);
            root.transform.localScale = Vector3.one * 3f;
            var render = type.GetMethod("Render");
            render.Invoke(effect, new object[] { 1.5f, 0.25f, 1f });
            var lines = root.GetComponentsInChildren<LineRenderer>();
            Assert.That(lines.Length, Is.GreaterThanOrEqualTo(12));
            var flow = root.transform.Find("Flow0").GetComponent<LineRenderer>();
            var dust = root.transform.Find("Dust0").GetComponent<LineRenderer>();
            material = flow.sharedMaterial;
            Assert.That(flow.positionCount, Is.GreaterThanOrEqualTo(12));
            Vector3 a = flow.GetPosition(0), b = flow.GetPosition(flow.positionCount / 2), c = flow.GetPosition(flow.positionCount - 1);
            Assert.That(Vector3.Cross(b - a, c - b).sqrMagnitude, Is.GreaterThan(0.00000001f));
            float before = dust.GetPosition(1).magnitude;
            render.Invoke(effect, new object[] { 1.5f, 0.4f, 1f });
            Assert.That(dust.GetPosition(1).magnitude, Is.LessThan(before));
            foreach (var line in lines)
            {
                Assert.That(line.useWorldSpace, Is.False);
                Assert.That(line.sharedMaterial, Is.SameAs(material));
                for (int i = 0; i < line.positionCount; i++) Assert.That(line.GetPosition(i).magnitude, Is.LessThanOrEqualTo(0.501f));
            }
        }
        finally { DestroyEffect(root); }
        Assert.That(material == null, Is.True, "Effect cleanup must release its shared material.");
    }

    [Test]
    public void Horn_has_staggered_outward_arcs_and_a_short_central_flash()
    {
        var root = new GameObject("Horn motion test");
        Material material = null;
        try
        {
            var type = System.Type.GetType("VacuumSurvivor.HornPulseEffect, VacuumSurvivor");
            Assert.That(type, Is.Not.Null);
            var effect = root.AddComponent(type);
            var render = type.GetMethod("Render");
            root.transform.localScale = Vector3.one * (8f * 0.2f);
            render.Invoke(effect, new object[] { 4f, 0.2f });
            var arc = root.transform.Find("Wave0Arc0").GetComponent<LineRenderer>();
            var flash = root.transform.Find("Flash0").GetComponent<LineRenderer>();
            material = arc.sharedMaterial;
            float before = root.transform.TransformPoint(arc.GetPosition(0)).magnitude;
            Assert.That(flash.enabled, Is.True);
            Assert.That(arc.GetPosition(0), Is.Not.EqualTo(arc.GetPosition(arc.positionCount - 1)));
            root.transform.localScale = Vector3.one * (8f * 0.55f);
            render.Invoke(effect, new object[] { 4f, 0.55f });
            Assert.That(root.transform.TransformPoint(arc.GetPosition(0)).magnitude, Is.GreaterThan(before));
            Assert.That(flash.enabled, Is.False);
            int visible = 0;
            foreach (var line in root.GetComponentsInChildren<LineRenderer>())
            {
                if (!line.enabled) continue;
                visible++;
                for (int i = 0; i < line.positionCount; i++)
                    Assert.That(root.transform.TransformPoint(line.GetPosition(i)).magnitude, Is.LessThanOrEqualTo(4.01f));
            }
            Assert.That(visible, Is.GreaterThanOrEqualTo(8));
        }
        finally { DestroyEffect(root); }
        Assert.That(material == null, Is.True);
    }

    [Test]
    public void Weapons_attach_motion_effects_to_their_existing_visual_roots()
    {
        var root = new GameObject("Weapon hook test");
        try
        {
            var suctionRing = new GameObject("Suction");
            suctionRing.transform.SetParent(root.transform);
            suctionRing.AddComponent<SpriteRenderer>();
            var suction = root.AddComponent<SuctionWeapon>();
            suction.Ring = suctionRing.transform;
            typeof(SuctionWeapon).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(suction, null);
            var suctionType = System.Type.GetType("VacuumSurvivor.SuctionFlowEffect, VacuumSurvivor");
            Assert.That(suctionType, Is.Not.Null);
            Assert.That(suctionRing.GetComponent(suctionType), Is.Not.Null);
            var hornRing = new GameObject("Horn");
            hornRing.transform.SetParent(root.transform);
            hornRing.AddComponent<SpriteRenderer>();
            var horn = root.AddComponent<PushWeapon>();
            horn.Ring = hornRing.transform;
            typeof(PushWeapon).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(horn, null);
            var hornType = System.Type.GetType("VacuumSurvivor.HornPulseEffect, VacuumSurvivor");
            Assert.That(hornType, Is.Not.Null);
            Assert.That(hornRing.GetComponent(hornType), Is.Not.Null);
            Assert.That(hornRing.GetComponent<SpriteRenderer>().enabled, Is.False);
        }
        finally { Object.DestroyImmediate(root); }
    }
}
