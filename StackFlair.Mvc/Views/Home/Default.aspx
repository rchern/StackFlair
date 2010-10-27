<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>StackFlair</title>
    <script src="../../Scripts/jquery-1.4.2.min.js" type="text/javascript"></script>
    <script src="../../Scripts/jquery-ui-1.8.5.custom.min.js" type="text/javascript"></script>
    <script src="../../Scripts/Soapi.min.js" type="text/javascript"></script>
    <link href="../../Content/reset.css" rel="stylesheet" type="text/css" />
    <link href="../../Content/redmond/jquery-ui-1.8.5.custom.css" rel="stylesheet" type="text/css" />
    <link href="../../Content/StackFlair.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        $(function () {
            $("#themes").click(function () {
                $(this).find('[name="themes"]:checked').parent('li').addClass('selected').siblings().removeClass('selected');
            });
            $(".buttonset").buttonset();
            $(":submit").button();

            $.widget("ui.combobox", {
                _create: function () {
                    var self = this,
					select = this.element.hide(),
					selected = select.children(":selected"),
					value = selected.val() ? selected.text() : "";
                    var input = $("<input>")
					.insertAfter(select)
					.val(value)
					.autocomplete({
					    delay: 0,
					    minLength: 0,
					    source: function (request, response) {
					        var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
					        response(select.children("option").map(function () {
					            var text = $(this).text();
					            var value = $(this).val();
					            if (this.value && (!request.term || matcher.test(text)))
					                return {
					                    label: text.replace(
											new RegExp(
												"(?![^&;]+;)(?!<[^<>]*)(" +
												$.ui.autocomplete.escapeRegex(request.term) +
												")(?![^<>]*>)(?![^&;]+;)", "gi"
											), "<strong>$1</strong>"),
					                    value: value,
					                    plaintext: text,
					                    option: this
					                };
					        }));
					    },
					    focus: function (event, ui) {
					        $(this).val(ui.item.plaintext);
					        return false;
					    },
					    select: function (event, ui) {
					        $(this).val(ui.item.plaintext);
					        ui.item.option.selected = true;
					        self._trigger("selected", event, {
					            item: ui.item.option
					        });
					        return false;
					    },
					    change: function (event, ui) {
					        if (!ui.item) {
					            var matcher = new RegExp("^" + $.ui.autocomplete.escapeRegex($(this).val()) + "$", "i"),
									valid = false;
					            select.children("option").each(function () {
					                if (this.value.match(matcher)) {
					                    this.selected = valid = true;
					                    return false;
					                }
					            });
					            if (!valid) {
					                // remove invalid value, as it didn't match anything
					                $(this).val("");
					                select.val("");
					                return false;
					            }
					        }
					    }
					})
					.addClass("ui-widget ui-widget-content ui-corner-left");

                    input.data("autocomplete")._renderItem = function (ul, item) {
                        return $("<li></li>")
						.data("item.autocomplete", item)
						.append("<a><img src='" + item.value + "/favicon.ico' />" + item.label + "</a>")
						.appendTo(ul);
                    };

                    $("<button type='button'>&nbsp;</button>")
					.attr("tabIndex", -1)
					.attr("title", "Show All Items")
					.insertAfter(input)
					.button({
					    icons: {
					        primary: "ui-icon-triangle-1-s"
					    },
					    text: false
					})
					.removeClass("ui-corner-all")
					.addClass("ui-corner-right ui-button-icon")
					.click(function () {
					    // close if already visible
					    if (input.autocomplete("widget").is(":visible")) {
					        input.autocomplete("close");
					        return;
					    }

					    // pass empty string as value to search for, displaying all results
					    input.autocomplete("search", "");
					    input.focus();
					});
                }
            });


            $("#user").autocomplete({
                minLength: 0,
                source: function (request, response) {
                    var route = Soapi.RouteFactory($("#sites").val().replace("http://", "http://api."), "VkUqga2oSkipyf-l9fi7sw").Users();
                    route.filter = $("#user").val();
                    route.pagesize = 50;
                    route.sort = "name";
                    route.order = "asc";
                    route.min = route.filter;
                    route.page = 1;

                    var users = [];
                    route.getResponse(function (data) {
                        // aggregate the users
                        $.each(data.items, function (i, item) {
                            var user = { "label": item.display_name, "value": item.user_id, "rep": item.reputation, "email_hash": item.email_hash };
                            users.push(user);
                        });
                        //console.log(users); // aggregate the results from this site
                        console.log(users.length);
                        response(users);

                    }, function (error) {
                        //console.log(error);
                    });
                },
                focus: function (event, ui) {
                    $("#user").val(ui.item.label);
                    return false;
                },
                select: function (event, ui) {
                    $("#user").val(ui.item.label);
                    $("#userId").val(ui.item.value);
                    //$("#project-description").html(ui.item.desc);
                    //$("#project-icon").attr("src", "images/" + ui.item.icon);

                    return false;
                }
            })
		    .data("autocomplete")._renderItem = function (ul, item) {
		        return $("<li></li>")
				    .data("item.autocomplete", item)
				    .append("<a><div><img src='http://www.gravatar.com/avatar/" + item.email_hash + "?s=32&d=identicon&=PG' style='float:left;'/>" + item.label + "<br>" + item.rep + "</div></a>")
				    .appendTo(ul);
		    };


            $("#sites").combobox();

            $('#combined').change(function () {
                if (this.checked) {
                    $('#beta').parent('li').slideDown();
                } else {
                    $('#beta').parent('li').slideUp();
                }
            });
        });
    </script>
