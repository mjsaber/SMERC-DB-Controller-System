<%@ Page Title="EVSmartPlug" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Feedback.aspx.cs" Inherits="RTMC.Account.Feedback" EnableEventValidation="false"%>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        .button_green{
        border:0px solid #5c7a2d; -webkit-border-radius: 10px; -moz-border-radius: 10px;border-radius: 10px;width:auto;font-size:48px;font-family:arial, helvetica, sans-serif; padding: 15px 15px 15px 15px; font-weight:bold; text-align: center; color: #FFFFFF; background-color: #7BA33C;
        background: #7ba33c; /* Old browsers */
        background: -moz-linear-gradient(top,  #7ba33c 0%, #7ba33c 50%, #6a8936 51%, #7ba23c 100%); /* FF3.6+ */
        background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#7ba33c), color-stop(50%,#7ba33c), color-stop(51%,#6a8936), color-stop(100%,#7ba23c)); /* Chrome,Safari4+ */
        background: -webkit-linear-gradient(top,  #7ba33c 0%,#7ba33c 50%,#6a8936 51%,#7ba23c 100%); /* Chrome10+,Safari5.1+ */
        background: -o-linear-gradient(top,  #7ba33c 0%,#7ba33c 50%,#6a8936 51%,#7ba23c 100%); /* Opera 11.10+ */
        background: -ms-linear-gradient(top,  #7ba33c 0%,#7ba33c 50%,#6a8936 51%,#7ba23c 100%); /* IE10+ */
        background: linear-gradient(to bottom,  #7ba33c 0%,#7ba33c 50%,#6a8936 51%,#7ba23c 100%); /* W3C */
        filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#7ba33c', endColorstr='#7ba23c',GradientType=0 ); /* IE6-9 */
        }

        .button_green:hover{
        border:0px solid #5c7a2d; -webkit-border-radius: 10px; -moz-border-radius: 10px;border-radius: 10px;width:auto;font-size:48px;font-family:arial, helvetica, sans-serif; padding: 15px 15px 15px 15px; font-weight:bold; text-align: center; color: #FFFFFF; background-color: #7BA33C;
        background: #7ba23c; /* Old browsers */
        background: -moz-linear-gradient(top,  #7ba23c 0%, #6a8936 50%, #7ba33c 51%, #7ba33c 100%); /* FF3.6+ */
        background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#7ba23c), color-stop(50%,#6a8936), color-stop(51%,#7ba33c), color-stop(100%,#7ba33c)); /* Chrome,Safari4+ */
        background: -webkit-linear-gradient(top,  #7ba23c 0%,#6a8936 50%,#7ba33c 51%,#7ba33c 100%); /* Chrome10+,Safari5.1+ */
        background: -o-linear-gradient(top,  #7ba23c 0%,#6a8936 50%,#7ba33c 51%,#7ba33c 100%); /* Opera 11.10+ */
        background: -ms-linear-gradient(top,  #7ba23c 0%,#6a8936 50%,#7ba33c 51%,#7ba33c 100%); /* IE10+ */
        background: linear-gradient(to bottom,  #7ba23c 0%,#6a8936 50%,#7ba33c 51%,#7ba33c 100%); /* W3C */
        filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#7ba23c', endColorstr='#7ba33c',GradientType=0 ); /* IE6-9 */
        }


        .button_green:disabled{
        border:0px solid #5c7a2d; -webkit-border-radius: 10px; -moz-border-radius: 10px;border-radius: 10px;width:auto;font-size:48px;font-family:arial, helvetica, sans-serif; padding: 15px 15px 15px 15px; font-weight:bold; text-align: center; color: #FFFFFF; background-color: #7BA33C;
        background: #e6e6e6; /* Old browsers */
        background: -moz-linear-gradient(top,  #e6e6e6 0%, #e6e6e6 50%, #c0bfbf 51%, #e4e4e4 100%); /* FF3.6+ */
        background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#e6e6e6), color-stop(50%,#e6e6e6), color-stop(51%,#c0bfbf), color-stop(100%,#e4e4e4)); /* Chrome,Safari4+ */
        background: -webkit-linear-gradient(top,  #e6e6e6 0%,#e6e6e6 50%,#c0bfbf 51%,#e4e4e4 100%); /* Chrome10+,Safari5.1+ */
        background: -o-linear-gradient(top,  #e6e6e6 0%,#e6e6e6 50%,#c0bfbf 51%,#e4e4e4 100%); /* Opera 11.10+ */
        background: -ms-linear-gradient(top,  #e6e6e6 0%,#e6e6e6 50%,#c0bfbf 51%,#e4e4e4 100%); /* IE10+ */
        background: linear-gradient(to bottom,  #e6e6e6 0%,#e6e6e6 50%,#c0bfbf 51%,#e4e4e4 100%); /* W3C */
        filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#e6e6e6', endColorstr='#e4e4e4',GradientType=0 ); /* IE6-9 */
        }  
               
        .button_o
        {
            border:0px solid #5c7a2d; -webkit-border-radius: 75px; -moz-border-radius: 75px;border-radius: 75px;width:auto;font-size:48px;font-family:arial, helvetica, sans-serif; padding: 15px 15px 15px 15px; font-weight:bold; text-align: center; color: #FFFFFF; background-color: #f59630;
            background: #f59630; /* Old browsers */
        }
        .button_o:disabled{
        border:0px solid #5c7a2d; -webkit-border-radius: 75px; -moz-border-radius: 75px;border-radius: 75px;width:auto;font-size:48px;font-family:arial, helvetica, sans-serif; padding: 15px 15px 15px 15px; font-weight:bold; text-align: center; color: #FFFFFF; background-color: #7BA33C;
        background: #e6e6e6; /* Old browsers */
        }
        .style5
        {
            width: 758px;
        }
        .result2	
        {
	        font-size:1em;
	        color:#999;
        }
        .feedbackListText {
            font-size: 20px;
            text-align: center;
        }
        .RowStyle {
          height: 100px;
        }
        .AlternateRowStyle {
          height: 100px;
        }
        </style>
 
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

    <h2>
        <asp:Image ID="Image1" runat="server" 
            ImageUrl="~/Images/icon_page_feedback.png" />
    </h2>
<!--    <p style="font-family: Arial; font-size: large; font-weight: bold; color: #C0C0C0" 
        align="center">
        In construction</p>-->
    <table align="center" style="font-size: xx-large; font-weight: bolder" width="100%">
    <tr>
    <td style="font-size:20pt">
    <asp:Label ID="lblErrorMsg" runat="server" ForeColor="Red"/>
    </td></tr>
    <tr>
        <td align="center" class="style5" style="font-size: 50%; font-weight: bolder">
            <asp:HiddenField ID="hfCurrentRowIndex" runat="server"></asp:HiddenField>
            <asp:HiddenField ID="hfParentContainer" runat="server"></asp:HiddenField>   
            <asp:GridView ID="gvFeedbackList" runat="server" 
                AlternatingRowStyle-BackColor="#CCCCCC" AutoGenerateColumns="False" 
                HeaderStyle-HorizontalAlign="Left" HeaderStyle-BackColor="Black" 
                HeaderStyle-ForeColor="White" Width="100%"
                onrowcreated ="FeedbackListRowCreated"
                onselectedindexchanged="FeedbackListSelectedChanged"
                OnRowDataBound="FeedbackListDataBound" 
                DataKeyNames="ID"               
                >
                <RowStyle Height="100px" />
                <AlternatingRowStyle Height="100px" BackColor="#CCCCCC"></AlternatingRowStyle>
                <Columns>
                    <asp:BoundField HeaderText="ID" DataField="ID" SortExpression="ID" />
                    <asp:BoundField HeaderText="Date" DataField="Timestamp" 
                        SortExpression="Date" ItemStyle-CssClass="stationlistbig"
                        DataFormatString="{0:MM/dd/yyyy HH:MM}" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="10%" ItemStyle-Wrap="false"
                        >
                    </asp:BoundField>
                    <asp:TemplateField HeaderText="Fix" 
                        SortExpression="Closed" ItemStyle-CssClass="stationlistbig" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate><%# (Boolean.Parse(Eval("Closed").ToString())) ? "Y" : "N" %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="MSG" DataField="ResponseNo" SortExpression="ResponseNo"  
                        ItemStyle-CssClass="stationlistbig" ItemStyle-HorizontalAlign="Center" >
                    </asp:BoundField>
                    <asp:BoundField HeaderText="Feedback" DataField="Feedback" 
                        SortExpression="Feedback" ItemStyle-Font-Size="20pt"  ItemStyle-Wrap="true" ItemStyle-ForeColor="#999">
                    </asp:BoundField>
                </Columns>

                <HeaderStyle HorizontalAlign="Left" BackColor="#0E784A" ForeColor="White" Font-Size="XX-Large"></HeaderStyle>
            </asp:GridView>                
        </td>
    </tr>
    </table>
    <asp:Table ID = "ResponseTable" runat="server" >
    </asp:Table>
    <table align="center" style="font-size: xx-large; font-weight: bolder" width="100%">
    <tr>
    <td style="font-size:20pt">
    Maximum 2000 characters allowed.
    </td></tr>
    <tr>
    <td align="center" class="style5" style="font-size: 50%; font-weight: bolder">
    <asp:TextBox ID="tbFeedback" runat="server" TextMode="MultiLine" width="100%" 
            Height="400px" font-size="20pt" />
    </td>
    </tr>
    <tr>
        <td align="center" class="style5" style="font-size: 130%; font-weight: bolder">
            <asp:Button ID="btnSubmitResponse" runat="server" Text="Response" 
                 Font-Bold="True" Font-Size="130%"  
                CssClass="button_green" onclick="btnSubmitResponse_Click" Visible="false"/>
            <asp:Button ID="btnSubmit" runat="server" Text="Feedback" 
                Font-Bold="True" Font-Size="130%"  
                CssClass="button_green" onclick="btnSubmit_Click"/>
            
        </td>
    </tr>
    </table>
</asp:Content>
