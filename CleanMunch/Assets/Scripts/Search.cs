using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Models;
using Proyecto26;
using System.Collections;
using DeadMosquito.AndroidGoodies;
using System;

public class Search : MonoBehaviour
{

    [SerializeField]
    private Text FirstResult;
    [SerializeField]
    private GameObject FirstPanel;
    [SerializeField]
    private Text SecondResult;
    [SerializeField]
    private GameObject SecondPanel;
    [SerializeField]
    private Text ThirdResult;
    [SerializeField]
    private GameObject ThirdPanel;
    [SerializeField]
    private Text FourthResult;
    [SerializeField]
    private GameObject FourthPanel;
    [SerializeField]
    private Text FifthResult;
    [SerializeField]
    private GameObject FifthPanel;
    [SerializeField]
    private InputField inputbox;
    [SerializeField]
    private Text PageText;
    [SerializeField]
    private GameObject OutputText;
    [SerializeField]
    private Text OutputTextResult;
    [SerializeField]
    private GameObject Loading;
    [SerializeField]
    private GameObject NextButton;
    [SerializeField]
    private GameObject PreviousButton;

    private int CurrentPage = 1;
    private bool FirstPage;
    private bool LastPage;
    private bool OnlyPage;

    private bool SearchClicked = false;
    private bool LocationClicked = false;

    private readonly string basePath = "https://ratings.food.gov.uk/";

    private RequestHelper currentRequest;

    public void Start()
    {
        //Reset Values
        PlayerPrefs.SetString("Query", "");
        FirstPanel.SetActive(false);
        SecondPanel.SetActive(false);
        ThirdPanel.SetActive(false);
        FourthPanel.SetActive(false);
        FifthPanel.SetActive(false);
        OutputText.SetActive(true);
        Loading.SetActive(false);
        PreviousButton.SetActive(false);
        NextButton.SetActive(false);

        OutputTextResult.text = "Search for a business name";
    }

    public void ClickLocation() 
    {
        FirstPanel.SetActive(false);
        SecondPanel.SetActive(false);
        ThirdPanel.SetActive(false);
        FourthPanel.SetActive(false);
        FifthPanel.SetActive(false);
        PreviousButton.SetActive(false);
        NextButton.SetActive(false);
        OutputText.SetActive(false);
        Loading.SetActive(false);
        LocationClicked = true;
        try
        {
            //Ask for Permission
            AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.ACCESS_FINE_LOCATION");

            //If granted
            if (result == AndroidRuntimePermissions.Permission.Granted)
                //Grab lat and long
                StartCoroutine(GetLocation());
        }
        catch (Exception)
        {
            Loading.SetActive(false);
            OutputText.SetActive(true);
            OutputTextResult.text = "Failed to determine location";
        }
    }
   
    private IEnumerator GetLocation()
    {
        FirstPanel.SetActive(false);
        SecondPanel.SetActive(false);
        ThirdPanel.SetActive(false);
        FourthPanel.SetActive(false);
        FifthPanel.SetActive(false);
        OutputText.SetActive(false);
        Loading.SetActive(false);
        PreviousButton.SetActive(false);
        NextButton.SetActive(false);
        Loading.SetActive(true);

        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            OutputText.SetActive(true);
            Loading.SetActive(false);
            OutputTextResult.text = "Location not enabled";
            yield break;
        }
       
