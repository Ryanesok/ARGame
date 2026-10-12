using NUnit.Framework;
using UnityEngine;

public class GameplayLogicTests
{
    private GameObject root;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("GameplayLogicTests");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        PlayerPrefs.DeleteKey("BGMVolume");
        PlayerPrefs.DeleteKey("SFXVolume");
    }

    [Test]
    public void ForgingManager_Start_DisablesSwordAndCapturesInitialScale()
    {
        var manager = root.AddComponent<ForgingManager>();
        var visual = new GameObject("ForgingIngotVisual").transform;
        var sword = new GameObject("Sword");
        visual.SetParent(root.transform);
        sword.transform.SetParent(root.transform);
        visual.localScale = new Vector3(2f, 3f, 4f);

        SetPrivateField(manager, "forgingIngotVisual", visual);
        SetPrivateField(manager, "swordModel", sword);
        InvokePrivateMethod(manager, "Start");

        Assert.That(manager.ForgingCount, Is.EqualTo(0));
        Assert.That(manager.isSwordComplete, Is.False);
        Assert.That(sword.activeSelf, Is.False);
    }

    [Test]
    public void ForgingManager_RegisterForgeCompleted_UpdatesScalePerStage()
    {
        var manager = CreateForgingManager(new Vector3(2f, 3f, 4f));
        var visual = (Transform)GetPrivateField(manager, "forgingIngotVisual");

        manager.RegisterForgeCompleted();

        Assert.That(manager.ForgingCount, Is.EqualTo(1));
        Assert.That(visual.localScale.x, Is.EqualTo(2f * 1.08f).Within(0.0001f));
        Assert.That(visual.localScale.y, Is.EqualTo(3f).Within(0.0001f));
        Assert.That(visual.localScale.z, Is.EqualTo(4f * 0.85f).Within(0.0001f));
        Assert.That(manager.isSwordComplete, Is.False);
    }

    [Test]
    public void ForgingManager_RegisterForgeCompleted_FifthStageCompletesSword()
    {
        var manager = CreateForgingManager(Vector3.one);
        var originalIngot = new GameObject("OriginalIngot");
        var sword = (GameObject)GetPrivateField(manager, "swordModel");
        originalIngot.transform.SetParent(root.transform);
        SetPrivateField(manager, "originalIngotModel", originalIngot);

        for (var i = 0; i < manager.MaxForgingCount; i++)
        {
            manager.RegisterForgeCompleted();
        }

        Assert.That(manager.ForgingCount, Is.EqualTo(manager.MaxForgingCount));
        Assert.That(manager.isSwordComplete, Is.True);
        Assert.That(originalIngot.activeSelf, Is.False);
        Assert.That(sword.activeSelf, Is.True);

        manager.RegisterForgeCompleted();
        Assert.That(manager.ForgingCount, Is.EqualTo(manager.MaxForgingCount));
    }

    [Test]
    public void AudioSettingsData_ClampsAndPersistsVolumes()
    {
        AudioSettingsData.BGMVolume = -1f;
        AudioSettingsData.SFXVolume = 2f;

        Assert.That(AudioSettingsData.BGMVolume, Is.EqualTo(0f));
        Assert.That(AudioSettingsData.SFXVolume, Is.EqualTo(1f));
        Assert.That(PlayerPrefs.GetFloat("BGMVolume"), Is.EqualTo(0f));
        Assert.That(PlayerPrefs.GetFloat("SFXVolume"), Is.EqualTo(1f));
    }

    [Test]
    public void CombineManager_FinishHeating_SetsHotStateAndColor()
    {
        var manager = root.AddComponent<CombineManager>();
        var rendererObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        rendererObject.transform.SetParent(root.transform);
        var renderer = rendererObject.GetComponent<Renderer>();
        var hotColor = Color.yellow;

        manager.ingotRenderer = renderer;
        manager.warnaPanas = hotColor;
        manager.FinishHeating();

        Assert.That(manager.IsIngotHotForUI, Is.True);
        var materialProperties = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(materialProperties);
        Assert.That(materialProperties.GetColor("_Color"), Is.EqualTo(hotColor));
    }

    private ForgingManager CreateForgingManager(Vector3 initialScale)
    {
        var manager = root.AddComponent<ForgingManager>();
        var visual = new GameObject("ForgingIngotVisual").transform;
        var sword = new GameObject("Sword");
        var audio = root.AddComponent<AudioManager>();
        visual.SetParent(root.transform);
        sword.transform.SetParent(root.transform);
        visual.localScale = initialScale;

        SetPrivateField(manager, "forgingIngotVisual", visual);
        SetPrivateField(manager, "swordModel", sword);
        manager.audioManager = audio;
        InvokePrivateMethod(manager, "Start");
        return manager;
    }

    private static void SetPrivateField(Object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Field '{fieldName}' was not found.");
        field.SetValue(target, value);
    }

    private static object GetPrivateField(Object target, string fieldName)
    {
        var field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Field '{fieldName}' was not found.");
        return field.GetValue(target);
    }

    private static void InvokePrivateMethod(Object target, string methodName)
    {
        var method = target.GetType().GetMethod(
            methodName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, $"Method '{methodName}' was not found.");
        method.Invoke(target, null);
    }
}
