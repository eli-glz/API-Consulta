Imports ConsultaPoliza.Api.Options
Imports ConsultaPoliza.Api.Services

Namespace ConsultaPoliza.Api.Infrastructure
    Public Module ServiceRegistry
        Private ReadOnly Options As OraclePolicyOptions = OraclePolicyOptions.FromConfiguration()
        Private ReadOnly ReaGeneralPackageInstance As IReaGeneralPackage = New ReaGeneralPackage()
        Private ReadOnly PolicyResponseBuilderInstance As IPolicyResponseBuilder = New PolicyResponseBuilder(ReaGeneralPackageInstance)

        Public ReadOnly Property PolicyRepository As IPolicyRepository
            Get
                Return New OraclePolicyRepository(Options, PolicyResponseBuilderInstance)
            End Get
        End Property

        Public ReadOnly Property BranchRepository As IBranchRepository
            Get
                Return New OracleBranchRepository(Options)
            End Get
        End Property
    End Module
End Namespace
