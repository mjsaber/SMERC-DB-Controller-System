<%@ Page Title = "User Editor" MasterPageFile="../Site.Master" Language="C#" AutoEventWireup="true" 
CodeBehind="EVUserEditorHashed.aspx.cs" Inherits="RTMC.EVUserEditorHashed" EnableEventValidation="false"%>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        .style1
        {
            height: 26px;
        }
    </style>
    </asp:Content>
        
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        User Editor
        <br />
        <asp:Label ID="Message1" runat="server" Text="Label" 
            Font-Bold="True" ForeColor="#009900" 
            Font-Size="X-Large" Visible="False"></asp:Label>
        <asp:Label ID="UpdateErrorLabel" runat="server" 
            Text="Label" ForeColor="Red" Visible="False"></asp:Label>
        <asp:Label ID="ErrorMessage" runat="server" Text="Label" 
            ForeColor="Red" Font-Bold="True" Font-Size="Medium" 
            Visible="False"></asp:Label>    
   </h2>
        <table> <tr>
       <td style="font-size: medium; font-weight: bold;" 
               class="style1"> Select By Organization:</td>           
       <td class="style1"><asp:DropDownList ID="ddlSelectByCity" 
               runat="server" AutoPostBack="True" 
               
               onselectedindexchanged="ddlSelectByCity_SelectedIndexChanged" 
               style="height: 22px">
           </asp:DropDownList></td> <td style="font-size: medium; font-weight: bold;" 
               class="style1"> 
                       <asp:CheckBox ID="cbShowActivated" runat="server" 
                           AutoPostBack="True" 
                           oncheckedchanged="cbShowActivated_CheckedChanged" />Only Activated Users
                   </td>
                   <td style="font-size: medium; font-weight: bold;" 
               class="style1"> 
                       
            </td>
           </tr>
           <tr>
           <td>
               &nbsp;</td>
           <td><asp:Label ID="lblTest" runat="server" Font-Bold="True" 
        Font-Size="Medium" Text=""></asp:Label>

               </td>
           <td>
           &nbsp;<asp:Label ID="lblTotalUsers" runat="server" Font-Bold="True" 
        Font-Size="Medium" Text=""></asp:Label>
           </td>
           </tr>
           
           
           
           </table>

           <table>
           <tr>
            <td><asp:Label ID="lblCatchError" runat="server" Text="" 
                   ForeColor="Red" Visible="False"></asp:Label><asp:Button ID="btnHideCatchError" runat="server" 
                   Text="Hide" onclick="btnHideCatchError_Click" 
                   Visible="False" CausesValidation="False" /></td>
            <td style="font-size: medium; font-weight: bold;" class="style1"> Search By: </td> 
            <td><asp:DropDownList ID="ddlSearchKeywords" runat="server">
                <asp:ListItem Value="emailAddress">Email Address</asp:ListItem>
                <asp:ListItem Value="userName">User Name</asp:ListItem>
                <asp:ListItem Value="firstName">First Name</asp:ListItem>
                <asp:ListItem Value="lastName">Last Name</asp:ListItem>
                <asp:ListItem Value="phoneNo">Phone No.</asp:ListItem>
                <asp:ListItem Value="zipCode">Zip Code</asp:ListItem>
                <asp:ListItem Value="state">State</asp:ListItem>
                <asp:ListItem Value="roleName">Role Name</asp:ListItem>
                
            </asp:DropDownList></td>
            <td><asp:TextBox runat="server" ID="tbSearchKeywords" Width="170px"></asp:TextBox></td>
            <td><asp:Button runat="server" ID="btnSearch" Text="Search" onclick="btnSearchClick" CausesValidation="False"/></td>
            <td><asp:Button runat="server" ID="btnSearchClear" Text="Clear" onclick="btnSearchClearClick" CausesValidation="False"/></td>
           </tr>
           </table>

        <table>
        <tr>
        <td valign=top>
        <table>
        <tr>
        <td valign="top">
        <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>User List</h2></div></div></div>
                <div class="mid-outer"><div class="mid-inner"><div class="mid"> 

        <asp:GridView ID="GV_UserEditor" runat="server" CssClass="datatable" 
                        GridLines="Vertical"
            OnPageIndexChanging="GV_UserEditor_Paging" OnRowCreated="GV_UserEditor_rowCreated"
                        OnSorting="GV_UserEditor_Sorting" onselectedindexchanged="GV_UserEditor_SelectedIndexChanged"
        AllowSorting="True" AllowPaging="True" PageSize="15" CellPadding="0"
                        CellSpacing="0" BorderWidth="0" 
                        AutoGenerateColumns="False">
        <AlternatingRowStyle Wrap="False" />
        <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>      
            <asp:BoundField DataField="UserName" 
                SortExpression="UserName" HeaderText="User Name" 
                ApplyFormatInEditMode="False" FooterStyle-Wrap="False" ><FooterStyle Wrap="False"></FooterStyle>
            </asp:BoundField>
            <asp:BoundField DataField="UserId" SortExpression="UserId" HeaderText="UserId"/>
            <asp:BoundField DataField="Email" SortExpression="Email" HeaderText="Email"/>
            <asp:BoundField DataField="RoleName" SortExpression="RoleName" HeaderText="Role Name" ApplyFormatInEditMode="True" />
            <asp:BoundField DataField="EVID" SortExpression="EVID" HeaderText="EV"/>
            <asp:BoundField DataField="RoleArea" SortExpression="RoleArea" HeaderText="Role Area"/>
            <asp:BoundField DataField="IsApproved" SortExpression="IsApproved" HeaderText="Is Approved"/>
            <asp:BoundField DataField="IsLockedOut" SortExpression="IsLockedOut" HeaderText="Is Locked Out"/>
            <asp:BoundField DataField="Activated" SortExpression="Activated" HeaderText="Activated"/>
            
        </Columns>
        <PagerSettings Position="TopAndBottom" />
        <HeaderStyle Wrap="False" />
        <PagerStyle CssClass="pager-row" />
        <RowStyle CssClass="row" Wrap="False" />
        <SelectedRowStyle BackColor="#38CDB9" Font-Bold="True" 
                ForeColor="White" Wrap="False" /> 
            <SortedAscendingCellStyle BackColor="#FFCC66" 
                Wrap="False" />
            <SortedDescendingCellStyle BackColor="#FFCC66" 
                Wrap="False" />
    </asp:GridView> </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%> 
    </td>
    </tr>
    </table>
                  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    </td>


    <td valign=top>
                    <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>User Account Information</h2></div></div></div>
                <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 
   <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">User Name </td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td><asp:TextBox ID="tbUsername" autocomplete="off" Width="170px" runat="server" ></asp:TextBox>
            <asp:Label ID="UserNameVerifyText" runat="server" ForeColor="Red" 
                Text="Label"></asp:Label>
        </td>

     <tr>
        <td nowrap="nowrap">Password</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td><asp:TextBox ID="TxtBox_Password" autocomplete="off" Width="170px" runat="server"></asp:TextBox>&nbsp;<asp:Label ID="PasswordVerifyLabel" runat="server" ForeColor="Red" 
                Text="Label"></asp:Label> &nbsp;<asp:RegularExpressionValidator ID="valPassword" runat="server"
           ControlToValidate="TxtBox_Password"
           ErrorMessage="Minimum password length is 6"
           ValidationExpression=".{6}.*" ForeColor="Red" /> 
            </td>
     </tr>
     
     <tr style="background-color: #F4F4F4">
     <td nowrap="nowrap">Repeat Password</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td>
            <asp:TextBox ID="TxtBox_PasswordRepeat" autocomplete="off" Width="170px" runat="server"></asp:TextBox>
            <asp:Label ID="RepeatPasswordVerifyLabel" runat="server" ForeColor="Red" 
                Text="Label"></asp:Label>
         </td>
     </tr>    
     <tr>
     <td></td>
     <td></td>
     <td>
                    <asp:Button ID="btnResetPass" runat="server" Text="Reset Password?" 
                        Visible = "false" onclick="btnResetPass_Click" /> </td>
     </tr>
    <tr>
          <td nowrap="nowrap">EV Type</td>
          <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
          <td><asp:DropDownList ID="ddlEvModelList" runat="server" 
                  style="margin-bottom: 0px">
              </asp:DropDownList>  <asp:Label ID="lblEvVerify" runat="server" ForeColor="Red" 
                    Text="Label"></asp:Label></td>
     </tr>
     <tr style="background-color: #F4F4F4">
         <td nowrap="nowrap" class="style1">Activated?</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
         <td><asp:DropDownList ID="ddlIsActivated" runat="server">
         </asp:DropDownList>
                <asp:Label ID="lblActivateVerify" runat="server" ForeColor="Red" 
                    Text="Label"></asp:Label> </td>
     </tr>

     <tr>
              <td nowrap="nowrap" class="style1">Email</td>
              <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1"> *</td>
              <td> 
                  <asp:TextBox ID="tbEmail" runat="server" 
                      Width="219px"></asp:TextBox>
                  <asp:Label ID="lblEmailVerify" runat="server" Text="Label" 
                      ForeColor="Red"></asp:Label> &nbsp;<asp:RegularExpressionValidator
                        ID="reqAuthEmail"
                        RunAt="server"
                        ControlToValidate="tbEmail" 
                        ValidationExpression="^[\w-\.]+@([\w-]+\.)+[\w-]{2,3}$"
                        ErrorMessage="Valid Email Address required" ForeColor="Red" />
                  </td>     
     </tr>
    
     <tr style="background-color: #F4F4F4">
         <td nowrap="nowrap">Password Question</td>
         <td></td>
         <td><asp:TextBox ID="tbPassQuestion" Width="170px" runat="server"></asp:TextBox></td>     
     </tr>
     
     <tr>     
             <td nowrap="nowrap">Password Answer</td>
             <td></td>
             <td><asp:TextBox ID="tbPassAnswer" Width="170px" runat="server" ></asp:TextBox></td>     
     </tr>

     <tr style="background-color: #F4F4F4"> 
             <td nowrap="nowrap">Is Approved?</td>
             <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
             <td><asp:DropDownList ID="ddlIsApproved" runat="server">
         </asp:DropDownList>
         <asp:Label ID="lblIsApproved" runat="server" ForeColor="Red" Text="Label"></asp:Label></td>
     </tr>

     <tr>     
        <td nowrap="nowrap">Is Locked Out?</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td><asp:DropDownList ID="ddlIsLockedOut" runat="server">
         </asp:DropDownList>
         <asp:Label ID="lblIsLockedOutVerify" runat="server" ForeColor="Red" 
                Text="Label"></asp:Label>
                </td>
     </tr>



     <tr style="background-color: #F4F4F4">
     <td nowrap="nowrap" class="style1">Priority</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
     <td class="style1"><asp:DropDownList ID="ddlPriority" runat="server"></asp:DropDownList></td>
     </tr>

     
     <tr>
     <td nowrap="nowrap">Role Area</td>
      <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
     <td><asp:DropDownList ID="ddlRoleArea" runat="server" style="margin-bottom: 0px" 
             AutoPostBack="True" onselectedindexchanged="ddlRoleArea_SelectedIndexChanged"></asp:DropDownList><asp:Label ID="City_Verify_label" runat="server" ForeColor="Red" Text="Label"></asp:Label></td>
     </tr>
     <tr style="background-color: #F4F4F4">
            <td nowrap="nowrap">Role Name</td>
             <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
            <td nowrap="nowrap">
                <asp:CheckBoxList ID="cblRoleName" runat="server" 
                    BorderColor="#CCCCCC" BorderStyle="Solid" 
                    AutoPostBack="True" TextAlign="Left" 
                    onselectedindexchanged="cblRoleName_SelectedIndexChanged"></asp:CheckBoxList>
                    </td>
                    <td>
                    <asp:Label ID="lblRoleNameVerify" runat="server" ForeColor="Red" Text="Label"></asp:Label>
                    </td>
     </tr>      

     <tr>
     <td>SMERC ID</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
     <td style="color: #808080; font-weight: normal">
         <asp:TextBox ID="tbSMERCID" Width="160px" 
             runat="server" style="margin-bottom: 0px"></asp:TextBox>
         <asp:Label ID="LBL_SMERCID_Verify" runat="server" 
             ForeColor="Red" Text="Label"></asp:Label>
         &nbsp;If not specified, the username will be used for the SMERCID.</td></tr>
     <tr style="background-color: #F4F4F4">
     <td>Smart Phone OS</td>
     <td></td>
     <td><asp:DropDownList ID="ddlSmartPhoneOS" runat="server"></asp:DropDownList></td></tr>
     </tr>
     <tr>
     <td>Phone Service Carrier</td>
     <td></td>
     <td><asp:DropDownList ID="ddlPhoneServiceCarrier" runat="server"></asp:DropDownList></td></tr>
     <tr style="background-color: #F4F4F4">
     <td>Smart Phone Model No</td>
    
     <td></td>
     <td>
         <asp:TextBox ID="tb_SmartPhoneModelNo" Width="160px" runat="server"></asp:TextBox>
         </td></tr>
    
    <tr>
        <td>RTMC User Account Type</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:DropDownList ID="ddlRTMCUserAccountType" runat="server" style="margin-bottom: 0px" >
            </asp:DropDownList> <asp:Label ID="lblRTMCUserAccountTypeVerify" runat="server" ForeColor="Red" 
                    Text="Label"></asp:Label>
        </td>
    </tr>

    <tr style="background-color: #F4F4F4">
        <td>RTMC User Account Expiration Window</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:DropDownList ID="ddlRTMCUserAccountExpirationWindow" runat="server" style="margin-bottom: 0px" >
            </asp:DropDownList> <asp:Label ID="lblRTMCUserAccountExpirationWindow" runat="server" ForeColor="Red" 
                    Text="Label"></asp:Label>
        </td>
    </tr>

     <tr>
        <td>EV User Account Type</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:DropDownList ID="ddlEVUserAccountType" runat="server" style="margin-bottom: 0px" >
            </asp:DropDownList><asp:Label ID="lblEVUserAccountType" runat="server" ForeColor="Red" 
                    Text="Label"></asp:Label>
        </td>
    </tr>

    <tr style="background-color: #F4F4F4">
        <td>EV User Account Expiration Window</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:DropDownList ID="ddlEVUserAccountExpirationWindow" runat="server" style="margin-bottom: 0px" >
            </asp:DropDownList><asp:Label ID="lblEVUserAccountExpirationWindow" runat="server" ForeColor="Red" 
                    Text="Label"></asp:Label>
        </td>
    </tr>

    <tr>
        <td>RTMC Report Intervals</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:DropDownList ID="ddlRTMCReportIntervals" runat="server" style="margin-bottom: 0px" >
            </asp:DropDownList>
        </td>
    </tr>

    <tr style="background-color: #F4F4F4">
        <td>RTMC Chart And Report Type</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:DropDownList ID="ddlRTMCChartAndReport" runat="server" style="margin-bottom: 0px">
            </asp:DropDownList><asp:Label ID="lblRTMCChartAndReport" runat="server" ForeColor="Red" 
                    Text="Label"></asp:Label>
        </td>
    </tr>

     <tr>
        <td>Charging Points</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:TextBox ID="tbChargingPoints" Width="160px" runat="server" style="margin-bottom: 0px" Text="0"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="tbChargingPoints" 
            ErrorMessage="Charging Points is required."  ForeColor="Red"></asp:RequiredFieldValidator>                             &nbsp;<asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server"    
            ErrorMessage="Charging Points is integer. Please fix"
            ControlToValidate="tbChargingPoints"
            ValidationExpression="^\d+$" ForeColor="Red" />
        </td>
    </tr>

    <tr style="background-color: #F4F4F4">
        <td>EVUser Session Timeout</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:TextBox ID="tbEVUserSessionTimeout" Width="160px" runat="server" style="margin-bottom: 0px" Text="1"></asp:TextBox>
            <asp:RequiredFieldValidator ID="vldrEVUserSessionTimeout" runat="server" ControlToValidate="tbEVUserSessionTimeout" 
            ErrorMessage="EVUserSessionTimeout is required."  ForeColor="Red"></asp:RequiredFieldValidator>                             &nbsp;<asp:RegularExpressionValidator ID="reEVUserSessionTimeout" runat="server"    
            ErrorMessage="EVUserSessionTimeout is integer. Please fix"
            ControlToValidate="tbEVUserSessionTimeout"
            ValidationExpression="^\d+$" ForeColor="Red" />
        </td>
    </tr>

     <tr>
        <td>RTMC Session Timeout</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:TextBox ID="tbRTMCSessionTimeout" Width="160px" runat="server" style="margin-bottom: 0px" Text="1"></asp:TextBox>
            <asp:RequiredFieldValidator ID="vldrRTMCSessionTimeout" runat="server" ControlToValidate="tbRTMCSessionTimeout" 
            ErrorMessage="RTMCSessionTimeout is required."  ForeColor="Red"></asp:RequiredFieldValidator>                             &nbsp;<asp:RegularExpressionValidator ID="reRTMCSessionTimeout" runat="server"    
            ErrorMessage="RTMCSessionTimeout is integer. Please fix"
            ControlToValidate="tbRTMCSessionTimeout"
            ValidationExpression="^\d+$" ForeColor="Red" />
        </td>
    </tr>

    <tr style="background-color: #F4F4F4">
        <td>Maximum Vehicles</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1"> </td>
        <td>
            <asp:DropDownList ID="ddlMaxVehicles" runat="server" style="margin-bottom: 0px">
            </asp:DropDownList>
        </td>
    </tr>
               
    <tr>
        <td>Allow Text Message</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1"> </td>
        <td>
            <asp:DropDownList ID="ddlAllowTextMsg" runat="server" style="margin-bottom: 0px" >
            <asp:ListItem Value="0">False</asp:ListItem>
            <asp:ListItem Value="1">True</asp:ListItem>        
            </asp:DropDownList>
        </td>
    </tr>
    
    <tr style="background-color: #F4F4F4">
        <td>EV List</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1"> </td>
        <td>
            <div style ="height:100px; overflow: auto;">
            <asp:GridView ID="GvEvList" runat="server" CssClass="datatable" 
                        GridLines="Vertical" OnRowCreated="GvEvListRowCreated"
                        onselectedindexchanged="GvEvListSelectedIndexChanged"
                        OnRowDataBound="GvEvListDataBound"
                        CellPadding="0" CellSpacing="0" BorderWidth="1" AutoGenerateColumns="False"
                        DataKeyNames="ID">
                <AlternatingRowStyle Wrap="False" />
                <Columns>
                    <asp:BoundField DataField="ID" HeaderText="ID"/> 
                    <asp:BoundField DataField="EvModelID" HeaderText="EvModelID"/>
                    <asp:BoundField DataField="Number" HeaderText="No."/>
                    <asp:BoundField DataField="EvName" HeaderText="EV Name"/> 
                    <asp:BoundField DataField="Nickname" HeaderText="Nickname"/>
                </Columns>
                <PagerSettings Position="TopAndBottom" />
                <HeaderStyle Wrap="False" />
                <RowStyle CssClass="row" Wrap="False" />
                <SelectedRowStyle BackColor="#38CDB9" Font-Bold="True" ForeColor="White" Wrap="False" /> 
            </asp:GridView>
                </div>
            <br/>
            <asp:DropDownList ID="ddlGvEvListModel" runat="server" Visible="false" style="margin-bottom: 0px"></asp:DropDownList>
            <asp:TextBox ID="tbNickname" runat="server" Visible="false" style="margin-bottom: 0px"></asp:TextBox>
            <asp:Button ID="btnGvEvListAdd" runat="server" Text="add" visible = "false" onclick="btnGvEvListAddClick" Font-Size="Small" />               
            <asp:Button ID="btnGvEvListModify" runat="server" Font-Size="Small" Text="modify" 
                Visible="false" onclick="btnGvEvListModifyClick"/>    
            <asp:Button ID="btnGvEvListDelete" runat="server" Font-Size="Small" Text="delete" 
                Visible="false" onclick="btnGvEvListDeleteClick" CausesValidation="False"/>
        
        </td>
    </tr>
        
    <tr>
        <td>Monitor Refresh Interval</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1"> </td>
        <td>
            <asp:DropDownList ID="ddlMonitorRefreshInterval" Width="160px" runat="server">
                <asp:ListItem>1</asp:ListItem><asp:ListItem>2</asp:ListItem><asp:ListItem>3</asp:ListItem><asp:ListItem>4</asp:ListItem><asp:ListItem>5</asp:ListItem><asp:ListItem>6</asp:ListItem><asp:ListItem>7</asp:ListItem><asp:ListItem>8</asp:ListItem><asp:ListItem>9</asp:ListItem><asp:ListItem>10</asp:ListItem><asp:ListItem>15</asp:ListItem><asp:ListItem>20</asp:ListItem><asp:ListItem>25</asp:ListItem><asp:ListItem>30</asp:ListItem><asp:ListItem>35</asp:ListItem><asp:ListItem>40</asp:ListItem><asp:ListItem>45</asp:ListItem><asp:ListItem>50</asp:ListItem><asp:ListItem>55</asp:ListItem><asp:ListItem>60</asp:ListItem><asp:ListItem>90</asp:ListItem><asp:ListItem>120</asp:ListItem><asp:ListItem>180</asp:ListItem><asp:ListItem>300</asp:ListItem><asp:ListItem>360</asp:ListItem><asp:ListItem>420</asp:ListItem><asp:ListItem>480</asp:ListItem><asp:ListItem>540</asp:ListItem><asp:ListItem>600</asp:ListItem>

            </asp:DropDownList>
        </td>
    </tr>