        // Start service before querying location
        Input.location.Start();
        Loading.SetActive(true);

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Loading.SetActive(false);
            OutputText.SetActive(true);
            OutputTextResult.text = "Timed out";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Loading.SetActive(false);
            OutputText.SetActive(true);
            OutputTextResult.text = "Connection failed";
            yield break;
        }
        // Access granted and location value could be retrieved
        else
        {
            Loading.SetActive(false);
            OutputText.SetActive(false);

            if (LocationClicked == true)
            {
                //Go to first page
                CurrentPage = 1;
            }

            //We can add default request headers for all requests
            RestClient.DefaultRequestHeaders["Authorization"] = "Bearer ...";

            //search for establishment name in all locations (non-cultured)
            RestClient.Get<RootObject>(basePath + "/enhanced-search/en-GB/^/^/DISTANCE/0/^/" + Convert.ToString(Input.location.lastData.longitude) + "/" + Convert.ToString(Input.location.lastData.latitude) + "/1/5/json").Then(EstablishmentDetail => {

                //Store results in a string
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);

                //Create response object
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                //Count number of responses
                switch (response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail.Count)
                {
                    case 0:
                        Debug.Log("No Results!");
                        OutputText.SetActive(true);
                        OutputTextResult.text = "No results found...";
                        PageText.text = "Page 1 of 1";
                        FirstPanel.SetActive(false);
                        SecondPanel.SetActive(false);
                        ThirdPanel.SetActive(false);
                        FourthPanel.SetActive(false);
                        FifthPanel.SetActive(false);
                        break;
                    case 1:
                        Debug.Log("1 Result!");
                        OutputText.SetActive(false);
                        PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                        FirstPanel.SetActive(true);
                        SecondPanel.SetActive(false);
                        ThirdPanel.SetActive(false);
                        FourthPanel.SetActive(false);
                        FifthPanel.SetActive(false);
                        //Parse JSON and store as object, Update Results Textboxes
                        FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                        break;
                    case 2:
                        Debug.Log("2 Results!");
                        OutputText.SetActive(false);
                        PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                        FirstPanel.SetActive(true);
                        SecondPanel.SetActive(true);
                        ThirdPanel.SetActive(false);
                        FourthPanel.SetActive(false);
                        FifthPanel.SetActive(false);
                        //Parse JSON and store as object, Update Results Textboxes
                        FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                        SecondResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue + ")";
                        break;
                    case 3:
                        Debug.Log("3 Results!");
                        OutputText.SetActive(false);
                        PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                        FirstPanel.SetActive(true);
                        SecondPanel.SetActive(true);
                        ThirdPanel.SetActive(true);
                        FourthPanel.SetActive(false);
                        FifthPanel.SetActive(false);
                        //Parse JSON and store as object, Update Results Textboxes
                        FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                        SecondResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue + ")";
                        ThirdResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingValue + ")";
                        break;
                    case 4:
                        Debug.Log("4 Results!");
                        OutputText.SetActive(false);
                        PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                        FirstPanel.SetActive(true);
                        SecondPanel.SetActive(true);
                        ThirdPanel.SetActive(true);
                        FourthPanel.SetActive(true);
                        FifthPanel.SetActive(false);
                        //Parse JSON and store as object, Update Results Textboxes
                        FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                        SecondResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue + ")";
                        ThirdResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingValue + ")";
                        FourthResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].RatingValue + ")";
                        break;
                    case 5:
                        Debug.Log(response.FHRSEstablishment.Header.ItemCount + " Results!");
                        OutputText.SetActive(false);
                        PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                        FirstPanel.SetActive(true);
                        SecondPanel.SetActive(true);
                        ThirdPanel.SetActive(true);
                        FourthPanel.SetActive(true);
                        FifthPanel.SetActive(true);
                        //Parse JSON and store as object, Update Results Textboxes
                        FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                        SecondResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue + ")";
                        ThirdResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingValue + ")";
                        FourthResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].RatingValue + ")";
                        FifthResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].RatingValue + ")";
                        break;
                }

                //If page is first, and isn't last
                if (CurrentPage == 1 && (!(CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount))))
                {
                    FirstPage = true;
                    LastPage = false;
                    OnlyPage = false;
                    Debug.Log("This is the first page!");

                    PreviousButton.SetActive(false);
                    NextButton.SetActive(true);
                    //Page x of y, x ≠ y
                }
                else if (!(CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount)))
                {
                    FirstPage = false;
                    LastPage = false;
                    OnlyPage = false;

                    PreviousButton.SetActive(true);
                    NextButton.SetActive(true);
                }  //If we reached the last page
                else if (CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount) && (CurrentPage > 1))
                {
                    LastPage = true;
                    FirstPage = false;
                    Debug.Log("This is the last page!");

                    PreviousButton.SetActive(true);
                    NextButton.SetActive(false);
                }
                //If this is the only page
                if ((System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount) == 1) && (CurrentPage == 1))
                {
                    LastPage = true;
                    FirstPage = true;
                    OnlyPage = true;
                    Debug.Log("This is the only page!");

                    PreviousButton.SetActive(false);
                    NextButton.SetActive(false);
                }
                //reset bool
                LocationClicked = false;

            })
            #if UNITY_EDITOR
            .Catch(err =>
            EditorUtility.DisplayDialog("Error", err.Message, "Ok"))
            #endif
            ;

        }

        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }



    public void Get()
    {
        if (SearchClicked == true)
        {
            //Go to first page
            CurrentPage = 1;
        }

        //We can add default request headers for all requests
        RestClient.DefaultRequestHeaders["Authorization"] = "Bearer ...";
                
        //search for establishment name in all locations (non-cultured)
        RestClient.Get<RootObject>(basePath + "search/" + inputbox.text + "/^/" + CurrentPage + "/5/json").Then(EstablishmentDetail => {

            //Temp save what we searched
            PlayerPrefs.SetString("Query", inputbox.text);

            //Display results in dialog
            //EditorUtility.DisplayDialog("Search results...", JsonUtility.ToJson(EstablishmentDetail, true), "Ok");

            //Store results in a string
            string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);

            //Create response object
            RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

            //Count number of responses
            switch (response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail.Count)
            {
                case 0:
                    Debug.Log("No Results!");
                    OutputText.SetActive(true);
                    OutputTextResult.text = "No results found...";
                    PageText.text = "Page 1 of 1";
                    FirstPanel.SetActive(false);
                    SecondPanel.SetActive(false);
                    ThirdPanel.SetActive(false);
                    FourthPanel.SetActive(false);
                    FifthPanel.SetActive(false);
                    break;
                case 1:
                    Debug.Log("1 Result!");
                    OutputText.SetActive(false);
                    PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                    FirstPanel.SetActive(true);
                    SecondPanel.SetActive(false);
                    ThirdPanel.SetActive(false);
                    FourthPanel.SetActive(false);
                    FifthPanel.SetActive(false);
                    //Parse JSON and store as object, Update Results Textboxes
                    FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                    break;
                case 2:
                    Debug.Log("2 Results!");
                    OutputText.SetActive(false);
                    PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                    FirstPanel.SetActive(true);
                    SecondPanel.SetActive(true);
                    ThirdPanel.SetActive(false);
                    FourthPanel.SetActive(false);
                    FifthPanel.SetActive(false);
                    //Parse JSON and store as object, Update Results Textboxes
                    FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                    SecondResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue + ")";
                    break;
                case 3:
                    Debug.Log("3 Results!");
                    OutputText.SetActive(false);
                    PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                    FirstPanel.SetActive(true);
                    SecondPanel.SetActive(true);
                    ThirdPanel.SetActive(true);
                    FourthPanel.SetActive(false);
                    FifthPanel.SetActive(false);
                    //Parse JSON and store as object, Update Results Textboxes
                    FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                    SecondResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue + ")";
                    ThirdResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingValue + ")";
                    break;
                case 4:
                    Debug.Log("4 Results!");
                    OutputText.SetActive(false);
                    PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                    FirstPanel.SetActive(true);
                    SecondPanel.SetActive(true);
                    ThirdPanel.SetActive(true);
                    FourthPanel.SetActive(true);
                    FifthPanel.SetActive(false);
                    //Parse JSON and store as object, Update Results Textboxes
                    FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                    SecondResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue + ")";
                    ThirdResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingValue + ")";
                    FourthResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].RatingValue + ")";
                    break;
                case 5:
                    Debug.Log(response.FHRSEstablishment.Header.ItemCount + " Results!");
                    OutputText.SetActive(false);
                    PageText.text = "Page " + CurrentPage + " of " + response.FHRSEstablishment.Header.PageCount;
                    FirstPanel.SetActive(true);
                    SecondPanel.SetActive(true);
                    ThirdPanel.SetActive(true);
                    FourthPanel.SetActive(true);
                    FifthPanel.SetActive(true);
                    //Parse JSON and store as object, Update Results Textboxes
                    FirstResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue + ")";
                    SecondResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue + ")";
                    ThirdResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingValue + ")";
                    FourthResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].RatingValue + ")";
                    FifthResult.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].BusinessName + " (" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].RatingValue + ")";
                    break;
            }

            //If page is first, and isn't last
            if (CurrentPage == 1 && (!(CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount))))
            {
                FirstPage = true;
                LastPage = false;
                OnlyPage = false;
                Debug.Log("This is the first page!");

                PreviousButton.SetActive(false);
                NextButton.SetActive(true);
            //Page x of y, x ≠ y
            } else if (!(CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount))) {
                FirstPage = false;
                LastPage = false;
                OnlyPage = false;

                PreviousButton.SetActive(true);
                NextButton.SetActive(true);
            }  //If we reached the last page
            else if (CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount) && (CurrentPage > 1)) {
                LastPage = true;
                FirstPage = false;
                Debug.Log("This is the last page!");

                PreviousButton.SetActive(true);
                NextButton.SetActive(false);
            }
            //If this is the only page
            if ((System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount) == 1) && (CurrentPage == 1))
            {
                LastPage = true;
                FirstPage = true;
                OnlyPage = true;
                Debug.Log("This is the only page!");

                PreviousButton.SetActive(false);
                NextButton.SetActive(false);
            }
            //reset bool
            SearchClicked = false;

            })
            #if UNITY_EDITOR
            .Catch(err =>
            EditorUtility.DisplayDialog("Error", err.Message, "Ok"))
            #endif
            ;
    }

    public void AbortRequest()
    {
        if (currentRequest != null)
        {
            currentRequest.Abort();
            currentRequest = null;
        }
    }

    public void ClickSearch()
    {
        if (!(inputbox.text == "")) 
        {
            SearchClicked = true;
            Get();
        }
    }

    public void NextPage()
    {
        //Make sure nothing accidentally typed in there will affect the result
        inputbox.text = PlayerPrefs.GetString("Query");

        if (LastPage == false && OnlyPage == false)
        {
            CurrentPage++;
            Get();
            Debug.Log("You are on page " + CurrentPage);
        }
    }

    public void PreviousPage()
    {
        //Make sure nothing accidentally typed in there will affect the result
        inputbox.text = PlayerPrefs.GetString("Query");

        if (FirstPage == false && OnlyPage == false)
        {
            CurrentPage--;
            Get();
            Debug.Log("You are on page " + CurrentPage);
        }
    }
}