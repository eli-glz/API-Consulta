Imports System.Web
Imports System.Web.Http
Imports ConsultaPoliza.Api.App_Start

Namespace ConsultaPoliza.Api
    Public Class WebApiApplication
        Inherits HttpApplication

        Protected Sub Application_Start()
            GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
        End Sub
    End Class
End Namespace
