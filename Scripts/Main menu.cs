using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mainmenu : MonoBehaviour
{
   [SerializeField]public Button _quitbtn;
   [SerializeField]public Button _playbtn;
   void Start()
   {
      _playbtn.onClick.AddListener(() =>
      {
         SceneManager.LoadScene("Level Selection");
      });
      _quitbtn.onClick.AddListener(() =>
      {
         Application.Quit();
      });
   }
}