</head>
<body>
    <div>
        <% using (Html.BeginForm("Create", "Home", FormMethod.Post, new { id = "flair" })) {%>
        <h3>
            Create New StackFlair</h3>
        <ul>
            <%-- Sites  --%>
            <li>
                <label for="sites">
                    Select a site:</label>
                <%=Html.DropDownList("sites", (IEnumerable<SelectListItem>) ViewData["sites"])%>
            </li>
            <%-- User Id --%>
            <li>
                <label for="user">Enter your display name:<br />(and select your gravatar)</label>
                <%=Html.TextBox("user")%> 
            </li>
            <li>
                <label for="userId">Enter your numeric id</label>
                <%= Html.TextBox("userId") %>
            </li>
            <li>
                <label for="combined">
                    Show combined flair:</label>
                <%=Html.CheckBox("combined", true)%>
            </li>
            <%-- Beta --%>
            <li>
                <label for="beta">
                    Exclude beta SE sites:</label>
                <%=Html.CheckBox("beta", false)%>
            </li>
            <%-- Format --%>
            <li>
                <label for="format">
                    Select a format:</label>
                <div class="buttonset" id="format">
                    <% var radioButtonList = new List<ListItem>() {
                                                    new ListItem() {Text = "Image", Value = "png", Selected = true}, 
                                                    new ListItem() {Text = "HTML", Value = "html"}
                                                 };
                       foreach (var radiobutton in radioButtonList) { %>
                    <%=Html.RadioButton("format", radiobutton.Value, radiobutton.Selected,new {id=String.Format("format_{0}",radiobutton.Value)})%>
                    <label for='<%= String.Format("format_{0}",radiobutton.Value) %>'>
                        <%=radiobutton.Text %></label>
                    <% } %>
                </div>
            </li>
            <%-- Theme --%>
            <li>
                <h3>
                    Select a theme:</h3>
                <ul id="themes">
                    <% var themesList = new List<ListItem>() {
                                                    new ListItem() {Text = "Default", Value = "default", Selected = true}, 
                                                    new ListItem() {Text = "Black", Value="black"},
                                                    new ListItem() { Text="HotDog", Value="hotdog"},
                                                    new ListItem() {Text="HoLy",Value="holy"},
                                                    new ListItem() {Text="Nimbus", Value="nimbus"}
                                                };
                       foreach (var theme in themesList) {%>
                    <li <%= theme.Selected ? "class='selected'" : "" %>>
                        <label for='<%= String.Format("themes_{0}",theme.Value) %>'>
                            <img src='<%= String.Format("http://stackflair.com/Generate/options/theme={0}/{1}.png",theme.Value,ViewData["userGuid"]) %>'
                                alt='<%= theme.Text %>' />
                            <strong>
                                <%= theme.Text %></strong>
                        </label>
                        <%= Html.RadioButton("themes",theme.Value,theme.Selected, new { id = String.Format("themes_{0}",theme.Value)}) %>
                    </li>
                    <% } %>
                </ul>
            </li>
        </ul>
        <input name="submit" value="Generate Flair" id="submit" type="submit" />
        <% } %>
    </div>
</body>
</html>
