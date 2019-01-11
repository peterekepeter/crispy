namespace Crispy
{
    /// <summary> Authorization requirements of an endpoint </summary>
    public class AuthorizationInfo
    {
        /// <summary> If anyone can access without logging in. </summary>
        public bool AllowAnonymous, IsSigninRequired;

        /// <summary> If there are any role requirements. </summary>
        public string Roles;

        /// <summary> If there are any other policies that apply. </summary>
        public string Policy;
    }
}