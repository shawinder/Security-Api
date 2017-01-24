Active Directory Integration
- Remote Administrator Groups should be installed on the server hosting the security API. 
You can check it by running "dsquery" on the command prompt.


*IMP for published copy
//===============================================
Copy EntityFramework.SqlServer.dll from Security.Data > bin and put it inside the published > bin
Add Applications > New application named "SecurityAdmin" And add user reference for admin@ur-channel.com in ApplicaitonUsers


//Setup Owin server and configure ASP.NET Web API
//===================================================
Install-Package Microsoft.AspNet.WebApi.Owin -Version 5.1.2
Install-Package Microsoft.Owin.Host.SystemWeb -Version 2.1.0
Install-Package Microsoft.AspNet.Identity.EntityFramework -Version 2.0.1

//ASP.NET Identity Core
//===================================================
Install-Package Microsoft.AspNet.Identity.Core -Version 2.0.0

//Owin OAuth
//===================================================
Install-Package Microsoft.Owin.Security.OAuth -Version 2.1.0

//Owin CORS
//===================================================
Install-Package Microsoft.Owin.Cors -Version 2.1.0

//WebAPI CORS
//====================================================
Install-Package Microsoft.AspNet.WebApi.Cors

//References
//===================================================
System.Runtime.Serialization (Help Pages)

//Intall Package through Nuget Manager
Newtonsoft.Json (1.0.3)

//Install Web API Help Pages
//=============================================================
Install-Package Microsoft.AspNet.WebApi.HelpPage -Version 5.1.2

//Fiddler - Add Application
POST http://localhost:86/Application/add
Content-Type: application/json
//Request Header
Authorization: Bearer rlvxY05dQaw5YaZh127weXBuHBsoth08ytM8wUYg8tgls3acVZXTl7rKeWoc0niboubno8JF6YfZPq33uDR68eFPb5y7qrB0a4wC6B3V-Z11UW_Hg1LEsBe8sjREw9OFgcWSADaHLfGhGo5PWmklSWtqCs9qynsd1PjIVffFfr4zBTURY12diutxHEwVamgKFUTz-uMlkNyiR8EVl7mlXQRYVBKGlaOpX18F1fss6s8iUClaihRDo-tXPX2B2C5APxkw1wPikCSU-E5Rq9KokE97Lu83plPoww8Sh1Vql0E
//Request Body
{
"ApplicationName": "WFG"
}

//Fiddler - Register User
POST http://localhost:86/user/add HTTP/1.1
Content-Type: application/json; charset=utf-8
//Request Body
{
  "applicationid": "d86502e4-3c34-e411-80cf-0ad1e5deca71",
  "Username":"test@ur-channel.com",
  "firstname": "Security",
  "lastname": "Admin",
  "email": "test@ur-channel.com",
  "password": "test",
  "confirmPassword": "test",
  "IPAddress":"127.0.0.1"
}

//Fiddler - Token Generation - Parameter order should be strictly followed
POST http://localhost:86/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded
//Request Body
grant_type=password&username=security-admin@ur-channel.com&password=$4t@xm3n&appId=d86502e4-3c34-e411-80cf-0ad1e5deca71

//Fidler - Update User-Application relations (Should be passed as an Array)
POST http://localhost:86/user/application/update HTTP/1.1
Content-Type: application/json; charset=utf-8
//Request Body
[{
  "User":{
     "Id": "8abf616b-efb4-4f79-880d-9bd3a0222f87"
   },
  "App":{
      "ApplicationId": "2150784f-3a85-4afb-b82e-3fa1cd402092"
  }
}]
//And For Multiple Apps
[
 {
  "User":{
     "Id": "8abf616b-efb4-4f79-880d-9bd3a0222f87"
   },
  "App":{
      "ApplicationId": "2150784f-3a85-4afb-b82e-3fa1cd402092"
  }
},
{
  "User":{
     "Id": "8abf616b-efb4-4f79-880d-9bd3a0222f87"
   },
  "App":{
      "ApplicationId": "2b3b4086-f93e-4a64-9f49-1b8cc0686794"
  }
 }
]

//Fiddler - View Secure Content by passing the token [generated above]
GET http://localhost:86/securedata HTTP/1.1
Content-Type: application/json
Authorization: Bearer 9IilJyW4_Rse5aPcxCPOUXEZqqfy70YPxz0oRvkK7pp_gp-ZwZiH9Z3eB5y-xBueHGXDbqFDBbp4CulrLi_ZFkj3g6kUu4ujqw8SpA6oP8R-cLGAvzWnYwEm0yQLwXKOkOy19Bp8OGsfXR8YPQabuG75RrtTXTn95oEehkkvOTPnfE8YryOtgfLPY9esje2fCs3-7kZwWGmRQiK14Nco-B8-1G2S_mtVcRb6HnRjldA


//Fiddler - Read Claim information from Token
POST http://localhost:86/user/read HTTP/1.1
//Request Header
Authorization: Bearer rlvxY05dQaw5YaZh127weXBuHBsoth08ytM8wUYg8tgls3acVZXTl7rKeWoc0niboubno8JF6YfZPq33uDR68eFPb5y7qrB0a4wC6B3V-Z11UW_Hg1LEsBe8sjREw9OFgcWSADaHLfGhGo5PWmklSWtqCs9qynsd1PjIVffFfr4zBTURY12diutxHEwVamgKFUTz-uMlkNyiR8EVl7mlXQRYVBKGlaOpX18F1fss6s8iUClaihRDo-tXPX2B2C5APxkw1wPikCSU-E5Rq9KokE97Lu83plPoww8Sh1Vql0E

//Set default date manually for online setup
//==========================================
ALTER TABLE [Security].[dbo].[Applications] 
ADD CONSTRAINT DEFAULT_DATE
DEFAULT GETDATE() FOR DateCreated
