Imports System.Net.Http.Headers
Imports System.Web.Http
Imports Newtonsoft.Json.Serialization

Namespace ConsultaPoliza.Api.App_Start
    Public Module WebApiConfig
        Public Sub Register(config As HttpConfiguration)
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly

            config.MapHttpAttributeRoutes()

            config.Routes.MapHttpRoute(
                name:="DefaultApi",
                routeTemplate:="api/{controller}/{id}",
                defaults:=New With {.id = RouteParameter.Optional})

            Dim jsonFormatter = config.Formatters.JsonFormatter
            jsonFormatter.SerializerSettings.ContractResolver = New CamelCasePropertyNamesContractResolver()
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include
            jsonFormatter.SupportedMediaTypes.Add(New MediaTypeHeaderValue("text/html"))
        End Sub
    End Module
End Namespace
