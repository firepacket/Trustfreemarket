using DataAnnotationsExtensions.ClientValidation;
using System.Web.Helpers;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using WebMatrix.WebData;

[assembly: WebActivator.PreApplicationStartMethod(typeof(AnarkRE.App_Start.RegisterClientValidationExtensions), "Start")]
 
namespace AnarkRE.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            
        }
    }
}