Namespace ConsultaPoliza.Api.Models
    Public Class BranchResponse
        Public Sub New()
        End Sub

        Public Sub New(codigo As Integer, descripcion As String)
            Me.Codigo = codigo
            Me.Descripcion = descripcion
        End Sub

        Public Property Codigo As Integer
        Public Property Descripcion As String = ""
    End Class
End Namespace
