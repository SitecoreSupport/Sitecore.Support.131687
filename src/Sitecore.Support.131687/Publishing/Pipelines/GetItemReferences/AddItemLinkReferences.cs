

namespace Sitecore.Support.Publishing.Pipelines.GetItemReferences
{
  using Sitecore.Data;
  using Sitecore.Data.Comparers;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Publishing;
  using Sitecore.Publishing.Pipelines.GetItemReferences;
  using Sitecore.Publishing.Pipelines.PublishItem;
  using Sitecore.Support.Publishing.Pipelines.Publish;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  public class AddItemLinkReferences : GetItemReferencesProcessor
  {
    [NotNull]
    protected override List<Item> GetItemReferences([NotNull] PublishItemContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      var list = new List<Item>();
      if (context.PublishOptions.Mode != PublishMode.SingleItem)
      {
        return list;
      }

      switch (context.Action)
      {
        case PublishAction.PublishVersion:
          Item sourceVersion = context.VersionToPublish;
          if (sourceVersion == null)
          {
            return list;
          }

          list.AddRange(this.GetReferences(sourceVersion, false, context.PublishOptions.TargetDatabase));
          break;
        case PublishAction.PublishSharedFields:
          Item sourceItem = context.PublishHelper.GetSourceItem(context.ItemId);
          if (sourceItem == null)
          {
            return list;
          }

          list.AddRange(this.GetReferences(sourceItem, true, context.PublishOptions.TargetDatabase));
          break;
        default:
          return list;
      }

      return list;
    }

    private IEnumerable<Item> GetReferences([NotNull] Item item, bool sharedOnly, Database targetDatabase)
    {
      Assert.ArgumentNotNull(item, "item");

      var list = new List<Item>();

      var itemLinks = item.Links.GetValidLinks();
      itemLinks = itemLinks.Where(link => item.Database.Name.Equals(link.TargetDatabaseName, StringComparison.OrdinalIgnoreCase)).ToArray();
      if (sharedOnly)
      {
        itemLinks = itemLinks.Where(link =>
        {
          var sourceItem = link.GetSourceItem();
          return sourceItem != null && (ID.IsNullOrEmpty(link.SourceFieldID) || sourceItem.Fields[link.SourceFieldID].Shared);
        }).ToArray();
      }

      var relatedItems = itemLinks.Select(link => link.GetTargetItem()).Where(relatedItem => relatedItem != null && relatedItem.ID != item.ID).ToList();

      foreach (var relatedItem in relatedItems)
      {
        list.AddRange(PublishQueue.GetParentsIfNotExist(relatedItem, targetDatabase));
        list.Add(relatedItem);
      }

      return list.Distinct(new ItemIdComparer());
    }
  }
}