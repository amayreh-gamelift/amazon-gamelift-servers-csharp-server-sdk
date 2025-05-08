namespace Aws.GameLift.Server.Model
{
    internal class ResponseMessage : Message
    {
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }

        public ResponseMessage()
        {
            Action = null;
            RequestId = null;
            ErrorMessage = null;
        }
    }
}