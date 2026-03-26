using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace DnnFree.Modules.SPA.Angular
{
    public partial class View : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        public string ErrorMessage { get; set; } = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                RegisterAngularAssets();
                InjectTokenToPage();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        private void InjectTokenToPage()
        {
            if (Page.Header == null) return;

            string token = GetAntiForgeryToken();
            var metaTag = new HtmlMeta
            {
                Name = "RequestVerificationToken",
                Content = token
            };
            Page.Header.Controls.Add(metaTag);
        }

        private string GetAntiForgeryToken()
        {
            try
            {
                // برای DNN 9.4+
                var tokenField = typeof(DotNetNuke.Web.Common.SharedConstants)
                    .GetField("RequestVerificationToken");
                if (tokenField != null)
                {
                    return tokenField.GetValue(null)?.ToString() ?? ModuleId.ToString();
                }
            }
            catch { }

            return ModuleId.ToString();
        }

        private void RegisterAngularAssets()
        {
            string moduleVirtualPath = AppRelativeTemplateSourceDirectory;
            string distVirtualPath = $"{moduleVirtualPath.TrimEnd('/')}/dist";
            string distPhysicalPath = Server.MapPath(distVirtualPath);

            if (!Directory.Exists(distPhysicalPath))
            {
                ErrorMessage = $"<div class='dnnFormMessage dnnFormError'><strong>Error:</strong> Angular dist folder not found at: {distPhysicalPath}</div>";
                DotNetNuke.Services.Exceptions.Exceptions.LogException(
                    new DirectoryNotFoundException($"Angular dist folder not found: {distPhysicalPath}")
                );
                return;
            }

            LoadJsFiles(distPhysicalPath, distVirtualPath);
            LoadCssFiles(distPhysicalPath, distVirtualPath);
        }

        private void LoadJsFiles(string physicalPath, string virtualPath)
        {
            var priorityOrder = new[] { "runtime", "polyfills", "main" };
            int basePriority = (int)FileOrder.Js.DefaultPriority;

            var allJsFiles = Directory
                .GetFiles(physicalPath, "*.js")
                .Select(f => Path.GetFileName(f))
                .ToList();

            foreach (var prefix in priorityOrder)
            {
                var match = allJsFiles.FirstOrDefault(f =>
                    f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    ClientResourceManager.RegisterScript(
                        Page,
                        $"{virtualPath}/{match}",
                        basePriority++
                    );
                    allJsFiles.Remove(match);
                }
            }

            foreach (var chunkFile in allJsFiles.OrderBy(f => f))
            {
                ClientResourceManager.RegisterScript(
                    Page,
                    $"{virtualPath}/{chunkFile}",
                    basePriority++
                );
            }
        }

        private void LoadCssFiles(string physicalPath, string virtualPath)
        {
            var cssFiles = Directory
                .GetFiles(physicalPath, "*.css")
                .Select(f => Path.GetFileName(f))
                .OrderBy(f => f);

            int priority = (int)FileOrder.Css.DefaultPriority;

            foreach (var cssFile in cssFiles)
            {
                ClientResourceManager.RegisterStyleSheet(
                    Page,
                    $"{virtualPath}/{cssFile}",
                    priority++
                );
            }
        }
    }
}