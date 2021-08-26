using System;
    using System.Linq;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Shell.Applications.ContentEditor;
    using Sitecore.Text;
    using Sitecore.Web;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.WebControls;
    public class MultiRootTreeList : TreeList
    {
        private const String QueryKey = "query:";
        private const String DataSourceKey = "DataSourceKey";
        private String ExtractQuery(String value)
        {
            return value.Substring(QueryKey.Length);
        }
        private String ExtractDataSource(String value)
        {
            return value.Substring(DataSourceKey.Length);
        }
 
        protected override void OnLoad(EventArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            base.OnLoad(args);
 
            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                var existingTreeView = (TreeviewEx)WebUtil.FindControlOfType(this, typeof(TreeviewEx));
                var treeviewParent = existingTreeView.Parent;
 
                existingTreeView.Parent.Controls.Clear();
                var dataContext = (DataContext)WebUtil.FindControlOfType(this, typeof(DataContext));
                var dataContextParent = dataContext.Parent;
 
                dataContextParent.Controls.Remove(dataContext);
 
                var impostor = new Sitecore.Web.UI.WebControls.MultiRootTreeview();
                impostor.ID = existingTreeView.ID;
                impostor.DblClick = existingTreeView.DblClick;
                impostor.Enabled = existingTreeView.Enabled;
                impostor.DisplayFieldName = existingTreeView.DisplayFieldName;
 
                var dataContexts = ParseDataContexts(dataContext);
 
                impostor.DataContext = string.Join("|", dataContexts.Where(x => x != null).Select(x => x.ID));
                foreach (var context in dataContexts)
                {
                    if (context != null)
                    {
                        dataContextParent.Controls.Add(context);
                    }
                }
 
                treeviewParent.Controls.Add(impostor);
            }
        }
 
        protected virtual DataContext[] ParseDataContexts(DataContext originalDataContext)
        {
            return new ListString(Source).Select(x => CreateDataContext(originalDataContext, x)).ToArray();
        }
 
        protected virtual DataContext CreateDataContext(DataContext baseDataContext, string dataSource)
        {
            DataContext dataContext = new DataContext();
            dataContext.ID = GetUniqueID("D");
            dataContext.Filter = baseDataContext.Filter;
            dataContext.DataViewName = "Master";
            if (!string.IsNullOrEmpty(DatabaseName))
            {
                dataContext.Parameters = "databasename=" + DatabaseName;
            }
            if (dataSource.Contains(QueryKey))
            {
                Item contextItem = Sitecore.Context.ContentDatabase.Items[ItemID];
                Item dataSourceItem = contextItem.Axes.SelectSingleItem(ExtractQuery(dataSource));
                if (dataSourceItem == null)
                {
                    return null;
                }
                dataSource = dataSourceItem.Paths.FullPath;
            }
            if (dataSource.Contains("DataSource="))
            {
                dataSource = ExtractDataSource(dataSource);
                SetCustomProp(dataSource);
                var ampersand = dataSource.IndexOf('&');
                if(ampersand>=0){
                    dataSource = dataSource.Substring(0, ampersand);
                }
            }
            dataContext.Root = dataSource;
            dataContext.Language = Language.Parse(ItemLanguage);
            dataContext.Filter = this.FormTemplateFilterForDisplay();
            return dataContext;
        }
        void SetCustomProp(string source)
        {
            ExcludeTemplatesForSelection = StringUtil.ExtractParameter("ExcludeTemplatesForSelection", source).Trim();
            IncludeTemplatesForSelection = StringUtil.ExtractParameter("IncludeTemplatesForSelection", source).Trim();
            IncludeTemplatesForDisplay = StringUtil.ExtractParameter("IncludeTemplatesForDisplay", source).Trim();
            ExcludeTemplatesForDisplay = StringUtil.ExtractParameter("ExcludeTemplatesForDisplay", source).Trim();
            ExcludeItemsForDisplay = StringUtil.ExtractParameter("ExcludeItemsForDisplay", source).Trim();
            IncludeItemsForDisplay = StringUtil.ExtractParameter("IncludeItemsForDisplay", source).Trim();
            string strA = StringUtil.ExtractParameter("AllowMultipleSelection", source).Trim().ToLowerInvariant();
            AllowMultipleSelection = string.Compare(strA, "yes", StringComparison.InvariantCultureIgnoreCase) == 0;
            DatabaseName = StringUtil.ExtractParameter("databasename", source).Trim().ToLowerInvariant();
        }
    }