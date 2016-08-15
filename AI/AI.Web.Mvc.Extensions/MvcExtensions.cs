using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public static class MvcExtensions
{
	public static string Content2(this UrlHelper helper, string contentPath)
	{
		string currentUrl = helper.Content(contentPath);
		try
		{
			string filePath = HttpContext.Current.Server.MapPath(contentPath);
			DateTime fileLastModDate = File.GetLastWriteTime(filePath);
			string urlAppend = fileLastModDate.ToString("yyyyMMddHHmmss");
			if (currentUrl.Contains("?")) //append to query string with &
			{
				currentUrl += "&t=" + urlAppend;
			}
			else //create query string with ?
			{
				currentUrl += "?t=" + urlAppend;
			}
			return currentUrl;
		}
		catch
		{
			return currentUrl;
		}
	}


    public static IList<SelectListItem> DropdownForEnum(this HtmlHelper helper, Type enumeration)
    {
        var values = Enum.GetNames(enumeration);
        var list = new List<SelectListItem>();
        foreach (var value in values)
        {
            list.Add(new SelectListItem()
            {
                Text = value,
                Value = value
            });
        }
        return list;
    }
 


    

	public static string Action2<TController>(this UrlHelper helper, Expression<Func<TController, ActionResult>> controllerAction) where TController : Controller
	{
		string controllerName = typeof(TController).Name.Trim("Controller");
		string actionName = (controllerAction.Body as MethodCallExpression).Method.Name;
		return helper.Action(actionName, controllerName);
	}

    public static IHtmlString LabelAndTextBoxFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, object labelAttributes, object textboxAttributes)
    {
        var labelOutput = helper.LabelFor(expression, labelAttributes);
        var textBoxOutput = helper.TextBoxFor(expression, textboxAttributes);
        var html = labelOutput.ToHtmlString() + textBoxOutput.ToHtmlString();
        return helper.Raw(html);
    }

    public static IHtmlString LabelAndTextBoxFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
    {
        return LabelAndTextBoxFor(helper, expression, null, null);
    }

    public static MvcHtmlString RadioButtonForEnum<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
    {
        var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

        var names = Enum.GetNames(metaData.ModelType);
        var sb = new StringBuilder();
        foreach (var name in names)
        {
            
            var description = name;

            var memInfo = metaData.ModelType.GetMember(name);
            if (memInfo != null)
            {
                var attributes = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attributes != null && attributes.Length > 0)
                    description = ((DisplayAttribute)attributes[0]).Description;
            }
            var id = string.Format(
                "{0}_{1}_{2}",
                htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix,
                metaData.PropertyName,
                name
            );

            var radio = htmlHelper.RadioButtonFor(expression, name, new { id = id }).ToHtmlString();

            sb.AppendFormat(
                "<label for=\"{0}\">{1}</label> {2}",
                id,
                HttpUtility.HtmlEncode(description),
                radio
            );
        }
        return MvcHtmlString.Create(sb.ToString());
    }

}