namespace Configuration
{
    /// <summary>
    /// Here we define all the backend requests used in the application.
    /// </summary>
    public enum BackendRequests
    {
        //Authentication
        Login = 10,
        Register = 20,
        Logout = 30,
        ChangePassword = 40,
        ResetPassword = 50,
        SendValidationCode = 60,
        ConfirmValidationCode = 70,
        RefreshToken = 80,  

        //Account

    }
}
