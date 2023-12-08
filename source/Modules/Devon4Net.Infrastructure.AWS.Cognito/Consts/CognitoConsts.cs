namespace ADC.PostNL.BuildingBlocks.AWS.Cognito.Consts
{
    public static class CognitoConsts
    {
        //Create AppClient Consts
        public const string ExplicitAuthFlows_AllowRefreshTokenAuth = "ALLOW_REFRESH_TOKEN_AUTH";
        public const string ExplicitAuthFlows_AllowUserPasswordAuth = "ALLOW_USER_PASSWORD_AUTH";
        public const string ExplicitAuthFlows_AllowAdminUserPasswordAuth = "ALLOW_ADMIN_USER_PASSWORD_AUTH";
        public const string ExplicitAuthFlows_AllowCustomAuth = "ALLOW_CUSTOM_AUTH";
        public const string ExplicitAuthFlows_AllowUserSrpAuth = "ALLOW_USER_SRP_AUTH";

        public const string AllowedOAuthFlows_code = "code";
        public const string AllowedOAuthFlows_implicit = "implicit";
        public const string AllowedOAuthFlows_client_credentials = "client_credentials";
    }
}
