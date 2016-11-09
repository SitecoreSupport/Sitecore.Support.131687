using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.Support.Publishing.Pipelines.Publish
{
  public class PublishQueue
  {
    [NotNull]
    public static IEnumerable<Item> GetParentsIfNotExist([NotNull] Item item, Database targetDatabase)
    {
      Assert.ArgumentNotNull(item, "item");

      var parentList = new HashSet<Item>();

      using (new SecurityDisabler())
      {
        var parent = item.Parent;
        while (parent != null && !Sitecore.Publishing.Pipelines.Publish.PublishQueue.IsPredefinedRootId(parent.ID))
        {
          if (targetDatabase.GetItem(parent.ID) == null)
          {
            parentList.Add(parent);
          }

          parent = parent.Parent;
        }
      }

      return parentList.Reverse();
    }
  }
}