namespace Sitecore.Support.Publishing.Pipelines.Publish
{
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;    
    using Sitecore.SecurityModel;    
    using System.Collections.Generic;
    using System.Linq;

    public class PublishQueue
    {
        
        public PublishQueue()
        {
        }                

        public static IEnumerable<Item> GetParentsIfNotExist(Item item, Database targetDatabase)
        {
            Assert.ArgumentNotNull(item, "item");
            HashSet<Item> items = new HashSet<Item>();
            using (SecurityDisabler securityDisabler = new SecurityDisabler())
            {
                for (Item i = item.Parent; i != null && !Sitecore.Publishing.Pipelines.Publish.PublishQueue.IsPredefinedRootId(i.ID); i = i.Parent)
                {
                    #region Modified code
                    // added 'if' statement that is using for adding parent items only if they are not exist in target database
                    if (targetDatabase.GetItem(i.ID) == null)
                    {
                        items.Add(i);
                    }
                    #endregion
                }
            }
            return items.Reverse<Item>();
        }

        
    }
}