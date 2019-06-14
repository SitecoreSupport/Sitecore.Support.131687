using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Comparers;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.GetItemReferences;
using Sitecore.Publishing.Pipelines.PublishItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
namespace Sitecore.Support.Publishing.Pipelines.GetItemReferences
{
    /// <summary>
    /// Adds referenced items from the links database.
    /// </summary>
    public class AddItemLinkReferences : GetItemReferencesProcessor
    {
        /// <summary>
        /// Gets or sets a value indicating whether to deep scan the references.
        /// </summary>
        /// <value>
        /// The deep scan.
        /// </value>
        public bool DeepScan
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Publishing.Pipelines.GetItemReferences.AddItemLinkReferences" /> class. 
        /// </summary>
        public AddItemLinkReferences()
        {
            this.DeepScan = true;
        }

        /// <summary>
        /// Gets the list of item references.
        /// </summary>
        /// <param name="context">The publish item context.</param>
        /// <returns>
        /// The list of item references.
        /// </returns>
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
                items.AddRange(this.GetReferences(sourceItem, true, new HashSet<ID>(), context.PublishOptions.TargetDatabase));
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
                items.AddRange(this.GetReferences(versionToPublish, false, new HashSet<ID>(), context.PublishOptions.TargetDatabase));
            }
            return items;
        }

        /// <summary>
        /// Gets the related references.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="sharedOnly">Determines whether to process shared fields only or not.</param>
        /// <param name="processedItems">Recursively processed items.</param>
        /// <returns>
        /// The related references.
        /// </returns>
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
                items.AddRange(Sitecore.Support.Publishing.Pipelines.Publish.PublishQueue.GetParentsIfNotExist(item1, targetDatabase));
                items.Add(item1);
            }
            return items.Distinct<Item>(new ItemIdComparer());
        }
    }
}