<%@ Register TagPrefix="asp" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit"%>
<%@ Page Title = "Email Form" MasterPageFile="~/Site.master" Language="C#" AutoEventWireup="true" CodeBehind="EmailForm.aspx.cs" Inherits="EVEditor.EmailForm" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit.HTMLEditor" TagPrefix="cc1" %>



<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        .style1
        {
            height: 26px;
        }
    </style>
    </asp:Content>
        
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <%--Needed for HTML Editor--%>
    <asp:ToolkitScriptManager ID="ToolkitScriptManager1"
        runat="server">
    </asp:ToolkitScriptManager>  
      

    <%--Start the page--%>
    <table> 
       <tr>
           <td style="font-size: medium; font-weight: bold;" class="style1"> Select By Organization:
           </td>      
                
           <td class="style1"><asp:DropDownList ID="ddlSelectByCity" runat="server" AutoPostBack="True" style="height: 22px" onselectedindexchanged="ddlSelectByCity_SelectedIndexChanged"></asp:DropDownList>
           </td> 

           <td style="font-size: medium; font-weight: bold;" class="style1"> 
                       <asp:CheckBox ID="cbShowActivated" runat="server" AutoPostBack="True" oncheckedchanged="cbShowActivated_CheckedChanged" />Only Activated Users
           </td>

      </tr>

       <tr>
           <td>&nbsp;</td>
           <td></td>
           <td>
               <asp:Label ID="lblTest2" runat="server" Text=""></asp:Label>
               <asp:Label ID="lblTest" runat="server" Text=""></asp:Label>
               <asp:Label ID="lblTotalUsers" runat="server" Font-Bold="True" Font-Size="Medium" Text=""></asp:Label>
           </td>

       </tr>

       <tr>
           <td>
               <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
               </td>
           <td></td>
           <td>  
               <asp:Label ID="lblErrorMessage" runat="server" ForeColor="Red" Visible="False"></asp:Label> 
               <asp:Button ID="btnHideError" runat="server" Text="Hide" CausesValidation="False" onclick="btnHideError_Click" Visible="False" />
               <asp:Label ID="lblTest3" runat="server" Text=""></asp:Label>
           </td>
        </tr>
        
        <tr>
            <td style="font-size: medium; font-weight: bold;" class="style1" align="right"> Search By: </td> 
            <td><asp:DropDownList ID="ddlSearchKeywords" runat="server" Width="295px">
                <asp:ListItem Value="emailAddress">Email Address</asp:ListItem>
                <asp:ListItem Value="userName">User Name</asp:ListItem>
                <asp:ListItem Value="firstName">First Name</asp:ListItem>
                <asp:ListItem Value="lastName">Last Name</asp:ListItem>
                <asp:ListItem Value="roleName">Role Name</asp:ListItem>
              
            </asp:DropDownList></td> 
            <td><asp:TextBox runat="server" ID="tbSearchKeywords" Width="170px"></asp:TextBox></td>
            <td><asp:Button runat="server" ID="btnSearch" Text="Search" onclick="btnSearchClick" CausesValidation="False"/></td>
            <td><asp:Button runat="server" ID="btnSearchClear" Text="Clear" onclick="btnSearchClearClick" CausesValidation="False"/></td>
        </tr>
    </table>

    <%--Start Second Table of User List, gridviews--%>

        <table>
            <tr>
                <td valign="top">
                    <%--Start table to split the gridview and column--%>
                    <table>
                        <tr>
                            <td valign=top>
                                <%--Start lefthand Column--%>
                                <div class="grid">
                                    <div class="rounded">
                                        <div class="top-outer"><div class="top-inner"><div class="top">
                                            <h2>User List</h2></div></div></div>
                                        <div class="mid-outer"><div class="mid-inner"><div class="mid">

                                                <asp:GridView ID="gvUserList" runat="server" CssClass="datatable" GridLines="Vertical" DataKeyNames= "UserId" OnPageIndexChanging="gvUserList_Paging" OnRowCreated="gvUserList_RowCreated"
                                                OnSorting="gvUserList_Sorting" AllowSorting="True" AllowPaging="True" PageSize="25" CellPadding="0" CellSpacing="0" BorderWidth="0" AutoGenerateColumns="False">
                                                <AlternatingRowStyle Wrap="False" />
                                                <Columns>
                                                <asp:TemplateField HeaderText="Select">
                                                <ItemTemplate><asp:CheckBox ID="cbSelected"  runat="server" AutoPostBack="true"/></ItemTemplate>
                                                </asp:TemplateField>
                                                    <asp:BoundField DataField="UserName"
                                                        SortExpression="UserName" HeaderText="User Name" 
                                                        ApplyFormatInEditMode="False" FooterStyle-Wrap="False" >
                                        <FooterStyle Wrap="False"></FooterStyle>
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Email" SortExpression="Email" HeaderText="Email"/>
                                                    <asp:BoundField DataField="FirstName" SortExpression="FirstName" HeaderText="FirstName"/>
                                                    <asp:BoundField DataField="LastName" SortExpression="LastName" HeaderText="LastName"/>
                                        <%--            <asp:BoundField DataField="RoleArea" SortExpression="RoleArea" HeaderText="Role Area"/>--%>
                                                    <asp:BoundField DataField="RoleName" SortExpression="RoleName" HeaderText="Role Name"/>
                                                    <asp:BoundField DataField="EVName" SortExpression="EVName" HeaderText="EV"/>            
                                                    <asp:BoundField DataField="IsApproved" SortExpression="IsApproved" HeaderText="Approved"/>
                                                    <asp:BoundField DataField="IsLockedOut" SortExpression="IsLockedOut" HeaderText="Locked Out"/>
                                                    <asp:BoundField DataField="Activate" SortExpression="Activate" HeaderText="Activated"/>
                                                </Columns>
                                                <PagerSettings Position="TopAndBottom" />
                                                <HeaderStyle Wrap="False" />
                                                <PagerStyle CssClass="pager-row" />
                                                <RowStyle CssClass="row" Wrap="False" />
                                                <SelectedRowStyle BackColor="#38CDB9" Font-Bold="True" ForeColor="White" Wrap="False" /> 
                                                    <SortedAscendingCellStyle BackColor="#FFCC66" Wrap="False" />
                                                    <SortedDescendingCellStyle BackColor="#FFCC66" Wrap="False" />
                                            </asp:GridView> 
                                       </div></div></div> <%-- End mid div x 3--%>
                                    <div class="bottom-outer"><div class="bottom-inner">
                                    <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
                                </div> <%-- end div round--%>
                               </div> <%-- end div grid --%>

                     <asp:Button ID="btnCheckAll" runat="server" Text="Check all" 
                            onclick="btnCheckAll_Click" />        
                     <asp:Button ID="btnUncheckAll" runat="server" Text="Clear All" 
                            onclick="btnUncheckAll_Click"/>
                    </td>
                        <td valign="top">
                         <div class="grid">
                            <div class="rounded">
                            <div class="top-outer"><div class="top-inner"><div class="top">
                            <h2>Email Message</h2></div></div></div>
                            <div class="mid-outer"><div class="mid-inner"><div class="mid">
                
                    <table style="vertical-align:top"> 
                    <tr>
                    <td>
                        <asp:Label ID="lblFromEmail" runat="server" Text="From:" ToolTip="Read Only"></asp:Label>
                        </td>
                    <td>
                        <asp:DropDownList ID="ddlFromEmail" runat="server" 
                            AutoPostBack="True" 
                            onselectedindexchanged="ddlFromEmail_SelectedIndexChanged">
                        </asp:DropDownList> 
                        </td>
                    </tr>
                    <tr>
                        <td>To:</td>
                        <td><asp:TextBox ID="tbSendToEmail" runat="server" Height="110px" Width="800px" TextMode="MultiLine" ReadOnly="True"></asp:TextBox></td>
                    </tr>

                    <tr>
                        <td>Subject:</td>
                        <td><asp:TextBox ID="tbSubject" runat="server" Height="25px" Width="800px"></asp:TextBox></td>
                    </tr>

                    <tr>
                        <td>Message:</td>
                        <td>       
                        <cc1:Editor ID="tbUpdatedTextbox" runat="server" Height="900px" Width="1000px" />
                        </td>
                    </tr>


                    </table>


                    </div></div></div> <%-- End mid div x 3--%>

                    <div class="bottom-outer"><div class="bottom-inner">
                            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
                       </div> <%-- end div round--%>
                       </div> <%-- end div grid --%>

                       <asp:Button ID="btnSendEmail" runat="server" Text="Send Email" 
                                Font-Size="Large" onclick="btnSendEmail_Click" />     
                       <asp:Button ID="btnClearEmailFields" runat="server" Text="Clear Fields" 
                                Font-Size="Large" onclick="btnClearEmailFields_Click" />  
      

                    </td>
                    </tr>
                </table>
                          &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            </td>


    
        </tr>
    </table>                    
                      
        </asp:Content>