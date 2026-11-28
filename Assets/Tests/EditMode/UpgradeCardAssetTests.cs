using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VacuumSurvivor;

public class UpgradeCardAssetTests
{
    [TestCase("CardFrame")]
    public void Card_PNG_has_an_opaque_light_center(string name)
    {
        var texture = new Texture2D(2, 2);
        try
        {
            Assert.That(ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/Art/Interface/UpgradePanel/" + name + ".png")), Is.True);
            for (int y = 5; y <= 15; y++)
            for (int x = 5; x <= 15; x++)
            {
                Color pixel = texture.GetPixel(texture.width * x / 20, texture.height * y / 20);
                Assert.That(pixel.a, Is.GreaterThanOrEqualTo(0.98f), name + " contains a transparent interior pixel");
                Assert.That(pixel.grayscale, Is.GreaterThan(0.6f), name + " contains a dark interior stain");
            }
        }
        finally { Object.DestroyImmediate(texture); }
    }

    [Test]
    public void Upgrade_cards_have_a_small_gap_and_no_concealing_background_layers()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/LevelUpPanel.prefab");
        var buttons = new SerializedObject(prefab.GetComponent<LevelUpPanel>()).FindProperty("buttons");
        RectTransform previous = null;
        for (int i = 0; i < 3; i++)
        {
            var card = (Component)buttons.GetArrayElementAtIndex(i).objectReferenceValue;
            var rect = card.GetComponent<RectTransform>();
            if (previous != null)
            {
                float gap = rect.anchoredPosition.x - previous.anchoredPosition.x - (previous.sizeDelta.x + rect.sizeDelta.x) * 0.5f;
                Assert.That(gap, Is.InRange(0f, 10f));
            }
            Assert.That(card.transform.Find("Visual/Opaque backing"), Is.Null);
            Assert.That(card.transform.Find("Visual/Card fill"), Is.Null);
            previous = rect;
        }
    }
    [Test]
    public void Hover_uses_an_authored_gold_border_around_the_same_filled_card()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/LevelUpPanel.prefab");
        var buttons = new SerializedObject(prefab.GetComponent<LevelUpPanel>()).FindProperty("buttons");
        for (int i = 0; i < 3; i++)
        {
            var card = (Component)buttons.GetArrayElementAtIndex(i).objectReferenceValue;
            var feedback = card.GetComponent<UpgradeCardFeedback>();
            var property = new SerializedObject(feedback).FindProperty("highlightBorder");
            Assert.That(property, Is.Not.Null);
            var border = property.objectReferenceValue as GameObject;
            Assert.That(border, Is.Not.Null);
            Assert.That(border.activeSelf, Is.False);
            Assert.That(border.GetComponent("Mask"), Is.Null, "A hard stencil exposes PNG alpha specks");
            var fill = border.transform.Find("Gold").GetComponent("Image");
            var material = new SerializedObject(fill).FindProperty("m_Material").objectReferenceValue as Material;
            Assert.That(material, Is.Not.Null);
            Assert.That(material.shader.name, Is.EqualTo("VacuumSurvivor/UIAlphaTint"));
            Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False);
            Color color = new SerializedObject(fill).FindProperty("m_Color").colorValue;
            Assert.That(color.r, Is.GreaterThanOrEqualTo(0.95f));
            Assert.That(color.g, Is.GreaterThanOrEqualTo(0.7f));
            Assert.That(color.b, Is.LessThanOrEqualTo(0.25f));
        }
    }

}
