using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Models;
using Proyecto26;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

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
    private GameObject PageTextObj;
    [SerializeField]
    private GameObject OutputText;
    [SerializeField]
    private Text OutputTextResult;
    [SerializeField]
    private GameObject Loading;
    [SerializeField]
    private GameObject NextButtonSearch;
    [SerializeField]
    private GameObject PreviousButtonSearch;
    [SerializeField]
    private GameObject NextButtonLocation;
    [SerializeField]
    private GameObject PreviousButtonLocation;
    [SerializeField]
    private Text ResultInfo;

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
        PreviousButtonSearch.SetActive(false);
        NextButtonSearch.SetActive(false);
        PreviousButtonLocation.SetActive(false);
        NextButtonLocation.SetActive(false);
        PageTextObj.SetActive(false);

        OutputTextResult.text = "Search for a business name";
    }

    public void ClickLocation() 
    {
        CurrentPage = 1;
        FirstPanel.SetActive(false);
        SecondPanel.SetActive(false);
        ThirdPanel.SetActive(false);
        FourthPanel.SetActive(false);
        FifthPanel.SetActive(false);
        OutputText.SetActive(false);
        Loading.SetActive(false);


        PreviousButtonSearch.SetActive(false);
        NextButtonSearch.SetActive(false);
        PreviousButtonLocation.SetActive(false);
        NextButtonLocation.SetActive(false);

        LocationClicked = true;
        SearchClicked = false;
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
            PageTextObj.SetActive(false);
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
        PreviousButtonSearch.SetActive(false);
        NextButtonSearch.SetActive(false);
        PreviousButtonLocation.SetActive(false);
        NextButtonLocation.SetActive(false);
        Loading.SetActive(true);

        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            OutputText.SetActive(true);
            Loading.SetActive(false);
            PageTextObj.SetActive(false);
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
            PageTextObj.SetActive(false);
            OutputTextResult.text = "Timed out";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Loading.SetActive(false);
            OutputText.SetActive(true);
            PageTextObj.SetActive(false);
            OutputTextResult.text = "Connection failed";
            yield break;
        }
        // Access granted and location value could be retrieved
        else
        {
            OutputText.SetActive(false);

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
                        Loading.SetActive(false);
                        Debug.Log("No Results!");
                        OutputText.SetActive(true);
                        OutputTextResult.text = "No results found...";
                        PageTextObj.SetActive(false);
                        PageText.text = "Page 1 of 1";
                        FirstPanel.SetActive(false);
                        SecondPanel.SetActive(false);
                        ThirdPanel.SetActive(false);
                        FourthPanel.SetActive(false);
                        FifthPanel.SetActive(false);
                        break;
                    case 1:
                        Loading.SetActive(false);
                        Debug.Log("1 Result!");
                        OutputText.SetActive(false);
                        PageTextObj.SetActive(true);
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
                        Loading.SetActive(false);
                        Debug.Log("2 Results!");
                        OutputText.SetActive(false);
                        PageTextObj.SetActive(true);
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
                        Loading.SetActive(false);
                        Debug.Log("3 Results!");
                        OutputText.SetActive(false);
                        PageTextObj.SetActive(true);
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
                        Loading.SetActive(false);
                        Debug.Log("4 Results!");
                        OutputText.SetActive(false);
                        PageTextObj.SetActive(true);
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
                        Loading.SetActive(false);
                        Debug.Log(response.FHRSEstablishment.Header.ItemCount + " Results!");
                        OutputText.SetActive(false);
                        PageTextObj.SetActive(true);
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

                    PreviousButtonLocation.SetActive(false);
                    NextButtonLocation.SetActive(true);
                    //Page x of y, x ≠ y
                }
                else if (!(CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount)))
                {
                    FirstPage = false;
                    LastPage = false;
                    OnlyPage = false;

                    PreviousButtonLocation.SetActive(true);
                    NextButtonLocation.SetActive(true);
                }  //If we reached the last page
                else if (CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount) && (CurrentPage > 1))
                {
                    LastPage = true;
                    FirstPage = false;
                    Debug.Log("This is the last page!");

                    PreviousButtonLocation.SetActive(true);
                    NextButtonLocation.SetActive(false);
                }
                //If this is the only page
                if ((System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount) == 1) && (CurrentPage == 1))
                {
                    LastPage = true;
                    FirstPage = true;
                    OnlyPage = true;
                    Debug.Log("This is the only page!");

                    PreviousButtonLocation.SetActive(false);
                    NextButtonLocation.SetActive(false);
                }
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
                    PageTextObj.SetActive(false);
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
                    PageTextObj.SetActive(true);
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
                    PageTextObj.SetActive(true);
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
                    PageTextObj.SetActive(true);
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
                    PageTextObj.SetActive(true);
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
                    PageTextObj.SetActive(true);
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

                PreviousButtonSearch.SetActive(false);
                NextButtonSearch.SetActive(true);
            //Page x of y, x ≠ y
            } else if (!(CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount))) {
                FirstPage = false;
                LastPage = false;
                OnlyPage = false;

                PreviousButtonSearch.SetActive(true);
                NextButtonSearch.SetActive(true);
            }  //If we reached the last page
            else if (CurrentPage == System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount) && (CurrentPage > 1)) {
                LastPage = true;
                FirstPage = false;
                Debug.Log("This is the last page!");

                PreviousButtonSearch.SetActive(true);
                NextButtonSearch.SetActive(false);
            }
            //If this is the only page
            if ((System.Convert.ToInt32(response.FHRSEstablishment.Header.PageCount) == 1) && (CurrentPage == 1))
            {
                LastPage = true;
                FirstPage = true;
                OnlyPage = true;
                Debug.Log("This is the only page!");

                PreviousButtonSearch.SetActive(false);
                NextButtonSearch.SetActive(false);
            }
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
            CurrentPage = 1;
            SearchClicked = true;
            LocationClicked = false;
            Get();
        }
    }

    public void NextPageSearch()
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

    public void PreviousPageSearch()
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

    public void NextPageLocation()
    {
        if (LastPage == false && OnlyPage == false)
        {
            CurrentPage++;
            GetLocation();
            Debug.Log("You are on page " + CurrentPage);
        }
    }

    public void PreviousPageLocation()
    {
        if (FirstPage == false && OnlyPage == false)
        {
            CurrentPage--;
            GetLocation();
            Debug.Log("You are on page " + CurrentPage);
        }
    }

    #region result buttons
    public void ResultClick1()
    {
        if (SearchClicked == true)
            RestClient.Get<RootObject>(basePath + "search/" + inputbox.text + "/^/" + CurrentPage + "/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].LocalAuthorityEmailAddress;
            });
        else if (LocationClicked == true)
            RestClient.Get<RootObject>(basePath + "/enhanced-search/en-GB/^/^/DISTANCE/0/^/" + Convert.ToString(Input.location.lastData.longitude) + "/" + Convert.ToString(Input.location.lastData.latitude) + "/1/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[0].LocalAuthorityEmailAddress;
            });

    }

    public void ResultClick2()
    {
        if (SearchClicked == true)
            RestClient.Get<RootObject>(basePath + "search/" + inputbox.text + "/^/" + CurrentPage + "/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].LocalAuthorityEmailAddress;
            });
        else if (LocationClicked == true)
            RestClient.Get<RootObject>(basePath + "/enhanced-search/en-GB/^/^/DISTANCE/0/^/" + Convert.ToString(Input.location.lastData.longitude) + "/" + Convert.ToString(Input.location.lastData.latitude) + "/1/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[1].LocalAuthorityEmailAddress;
            });
    }

    public void ResultClick3()
    {
        if (SearchClicked == true)
            RestClient.Get<RootObject>(basePath + "search/" + inputbox.text + "/^/" + CurrentPage + "/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].LocalAuthorityEmailAddress;
            });
        else if (LocationClicked == true)
            RestClient.Get<RootObject>(basePath + "/enhanced-search/en-GB/^/^/DISTANCE/0/^/" + Convert.ToString(Input.location.lastData.longitude) + "/" + Convert.ToString(Input.location.lastData.latitude) + "/1/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[2].LocalAuthorityEmailAddress;
            });
    }

    public void ResultClick4()
    {
        if (SearchClicked == true)
            RestClient.Get<RootObject>(basePath + "search/" + inputbox.text + "/^/" + CurrentPage + "/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].LocalAuthorityEmailAddress;
            });
        else if (LocationClicked == true)
            RestClient.Get<RootObject>(basePath + "/enhanced-search/en-GB/^/^/DISTANCE/0/^/" + Convert.ToString(Input.location.lastData.longitude) + "/" + Convert.ToString(Input.location.lastData.latitude) + "/1/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[3].LocalAuthorityEmailAddress;
            });
    }

    public void ResultClick5()
    {
        if (SearchClicked == true)
            RestClient.Get<RootObject>(basePath + "search/" + inputbox.text + "/^/" + CurrentPage + "/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].LocalAuthorityEmailAddress;
            });
        else if (LocationClicked == true)
            RestClient.Get<RootObject>(basePath + "/enhanced-search/en-GB/^/^/DISTANCE/0/^/" + Convert.ToString(Input.location.lastData.longitude) + "/" + Convert.ToString(Input.location.lastData.latitude) + "/1/5/json").Then(EstablishmentDetail =>
            {
                string jsonResult = JsonUtility.ToJson(EstablishmentDetail, true);
                RootObject response = JsonUtility.FromJson<RootObject>(jsonResult);

                ResultInfo.text = response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].BusinessName
                + "\nID: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].FHRSID
                + "\nType: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].BusinessType
                + "\n"
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].AddressLine1
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].AddressLine2
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].AddressLine3
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].AddressLine4
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].PostCode
                + "\n"
                + "\nRating: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].RatingValue
                + "\nRating Date: " + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].RatingDate
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].LocalAuthorityName
                + "\n" + response.FHRSEstablishment.EstablishmentCollection.EstablishmentDetail[4].LocalAuthorityEmailAddress;
            });
    }
    #endregion
}