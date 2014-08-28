using System.Web;
using System.Web.Optimization;

namespace AnarkRE
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/crypto").Include(
                        "~/Scripts/jsencrypt.js",
                        "~/Scripts/triplesec-3.0.0-min.js",
                        "~/Scripts/commonescrow.js",
                        "~/Scripts/md5.js"));

            bundles.Add(new ScriptBundle("~/bundles/bitescrow").Include(
                        "~/Scripts/bitescrow/array.map.js",
                        "~/Scripts/bitescrow/crypto.js",
                        "~/Scripts/bitescrow/secure.random.js",
                        "~/Scripts/bitescrow/elliptic.curve.js",
                        "~/Scripts/bitescrow/bigint.js",
                        "~/Scripts/bitescrow/bitcoin.base58.js",
                        "~/Scripts/bitescrow/bitcoin.address.js",
                        
                        "~/Scripts/bitescrow/bitcoin.ecsa.js",
                        "~/Scripts/bitescrow/bitcoin.escrow.js",
                        "~/Scripts/bitescrow/bitescrow.js"
                        
                        
                        ));

            bundles.Add(new ScriptBundle("~/bundles/grid").Include(
                        "~/Scripts/gridmvc.min.js"
            ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new StyleBundle("~/Content/grid/css").Include(
                "~/Content/Gridmvc.css",
                "~/Content/gridmvc.datepicker.min.css",
                "~/Content/pagination.css"
                ));
            
        }
    }
}