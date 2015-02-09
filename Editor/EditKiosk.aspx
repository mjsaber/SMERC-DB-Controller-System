<%@ Page Title="Edit Kiosk" Language="C#"  AutoEventWireup="true" CodeBehind="EditKiosk.aspx.cs" MasterPageFile="~/Editor.master"  Inherits="EVEditor.EditKiosk" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">

        .style1
        {
            width: 400px;
        }

    </style>
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>Edit Kiosk</h2>


    <table>

        <%--1st Row--%>
        <tr>
            <td>By Organization</td>
            <td>

              <asp:DropDownList ID="ddlModeCity" runat="server" AutoPostBack="True" Width="150px" 
                onselectedindexchanged="ddlModeCity_SelectedIndexChanged"></asp:DropDownList>

            </td>
            <td>
                <asp:Label ID="lblCatchError" runat="server" ForeColor="Red" Text="" Visible="False"></asp:Label>
                 &nbsp;<asp:Button ID="btnHideCatchError" runat="server" CausesValidation="False" onclick="btnHideCatchError_Click" Text="Hide" Visible="False" />
                <asp:Label ID="lblTest" runat="server" Text=""></asp:Label>   

            </td>
        </tr>

        <%--2nd Row--%>
        <tr>
            <td>

                By Parking Lot</td>
            <td>
        <asp:DropDownList ID="ddlModeParkingLot" runat="server" Width="150px" 
                       AutoPostBack="True" 
                       onselectedindexchanged="ddlModeParkingLot_SelectedIndexChanged" style="margin-bottom: 0px"></asp:DropDownList>
            </td>
            <td>

                <asp:Label ID="lblTest2" runat="server" Text=""></asp:Label>

            </td>
        </tr>

        <%--3rd Row--%>
        <tr>
            <td>

                By Kiosk</td>
            <td>
                <asp:DropDownList ID="ddlKiosk" runat="server" Width="150px" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="True">
                </asp:DropDownList>
            </td>
            <td>
                <asp:CheckBox ID="cbShowActivated" runat="server" AutoPostBack="True" oncheckedchanged="cbShowActivated_CheckedChanged" />
                <strong>Show Only Activated Gateways</strong></td>
        </tr>
    </table>
    <p>
             
    </p>

        <table>
            <tr>
                <td valign="top">
        <table>
        <tr>
        <td valign="top">
        <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>Kiosk List</h2>
                </div></div></div>
           
             <div class="mid-outer"><div class="mid-inner"><div class="mid"> 

        <asp:GridView ID="gvKiosk" runat="server" CssClass="datatable" 
                     GridLines="Vertical" OnPageIndexChanging="gvKioskPaging" onrowcreated="gvKioskRowCreated" 
                     OnSorting="gvKioskSorting" onselectedindexchanged="gvKioskSelectedIndex" AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" DataKeyNames="KioskID" Width="700px" PageSize="10">
        <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>                                  
            <asp:BoundField DataField="KioskID" HeaderText="KioskID" SortExpression="KioskID" /> 
            <asp:BoundField DataField="CityGUID" HeaderText="CityGUID" SortExpression="CityGUID" /> 
            <asp:BoundField DataField="KioskName" HeaderText="Kiosk Name"  SortExpression="KioskName" />    
            <asp:BoundField DataField="ParkingLotID" HeaderText="ParkingLotID"  SortExpression="ParkingLotID" />
            <asp:BoundField DataField="ParkingLotName" HeaderText="Parking Lot Name"  SortExpression="ParkingLotName" />   
            <asp:BoundField DataField="TransactionServiceProviderID" HeaderText="TransactionServiceProviderID"  SortExpression="TransactionServiceProviderID" />              
            <asp:BoundField DataField="ServiceProviderName" HeaderText="Service Provider Name"  SortExpression="ServiceProviderName" />  
            <asp:BoundField DataField="CityName" HeaderText="Organization"  SortExpression="CityName" /> 
            <asp:BoundField DataField="GatewayName" HeaderText="Gateway Name"  SortExpression="GatewayName" />              
            <asp:CheckBoxField DataField="CreditCardPayment" HeaderText="Credit Card Payment" SortExpression="CreditCardPayment" />            
            <asp:BoundField DataField="GatewayID" HeaderText="GatewayID"  SortExpression="GatewayID" />               
            <asp:BoundField DataField="IP Address" SortExpression="IP Address" HeaderText="IP Address" />                      
            <asp:BoundField DataField="Charging Level" HeaderText="Charging Level" SortExpression="Charging Level" />
            <asp:BoundField DataField="ChargingName" HeaderText="Charging Algorithm" SortExpression="ChargingName"></asp:BoundField>
            <asp:BoundField DataField="ChargingID" HeaderText="ChargingID" SortExpression="ChargingID" />   
            
            
            <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
            <asp:BoundField DataField="Max Current" HeaderText="Max Current" SortExpression="Max Current" />
            <asp:BoundField DataField="Max Station Current" HeaderText="Max Station Current" SortExpression="Max Station Current" />
            <asp:BoundField DataField="Max Voltage" HeaderText="Max Voltage" SortExpression="Max Voltage" />
            <asp:BoundField DataField="Max Station Voltage" HeaderText="Max Station Voltage" SortExpression="Max Station Voltage" />
            <asp:BoundField DataField="Source Current" HeaderText="Source Current" SortExpression="Source Current" />
            <asp:BoundField DataField="Source Voltage" HeaderText="Source Voltage" SortExpression="Source Voltage" />
            <asp:BoundField DataField="Retrieve Interval" HeaderText="Retrieve Interval" SortExpression="Retrieve Interval" />            
            <asp:BoundField DataField="Time Quantum" HeaderText="Time Quantum" SortExpression="Time Quantum" />
            <asp:BoundField DataField="Time Out" HeaderText="Time Out" SortExpression="Time Out" />
            <asp:BoundField DataField="Retry Times" HeaderText="Retry Times" SortExpression="Retry Times" />
            <asp:BoundField DataField="CurrentValve" HeaderText="Current Threshold" SortExpression="CurrentValve" />
            <asp:BoundField DataField="NodeControlDelay" HeaderText="Node Control Delay" SortExpression="NodeControlDelay" />
            <asp:CheckBoxField DataField="Activate" HeaderText="Activate" SortExpression="Activate" />
            <asp:CheckBoxField DataField="Enable" HeaderText="Enable" SortExpression="Enable" />
            <asp:CheckBoxField DataField="HasSOC" HeaderText="State of Charge" SortExpression="HasSOC" />
            <asp:CheckBoxField DataField="Controllable" HeaderText="Controllable" SortExpression="Controllable" />
            <asp:BoundField DataField="InDays" HeaderText="In Days" SortExpression="InDays" />
            <asp:BoundField DataField="MaxTimesInDay" HeaderText="Max Times In Day" SortExpression="MaxTimesInDay" />
            <asp:BoundField DataField="LeftCurrentValve" HeaderText="Left Current Threshold" SortExpression="LeftCurrentValve" />    
            <asp:BoundField DataField="Note" HeaderText="Note" SortExpression="Note" />                
            <asp:BoundField DataField="PrimaryEmail" HeaderText="PrimaryEmail" SortExpression="PrimaryEmail" />              
            
            
        </Columns>

        <PagerSettings Position="TopAndBottom" /><PagerStyle CssClass="pager-row" /> <RowStyle CssClass="row" Wrap="True" />
        <SelectedRowStyle BackColor="#38CDB9" Font-Bold="True" ForeColor="White" Wrap="True" /> 
            <SortedAscendingCellStyle BackColor="#FFCC66" Wrap="False" />
            <SortedDescendingCellStyle BackColor="#FFCC66" Wrap="False" />
    </asp:GridView> 
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>
    </td>
    </tr>
    </table>
    </td>


    <td valign="top">
                    <div class="grid">


            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">

                    <h2>Selected Kiosk Information</h2></div></div></div>

                <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 

         <tr>
            <td nowrap="nowrap">Kiosk Name</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1" >
                <asp:TextBox ID="tbKioskName" runat="server" MaxLength="50"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvtbKioskName" runat="server" ControlToValidate="tbKioskName" Display="Dynamic" ErrorMessage="Required Field." ForeColor="Red"></asp:RequiredFieldValidator>
                <asp:Label ID="lbltbKioskNameError" runat="server" ForeColor="Red" Text=""></asp:Label>
             </td>
         </tr>

         <tr style="background-color: #F4F4F4">
            <td nowrap="nowrap" style="width:180px">Parking Lot</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1">
                <asp:DropDownList ID="ddlParkingLot" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlParkingLot_SelectedIndexChanged"></asp:DropDownList>
                <asp:RequiredFieldValidator ID="vldrddlParkingLot" runat="server" ControlToValidate="ddlParkingLot" Display="Dynamic" ErrorMessage="Required Field." ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
                <asp:Label ID="lblddlParkingLotError" runat="server" ForeColor="Red" 
                    Text=""></asp:Label>
            &nbsp;</td></tr>

        <tr>
            <td nowrap="nowrap">Credit Card Payment</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1" >
                <asp:DropDownList ID="ddlCreditCardPayment" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlCreditCardPayment_SelectedIndexChanged">
                    <asp:ListItem Value="1">True</asp:ListItem>
                    <asp:ListItem Value="0">False</asp:ListItem>
                </asp:DropDownList>
                <asp:Label ID="lblddlCreditCardPaymentError" runat="server" ForeColor="Red" Text=""></asp:Label>
             </td>
         </tr>

        <tr style="background-color: #F4F4F4">
            <td nowrap="nowrap">Transaction Service Provider</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">&nbsp;</td>
            <td class="style1" >
                <asp:DropDownList ID="ddlTransactionServiceProvider" runat="server">
                </asp:DropDownList>
                <asp:Label ID="lblddlTransactionServiceProviderError" runat="server" ForeColor="Red" Text=""></asp:Label>
             </td>
         </tr>

        <tr>
            <td nowrap="nowrap">Gateway Name</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1" >
                <asp:DropDownList ID="ddlGatewayName" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlGatewayName_SelectedIndexChanged">
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="vldrddlGatewayName" runat="server" ControlToValidate="ddlGatewayName" Display="Dynamic" ErrorMessage="Required Field." ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
                <asp:Label ID="lblddlGatewayNameError" runat="server" ForeColor="Red" Text=""></asp:Label>
             </td>
         </tr>

     </table>

    
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>




          
       <asp:Button ID="btnNew" runat="server" Font-Size="Large" Text="New" visible = "true" onclick="btnNewClick"/>      
       <asp:Button ID="Clear" runat="server" Font-Size="Large" Text="Clear" visible = "true" CausesValidation="false" onclick="btnClearClick"/>                 
       <asp:Button ID="btnUpdate" runat="server" Font-Size="Large" Text="Update" visible = "false" onclick="btnUpdateClick"/>
        <asp:Button ID="btnDelete" runat="server" Font-Size="Large" Text="Delete" visible = "false" OnClick="btnDelete_Click"/>    
                    <br />
    </td>
    </tr>
    </table>
                      
        <br />
        </asp:Content>