<%--        
    <tr style="background-color: #F4F4F4">
        <td>Nickname</td>
        <td></td>
        <td>
            <asp:TextBox runat="server" ID ="tbNickname"></asp:TextBox>
        </td>
    </tr>--%>

<%--    <tr style="background-color: #F4F4F4">
        <td>test</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td>
            <asp:TextBox ID="tb" Width="160px" runat="server" style="margin-bottom: 0px"></asp:TextBox>
        </td>
    </tr>--%>
            
             </table>

    
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>




            <div class="grid">
            <div class="rounded">
            <div class="top-outer"><div class="top-inner"><div class="top">
            <h2>User Billing Information</h2></div></div></div>
            <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 

    <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">First Name</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td><asp:TextBox ID="tbFirstName" Width="160px" runat="server"></asp:TextBox>
                             <asp:RequiredFieldValidator ID="rflFN" runat="server" ControlToValidate="tbFirstName" 
                             ErrorMessage="First Name is required."   ForeColor="Red">First Name is required.</asp:RequiredFieldValidator>
                             </td> 
                             
     </tr>

     <tr>
        <td nowrap="nowrap">Last Name</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td><asp:TextBox ID="tbLastName" Width="160px" runat="server"></asp:TextBox>
        <asp:RequiredFieldValidator ID="rflLN" runat="server" ControlToValidate="tbLastName" 
                             ErrorMessage="Last Name is required."  ForeColor="Red">Last Name is required.</asp:RequiredFieldValidator>
                             </td>
     </tr>

     <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap">Phone Number</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
        <td><asp:TextBox ID="tbPhoneNumber" Width="160px" MaxLength="20" runat="server"></asp:TextBox>
            Example format: 310-267-6979&nbsp;<asp:RequiredFieldValidator ID="rflPhoneNo" runat="server" ControlToValidate="tbPhoneNumber" 
                             ErrorMessage="Phone Number is required."  ForeColor="Red">Phone Number is required.</asp:RequiredFieldValidator>
                             &nbsp;<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server"
           ControlToValidate="tbPhoneNumber"
           ErrorMessage="Error Format.  See Left."
           ValidationExpression="^[2-9]\d{2}-\d{3}-\d{4}$" ForeColor="Red" />
                             </td>     
     </tr>

     
     <tr>
     <td nowrap="nowrap">Address1</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
     <td> <asp:TextBox ID="tbAddress1" Width="160px" runat="server"></asp:TextBox>
     <asp:RequiredFieldValidator ID="rflAddress1" runat="server" ControlToValidate="tbAddress1" 
                             ErrorMessage="Address is required."  ForeColor="Red">Address is required.</asp:RequiredFieldValidator>
                             </td>
     </tr>

     <tr style="background-color: #F4F4F4">
     <td nowrap="nowrap">Address2</td>
     <td></td>
     <td><asp:TextBox ID="tbAddress2" Width="160px" runat="server"></asp:TextBox></td>
     </tr>

     <tr>
     <td nowrap="nowrap" class="style1">Zip Code</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
     <td class="style1"><asp:TextBox ID="tbZipCode" Width="160px" runat="server"></asp:TextBox>  
        <asp:RequiredFieldValidator ID="rflZipCode" runat="server" ControlToValidate="tbZipCode" 
                             ErrorMessage="Zip Code is required."  ForeColor="Red">Zip Code is required.</asp:RequiredFieldValidator>                             
         &nbsp;<asp:RegularExpressionValidator ID="RegExp1" runat="server"    
        ErrorMessage="Zipcode text error.  Please fix"
        ControlToValidate="tbZipCode"    
        ValidationExpression="^[a-zA-Z0-9'@&#.\s]{5,10}$" ForeColor="Red" />
                             </td>
     </tr>

     <tr style="background-color: #F4F4F4">
     <td>City</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
     <td>
         <asp:TextBox ID="tbCity" runat="server" Width="160px"></asp:TextBox>
         &nbsp;<asp:Label ID="lblCityVerify" runat="server" 
             ForeColor="Red" Text="Label"></asp:Label>
         </td>
     </tr>

     <tr>
     <td>State</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium" class="style1">*</td>
     <td>
         <asp:DropDownList ID="ddlUserState" runat="server">
         </asp:DropDownList>
         <asp:Label ID="lblUserStateVerify" runat="server" 
             ForeColor="Red" Text="Label"></asp:Label>
         </td>
     </tr>
    </table>


    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>
       <asp:Button ID="btnUpdate" runat="server" Text="Update user?" visible = "false" onclick="btnUpdate_Click" Font-Size="Large" />               
     <asp:Button ID="btnNewUser" runat="server" Font-Size="Large" Text="Create New User?" onclick="btnNewUser_Click"/>     
     <asp:Button ID="btnClearAll" runat="server" Font-Size="Large" Text="Clear All" onclick="btnClearAll_Click" CausesValidation="False"/>
    </td>
    </tr>
    </table>                    
                      
        </asp:Content>