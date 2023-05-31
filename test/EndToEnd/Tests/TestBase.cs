// namespace EndToEndTests
// {
//     public class TestBase
//     {
//         private const string DEFAULT_HOST_URI = "http://localhost";
//         private const string DEFAULT_APP_PORT = "7071";
//         private const string DEFAULT_DAPR_HTTP_PORT = "3500";

//         private string hostUri;
//         private string appPort;
//         private string daprHttpPort;

//         public string FunctionsAppUri
//         {
//             get
//             {
//                 return $"{hostUri}:{appPort}";
//             }
//         }

//         public string DaprHttpUri
//         {
//             get
//             {
//                 return $"{hostUri}:{daprHttpPort}";
//             }
//         }

//         public TestBase()
//         {
//             hostUri = Environment.GetEnvironmentVariable(Constants.ENVKEY_HOST_URI) ?? DEFAULT_HOST_URI;
//             appPort = Environment.GetEnvironmentVariable(Constants.ENVKEY_APP_PORT) ?? DEFAULT_APP_PORT;
//             daprHttpPort = Environment.GetEnvironmentVariable(Constants.ENVKEY_DAPR_HTTP_PORT) ?? DEFAULT_DAPR_HTTP_PORT;
//         }
//     }
// }