Imports System.Globalization

Partial Public Class MainForm
    Private Sub InitializeNavigationTree(policy As PolicyResponse)
        _navigationTree.BeginUpdate()
        _navigationTree.Nodes.Clear()

        Dim policyNode = CreateNode("Poliza", NodePolicy)
        policyNode.Nodes.Add(CreateNode("Estado", NodeStatus))

        Dim rolesNode = CreateNode("Roles", NodeRoles)

        If policy IsNot Nothing AndAlso policy.Roles IsNot Nothing AndAlso policy.Roles.Count > 0 Then
            For Each role In policy.Roles
                Dim roleNode = CreateNode(BuildRoleNodeText(role), NodeRoles)
                roleNode.Nodes.Add(CreateNode("Direcciones", NodeRoleDirections))
                rolesNode.Nodes.Add(roleNode)
            Next
        Else
            Dim holderNode = CreateNode("Rol 1 - Contratante", NodeRoleHolder)
            holderNode.Nodes.Add(CreateNode("Direcciones", NodeRoleDirections))
            rolesNode.Nodes.Add(holderNode)
            rolesNode.Nodes.Add(CreateNode("Rol 2 - Asegurado", NodeRoleInsured))
        End If

        policyNode.Nodes.Add(rolesNode)

        policyNode.Nodes.Add(CreateNode("Intermediarios", NodeIntermediaries))
        policyNode.Nodes.Add(CreateNode("Debitos directos", NodeDirectDebits))

        Dim coveragesNode = CreateNode("Coberturas", NodeCoverages)
        If policy IsNot Nothing AndAlso policy.CoberturaPrincipal.HasValue Then
            coveragesNode.Nodes.Add(CreateNode("Cobertura " & policy.CoberturaPrincipal.Value.ToString(), NodeCoverage))
        End If
        policyNode.Nodes.Add(coveragesNode)

        policyNode.Nodes.Add(CreateNode("Clausulas", NodeClauses))
        policyNode.Nodes.Add(CreateNode("Descuentos / Recargos", NodeDiscounts))
        policyNode.Nodes.Add(CreateNode("Movimientos historicos", NodeMovements))
        Dim receiptsNode = CreateNode("Recibos", NodeReceipts)
        If policy IsNot Nothing AndAlso
        policy.Recibos IsNot Nothing AndAlso
        policy.Recibos.Count > 0 Then

            For Each receipt In policy.Recibos
                If receipt Is Nothing Then
                    Continue For
                End If

                Dim receiptText =
                    "Recibo " &
                    receipt.NumeroRecibo.ToString(
                        CultureInfo.InvariantCulture)

                receiptsNode.Nodes.Add(
                    CreateNode(receiptText, NodeReceipt))
            Next
        End If

        policyNode.Nodes.Add(receiptsNode)
        Dim addressNode = CreateNode("Direccion de poliza", NodePolicyAddress)
        Dim addressItemNode = CreateNode("Direccion de poliza 1", NodePolicyAddressItem)
        addressItemNode.Nodes.Add(CreateNode("Telefonos", NodePhones))
        addressNode.Nodes.Add(addressItemNode)
        policyNode.Nodes.Add(addressNode)

        _navigationTree.Nodes.Add(policyNode)
        policyNode.Expand()
        _navigationTree.Enabled = policy IsNot Nothing
        _navigationTree.EndUpdate()

        If policy IsNot Nothing Then
            _navigationTree.SelectedNode = policyNode
        End If
    End Sub

    Private Shared Function CreateNode(text As String, tag As String) As TreeNode
        Return New TreeNode(text) With {.Tag = tag}
    End Function

    Private Shared Function BuildRoleNodeText(role As PolicyRoleResponse) As String
        If role Is Nothing OrElse Not HasText(role.Rol) Then
            Return "Rol"
        End If

        Return "Rol " & role.Rol.Trim()
    End Function

    Private Shared Function CreateInputLabel(text As String) As Label
        Return New Label With {
            .Dock = DockStyle.Fill,
            .Text = text,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(0)
        }
    End Function
End Class