using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sidebar : MonoBehaviour
{
    public void MenuClick_Search()
    {
        StartCoroutine(WaitForSeconds_Search(0.25f));
    }
    private IEnumerator WaitForSeconds_Search(float secs)
    {
        yield return new WaitForSeconds(secs);
        SceneManager.LoadScene("Search");
    }

    public void MenuClick_About()
    {
        StartCoroutine(WaitForSeconds_About(0.25f));
    }
    private IEnumerator WaitForSeconds_About(float secs)
    {
        yield return new WaitForSeconds(secs);
        SceneManager.LoadScene("About");
    }

    public void MenuClick_Legal()
    {
        StartCoroutine(WaitForSeconds_Legal(0.25f));
    }
    private IEnumerator WaitForSeconds_Legal(float secs)
    {
        yield return new WaitForSeconds(secs);
        SceneManager.LoadScene("Legal");
    }
}