namespace Configuration
{
    public static class RequestDictionary
    {
        public static Dictionary<BackendRequests, string> ApiRequests;

        /// <summary>
        /// Here we link the enum to the actual request paths
        /// Usage: { BackendRequests.EnumValue, "path to request" (taken from swagger without the /api part)
        /// Parameters can be added using {0}, {1}, etc. and then they are replaced by the AsPath extension method
        /// </summary>
        static RequestDictionary()
        {
            ApiRequests = new Dictionary<BackendRequests, string>
            {
                //Authentication
                { BackendRequests.Login, "/account/login" },
                { BackendRequests.Register, "/account/register" },
                { BackendRequests.Logout, "/account/logout" },
                { BackendRequests.ChangePassword, "/account/change-password" },
                { BackendRequests.ResetPassword, "/account/reset-password" },
                { BackendRequests.SendValidationCode, "/account/send-code" },
                { BackendRequests.ConfirmValidationCode, "/account/confirm" },
                { BackendRequests.RefreshToken, "/account/refresh-token" },
            };
        }
    }
}
