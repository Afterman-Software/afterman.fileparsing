using System;
using System.Web;
using System.Web.Mvc;

public static class TwitterBootstrap
{

    
	public static IHtmlString NavbarItem(this HtmlHelper helper, string text, string url, string icon, params string[] securityRoleRequired)
	{
		//<li><a href="@Url.Content("~[<url]")">[text]</a></li>
		var html = String.Empty;
        var iconString = String.Empty;
        if (!String.IsNullOrEmpty(icon))
        {
            iconString = String.Format("<i class='{0}'> </i> ",icon);
        }
		if (securityRoleRequired == null || securityRoleRequired.Length == 0)
		{
			html = String.Format(@"<li><a href='{0}'>{2}{1}</a></li>", url, text,iconString);
		}
		else
		{
			foreach (string securityRole in securityRoleRequired)
			{
				if (String.IsNullOrEmpty(securityRole) || HttpContext.Current.User.IsInRole(securityRole))
				{
					html = String.Format(@"<li><a href='{0}'>{2}{1}</a></li>", url, text,iconString);
					break;
				}
			}
		}

		return helper.Raw(html);
	}
}

