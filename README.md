# Sitecore.Support.131687

When publishing a single item with enabled the `Publish Related Items` option, parent items (up to the `Home` item) and their related items could be published too.

## Main

This repository contains Sitecore Patch #131687, which overrides the `AddItemLinkReferences` processor to drop a published item from a colection of its related items.

``` xml
<getItemReferences>
...
  <processor type="Sitecore.Publishing.Pipelines.GetItemReferences.AddItemLinkReferences, Sitecore.Kernel" />
...
</getItemReferences>
```

## License

This patch is licensed under the [Sitecore Corporation A/S License](./LICENSE).

## Download

Downloads are available via [GitHub Releases](https://github.com/SitecoreSupport/Sitecore.Support.131687/releases).
