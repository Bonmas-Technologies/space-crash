on sssss System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUiButtons : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _todoData;

    private void Start()
    {
        if (_todoData == null)
            return;

        WebClient cl = new();

        try
        {
            var todoHtml = cl.DownloadString("https://raw.githubusercontent.com/Bonmas-Technologies/space-crash/master/todo.txt"); // simple to-do list integration

            _todoData.text = todoHtml;
        }
        catch (System.Exception)
        {
            _todoData.text = "Unable to connect for todo log";
        }
    }

    public void RunGame()
    {
        SceneManager.LoadScene(1);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
