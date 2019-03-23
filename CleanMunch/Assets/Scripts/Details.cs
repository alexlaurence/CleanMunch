using System;
using System.Collections.Generic;

namespace Models
{
    [Serializable]
    public class Xml
    {
        public string _version;
    }

    [Serializable]
    public class Header
    {
        public string _text;
        public string ExtractDate;
        public string ItemCount;
        public string ReturnCode;
        public string PageNumber;
        public string PageSize;
        public string PageCount;
    }

    [Serializable]
    public class Scores
    {
        public string Hygiene;
        public string Structural;
        public string ConfidenceInManagement;
    }

    [Serializable]
    public class Geocode
    {
        public string Longitude;
        public string Latitude;
    }

    [Serializable]
    public class Distance
    {
        public string _xsinil;
    }

    [Serializable]
	public class EstablishmentDetail
    {
        public string FHRSID;
        public string LocalAuthorityBusinessID;
        public string BusinessName;
        public string BusinessType;
        public string BusinessTypeID;
        public string AddressLine1;
        public string AddressLine2;
        public string AddressLine3;
        public string AddressLine4;
        public string PostCode;
        public string RatingValue;
        public string RatingKey;
        public object RightToReply;
        public string RatingDate;
        public string LocalAuthorityCode;
        public string LocalAuthorityName;
        public string LocalAuthorityWebSite;
        public string LocalAuthorityEmailAddress;
        public Scores Scores;
        public string SchemeType;
        public string NewRatingPending;
        public Geocode Geocode;
        public Distance Distance;

        public override string ToString()
        {
            return UnityEngine.JsonUtility.ToJson(this, true);
        }
    }

    [Serializable]
    public class EstablishmentCollection
    {
        public string _xmlnsxsd;
        public string _xmlnsxsi;
        public List<EstablishmentDetail> EstablishmentDetail;
    }

    [Serializable]
    public class FHRSEstablishment
    {
        public Header Header;
        public EstablishmentCollection EstablishmentCollection;
    }

    [Serializable]
    public class RootObject
    {
        public Xml _xml;
        public FHRSEstablishment FHRSEstablishment;

        public static implicit operator string(RootObject v)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return UnityEngine.JsonUtility.ToJson(this, true);
        }
    }
}

