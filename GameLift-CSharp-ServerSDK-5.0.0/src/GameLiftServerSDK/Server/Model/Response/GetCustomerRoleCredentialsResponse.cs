namespace Aws.GameLift.Server.Model
{
    /// <summary>
    /// The service role credentials provided to GameLift that allow the compute
    /// access to other AWS services in your account. 
    /// </summary>
    public class GetCustomerRoleCredentialsResponse : Response
    {
        /// <summary>
        /// The Amazon Resource Name (ARN) of the user that the service role belongs to.
        /// </summary>
        public string AssumedRoleUserArn { get; set; }
        /// <summary>
        /// The ID of the user that the service role belongs to.
        /// </summary>
        public string AssumedRoleId { get; set; }
        /// <summary>
        /// The access key ID used to authenticate and provide access to your AWS resources.
        /// </summary>
        public string AccessKeyId { get; set; }
        /// <summary>
        /// The secret access key ID used for authentication.
        /// </summary>
        public string SecretAccessKey { get; set; }
        /// <summary>
        /// A token used to identify the current active session interacting with your AWS resources.
        /// </summary>
        public string SessionToken { get; set; }
        /// <summary>
        /// The epoch time when your session credentials expire.
        /// </summary>
        public long Expiration { get; set; }
    }
}
