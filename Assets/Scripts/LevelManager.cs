using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void LoadScene(int index)
    {
        // Сохраняем прогресс перед загрузкой
       

        SceneManager.LoadScene(index);
    }

    // private void SaveCurrentProgress()
    // {
    //     // Можно сохранить в PlayerPrefs или файл
    //     if (PointData.Instance != null)
    //     {
    //         // Сохраняем данные точек в JSON
    //         string json = JsonUtility.ToJson(PointData.Instance.pointsData);
    //         PlayerPrefs.SetString("MapProgress", json);
    //         PlayerPrefs.SetString("LastPoint", PointData.Instance.lastActivePointName);
    //         PlayerPrefs.Save();
    //     }
    // }
}