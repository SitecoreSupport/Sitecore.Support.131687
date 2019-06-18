namespace Sitecore.Support.Publishing.Pipelines.GetItemReferences
{

    using Sitecore.Data;
    using Sitecore.Data.Comparers;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Links;
    using Sitecore.Publishing;
    using Sitecore.Publishing.Pipelines.GetItemReferences;
    using Sitecore.Publishing.Pipelines.PublishItem;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AddItemLinkReferences : GetItemReferencesProcessor
    {        
        
        public bool DeepScan
        {
            get;
            set;
        }

        public AddItemLinkReferences()
        {
            this.DeepScan = true;
        }
        

        
      
        protected override List<Item> GetItemReferences(PublishItemContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            List<Item> items = new List<Item>();
            if (context.PublishOptions.Mode != PublishMode.SingleItem)
            {
                return items;
            }
            PublishAction action = context.Action;
            if (action == PublishAction.PublishSharedFields)
            {
                Item sourceItem = context.PublishHelper.GetSourceItem(context.ItemId);
                if (sourceItem == null)
                {
                    return items;
                }

                #region Modified code
                // to original items.AddRange(this.GetReferences(sourceItem, true, new HashSet<ID>())) method added last parameter to pass context for custom PublishQueue.GetParentsIfNotExist() method
                items.AddRange(this.GetReferences(sourceItem, true, new HashSet<ID>(), context.PublishOptions.TargetDatabase));
                #endregion
            }
            else
            {
                if (action != PublishAction.PublishVersion)
                {
                    return items;
                }
                Item versionToPublish = context.VersionToPublish;
                if (versionToPublish == null)
                {
                    return items;
                }

                #region Modified code
                // to original items.AddRange(this.GetReferences(sourceItem, true, new HashSet<ID>())) method added last parameter to pass context to the custom PublishQueue.GetParentsIfNotExist() method
                items.AddRange(this.GetReferences(versionToPublish, false, new HashSet<ID>(), context.PublishOptions.TargetDatabase));
                #endregion
            }
            return items;
        }

        #region Modified code
        // added additional 'targetDatabase' parameter to pass context to the custom PublishQueue.GetParentsIfNotExist() method
        private IEnumerable<Item> GetReferences(Item item, bool sharedOnly, HashSet<ID> processedItems, Database targetDatabase)
        {
            Assert.ArgumentNotNull(item, "item");
            processedItems.Add(item.ID);
            List<Item> items = new List<Item>();
            ItemLink[] validLinks = item.Links.GetValidLinks();
            validLinks = (
                from link in validLinks
                where item.Database.Name.Equals(link.TargetDatabaseName, StringComparison.OrdinalIgnoreCase)
                select link).ToArray<ItemLink>();
            if (sharedOnly)
            {
                validLinks = ((IEnumerable<ItemLink>)validLinks).Where<ItemLink>((ItemLink link) => {
                    Item sourceItem = link.GetSourceItem();
                    if (sourceItem == null)
                    {
                        return false;
                    }
                    if (ID.IsNullOrEmpty(link.SourceFieldID))
                    {
                        return true;
                    }
                    return sourceItem.Fields[link.SourceFieldID].Shared;
                }).ToArray<ItemLink>();
            }
            List<Item> list = (
                from link in (IEnumerable<ItemLink>)validLinks
                select link.GetTargetItem() into relatedItem
                where relatedItem != null
                select relatedItem).ToList<Item>();
            foreach (Item item1 in list)
            {
                if (this.DeepScan && !processedItems.Contains(item1.ID))
                {
                    items.AddRange(this.GetReferences(item1, sharedOnly, processedItems, targetDatabase));
                }

                #region Modified code
                // original items.AddRange(PublishQueue.GetParents(item1)) method replaced with custom PublishQueue.GetParentsIfNotExist() method
                items.AddRange(Sitecore.Support.Publishing.Pipelines.Publish.PublishQueue.GetParentsIfNotExist(item1, targetDatabase));
                #endregion

                items.Add(item1);
            }
            return items.Distinct<Item>(new ItemIdComparer());
        }

        #endregion
    }
}