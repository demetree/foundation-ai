using System.Collections.Generic;

namespace Foundation
{
    public class UserFavourite
    {
        public string entity { get; set; }
        public string description { get; set; }
        public int id { get; set; }
    }


    public class UserFavouriteMinimalData
    {
        public int id { get; set; }
        public string description { get; set; }
    }


    public class UserFavouriteListObject
    {
        //
        // Class is here just to be able to serialize to an object because the user settings handlers don't directly support arrays, so this is just a wrapper around a list array.
        // 
        public List<UserFavourite> favourites { get; set; }

        public UserFavouriteListObject()
        {
            favourites = new List<UserFavourite>();
        }
    }


    public class UserMostRecent
    {
        public int sequence { get; set; }
        public string entity { get; set; }
        public string description { get; set; }
        public int id { get; set; }
    }


    public class UserMostRecentListObject
    {
        //
        // Class is here just to be able to serialize to an object because the user settings handlers don't directly support arrays, so this is just a wrapper around a list array.
        // 
        public List<UserMostRecent> mostRecents { get; set; }

        public UserMostRecentListObject()
        {
            mostRecents = new List<UserMostRecent>();
        }
    }

    public class BasicListItem
    {
        public BasicListItem()
        {

        }
        public BasicListItem(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public int id { get; set; }
        public string name { get; set; }
    }

    public class BasicListItemWithGuid
    {
        public BasicListItemWithGuid()
        {

        }

        public BasicListItemWithGuid(int id, string guid, string name)
        {
            this.id = id;
            this.guid = guid;
            this.name = name;

        }

        public int id { get; set; }
        public string guid { get; set; }
        public string name { get; set; }
    }


    public class ComplexListItem
    {
        public ComplexListItem()
        {

        }

        public ComplexListItem(int id, string name, string description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
        }

        public ComplexListItem(int id, int parentId, string name, string description)
        {
            this.id = id;
            this.parentId = parentId;
            this.name = name;
            this.description = description;
        }


        public int id { get; set; }
        public int? parentId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

}