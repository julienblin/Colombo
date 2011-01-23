<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        <%: ViewData["Message"] %></h2>
    <p>
        This behavior will be submitted via an AJAX call directly to the message bus via
        the ClientRestService:

        <form action="/Colombo.svc/HelloWorld" data-remote="true">
            <label for="name">What's your name?</label>
            <br />
            <input type="text" name="name" />
            <br />
            <input type="submit" value="Submit" />
        </form>
    </p>
    <p id="result">
    </p>
    <script type="text/javascript">
        $('form').bind('ajax:success', function (e, data, status, xhr) {
            if (data.ValidationResults.length > 0) {
                alert(data.ValidationResults[0].ErrorMessage);
            } else {
                $('#result').html(data.Message);
            }
        });
    </script>
</asp:Content>
