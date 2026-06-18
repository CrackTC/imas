using System.Collections;
using Imas;
using UnityEngine;

public class Demo : MonoBehaviour
{
    private IEnumerator Init()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        // Screen.SetResolution(Screen.width * 720 / Screen.height, 720, true);
        Shader.SetGlobalColor("_FadeColV", Color.white);
        yield return ScenarioManager.Instance.LoadScenario("main_befo_001");
        ScenarioManager.Instance.Play();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PoseManager.Create();
        PoseManager.Setup();

        StartCoroutine(Init());
    }
}
