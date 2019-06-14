using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Publishing.Pipelines.Publish;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.Support.Publishing.Pipelines.Publish
{
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
                    if (targetDatabase.GetItem(i.ID) == null)
                    {
                        items.Add(i);
                    }
                }
            }
            return items.Reverse<Item>();
        }
    }
}